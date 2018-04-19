using System.Threading.Tasks;
using InpaintService.Activities;
using Microsoft.Azure.WebJobs;

namespace InpaintService
{
    public static class BuildNnfFunction
    {
        public const string Name = "BuildNnf";

        [FunctionName(Name)]
        public static async Task BuildNnf([OrchestrationTrigger] DurableOrchestrationContext ctx)
        {
            var input = ctx.GetInput<NnfInputData>();
            var levelIndex = input.LevelIndex;
            var settings = input.Settings;
            var mappings = input.Mappings;
            var nnfs = input.SplittedNnfNames;
            var inpaintArea = input.InpaintAreaName;
            var imageName = input.Image;
            var container = input.Container;

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
                        var pminput = NnfInputData.From(nnfs[mapIndex], container, imageName,
                            settings, mappings[mapIndex], inpaintArea, isForward, levelIndex,
                            settings.MeanShift.K, nnfs, mappings);
                        pminput.PatchMatchIteration = pmIteration;

                        tasks[mapIndex] = ctx.CallActivityAsync(NnfBuildActivity.Name, pminput);
                    }

                    await Task.WhenAll(tasks);

                    // TODO: merge nnf into one
                    await ctx.CallActivityAsync(NnfMergeActivity.Name, (nnfs: nnfs, mappings: mappings, resultNnf: input.NnfName, container: input.Container, input.Mapping));
                }
                else
                {
                    var pminput = NnfInputData.From(input.NnfName, container, imageName,
                        settings, input.Mapping, inpaintArea, isForward, levelIndex,
                        settings.MeanShift.K, nnfs, mappings);
                    pminput.PatchMatchIteration = pmIteration;

                    await ctx.CallActivityAsync(NnfBuildActivity.Name, pminput);
                }

                isForward = !isForward;
            }
        }
    }
}