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
            var container = BlobHelper.OpenBlobContainer(input.Container);
            var imageBlob = container.GetBlockBlobReference(input.Image);
            var image = (await BlobHelper.ConvertBlobToArgbImage(imageBlob))
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var mappingState = BlobHelper.ReadFromBlob<Area2DMapState>(input.MappingNames[0], container);
            var mapping = new Area2DMap(mappingState);

            var nnfState = BlobHelper.ReadFromBlob<NnfState>($"nnf{input.LevelIndex - 1}.json", container);
            var nnf = new Nnf(nnfState);

            nnf = nnf.CloneAndScale2XWithUpdate(image, image, input.Settings.PatchMatch, mapping, calculator);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            BlobHelper.SaveJsonToBlob(nnfData, container, input.NnfName);
        }
    }
}