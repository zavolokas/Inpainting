using System;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class InpaintIterationFinishedEventArgs : EventArgs
    {
        public ZsImage InpaintedLabImage { get; set; }
        public InpaintingResult InpaintResult { get; set; }
        public byte LevelIndex { get; set; }
        public int InpaintIteration { get; set; }
    }
}