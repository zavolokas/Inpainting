using System;
using Grapute;
using Zavolokas.ImageProcessing.PatchMatch;

namespace ConsoleWexlerPipeline
{
    internal class ScaleUpNnf : Node<WexlerLevelsData, WexlerLevelsData>
    {
        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            input.Pictures.Dequeue();
            input.RemoveAreas.Dequeue();
            input.ConfidenceMaps.Dequeue();
            input.PatchMatchSettingsQueue.Dequeue();
            input.PixelsAreas.Dequeue();

            input.Nnf = PatchMatchNnfBuilder.ScaleNnf2X(input.Nnf, input.CurrentMap, input.CurrentPicture, input.CurrentPicture, input.CurrentPixelsArea, input.Settings.PatchDistanceCalculator, input.PatchMatchSettings);

            Console.WriteLine($"ScaleUpNnf:\t{input.GetInfo()}");
            return new[] {input};
        }
    }
}