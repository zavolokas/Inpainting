using System;
using System.Configuration;
using System.Drawing;
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
            var containerName = args.FirstOrDefault(q => string.Compare(q.Key, "container", true) == 0)
                .Value;

            var blobName = args.FirstOrDefault(q => string.Compare(q.Key, "blob", true) == 0)
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

            CloudBlockBlob blob = null;
            try
            {
                var storageAccount = CloudStorageAccount.Parse(connectionString);
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(containerName);
                blob = container.GetBlockBlobReference(blobName);
            }
            catch (FormatException)
            {
                log.Error($"The format of the connection string is wrong: {connectionString}");
                throw;
            }
            catch (Exception e)
            {
                log.Error("Exception",e);
                throw;
            }

            // verify the image is there
            if (blob.Exists())
            {
                using (var imageData = new MemoryStream())
                {
                    await blob.DownloadToStreamAsync(imageData);
                    using (var bitmap = new Bitmap(imageData))
                    {

                        // TODO: modify image
                        // TODO: save the result back

                        return req.CreateResponse(HttpStatusCode.OK, $"Width: {bitmap.Width}; Height: {bitmap.Height}");
                    }
                }
            }
            else
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "Provided container or blob doesn't exist.");
            }
        }
    }
}
