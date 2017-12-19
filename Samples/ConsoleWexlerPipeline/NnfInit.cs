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

            PatchMatchNnfBuilder.RunRandomNnfInitIteration(input.Nnf, input.CurrentMap, input.CurrentPicture, input.CurrentPicture, input.CurrentPixelsArea, input.Settings.PatchDistanceCalculator, input.PatchMatchSettings);

            return new[] {input};
        }
    }
}