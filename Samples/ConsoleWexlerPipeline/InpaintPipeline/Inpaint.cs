using System;
using Grapute;
using Zavolokas.ImageProcessing.Inpainting;

namespace ConsoleWexlerPipeline
{
    internal class Inpaint : Node<WexlerLevelsData, WexlerLevelsData>
    {
        static int _iteration = 0;

        protected override WexlerLevelsData[] Process(WexlerLevelsData input)
        {
            _iteration++;

            WexlerInpainter.Inpaint(input.CurrentPicture, input.CurrentRemoveArea, input.NormalizedNnf, input.PatchMatchSettings.PatchSize, input.Settings.ColorResolveMethod, input.K);

            //input.SaveAll(_iteration);
            input.SaveResult(_iteration, currentPicture:true, removeArea:false, destArea:false);//, restored:true);//, nnf:true, normalizedNnf:true, restored:true, removeArea:true, destArea: true);//, removeArea: true, confidenceMap:true);

            input.Maps.Dequeue();
            input.KQueue.Dequeue();

            Console.WriteLine($"Inpaint:\t{input.GetInfo()}");
            return new[] { input };
        }
    }
}