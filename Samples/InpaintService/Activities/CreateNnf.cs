using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.Structures;

namespace InpaintService
{
    public static partial class InpaintOrchestration
    {
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
    }
}