using System;
using System.Configuration;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace InpaintService
{
    public static class ImageStamp
    {
        [FunctionName("ImageStamp")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]
            HttpRequestMessage req,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");

            // parse query parameter
            var args = req.GetQueryNameValuePairs();
            var containerName = args.FirstOrDefault(q => String.Compare(q.Key, "container", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            var blobName = args.FirstOrDefault(q => String.Compare(q.Key, "blob", StringComparison.OrdinalIgnoreCase) == 0)
                .Value;

            if (string.IsNullOrWhiteSpace(blobName) || string.IsNullOrWhiteSpace(containerName))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a container and blob names on the query string.");

            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);

            // make sure the connection string is provided
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                log.Error("Connection string to a storage account is required.");
                throw new ConfigurationErrorsException();
            }

            CloudBlockBlob imageBlob;
            CloudBlockBlob resultImageBlob;
            if (CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                imageBlob = container.GetBlockBlobReference(blobName);
                resultImageBlob = container.GetBlockBlobReference("result.png");
            }
            else
            {
                log.Error($"The format of the connection string is wrong: {connectionString}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // verify the image is there
            if (imageBlob.Exists())
            {
                using (var imageData = new MemoryStream())
                {
                    await imageBlob.DownloadToStreamAsync(imageData);
                    using (var bitmap = new Bitmap(imageData))
                    using (var graphics = Graphics.FromImage(bitmap))
                    using (var outputStream = new MemoryStream())
                    {
                        // modify image
                        graphics.DrawString("Stamp", SystemFonts.DefaultFont, Brushes.Red, new PointF(80, 80));
                        bitmap.Save(outputStream, ImageFormat.Png);

                        // save the result back
                        outputStream.Position = 0;
                        await resultImageBlob.UploadFromStreamAsync(outputStream);

                        return req.CreateResponse(HttpStatusCode.OK, $"Width: {bitmap.Width}; Height: {bitmap.Height}");
                    }
                }
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Provided container or blob doesn't exist.");
        }
    }
}
