using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;

namespace Inpaint
{
    class Program
    {
        static void Main(string[] args)
        {
            const string imagesPath = @"../../../images";
            const string imageName = "t009.jpg";
            const string markupName = "m009.png";

            // TODO: should be calculated based on image and markup size
            const byte levelsAmount = 6;
            const byte patchSize = 11;

            // open an image and an image with a marked area to inpaint
            var imageArgb = OpenArgbImage(Path.Combine(imagesPath, imageName));
            var markupArgb = OpenArgbImage(Path.Combine(imagesPath, markupName));

            // TODO: extract a part of the image that can be scaled down required
            // amount of times (levels)

            var originalWidth = imageArgb.Width;
            var originalHeight = imageArgb.Height;

            // Build pyramids by downscaling the image and the markup.
            // We also apply a smoothing filter to the scaled images 
            // to reduce high spatial ferquency introduced by scaling
            // (the filter is not applied to the inoainted area to avoid
            // inpainted object propagation out of its boundaries)
            var images = new Stack<ZsImage>();
            var mappings = new Stack<Area2DMap>();
            var markups = new Stack<Area2D>();

            var mapBuilder = new Area2DMapBuilder();

            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                // convert image to Lab color space and store it
                var imageCopy = imageArgb.Clone()
                    .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                    .FromRgbToLab();
                images.Push(imageCopy);

                // TODO: create a mapping for the level
                // TODO: for the top level, the target area should be
                // TODO: for the rest levels, the target area should be
                // slightly bigger then the inpaint area.
                var inpaintArea = markupArgb.FromArgbToArea2D();
                var imageArea = Area2D.Create(0, 0, imageArgb.Width, imageArgb.Height);
                var nnfTargetArea = inpaintArea
                    .Dilation(patchSize)
                    .Intersect(imageArea);
                var sourceArea = imageArea.Substract(inpaintArea);
                var mapping = mapBuilder.InitNewMap(nnfTargetArea, sourceArea)
                    .Build();

                mappings.Push(mapping);
                markups.Push(inpaintArea);

                #region Save data for debugging
#if DEBUG
                imageCopy
                    .FromLabToRgb()
                    .FromRgbToBitmap()
                    //.CloneWithScaleTo(originalWidth, originalHeight, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//t{levelIndex}.png", ImageFormat.Png);

                nnfTargetArea
                    .ToBitmap(Color.Red, imageArgb.Width, imageArgb.Height)
                    //.CloneWithScaleTo(originalWidth, originalHeight, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//m{levelIndex}t.png", ImageFormat.Png);

                sourceArea
                    .ToBitmap(Color.Green, imageArgb.Width, imageArgb.Height)
                    //.CloneWithScaleTo(originalWidth, originalHeight, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//m{levelIndex}s.png", ImageFormat.Png);
#endif
                #endregion

                if (levelIndex < levelsAmount - 1)
                {
                    // downscale for the next level
                    // NOTE: we shouldn't blur out the inpainted area so it is not getting bigger!!
                    imageArgb.PyramidDownArgb(sourceArea);
                    markupArgb.PyramidDownArgb(false);
                }
            }


            // TODO: go thru all the pyramid levels starting from the top one
            {
                // TODO: build a mapping that will define a source 
                // area (to get patches from) and related target area for which we need 
                // to calculate its pixels values.

                // TODO: if there is a NNF built on the prev level
                // scale it up 

                // TODO: start inpaint iterations
                {
                    // TODO: skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we don't have built 
                    // NNF yet)

                    // TODO: in order to find best mathes for the inpainted area,
                    // we build NNF for this image as a dest and a source 
                    // but excluding the inpainted area from the source area
                    // (our mapping already takes care of it)

                    // TODO: after NNF is built we calculate the values of the
                    // pixels in the inpainted area

                    // TODO: we also calculate the percent of pixels change during the iteration

                    // TODO: if the change is smaller then a treshold, we quit
                }
            }

            // TODO: paste result in the original bitmap where it was extracted from
            // TODO: convert image to a bitmap and save it
        }

        private static ZsImage OpenArgbImage(string path)
        {
            ZsImage image;
            using (var imageBitmap = new Bitmap(path))
            {
                image = imageBitmap.ToArgbImage();
            }
            return image;
        }
    }
}
