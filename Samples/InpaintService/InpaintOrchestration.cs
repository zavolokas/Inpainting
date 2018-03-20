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
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace InpaintService
{
    public static class InpaintOrchestration
    {
        [FunctionName("InpaintOrchestration")]
        public static async Task<long> Orchest(
            [OrchestrationTrigger]
            DurableOrchestrationContext backupContext)
        {
            var inpaintRequest = backupContext.GetInput<InpaintRequest>();

            var levelDetector = new PyramidLevelsDetector();
            var pyramidBuilder = new PyramidBuilder();
            var settings = new InpaintSettings();

            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(inpaintRequest.Container);
            var imageBlob = container.GetBlockBlobReference(inpaintRequest.Image);
            var removeMaskBlob = container.GetBlockBlobReference(inpaintRequest.RemoveMask);

            var imageArgb = await ConvertBlobToArgbImage(imageBlob);
            var removeMaskArgb = await ConvertBlobToArgbImage(removeMaskBlob);

            var levelsAmount = levelDetector.CalculateLevelsAmount(imageArgb, removeMaskArgb, settings.PatchSize);
            pyramidBuilder.Init(imageArgb, removeMaskArgb);
            var pyramid = pyramidBuilder.Build(levelsAmount, settings.PatchSize);

            var pyrextr = await backupContext.CallActivityAsync<long>("PyramidCheck", "pyramid");

            return 100;
        }

        public static Task<ZsImage> ConvertBlobToArgbImage([ActivityTrigger] CloudBlob imageBlob)
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

        [FunctionName("PyramidCheck")]
        public static Task<int> Calc([ActivityTrigger] string pyramid)
        {
            return Task.FromResult(pyramid.Length);
            //return Task.FromResult(pyramid.GetImage(0).Width);
        }
    }
}
