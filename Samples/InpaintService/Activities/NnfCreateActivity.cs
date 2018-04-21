using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
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
            storage.OpenContainer(input.Container);
            var imageArgb = await storage.ReadArgbImageAsync(input.Image);
            var nnf = new Nnf(imageArgb.Width, imageArgb.Height, imageArgb.Width, imageArgb.Height, input.Settings.PatchSize);

            storage.Save(nnf.GetState(), input.NnfName);
        }
    }
}