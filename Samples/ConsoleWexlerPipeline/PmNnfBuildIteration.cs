using System;
using Grapute;
using Zavolokas.ImageProcessing.PatchMatch;

namespace ConsoleWexlerPipeline
{
    internal class PmNnfBuildIteration : Node<WexlerLevelsData, WexlerLevelsData>
    {
        private readonly NeighboursCheckDirection _direction;

        public PmNnfBuildIteration(NeighboursCheckDirection direction)
        {
            _direction = direction;
        }

        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            Console.WriteLine($"NeighboursCheck:\t{input.GetInfo()}");
            PatchMatchNnfBuilder.RunBuildNnfIteration(input.Nnf, input.CurrentMap, input.CurrentPicture, input.CurrentPicture, input.CurrentPixelsArea, input.Settings.PatchDistanceCalculator, _direction, input.PatchMatchSettings);
            
            return new[] {input};
        }
    }
}