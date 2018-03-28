using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Zavolokas.ImageProcessing.Inpainting;

namespace InpaintService
{
    public static class InpaintOrchestration
    {
        [FunctionName("Orchestrate")]
        public static async Task Orchestrate(
            [OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var inpaintRequest = ctx.GetInput<InpaintRequest>();

            // TODO: get the setting from parameters
            var settings = new InpaintSettings();

            // TODO: downscale the input image
            // TODO: crop the input image

            var pyramid = await ctx.CallActivityAsync<CloudPyramid>("GeneratePyramids", inpaintRequest);

            var maxInpaintIterationsAmount = 10;//settings.MaxInpaintIterations;
            //var kStep = settings.MeanShift.KDecreaseStep;
            //var minK = settings.MeanShift.MinK;

            for (byte levelIndex = 0; levelIndex < pyramid.LevelsAmount; levelIndex++)
            {
                var imageName = pyramid.GetImageName(levelIndex);
                var mappings = pyramid.GetMapping(levelIndex);
                var inpaintArea = pyramid.GetInpaintArea(levelIndex);

                // if there is a NNF built on the prev level
                // scale it up
                var input = NnfInputData.From($"nnf{levelIndex}.json", inpaintRequest.Container, imageName, settings,
                    mappings, inpaintArea, false, levelIndex, settings.MeanShift.K);

                if (levelIndex == 0)
                {
                    await ctx.CallActivityAsync<string>("CreateNnf", input);
                }
                else
                {
                    await ctx.CallActivityAsync<string>("ScaleNnf", input);
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

                        await ctx.CallActivityAsync("RandomNnfInitIteration", input);

                        for (var pmIteration = 0; pmIteration < settings.PatchMatch.IterationsAmount; pmIteration++)
                        {
                            // TODO: split to many parts and process in parallel
                            input.IsForward = !input.IsForward;
                            input.PatchMatchIteration = pmIteration;
                            await ctx.CallActivityAsync("RunBuildNnfIteration", input);
                        }

                        // TODO: merge nnf into one
                    }

                    var inpaintResult = await ctx.CallActivityAsync<InpaintingResult>("InpaintImage", input);
                    //input.K = input.K > minK ? input.K - kStep : input.K;

                    // if the change is smaller then a treshold, we quit
                    //if (inpaintResult.ChangedPixelsPercent < changedPixelsPercentTreshold) break;
                    //if (levelIndex == pyramid.LevelsAmount - 1) break;
                }
            }
        }
    }
}
