using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;
using Zavolokas.ImageProcessing.Inpainting;

namespace InpaintService.Activities
{
    public static class PyramidsGenerateActivity
    {
        public const string Name = "GeneratePyramids";

        [FunctionName(Name)]
        public static async Task<CloudPyramid> GeneratePyramids([ActivityTrigger] InpaintRequest inpaintRequest)
        {
            var levelDetector = new PyramidLevelsDetector();
            var pyramidBuilder = new PyramidBuilder();
            var settings = new InpaintSettings();

            var container = BlobHelper.OpenBlobContainer(inpaintRequest.Container);
            var imageBlob = container.GetBlockBlobReference(inpaintRequest.Image);
            var removeMaskBlob = container.GetBlockBlobReference(inpaintRequest.RemoveMask);

            var imageArgb = await BlobHelper.ConvertBlobToArgbImage(imageBlob);
            var removeMaskArgb = await BlobHelper.ConvertBlobToArgbImage(removeMaskBlob);

            var levelsAmount = levelDetector.CalculateLevelsAmount(imageArgb, removeMaskArgb, settings.PatchSize);
            pyramidBuilder.Init(imageArgb, removeMaskArgb);
            var pyramid = pyramidBuilder.Build(levelsAmount, settings.PatchSize);
            var cloudPyramid = new CloudPyramid
            {
                Levels = new CloudPyramidLevel[pyramid.LevelsAmount]
            };

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var image = pyramid.GetImage(levelIndex);
                var fileName = $"{levelIndex}.png";
                await BlobHelper.SaveImageLabToBlob(image, container, fileName);
                cloudPyramid.Levels[levelIndex].ImageName = fileName;

                var inpaintArea = pyramid.GetInpaintArea(levelIndex);
                var inpaintAreaState = inpaintArea.GetState();
                var inpaintAreaFileName = $"ia{levelIndex}.json";
                var inpaintAreaData = JsonConvert.SerializeObject(inpaintAreaState);
                BlobHelper.SaveJsonToBlob(inpaintAreaData, container, inpaintAreaFileName);
                cloudPyramid.Levels[levelIndex].InpaintArea = inpaintAreaFileName;

                var mapping = pyramid.GetMapping(levelIndex).GetState();
                var mappingFileName = $"map{levelIndex}.json";
                var mappingData = JsonConvert.SerializeObject(mapping);
                BlobHelper.SaveJsonToBlob(mappingData, container, mappingFileName);
                cloudPyramid.Levels[levelIndex].Mappings = new[] { mappingFileName };
            }

            return cloudPyramid;
        }
    }
}