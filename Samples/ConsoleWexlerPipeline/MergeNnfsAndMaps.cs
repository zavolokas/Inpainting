using System;
using System.Linq;
using Grapute;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    internal class MergeNnfsAndMaps : Node<WexlerLevelsData[], WexlerLevelsData>
    {
        protected override WexlerLevelsData[] Process(WexlerLevelsData[] inputs)
        {
            var output = inputs[0];
            if (inputs.Length > 1)
            { 
                // Collect all the NNFs and Maps from the inputs.
                var nnfs = inputs.Select(i => i.Nnf).ToArray();
                var maps = inputs.Select(i => i.CurrentMap).ToArray();

                // Merge NNFs and set it to the output.
                var mergedNnf = PatchMatchNnfBuilder.MergeNnfs(nnfs, maps, nnfs[0].DstWidth, nnfs[0].SourceWidth, inputs[0].PatchMatchSettings).Item1;
                output.Nnf = mergedNnf;

                // Merge maps and set it to the output.
                var mergedMap = maps.Aggregate((m1, m2) => new Area2DMapBuilder()
                    .InitNewMap(m1)
                    .AddMapping(m2)
                    .Build());
                output.SubstituteMap(mergedMap);
            }

            Console.WriteLine($"MergeToOneNnf:\t\tinputs:{inputs.Length}");
            return new[] { output };
        }
    }
}