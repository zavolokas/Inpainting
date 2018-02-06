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
                for (int i = 1; i < nnfs.Length; i++)
                {
                    output.Nnf.Merge(nnfs[i], output.CurrentMap, maps[i], inputs[0].PatchMatchSettings);

                    var mergedMap = new Area2DMapBuilder()
                        .InitNewMap(output.CurrentMap)
                        .AddMapping(maps[i])
                        .Build();

                    output.SubstituteMap(mergedMap);
                }
            }

            Console.WriteLine($"MergeToOneNnf:\t\tinputs:{inputs.Length}");
            return new[] { output };
        }
    }
}