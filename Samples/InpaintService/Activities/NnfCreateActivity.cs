using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class NnfCreateActivity
    {
        public const string Name = "CreateNnf";

        [FunctionName(Name)]
        public static async Task CreateNnf([ActivityTrigger] NnfInputData input)
        {
            var storage = StorageFactory.Create();
            var container = storage.OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await storage.ConvertBlobToArgbImage(imageBlob);
            var nnf = new Nnf(imageArgb.Width, imageArgb.Height, imageArgb.Width, imageArgb.Height, input.Settings.PatchSize);
            var nnfData = JsonConvert.SerializeObject(nnf.GetState());

            storage.SaveJsonToBlob(nnfData, container, input.NnfName);
        }
    }
}