using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService.Activities
{
    public static class NnfRandomInitActivity
    {
        public const string Name = "NnfInitIteration";

        [FunctionName(Name)]
        public static async Task NnfInitIteration([ActivityTrigger] NnfInputData input)
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
            nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);

            var nnfData = nnf.GetState();
            storage.Save(nnfData, input.NnfName);

            foreach (var subNnfName in input.SplittedNnfNames)
            {
                storage.Save(nnfData, subNnfName);
            }
        }
    }
}