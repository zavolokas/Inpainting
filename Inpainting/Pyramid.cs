using System;
using System.Collections.Generic;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class Pyramid
    {
        internal Pyramid(IList<ZsImage> images, IList<Area2D> inpaintAreas, IList<Area2DMap> mappings)
        {
        }

        public ZsImage GetImage(byte level)
        {
            throw new NotImplementedException();
        }

        public Area2DMap GetMapping(byte level)
        {
            throw new NotImplementedException();
        }
        public Area2D GetInpaintArea(byte level)
        {
            throw new NotImplementedException();
        }

        public byte LevelsAmount { get; }
    }
}