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
            storage.OpenContainer(input.Container);

            var imageArgb = await storage.ReadArgbImageAsync(input.Image);
            var image = imageArgb
                .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                .FromRgbToLab();

            var imageArea = Area2D.Create(0, 0, image.Width, image.Height);
            var pixelsArea = imageArea;

            var nnfSettings = input.Settings.PatchMatch;
            var calculator = input.IsCie79Calc
                ? ImagePatchDistance.Cie76
                : ImagePatchDistance.Cie2000;

            var nnfState = storage.Read<NnfState>(input.NnfName);
            var nnf = new Nnf(nnfState);

            var mappingState = storage.Read<Area2DMapState>(input.Mapping);
            var mapping = new Area2DMap(mappingState);

            if (input.ExcludeInpaintArea)
            {
                var inpaintAreaState = storage.Read<Area2DState>(input.InpaintAreaName);
                var inpaintArea = Area2D.RestoreFrom(inpaintAreaState);
                pixelsArea = imageArea.Substract(inpaintArea);
            }

            var nnfBuilder = new PatchMatchNnfBuilder();

            var direction = input.IsForward
                ? NeighboursCheckDirection.Forward
                : NeighboursCheckDirection.Backward;

            nnfBuilder.RunBuildNnfIteration(nnf, image, image, direction, nnfSettings, calculator, mapping, pixelsArea);

            var nnfData = JsonConvert.SerializeObject(nnf.GetState());
            storage.SaveJson(nnfData, input.NnfName);
        }
    }
}