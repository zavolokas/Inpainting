using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;

namespace InpaintService
{

    public static class InpaintTrigger
    {
        [FunctionName("InpaintTrigger")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "inpaint")]
            HttpRequestMessage req,
            [OrchestrationClient]
            DurableOrchestrationClient orchestrationClient,
            TraceWriter log)
        {
            log.Info("C# HTTP trigger function processed a request.");


            string jsonContent = await req.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(jsonContent))
            {
                return req.CreateResponse(HttpStatusCode.BadRequest, "No body has been provided.");
            }

            var request = JsonConvert.DeserializeObject<InpaintRequest>(jsonContent);

            if (string.IsNullOrWhiteSpace(request.Image) || string.IsNullOrWhiteSpace(request.Container) || string.IsNullOrWhiteSpace(request.RemoveMask))
                return req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a container, image and remove mask names in the request body.");

            var connectionString = AmbientConnectionStringProvider.Instance.GetConnectionString(ConnectionStringNames.Storage);

            // make sure the connection string is provided
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                log.Error("Connection string to a storage account is required.");
                throw new ConfigurationErrorsException();
            }

            CloudBlockBlob imageBlob;
            CloudBlockBlob removeMaskBlob;
            if (CloudStorageAccount.TryParse(connectionString, out var storageAccount))
            {
                var blobClient = storageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference(request.Container);
                imageBlob = container.GetBlockBlobReference(request.Image);
                removeMaskBlob = container.GetBlockBlobReference(request.RemoveMask);
            }
            else
            {
                log.Error($"The format of the connection string is wrong: {connectionString}");
                return req.CreateResponse(HttpStatusCode.InternalServerError);
            }

            // verify the image is there
            if (imageBlob.Exists() && removeMaskBlob.Exists())
            {
                string instanceId = await orchestrationClient.StartNewAsync("Orchestrate", request);

                log.Info($"Started orchestration with id={instanceId}");
                return orchestrationClient.CreateCheckStatusResponse(req, instanceId);
            }

            return req.CreateResponse(HttpStatusCode.BadRequest, "Provided container or blob doesn't exist.");
        }
    }
}
