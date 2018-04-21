using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class NnfBuildActivity
    {
        public const string Name = "NnfBuildIteration";

        [FunctionName(Name)]
        public static async Task NnfBuildIteration([ActivityTrigger] NnfInputData input)
        {
            var storage = StorageFactory.Create();
            var container = storage.OpenBlobContainer(input.Container);

            var imageBlob = container.GetBlockBlobReference(input.Image);
            var imageArgb = await storage.ConvertBlobToArgbImage(imageBlob);
            var image = imageArgb
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var imageArea = Area2D.Create(0, 0, image.Width, image.Height);
            var pixelsArea = imageArea;

            var nnfSettings = input.Settings.PatchMatch;
            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var nnfState = storage.ReadFromBlob<NnfState>(input.NnfName, container);
            var nnf = new Nnf(nnfState);

            var mappingState = storage.ReadFromBlob<Area2DMapState>(input.Mapping, container);
            var mapping = new Area2DMap(mappingState);

            if (input.ExcludeInpaintArea)
            {
                var inpaintAreaState = storage.ReadFromBlob<Area2DState>(input.InpaintAreaName, container);
                var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);
                pixelsArea = imageArea.Substract(inpaintArea);
            }

            var nnfBuilder = new PatchMatchNnfBuilder();

            var direction = input.IsForward
                ? NeighboursCheckDirection.Forward
                : NeighboursCheckDirection.Backward;

            nnfBuilder.RunBuildNnfIteration(nnf, image, image, direction, nnfSettings, calculator, mapping, pixelsArea);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            storage.SaveJsonToBlob(nnfData, container, input.NnfName);
        }
    }
}