using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;


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
            var container= args.FirstOrDefault(q => string.Compare(q.Key, "container", true) == 0)
                .Value;

            var blob = args.FirstOrDefault(q => string.Compare(q.Key, "blob", true) == 0)
                .Value;

            return string.IsNullOrWhiteSpace(blob) || string.IsNullOrWhiteSpace(container)
                ? req.CreateResponse(HttpStatusCode.BadRequest, "Please pass a container and blob names on the query string.")
                : req.CreateResponse(HttpStatusCode.OK, $"{container}/{blob}");
        }
    }
}
