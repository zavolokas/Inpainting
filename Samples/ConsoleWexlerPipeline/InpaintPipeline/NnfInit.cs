using System;
using Grapute;
using Zavolokas.ImageProcessing.PatchMatch;

namespace ConsoleWexlerPipeline
{
    internal class NnfInit : Node<WexlerLevelsData, WexlerLevelsData>
    {
        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            Console.WriteLine($"NnfInit:\t{input.GetInfo()}");
            var pmBuilder = new PatchMatchNnfBuilder();
            pmBuilder.RunRandomNnfInitIteration(input.Nnf, input.CurrentPicture, input.CurrentPicture, input.PatchMatchSettings, input.Settings.PatchDistanceCalculator, input.CurrentMap, input.CurrentPixelsArea);

            return new[] {input};
        }
    }
}