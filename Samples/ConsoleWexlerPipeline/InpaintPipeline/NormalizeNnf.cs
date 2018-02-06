using System;
using Grapute;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    internal class NormalizeNnf : Node<WexlerLevelsData, WexlerLevelsData>
    {
        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            var nnf = input.Nnf.Clone();
            // Note: the normalization should be done not only for the 
            // remove area, but for the whole dest area since this information
            // will be used in the colors calculation.
            var destArea = (input.CurrentMap as IAreasMapping).DestArea;

            nnf.Normalize(destArea);
            input.NormalizedNnf = nnf;

            Console.WriteLine($"NormalizeNnf:\t{input.GetInfo()}");
            return new[] { input };
        }
    }
}