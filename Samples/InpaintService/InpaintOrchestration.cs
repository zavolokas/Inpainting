using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace InpaintService
{
    public static partial class InpaintOrchestration
    {
        [FunctionName("InpaintOrchestration")]
        public static async Task Orchest(
            [OrchestrationTrigger]
            DurableOrchestrationContext ctx)
        {
            var inpaintRequest = ctx.GetInput<InpaintRequest>();

            // TODO: get the setting from parameters
            var settings = new InpaintSettings();

            // TODO: downscale the input image
            // TODO: crop the input image

            var pyramid = await ctx.CallActivityAsync<CloudPyramid>("GeneratePyramids", inpaintRequest);

            var maxInpaintIterationsAmount = 10;//settings.MaxInpaintIterations;
            //var kStep = settings.MeanShift.KDecreaseStep;
            //var minK = settings.MeanShift.MinK;

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var imageName = pyramid.GetImageName(levelIndex);
                var mappings = pyramid.GetMapping(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);

                // if there is a NNF built on the prev level
                // scale it up
                var input = NnfInputData.From($"nnf{levelIndex}.json", inpaintRequest.Container, imageName, settings, mappings, inpaintArea, false, levelIndex, settings.MeanShift.K);

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

                        for (var pmIteration = 0; pmIteration < settings.PatchMatch.IterationsAmount; pmIteration++)
                        {
                            // TODO: split to many parts and process in parallel
                            input.IsForward = !input.IsForward;
                            input.PatchMatchIteration = pmIteration;
                            await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        }

                        // TODO: merge nnf into one
                    }

                    var inpaintResult = await ctx.CallActivityAsync<InpaintingResult>("InpaintImage", input);
                    //input.K = input.K > minK ? input.K - kStep : input.K;

                    // if the change is smaller then a treshold, we quit
                    //if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                    //if (levelIndex == pyramid.LevelsAmount - 1) break;
                }
            }
        }

        private static T ReadFromBlob<T>(string blobName, CloudBlobContainer container)
        {
            var blob = container.GetBlockBlobReference(blobName);
            var json = blob.DownloadText();
            var obj = JsonConvert.DeserializeObject<T>(json);
            return obj;
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
    }
}
