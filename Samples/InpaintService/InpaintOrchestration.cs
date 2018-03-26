using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService
{
    public static class InpaintOrchestration
    {
        [FunctionName("InpaintOrchestration")]
        public static async Task<long> Orchest(
            [OrchestrationTrigger]
            DurableOrchestrationContext ctx)
        {
            var inpaintRequest = ctx.GetInput<InpaintRequest>();

            var nnfBuilder = new PatchMatchNnfBuilder();
            var settings = new InpaintSettings();

            var pyramid = await ctx.CallActivityAsync<CloudPyramid>("GeneratePyramids", inpaintRequest);

            //ZsImage image = null;
            //Nnf nnf = null;

            #region cache settings
            var patchSize = settings.PatchSize;
            var calculator = settings.PatchDistanceCalculator;
            var nnfSettings = settings.PatchMatch;
            var changedPixelsPercentTreshold = settings.ChangedPixelsPercentTreshold;
            var maxInpaintIterationsAmount = 10;//settings.MaxInpaintIterations;
            var kStep = settings.MeanShift.KDecreaseStep;
            var minK = settings.MeanShift.MinK;
            #endregion

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var imageName = pyramid.GetImageName(levelIndex);
                var mapping = pyramid.GetMapping(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);

                //var imageArea = Area2D.Create(0, 0, image.Width, image.Height);

                // if there is a NNF built on the prev level
                // scale it up
                var input = NnfInputData.From(null, inpaintRequest.Container, imageName, settings, calculator, mapping,
                    null, false);

                if (levelIndex == 0)
                {
                    input.NnfName = await ctx.CallActivityAsync<string>("CreateNnf", input);
                }
                else
                {
                    input.NnfName = await ctx.CallActivityAsync<string>("ScaleNnf", input);
                }
                //nnf = nnf == null
                //    ? new Nnf(image.Width, image.Height, image.Width, image.Height, patchSize)
                //    : nnf.CloneAndScale2XWithUpdate(image, image, nnfSettings, mapping, calculator);

                //// start inpaint iterations
                //var k = settings.MeanShift.K;
                for (var inpaintIterationIndex = 0; inpaintIterationIndex < maxInpaintIterationsAmount; inpaintIterationIndex++)
                {
                    // Obtain pixels area.
                    // Pixels area defines which pixels are allowed to be used
                    // for the patches distance calculation. We must avoid pixels
                    // that we want to inpaint. That is why before the area is not
                    // inpainted - we should exclude this area.
                    //    var pixelsArea = imageArea;
                    if (levelIndex == 0 && inpaintIterationIndex == 0)
                    {
                        //        pixelsArea = imageArea.Substract(inpaintArea);
                    }

                    // skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we haven't built NNF yet)
                    if (levelIndex == 0 || inpaintIterationIndex > 0)
                    {
                        //        // in order to find best matches for the inpainted area,
                        //        // we build NNF for this imageLab as a dest and a source 
                        //        // but excluding the inpainted area from the source area
                        //        // (our mapping already takes care of it)

                        //        var res = await ctx.CallActivityAsync<int>("LengthCheck", "inputData");
                        //        //var inputData = NnfInputData.From(nnf, inpaintRequest.Container, levelImageName, nnfSettings, calculator, mapping, pixelsArea, false);
                        //        //var nnfState = await ctx.CallActivityAsync<NnfState>("RandomNnfInitIteration", inputData);
                        //        //nnf = new Nnf(nnfState);

                        //        nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);
                        //        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        //        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        //        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        //        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        //        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                    }

                    //    var nnfNormalized = nnf.Clone();
                    //    nnfNormalized.Normalize();

                    //    // after we have the NNF - we calculate the values of the pixels in the inpainted area
                    //    var inpaintResult = Inpaint(image, mapping, nnfNormalized, k, settings);
                    //    k = k > minK ? k - kStep : k;

                    //    //await SaveImageLabToBlob(image, container, $"{levelIndex}_{inpaintIterationIndex}.png");

                    //    // if the change is smaller then a treshold, we quit
                    //    if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                    //    if (levelIndex == pyramid.LevelsAmount - 1) break;
                }
            }

            return 100;
        }

        [FunctionName("CreateNnf")]
        public static async Task<string> CreateNnf([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var nnf = new Nnf(imageArgb.Width, imageArgb.Height, imageArgb.Width, imageArgb.Height, input.Settings.PatchSize);
            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            const string nnfBlobName = "nnf.json";
            SaveJsonToBlob(nnfData, container, nnfBlobName);
            return nnfBlobName;
        }

        [FunctionName("ScaleNnf")]
        public static async Task<string> ScaleNnf([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var image = (await ConvertBlobToArgbImage(imageBlob))
                .FromArgbToRgb(new[] {0.0, 0.0, 0.0})
                .FromRgbToLab();

            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var mappingState = ReadFromBlob<Area2DMapState>(input.Area2DMapName, container);
            var mapping = new Area2DMap(mappingState);

            var nnfState = ReadFromBlob<NnfState>(input.NnfName, container);
            var nnf = new Nnf(nnfState);

            nnf.CloneAndScale2XWithUpdate(image, image, input.Settings.PatchMatch, mapping, calculator);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            const string nnfBlobName = "nnf.json";
            SaveJsonToBlob(nnfData, container, nnfBlobName);

            return nnfBlobName;
        }

        private static T ReadFromBlob<T>(string blobName, CloudBlobContainer container)
        {
            var blob = container.GetBlockBlobReference(blobName);
            var json = blob.DownloadText();
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
        }

        [FunctionName("GeneratePyramids")]
        public static async Task<CloudPyramid> GeneratePyramids([ActivityTrigger] InpaintRequest inpaintRequest)
        {
            var levelDetector = new PyramidLevelsDetector();
            var pyramidBuilder = new PyramidBuilder();
            var settings = new InpaintSettings();

            var container = OpenBlobContainer(inpaintRequest.Container);
            var imageBlob = container.GetBlockBlobReference(inpaintRequest.Image);
            var removeMaskBlob = container.GetBlockBlobReference(inpaintRequest.RemoveMask);

            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var removeMaskArgb = await ConvertBlobToArgbImage(removeMaskBlob);

            var levelsAmount = levelDetector.CalculateLevelsAmount(imageArgb, removeMaskArgb, settings.PatchSize);
            pyramidBuilder.Init(imageArgb, removeMaskArgb);
            var pyramid = pyramidBuilder.Build(levelsAmount, settings.PatchSize);
            var cloudPyramid = new CloudPyramid
            {
                ImageNames = new string[pyramid.LevelsAmount],
                InpaintAreas = new string[pyramid.LevelsAmount],
                Mappings = new string[pyramid.LevelsAmount]
            };

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var image = pyramid.GetImage(levelIndex);
                var fileName = $"{levelIndex}.png";
                await SaveImageLabToBlob(image, container, fileName);
                cloudPyramid.ImageNames[levelIndex] = fileName;

                var inpaintArea = pyramid.GetInpaintArea(levelIndex).GetState();
                var inpaintAreaFileName = $"ia{levelIndex}.json";
                var inpaintAreaData = JsonConvert.SerializeObject(inpaintArea);
                SaveJsonToBlob(inpaintAreaData, container, inpaintAreaFileName);
                cloudPyramid.InpaintAreas[levelIndex] = inpaintAreaFileName;

                var mapping = pyramid.GetMapping(levelIndex).GetState();
                var mappingFileName = $"map{levelIndex}.json";
                var mappingData = JsonConvert.SerializeObject(mapping);
                SaveJsonToBlob(mappingData, container, mappingFileName);
                cloudPyramid.Mappings[levelIndex] = mappingFileName;
            }

            return cloudPyramid;
        }

        private static CloudBlobContainer OpenBlobContainer(string containerName)
        {
            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);
            return container;
        }

        private static void SaveJsonToBlob(string data, CloudBlobContainer container, string fileName)
        {
            var blob = container.GetBlockBlobReference(fileName);
            blob.DeleteIfExists();
            using (var stream = new MemoryStream(Encoding.Default.GetBytes(data), false))
            {
                blob.UploadFromStream(stream);
            }
        }

        private static async Task SaveImageLabToBlob(ZsImage imageLab, CloudBlobContainer container, string fileName)
        {
            var argbImage = imageLab
                .Clone()
                .FromLabToRgb()
                .FromRgbToArgb(Area2D.Create(0, 0, imageLab.Width, imageLab.Height));

            using (var bitmap = argbImage.FromArgbToBitmap())
            using (var outputStream = new MemoryStream())
            {
                // modify image
                bitmap.Save(outputStream, ImageFormat.Png);

                // save the result back
                outputStream.Position = 0;
                var resultImageBlob = container.GetBlockBlobReference(fileName);
                await resultImageBlob.UploadFromStreamAsync(outputStream);
            }
        }

        public static Task<ZsImage> ConvertBlobToArgbImage(CloudBlob imageBlob)
        {
            using (var imageData = new MemoryStream())
            {
                var downloadTask = imageBlob.DownloadToStreamAsync(imageData);
                downloadTask.Wait();
                using (var bitmap = new Bitmap(imageData))
                {
                    return Task.FromResult(bitmap.ToArgbImage());
                }
            }
        }

        //[FunctionName("RandomNnfInitIteration")]
        //public static Task<NnfState> NnfInit([ActivityTrigger] NnfInputData input)
        //{
        //    var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
        //    var storageAccount = CloudStorageAccount.Parse(connectionString);
        //    var blobClient = storageAccount.CreateCloudBlobClient();
        //    var container = blobClient.GetContainerReference(input.Container);
        //    var imageBlob = container.GetBlockBlobReference(input.Image);

        //    var imageArgb = ConvertBlobToArgbImage(imageBlob).Result;
        //    var image = imageArgb
        //                    .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
        //                    .FromRgbToLab();

        //    var nnf = new Nnf(input.NnfState);
        //    var nnfSettings = new PatchMatchSettings();// input.PmSettings;
        //    var calculator = input.IsCie79Calc
        //        ? ImagePatchDistance.Cie76
        //        : ImagePatchDistance.Cie2000;
        //    var mapping = new Area2DMap(input.Area2DMapState);
        //    var pixelsArea = Area2D.RestoreFrom(input.PixelsAreaState);

        //    var nnfBuilder = new PatchMatchNnfBuilder();
        //    nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);

        //    return Task.FromResult(nnf.GetState());
        //}

        [FunctionName("LengthCheck")]
        public static Task<int> Calc([ActivityTrigger] string input)
        {
            var task = Task.Delay(TimeSpan.FromSeconds(5));
            task.Wait();
            return Task.FromResult(input.Length);
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
