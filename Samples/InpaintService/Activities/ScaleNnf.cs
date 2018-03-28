using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService
{
    public static partial class InpaintOrchestration
    {
        [FunctionName("ScaleNnf")]
        public static async Task ScaleNnf([ActivityTrigger] NnfInputData input)
        {
            var container = OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var image = (await ConvertBlobToArgbImage(imageBlob))
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var mappingState = ReadFromBlob<Area2DMapState>(input.MappingNames[0], container);
            var mapping = new Area2DMap(mappingState);

            var nnfState = ReadFromBlob<NnfState>($"nnf{input.LevelIndex - 1}.json", container);
            var nnf = new Nnf(nnfState);

            nnf = nnf.CloneAndScale2XWithUpdate(image, image, input.Settings.PatchMatch, mapping, calculator);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            SaveJsonToBlob(nnfData, container, input.NnfName);
        }
    }
}