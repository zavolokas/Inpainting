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

            var settings = new InpaintSettings();

            var pyramid = await ctx.CallActivityAsync<CloudPyramid>("GeneratePyramids", inpaintRequest);

            var maxInpaintIterationsAmount = 3;//settings.MaxInpaintIterations;
            var kStep = settings.MeanShift.KDecreaseStep;
            var minK = settings.MeanShift.MinK;

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var imageName = pyramid.GetImageName(levelIndex);
                var mapping = pyramid.GetMapping(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);

                // if there is a NNF built on the prev level
                // scale it up
                var input = NnfInputData.From($"nnf{levelIndex}.json", inpaintRequest.Container, imageName, settings, mapping, inpaintArea, false, levelIndex, settings.MeanShift.K);

                if (levelIndex == 0)
                {
                    await ctx.CallActivityAsync<string>("CreateNnf", input);
                }
                else
                {
                    await ctx.CallActivityAsync<string>("ScaleNnf", input);
                }

                // start inpaint iterations
                for (var inpaintIterationIndex = 0; inpaintIterationIndex < maxInpaintIterationsAmount; inpaintIterationIndex++)
                {
                    // Obtain pixels area.
                    // Pixels area defines which pixels are allowed to be used
                    // for the patches distance calculation. We must avoid pixels
                    // that we want to inpaint. That is why before the area is not
                    // inpainted - we should exclude this area.
                    input.ExcludeInpaintArea = levelIndex == 0 && inpaintIterationIndex == 0;
                    input.IterationIndex = inpaintIterationIndex;

                    // skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we haven't built NNF yet)
                    if (levelIndex == 0 || inpaintIterationIndex > 0)
                    {
                        // in order to find best matches for the inpainted area,
                        // we build NNF for this imageLab as a dest and a source 
                        // but excluding the inpainted area from the source area
                        // (our mapping already takes care of it)

                        await ctx.CallActivityAsync("RandomNnfInitIteration", input);

                        input.IsForward = true;
                        await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        input.IsForward = false;
                        await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        input.IsForward = true;
                        await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        input.IsForward = false;
                        await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        input.IsForward = true;
                        await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                    }

                    var inpaintResult = await ctx.CallActivityAsync<InpaintingResult>("InpaintImage", input);
                    input.K = input.K > minK ? input.K - kStep : input.K;

                    // if the change is smaller then a treshold, we quit
                    //if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                    //if (levelIndex == pyramid.LevelsAmount - 1) break;
                }
            }

            return 100;
        }

        [FunctionName("InpaintImage")]
        public static async Task<InpaintingResult> InpaintImage([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);

            var imageBlob = container.GetBlockBlobReference(input.Image);
            var image = (await ConvertBlobToArgbImage(imageBlob))
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var inpaintAreaState = ReadFromBlob<Area2DState>(input.InpaintAreaName, container);
            var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);

            var nnfState = ReadFromBlob<NnfState>(input.NnfName, container);
            var nnf = new Nnf(nnfState);
            nnf.Normalize();

            // after we have the NNF - we calculate the values of the pixels in the inpainted area
            var inpaintResult = Inpaint(image, inpaintArea, nnf, input.K, input.Settings);
            await SaveImageLabToBlob(image, container, input.Image);
            await SaveImageLabToBlob(image, container, $"{input.LevelIndex}_{input.IterationIndex}.png");

            return inpaintResult;
        }

        [FunctionName("CreateNnf")]
        public static async Task CreateNnf([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var nnf = new Nnf(imageArgb.Width, imageArgb.Height, imageArgb.Width, imageArgb.Height, input.Settings.PatchSize);
            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            
            SaveJsonToBlob(nnfData, container, input.NnfName);
        }

        [FunctionName("ScaleNnf")]
        public static async Task ScaleNnf([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var image = (await ConvertBlobToArgbImage(imageBlob))
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var mappingState = ReadFromBlob<Area2DMapState>(input.Area2DMapName, container);
            var mapping = new Area2DMap(mappingState);

            var nnfState = ReadFromBlob<NnfState>($"nnf{input.LevelIndex-1}.json", container);
            var nnf = new Nnf(nnfState);

            nnf = nnf.CloneAndScale2XWithUpdate(image, image, input.Settings.PatchMatch, mapping, calculator);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            SaveJsonToBlob(nnfData, container, input.NnfName);
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

                var inpaintArea = pyramid.GetInpaintArea(levelIndex);
                var inpaintAreaState = inpaintArea.GetState();
                var inpaintAreaFileName = $"ia{levelIndex}.json";
                var inpaintAreaData = JsonConvert.SerializeObject(inpaintAreaState);
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

        [FunctionName("RandomNnfInitIteration")]
        public static async Task NnfInit([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);

            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var image = imageArgb
                            .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                            .FromRgbToLab();

            var imageArea = Area2D.Create(0, 0, image.Width, image.Height);
            var pixelsArea = imageArea;

            var nnfSettings = input.Settings.PatchMatch;
            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var nnfState = ReadFromBlob<NnfState>(input.NnfName, container);
            var nnf = new Nnf(nnfState);

            var mappingState = ReadFromBlob<Area2DMapState>(input.Area2DMapName, container);
            var mapping = new Area2DMap(mappingState);

            if (input.ExcludeInpaintArea)
            {
                var inpaintAreaState = ReadFromBlob<Area2DState>(input.InpaintAreaName, container);
                var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);
                pixelsArea = imageArea.Substract(inpaintArea);
            }

            var nnfBuilder = new PatchMatchNnfBuilder();
            nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            SaveJsonToBlob(nnfData, container, input.NnfName);
        }

        [FunctionName("RunBuildNnfIteration")]
        public static async Task RunBuildNnfIteration([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);

            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var image = imageArgb
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var imageArea = Area2D.Create(0, 0, image.Width, image.Height);
            var pixelsArea = imageArea;

            var nnfSettings = input.Settings.PatchMatch;
            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var nnfState = ReadFromBlob<NnfState>(input.NnfName, container);
            var nnf = new Nnf(nnfState);

            var mappingState = ReadFromBlob<Area2DMapState>(input.Area2DMapName, container);
            var mapping = new Area2DMap(mappingState);

            if (input.ExcludeInpaintArea)
            {
                var inpaintAreaState = ReadFromBlob<Area2DState>(input.InpaintAreaName, container);
                var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);
                pixelsArea = imageArea.Substract(inpaintArea);
            }

            var nnfBuilder = new PatchMatchNnfBuilder();

            var direction = input.IsForward
                ? NeighboursCheckDirection.Forward
                : NeighboursCheckDirection.Backward;

            nnfBuilder.RunBuildNnfIteration(nnf, image, image, direction, nnfSettings, calculator, mapping, pixelsArea);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            SaveJsonToBlob(nnfData, container, input.NnfName);
        }

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
