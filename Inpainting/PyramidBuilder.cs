using System;
using System.Collections.Generic;
using System.Linq;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public class PyramidBuilder
    {
        private ZsImage _imageArgb;
        private ZsImage _inpaintMarkup;
        private readonly double[] _backgroundColor;

        public PyramidBuilder()
        {
            _backgroundColor = new[] { 0.0, 0.0, 0.0, 0.0 };
        }

        public void Init(ZsImage imageArgb, ZsImage inpaintMarkupArgb)
        {
            if (imageArgb == null)
                throw new ArgumentNullException(nameof(imageArgb));

            if (inpaintMarkupArgb == null)
                throw new ArgumentNullException(nameof(inpaintMarkupArgb));

            if (imageArgb.NumberOfComponents != 4)
                throw new WrongImageFormatException();

            if (inpaintMarkupArgb.NumberOfComponents != 4)
                throw new WrongImageFormatException();

            _imageArgb = imageArgb;
            _inpaintMarkup = inpaintMarkupArgb;
        }

        public void AddDonorMarkup(ZsImage donor)
        {
        }

        /// <summary>
        /// Build pyramids by downscaling the image and the markup.
        /// We also apply a smoothing filter to the scaled images 
        /// to reduce high spatial ferquency introduced by scaling
        /// (the filter is not applied to the inoainted area to avoid
        /// inpainted object propagation out of its boundaries)
        /// </summary>
        /// <param name="levelsAmount">The levels amount.</param>
        /// <returns></returns>
        public Pyramid Build(byte levelsAmount, byte patchSize = 11)
        {
            if (levelsAmount < 1)
                throw new ArgumentOutOfRangeException(nameof(levelsAmount));

            if (_imageArgb == null)
                throw new InitializationException();

            int w = _imageArgb.Width;
            int h = _imageArgb.Height;

            if (w == 1 || h == 1)
                throw new WrongImageSizeException();

            for (int levelInadex = 1; levelInadex < levelsAmount; levelInadex++)
            {
                if (w % 2 > 0 || h % 2 > 0)
                    throw new WrongImageSizeException();

                w /= 2;
                h /= 2;

                if (w == 1 || h == 1)
                    throw new WrongImageSizeException();
            }

            var imageArea = Area2D.Create(0, 0, _imageArgb.Width, _imageArgb.Height);

            if (_inpaintMarkup.Width != _imageArgb.Width || _inpaintMarkup.Height != _imageArgb.Height)
            {
                // Adjus inpaint markup image so that it become the same size as
                // the image is
                var markupArea = Area2D.Create(0, 0, _inpaintMarkup.Width, _inpaintMarkup.Height);
                var srcArea = markupArea.Intersect(imageArea);
                var pixels = Enumerable.Repeat(0.0, _imageArgb.Width * _imageArgb.Height * 4).ToArray();
                var newInpaintMarkup = new ZsImage(pixels, _imageArgb.Width, _imageArgb.Height, 4);
                _inpaintMarkup = newInpaintMarkup.CopyFromImage(imageArea, _inpaintMarkup, srcArea);
            }

            var inpaintArea = _inpaintMarkup.FromArgbToArea2D();

            if (inpaintArea.IsEmpty)
                throw new NoAreaToInpaintException();

            if (imageArea.Substract(inpaintArea).IsEmpty)
                throw new AreaRemovedException();

            // Build pyramids

            var images = new List<ZsImage>(levelsAmount);
            var mappings = new List<Area2DMap>(levelsAmount);
            var markups = new List<Area2D>(levelsAmount);

            //var mapBuilder = new InpaintMapBuilder(new Area2DMapBuilder());
            var mapBuilder = new Area2DMapBuilder();

            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                // convert image to Lab color space and store it
                var imageCopy = _imageArgb.Clone()
                    .FromArgbToRgb(_backgroundColor)
                    .FromRgbToLab();
                images.Add(imageCopy);

                inpaintArea = _inpaintMarkup.FromArgbToArea2D();
                imageArea = Area2D.Create(0, 0, _imageArgb.Width, _imageArgb.Height);
                var nnfSourceArea = imageArea.Substract(inpaintArea);
                Area2D nnfTargetArea;

                // Obtain target area for the NNF building based on the level
                if (levelIndex == levelsAmount - 1)
                {
                    // For the top level, the target area should be the whole image area
                    nnfTargetArea = imageArea;
                }
                else
                {
                    // For the rest levels, the target area should be
                    // slightly bigger then the inpaint area.
                    nnfTargetArea = inpaintArea
                        .Dilation(patchSize)
                        .Intersect(imageArea);
                }

                // Create a mapping for the level.
                //var mapping = mapBuilder.InitNewMap(nnfTargetArea)
                //    .SetInpaintArea(inpaintArea)
                //    .Build();
                var mapping = mapBuilder.InitNewMap(nnfTargetArea, nnfSourceArea)
                    .Build();


                mappings.Add(mapping);
                markups.Add(inpaintArea);

                if (levelIndex < levelsAmount - 1)
                {
                    // downscale for the next level
                    // NOTE: we shouldn't blur out the inpainted area so it is not getting bigger!!
                    _imageArgb.PyramidDownArgb(nnfSourceArea);
                    _inpaintMarkup.PyramidDownArgb(false);
                }
            }

            return new Pyramid(images, markups, mappings);
        }
    }
}