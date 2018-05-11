using System;
using System.Threading.Tasks;
using InpaintService.Activities;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.Inpainting;

namespace InpaintService
{
    public static class InpaintOrchestratorFunction
    {
        public const string Name = "Orchestrate";

        [FunctionName(Name)]
        public static async Task Orchestrate(
            [OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var inpaintRequest = ctx.GetInput<InpaintRequest>();

            // TODO: get the setting from parameters
            var settings = new InpaintSettings();

            // TODO: downscale the input image
            // TODO: crop the input image

            var pyramid = await ctx.CallActivityAsync<CloudPyramid>(PyramidsGenerateActivity.Name, inpaintRequest);

            settings.MaxInpaintIterations = 10;
            settings.PatchMatch.IterationsAmount = 3;
            //var kStep = settings.MeanShift.KDecreaseStep;
            //var minK = settings.MeanShift.MinK;

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var imageName = pyramid.GetImageName(levelIndex);
                var mapping = pyramid.GetMapping(levelIndex);
                var nnf = pyramid.GetNnf(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);
                var mappings = pyramid.GetSplittedMappings(levelIndex);
                var nnfs = pyramid.GetSplittedNnfs(levelIndex);

                // TODO: this looks ugly
                var input = NnfInputData.From(nnf, inpaintRequest.Container, imageName, settings, mapping, inpaintArea, false, levelIndex, settings.MeanShift.K, nnfs, mappings, pyramid.LevelsAmount);
                await ctx.CallSubOrchestratorAsync(InpaintLevelFunction.Name, input);
            }
        }
    }
}
