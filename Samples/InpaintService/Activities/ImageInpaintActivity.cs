using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class ImageInpaintActivity
    {
        public const string Name = "InpaintImage";

        [FunctionName(Name)]
        public static async Task<InpaintingResult> InpaintImage([ActivityTrigger] NnfInputData input)
        {
            var blobStorage = StorageFactory.CreateBlob();
            blobStorage.OpenContainer(input.Container);

            var storage = StorageFactory.Create();
            storage.OpenContainer(input.Container);

            var image = (await storage.ReadArgbImageAsync(input.Image))
                .FromArgbToRgb(new[] {0.0, 0.0, 0.0})
                .FromRgbToLab();

            var inpaintAreaState = storage.Read<Area2DState>(input.InpaintAreaName);
            var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);

            var nnfState = storage.Read<NnfState>(input.NnfName);
            var nnf = new Nnf(nnfState);
            nnf.Normalize();

            // after we have the NNF - we calculate the values of the pixels in the inpainted area
            var inpaintResult = Inpaint(image, inpaintArea, nnf, input.K, input.Settings);
            await storage.SaveImageLabAsync(image, input.Image);

            // TODO: remove it later it is for debug purpose.
            await storage.SaveImageLabAsync(image, $"{input.LevelIndex}_{input.IterationIndex}.png");
            await blobStorage.SaveImageLabAsync(image, $"{input.LevelIndex}_{input.IterationIndex}.png");

            return inpaintResult;
        }

        private static InpaintingResult Inpaint(ZsImage image, Area2D removeArea, Nnf nnf, double k, IInpaintSettings settings)
        {
            // Since the nnf was normalized, the sigma now is normalized as well and it
            // has not any more some specific value.
            //const double naturalLogBase = System.Math.E;
            //const double minusSigmaCube2 = -0.91125;//-2 * sigma * sigma; sigma = 0.675 = 75percentil
            //double NaturalLogBase2 = System.Math.Pow(System.Math.E, 1.0 / minusSigmaCube2);
            const double naturalLogBasePowMinusSigmaCube2 = 0.33373978049163078;

            const double maxSquareDistanceInLab = 32668.1151;
            // Gamma is used for calculation of alpha in markup. The confidence.
            const double gamma = 1.3;
            // The value of confidence in non marked areas.
            const double confidentValue = 1.50;

            var patchSize = settings.PatchSize;
            var colorResolver = settings.ColorResolver;
            var pixelChangeTreshold = settings.PixelChangeTreshold;

            // Get points' indexes that that needs to be inpainted.
            var pointIndexes = new int[removeArea.ElementsCount];
            removeArea.FillMappedPointsIndexes(pointIndexes, image.Width);

            var patchOffset = (patchSize - 1) / 2;

            var imageWidth = image.Width;
            var nnfWidth = nnf.DstWidth;
            var nnfHeight = nnf.DstHeight;
            var srcWidth = nnf.SourceWidth;
            var srcHeight = nnf.SourceHeight;

            var nnfdata = nnf.GetNnfItems();
            var cmpts = image.NumberOfComponents;

            // Weighted color is color's components values + weight of the color.
            var weightedColors = new double[patchSize * patchSize * (cmpts + 1)];
            var confidence = removeArea.CalculatePointsConfidence(confidentValue, gamma);
            var resolvedColor = new double[cmpts];

            var totalDifference = 0.0;
            var changedPixelsAmount = 0;
            var changedPixelsDifference = 0.0;

            for (int ii = 0; ii < pointIndexes.Length; ii++)
            {
                var pointIndex = pointIndexes[ii];

                int pty = pointIndex / imageWidth;
                int ptx = pointIndex % imageWidth;

                int colorsCount = 0;

                //go thru the patch
                for (int y = pty - patchOffset, j = 0; j < patchSize; j++, y++)
                {
                    for (int x = ptx - patchOffset, i = 0; i < patchSize; i++, x++)
                    {
                        if (0 <= x && x < nnfWidth && 0 <= y && y < nnfHeight)
                        {
                            int destPatchPointIndex = y * nnfWidth + x;
                            int srcPatchPointIndex = (int)nnfdata[destPatchPointIndex * 2];

                            int srcPatchPointX = srcPatchPointIndex % nnfWidth;
                            int srcPatchPointY = srcPatchPointIndex / nnfWidth;

                            int srcPixelX = srcPatchPointX - i + patchOffset;
                            int srcPixelY = srcPatchPointY - j + patchOffset;

                            if (0 <= srcPixelX && srcPixelX < srcWidth && 0 <= srcPixelY && srcPixelY < srcHeight)
                            {
                                int srcPixelIndex = srcPixelY * imageWidth + srcPixelX;

                                for (int ci = 0; ci < cmpts; ci++)
                                {
                                    weightedColors[colorsCount * (cmpts + 1) + ci] = image.PixelsData[srcPixelIndex * cmpts + ci];
                                }
                                weightedColors[colorsCount * (cmpts + 1) + cmpts] =
                                    System.Math.Pow(naturalLogBasePowMinusSigmaCube2, nnfdata[destPatchPointIndex * 2 + 1]) *
                                    confidence[ii];

                                colorsCount++;
                            }
                        }
                    }
                }
                // calculate color
                colorResolver.Resolve(weightedColors, 0, colorsCount, cmpts, k, resolvedColor, 0);

                // how different is resolvedColor from the old color?
                var labL = resolvedColor[0] - image.PixelsData[pointIndex * cmpts + 0];
                var labA = resolvedColor[1] - image.PixelsData[pointIndex * cmpts + 1];
                var labB = resolvedColor[2] - image.PixelsData[pointIndex * cmpts + 2];
                var pixelsDiff = (labL * labL + labA * labA + labB * labB) / maxSquareDistanceInLab;
                totalDifference += pixelsDiff;

                if (pixelsDiff > pixelChangeTreshold)
                {
                    changedPixelsAmount++;
                    changedPixelsDifference += pixelsDiff;
                }

                for (var i = 0; i < cmpts; i++)
                {
                    image.PixelsData[pointIndex * cmpts + i] = resolvedColor[i];
                }
            }

            totalDifference /= pointIndexes.Length;
            var inpaintingResult = new InpaintingResult
            {
                TotalDifference = totalDifference / pointIndexes.Length,
                PixelsChangedAmount = changedPixelsAmount,
                PixelsToInpaintAmount = pointIndexes.Length,
                ChangedPixelsDifference = changedPixelsDifference
            };

            return inpaintingResult;
        }
    }
}