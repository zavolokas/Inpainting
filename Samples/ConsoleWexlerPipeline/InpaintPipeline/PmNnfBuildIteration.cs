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

            var pmBuilder = new PatchMatchNnfBuilder();

            pmBuilder.RunBuildNnfIteration(input.Nnf, input.CurrentPicture, input.CurrentPicture, _direction, input.PatchMatchSettings, input.Settings.PatchDistanceCalculator, input.CurrentMap, input.CurrentPixelsArea);
            
            return new[] {input};
        }
    }
}