using System;
using System.Collections.Generic;
using System.Linq;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class Inpainter
    {
        public event EventHandler<InpaintIterationFinishedEventArgs> IterationFinished;

        private InpaintingResult _inpaintingResult;
        private readonly PyramidLevelsDetector _levelDetector;
        private readonly PyramidBuilder _pyramidBuilder;
        private readonly PatchMatchNnfBuilder _nnfBuilder;

        public Inpainter()
        {
            _levelDetector = new PyramidLevelsDetector();
            _pyramidBuilder = new PyramidBuilder();
            _nnfBuilder = new PatchMatchNnfBuilder();
        }

        public ZsImage Inpaint(ZsImage imageArgb, ZsImage markupArgb, IEnumerable<ZsImage> donorsArgb = null)
        {
            return Inpaint(imageArgb, markupArgb, new InpaintSettings(), donorsArgb);
        }

        public ZsImage Inpaint(ZsImage imageArgb, ZsImage markupArgb, InpaintSettings settings, IEnumerable<ZsImage> donorsArgb = null)
        {
            #region validate the inputs

            if (imageArgb == null)
                throw new ArgumentNullException(nameof(imageArgb));

            if (imageArgb.NumberOfComponents != 4)
                throw new BadImageFormatException($"{nameof(imageArgb)} is expected to be in ARGB format");

            if (markupArgb == null)
                throw new ArgumentNullException(nameof(markupArgb));

            if (markupArgb.NumberOfComponents != 4)
                throw new BadImageFormatException($"{nameof(markupArgb)} is expected to be in ARGB format");

            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (donorsArgb == null)
            {
                donorsArgb = new ZsImage[0];
            }
            else if (donorsArgb.Any(donorArgb => donorArgb.NumberOfComponents != 4))
            {
                throw new BadImageFormatException($"{nameof(donorsArgb)} are expected to be in ARGB format");
            }
            #endregion

            // Prepare input data
            var originalImageArea = Area2D.Create(0, 0, imageArgb.Width, imageArgb.Height);
            ZsImage imageToPorcess;
            Pyramid imagePyramid;

            var levelsAmount = _levelDetector.CalculateLevelsAmount(imageArgb, markupArgb, settings.PatchSize);
            
            // Get the area of the image that can be scaled down required amount of times (levels)
            var areaToProcess = GetImageAreaToProcess(imageArgb, markupArgb, levelsAmount);

            if (!areaToProcess.IsSameAs(originalImageArea))
            {
                // We need to crop the original image, markup and donors
                // since the area to process differs from the image area
                imageToPorcess = CopyImageArea(imageArgb, areaToProcess);
                var markup = CopyImageArea(markupArgb, areaToProcess);
                var donors = donorsArgb.Select(donorImage => CopyImageArea(donorImage, areaToProcess));

                imagePyramid = BuildPyramid(imageToPorcess, markup, donors, levelsAmount, settings.PatchSize);
            }
            else
            {
                imageToPorcess = imageArgb.Clone();
                imagePyramid = BuildPyramid(imageToPorcess, markupArgb, donorsArgb, levelsAmount, settings.PatchSize);
            }

            imageToPorcess = InternalInpaint(imagePyramid, settings);

            // paste result in the original bitmap where it was extracted from
            var imageArea = Area2D.Create(0, 0, imageToPorcess.Width, imageToPorcess.Height);
            imageToPorcess.FromLabToRgb()
                .FromRgbToArgb(imageArea);

            imageArgb.CopyFromImage(areaToProcess, imageToPorcess, imageArea);

            return imageArgb;
        }

        private ZsImage InternalInpaint(Pyramid pyramid, InpaintSettings settings)
        {

            #region cache settings
            var patchSize = settings.PatchSize;
            var calculator = settings.PatchDistanceCalculator;
            var nnfSettings = settings.PatchMatch;
            var changedPixelsPercentTreshold = settings.ChangedPixelsPercentTreshold;
            var maxInpaintIterationsAmount = settings.MaxInpaintIterations;
            var kStep = settings.MeanShift.KDecreaseStep;
            var minK = settings.MeanShift.MinK;
            #endregion

            ZsImage image = null;

            // go thru all the pyramid levels starting from the top one
            Nnf nnf = null;

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                image = pyramid.GetImage(levelIndex);
                var mapping = pyramid.GetMapping(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);

                var imageArea = Area2D.Create(0, 0, image.Width, image.Height);

                // if there is a NNF built on the prev level
                // scale it up
                nnf = nnf == null
                    ? new Nnf(image.Width, image.Height, image.Width, image.Height, patchSize)
                    : nnf.CloneAndScale2XWithUpdate(image, image, nnfSettings, mapping, calculator);

                // start inpaint iterations
                var k = settings.MeanShift.K;
                for (var inpaintIterationIndex = 0; inpaintIterationIndex < maxInpaintIterationsAmount; inpaintIterationIndex++)
                {
                    // Obtain pixels area.
                    // Pixels area defines which pixels are allowed to be used
                    // for the patches distance calculation. We must avoid pixels
                    // that we want to inpaint. That is why before the area is not
                    // inpainted - we should exclude this area.
                    var pixelsArea = imageArea;
                    if (levelIndex == 0 && inpaintIterationIndex == 0)
                    {
                        pixelsArea = imageArea.Substract(inpaintArea);
                    }

                    // skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we haven't built NNF yet)
                    if (levelIndex == 0 || inpaintIterationIndex > 0)
                    {
                        // in order to find best matches for the inpainted area,
                        // we build NNF for this image as a dest and a source 
                        // but excluding the inpainted area from the source area
                        // (our mapping already takes care of it)
                        _nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);
                        _nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        _nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        _nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        _nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        _nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                    }

                    var nnfNormalized = nnf.Clone();
                    nnfNormalized.Normalize();

                    // after we have the NNF - we calculate the values of the pixels in the inpainted area
                    var inpaintResult = Inpaint(image, inpaintArea, nnfNormalized, k, settings);
                    k = k > minK ? k - kStep : k;

                    if (IterationFinished != null)
                    {
                        var eventArgs = new InpaintIterationFinishedEventArgs
                        {
                            InpaintedLabImage = image.Clone(),
                            InpaintResult = inpaintResult,
                            LevelIndex = levelIndex,
                            InpaintIteration = inpaintIterationIndex
                        };
                        IterationFinished(this, eventArgs);
                    }

                    // if the change is smaller then a treshold, we quit
                    if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                    if (levelIndex == pyramid.LevelsAmount - 1) break;
                }
            }

            return image;
        }

        private InpaintingResult Inpaint(ZsImage image, Area2D removeArea, Nnf nnf, double k, IInpaintSettings settings)
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
            _inpaintingResult.TotalDifference = totalDifference / pointIndexes.Length;
            _inpaintingResult.PixelsChangedAmount = changedPixelsAmount;
            _inpaintingResult.PixelsToInpaintAmount = pointIndexes.Length;
            _inpaintingResult.ChangedPixelsDifference = changedPixelsDifference;

            return _inpaintingResult;
        }

        private Pyramid BuildPyramid(ZsImage imageArgb, ZsImage markupArgb, IEnumerable<ZsImage> donorsArgb, byte levelsAmount, byte patchSize)
        {
            _pyramidBuilder.Init(imageArgb, markupArgb);
            foreach (var donorArgb in donorsArgb)
            {
                _pyramidBuilder.AddDonorMarkup(donorArgb);
            }

            var pyramid = _pyramidBuilder.Build(levelsAmount, patchSize);
            return pyramid;
        }

        public Tuple<int, int> Calculate(int width, int height, byte levels)
        {
            var resultWidth = width;
            var resultHeight = height;

            if (width % 2 != 0)
                resultWidth--;

            if (height % 2 != 0)
                resultHeight--;

            while (!IsValid(resultWidth, levels))
            {
                resultWidth -= 2;
            }

            while (!IsValid(resultHeight, levels))
            {
                resultHeight -= 2;
            }

            return new Tuple<int, int>(resultWidth, resultHeight);
        }

        private static bool IsValid(int val, byte levels)
        {
            bool wNoRest = true;
            for (int levelIndex = 0; levelIndex < (levels - 1) && wNoRest; levelIndex++)
            {
                if (val % 2 != 0)
                    wNoRest = false;

                val /= 2;
            }

            return wNoRest;
        }

        public Vector2D CalculateOffset(int width, int height, Area2D markup)
        {
            int x = 0;
            int y = 0;

            var bottomRightX = markup.Bound.Width + markup.Bound.X;
            var bottomRightY = markup.Bound.Height + markup.Bound.Y;

            while (x + width <= bottomRightX || x + width <= bottomRightX)
            {
                x++;
            }

            while (y + height <= bottomRightY || y + height <= bottomRightY)
            {
                y++;
            }

            return new Vector2D(x, y);
        }

        private static ZsImage CopyImageArea(ZsImage imageArgb, Area2D imageSrcArea)
        {
            var pixels = Enumerable.Repeat(0.0, imageSrcArea.Bound.Width * imageSrcArea.Bound.Height * 4).ToArray();
            var resultImage = new ZsImage(pixels, imageSrcArea.Bound.Width, imageSrcArea.Bound.Height, 4);
            var imageArgbArea = Area2D.Create(0, 0, imageSrcArea.Bound.Width, imageSrcArea.Bound.Height);
            resultImage.CopyFromImage(imageArgbArea, imageArgb, imageSrcArea);
            return resultImage;
        }

        private Area2D GetImageAreaToProcess(ZsImage imageArgb, ZsImage markupArgb, byte levelsAmount)
        {
            var size = Calculate(imageArgb.Width, imageArgb.Height, levelsAmount);

            var markupArgbArea = markupArgb.FromArgbToArea2D();
            var offset = CalculateOffset(size.Item1, size.Item2, markupArgbArea);

            var imageSrcArea = Area2D.Create((int)offset.X, (int)offset.Y, size.Item1, size.Item2);
            return imageSrcArea;
        }
    }
}