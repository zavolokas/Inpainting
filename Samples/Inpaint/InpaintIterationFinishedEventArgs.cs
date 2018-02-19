using System;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace Inpaint
{
    public class InpaintIterationFinishedEventArgs : EventArgs
    {
        public ZsImage InpaintedLabImage { get; set; }
        public InpaintingResult InpaintResult { get; set; }
        public byte LevelIndex { get; set; }
        public int InpaintIteration { get; set; }
    }
}