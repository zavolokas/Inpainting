using System.Threading.Tasks;
using InpaintService.Activities;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.Inpainting;

namespace InpaintService
{
    public static class InpaintLevelFunction
    {
        public const string Name = "InpaintLevel";

        [FunctionName(Name)]
        public static async Task InpaintLevel([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var input = ctx.GetInput<NnfInputData>();
            var levelIndex = input.LevelIndex;
            var settings = input.Settings;
            var mappings = input.Mappings;
            var maxInpaintIterationsAmount = settings.MaxInpaintIterations;
            var nnfs = input.SplittedNnfNames;
            var inpaintArea = input.InpaintAreaName;
            var imageName = input.Image;
            var container = input.Container;

            // if there is a NNF built on the prev level
            // scale it up

            if (levelIndex == 0)
            {
                await ctx.CallActivityAsync<string>(NnfCreateActivity.Name, input);
            }
            else
            {
                await ctx.CallActivityAsync<string>(NnfScaleActivity.Name, input);
            }

            // start inpaint iterations
            for (var inpaintIterationIndex = 0;
                inpaintIterationIndex < maxInpaintIterationsAmount;
                inpaintIterationIndex++)
            {
                // Obtain pixels area.
                // Pixels area defines which pixels are allowed to be used
                // for the patches distance calculation. We must avoid pixels
                // that we want to inpaint. That is why before the area is not
                // inpainted - we should exclude this area.
                input.ExcludeInpaintArea = levelIndex == 0 && inpaintIterationIndex == 0;
                input.IterationIndex = inpaintIterationIndex;

                // skip building NNF for the first iteration in the level
                // unless it is top level (for the top one we haven't built NNF yet)
                if (levelIndex == 0 || inpaintIterationIndex > 0)
                {
                    // in order to find best matches for the inpainted area,
                    // we build NNF for this imageLab as a dest and a source 
                    // but excluding the inpainted area from the source area
                    // (our mapping already takes care of it)

                    await ctx.CallActivityAsync(NnfRandomInitActivity.Name, input);

                    await ctx.CallSubOrchestratorAsync(BuildNnfFunction.Name, input);
                }

                var inpaintResult = await ctx.CallActivityAsync<InpaintingResult>(ImageInpaintActivity.Name, input);
                
                //input.K = input.K > minK ? input.K - kStep : input.K;

                // if the change is smaller then a treshold, we quit
                //if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                //if (levelIndex == pyramid.LevelsAmount - 1) break;
            }
        }
    }
}