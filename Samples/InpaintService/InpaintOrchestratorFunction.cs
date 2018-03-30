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

            var maxInpaintIterationsAmount = 2;//settings.MaxInpaintIterations;
            settings.PatchMatch.IterationsAmount = 2;
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
                var input = NnfInputData.From(nnf, inpaintRequest.Container, imageName, settings, mapping, inpaintArea, false, levelIndex, settings.MeanShift.K, nnfs);

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

                        var tasks = new Task[mappings.Length];

                        var isForward = true;

                        for (var pmIteration = 0; pmIteration < settings.PatchMatch.IterationsAmount; pmIteration++)
                        {
                            // process in parallel
                            if (mappings.Length > 1)
                            {
                                for (int mapIndex = 0; mapIndex < mappings.Length; mapIndex++)
                                {
                                    // TODO: this looks ugly
                                    var pminput = NnfInputData.From(nnfs[mapIndex], inpaintRequest.Container, imageName,
                                        settings, mappings[mapIndex], inpaintArea, isForward, levelIndex,
                                        settings.MeanShift.K, nnfs);
                                    pminput.PatchMatchIteration = pmIteration;

                                    tasks[mapIndex] = ctx.CallActivityAsync(NnfBuildActivity.Name, pminput);
                                }

                                await Task.WhenAll(tasks);

                                // TODO: merge nnf into one
                                await ctx.CallActivityAsync(NnfMergeActivity.Name, (nnfs: nnfs, mappings: mappings, resultNnf: input.NnfName, container: input.Container, input.Mapping));
                            }
                            else
                            {
                                var pminput = NnfInputData.From(input.NnfName, inpaintRequest.Container, imageName,
                                    settings, input.Mapping, inpaintArea, isForward, levelIndex,
                                    settings.MeanShift.K, nnfs);
                                pminput.PatchMatchIteration = pmIteration;

                                await ctx.CallActivityAsync(NnfBuildActivity.Name, pminput);
                            }

                            isForward = !isForward;
                        }
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
}
