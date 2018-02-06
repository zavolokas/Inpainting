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
            input.Nnf = input.Nnf.CloneAndScale2XWithUpdate(input.CurrentPicture, input.CurrentPicture, input.PatchMatchSettings, input.CurrentMap, input.Settings.PatchDistanceCalculator, input.CurrentPixelsArea);

            Console.WriteLine($"ScaleUpNnf:\t{input.GetInfo()}");
            return new[] {input};
        }
    }
}