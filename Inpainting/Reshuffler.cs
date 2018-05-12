using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class Reshuffler
    {
        public event EventHandler<InpaintIterationFinishedEventArgs> IterationFinished;

        private readonly Inpainter _inpainter;

        public Reshuffler()
        {
            _inpainter = new Inpainter();    
            
        }

        public ZsImage Reshuffle(ZsImage imageArgb, ZsImage markupArgb)
        {
            var settings = new InpaintSettings();
            settings.IgnoreInpaintedPixelsOnFirstIteration = false;
            settings.MaxInpaintIterations = 7;

            _inpainter.IterationFinished += (s, e) => IterationFinished?.Invoke(s, e);

            return _inpainter.Inpaint(imageArgb, markupArgb, settings);
        }
    }
}
