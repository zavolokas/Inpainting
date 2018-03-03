using System;
using System.Collections.Generic;
using System.Linq;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class Pyramid
    {
        private readonly ZsImage[] _images;
        private readonly Area2D[] _inpaintAreas;
        private readonly Area2DMap[] _mappings;

        internal Pyramid(IList<ZsImage> images, IList<Area2D> inpaintAreas, IList<Area2DMap> mappings)
        {
            if (images == null)
                throw new ArgumentNullException(nameof(images));

            if (inpaintAreas == null)
                throw new ArgumentNullException(nameof(inpaintAreas));

            if (mappings == null)
                throw new ArgumentNullException(nameof(mappings));

            // make sure images have correct size
            var w = images.First().Width;
            var h = images.First().Height;
            for (int levelIndex = 0; levelIndex < images.Count; levelIndex++)
            {
                var image = images[levelIndex];
                if (image.Width != w || image.Height != h)
                    throw new WrongImageSizeException();

                if (inpaintAreas[levelIndex].IsEmpty)
                    throw new WrongInpaintAreaSizeException();

                var imageArea = Area2D.Create(0, 0, w, h);
                var area = inpaintAreas[levelIndex].Substract(imageArea);
                if (!area.IsEmpty)
                    throw new WrongInpaintAreaSizeException();

                var mapping = mappings[levelIndex];
                var destArea = Area2D.Create(mapping.DestBounds.X, mapping.DestBounds.Y, 
                    mapping.DestBounds.Width,mapping.DestBounds.Height);

                if (!destArea.Substract(imageArea).IsEmpty)
                    throw new WrongMappingSizeException();

                w /= 2;
                h /= 2;
            }

            _images = images.Reverse().ToArray();
            _inpaintAreas = inpaintAreas.Reverse().ToArray();
            _mappings = mappings.Reverse().ToArray();
        }

        public ZsImage GetImage(byte levelIndex)
        {
            if (levelIndex >= _images.Length)
                throw new ArgumentOutOfRangeException();

            return _images[levelIndex];
        }

        public Area2DMap GetMapping(byte levelIndex)
        {
            if (levelIndex >= _mappings.Length)
                throw new ArgumentOutOfRangeException();

            return _mappings[levelIndex];
        }
        public Area2D GetInpaintArea(byte levelIndex)
        {
            if (levelIndex >= _inpaintAreas.Length)
                throw new ArgumentOutOfRangeException();

            return _inpaintAreas[levelIndex];
        }

        public byte LevelsAmount => (byte)_images.Length;
    }
}