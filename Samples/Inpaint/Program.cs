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
using Zavolokas.ImageProcessing.PatchMatch;
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
            const byte levelsAmount = 5;
            const byte patchSize = 11;

            var calculator = ImagePatchDistance.Cie2000;

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
            var nnfBuilder = new PatchMatchNnfBuilder();

            // Build pyramids
            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                // convert image to Lab color space and store it
                var imageCopy = imageArgb.Clone()
                    .FromArgbToRgb(new[] { 0.0, 0.0, 0.0 })
                    .FromRgbToLab();
                images.Push(imageCopy);

                var inpaintArea = markupArgb.FromArgbToArea2D();
                var imageArea = Area2D.Create(0, 0, imageArgb.Width, imageArgb.Height);
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
                var mapping = mapBuilder.InitNewMap(nnfTargetArea, nnfSourceArea)
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

                nnfSourceArea
                    .ToBitmap(Color.Green, imageArgb.Width, imageArgb.Height)
                    //.CloneWithScaleTo(originalWidth, originalHeight, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//m{levelIndex}s.png", ImageFormat.Png);
#endif
                #endregion

                if (levelIndex < levelsAmount - 1)
                {
                    // downscale for the next level
                    // NOTE: we shouldn't blur out the inpainted area so it is not getting bigger!!
                    imageArgb.PyramidDownArgb(nnfSourceArea);
                    markupArgb.PyramidDownArgb(false);
                }
            }

            // go thru all the pyramid levels starting from the top one
            Nnf nnf = null;
            var nnfSettings = new PatchMatchSettings { PatchSize = 5 };
            for (byte levelIndex = 0; levelIndex < levelsAmount; levelIndex++)
            {
                var image = images.Pop();
                var mapping = mappings.Pop();
                var inpaintArea = markups.Pop();

                var imageArea = Area2D.Create(0, 0, image.Width, image.Height);

                // TODO: if there is a NNF built on the prev level
                // scale it up
                if (nnf != null)
                {
                    
                }
                else
                {
                    nnf = new Nnf(image.Width, image.Height, image.Width, image.Height, nnfSettings.PatchSize);
                }

                // TODO: start inpaint iterations
                var inpaintIteration = 0;
                {
                    // TODO: Obtain pixels area.
                    // Pixels area defines which pixels are allowed to be used
                    // for the patches distance calculation. We must avoid pixels
                    // that we want to inpaint. That is why before the area is not
                    // inpainted - we should exclude this area.
                    var pixelsArea = imageArea;
                    if (levelIndex == 0 && nnf == null)
                    {
                        pixelsArea = imageArea.Substract(inpaintArea);
                    }

                    // skip building NNF for the first iteration in the level
                    // unless it is top level (for the top one we haven't built NNF yet)
                    if (levelIndex == 0 || inpaintIteration > 0)
                    {
                        // in order to find best matches for the inpainted area,
                        // we build NNF for this image as a dest and a source 
                        // but excluding the inpainted area from the source area
                        // (our mapping already takes care of it)
                        nnfBuilder.RunRandomNnfInitIteration(nnf, image, image, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Backward, nnfSettings, calculator, mapping, pixelsArea);
                        nnfBuilder.RunBuildNnfIteration(nnf, image, image, NeighboursCheckDirection.Forward, nnfSettings, calculator, mapping, pixelsArea);
                    }

                    nnf
                        .ToRgbImage()
                        .FromRgbToBitmap()
                        .SaveTo($"..//..//n{levelIndex}.png", ImageFormat.Png);


                    // TODO: after we have the NNF - we calculate the values of the
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
