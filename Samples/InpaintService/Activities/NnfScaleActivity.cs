using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class NnfScaleActivity
    {
        public const string Name = "ScaleNnf";

        [FunctionName(Name)]
        public static async Task ScaleNnf([ActivityTrigger] NnfInputData input)
        {
            var storage = StorageFactory.Create();
            storage.OpenContainer(input.Container);
            var image = (await storage.ReadArgbImageAsync(input.Image))
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var mappingState = storage.Read<Area2DMapState>(input.Mapping);
            var mapping = new Area2DMap(mappingState);

            var nnfState = storage.Read<NnfState>($"nnf{input.LevelIndex - 1}.json");
            var nnf = new Nnf(nnfState);

            nnf = nnf.CloneAndScale2XWithUpdate(image, image, input.Settings.PatchMatch, mapping, calculator);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            storage.SaveJson(nnfData, input.NnfName);
        }
    }
}