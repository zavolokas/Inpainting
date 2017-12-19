using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using Zavolokas;
using Zavolokas.GdiExtensions;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace ColorsCalculation
{
    class Program
    {
        static void Main(string[] args)
        {
            const string basePath = "..\\..\\..\\images";
            const string imageName = "t008.png";
            const string markupName = "m008.png";
            const int patchSize = 5;

            
            ZsImage image;
            Area2D removeArea, destArea;

            // Prepare 2 source images - one small and another 2x bigger
            using (var bitmap = new Bitmap(Path.Combine(basePath, imageName)))
            {
                image = bitmap
                    .ToRgbImage()
                    .FromRgbToLab();
            }

            using (var mrkpBitmap = new Bitmap(Path.Combine(basePath, markupName)))
            {
                removeArea = mrkpBitmap.ToArea();
                destArea = removeArea
                    .Dilation(patchSize)
                    .Intersect(Area2D.Create(0, 0, image.Width, image.Height));
            }
            
            // Here we remove information about pixels in the removeArea
            Area2D imagePixelsArea = Area2D.Create(0,0,image.Width, image.Height);
            imagePixelsArea = imagePixelsArea.Substract(removeArea);

            // Init an nnf
            var nnf = new Nnf(image.Width, image.Height, image.Width, image.Height, patchSize);
            var nnfdata = nnf.GetNnfItems();
            for (int i = 0; i < nnfdata.Length; i += 2)
            {
                nnfdata[i + 0] = i / 2;
                nnfdata[i + 1] = 0;
            }

            var points = new int[destArea.ElementsCount];
            destArea.FillMappedPointsIndexes(points, image.Width);

            for (int i = 0; i < points.Length; i++)
            {
                nnfdata[points[i] * 2 + 1] = double.MaxValue;
            }

            var settings = new PatchMatchSettings
            {
                PatchSize = patchSize,
            };

            var calculator = ImagePatchDistance.Cie76;

            // Create a mapping of the areas on the dest and source areas.
            var map = new InpaintMapBuilder()
                .InitNewMap(Area2D.Create(0, 0, image.Width, image.Height))
                .ReduceDestArea(destArea)
                .SetInpaintArea(removeArea)
                .Build();

            PatchMatchNnfBuilder.RunRandomNnfInitIteration(nnf, map, image, image, imagePixelsArea, calculator, settings);
            for (int i = 0; i < 5; i++)
            {
                PatchMatchNnfBuilder.RunBuildNnfIteration(nnf, map, image, image, imagePixelsArea, calculator, NeighboursCheckDirection.Forward, settings);
                PatchMatchNnfBuilder.RunBuildNnfIteration(nnf, map, image, image, imagePixelsArea, calculator, NeighboursCheckDirection.Backward, settings);
            }

            // Note: the normalization should be done not only for the 
            // remove area, but for the whole dest area since this information
            // will be used in the colors calculation.
            PatchMatchNnfBuilder.NormalizeNnf(nnf, destArea, settings);

            WexlerInpainter.Inpaint(image, removeArea, nnf, patchSize, ColorResolver.MeanShift, 3.0);

            nnf.ToRgbImage()
                .FromRgbToBitmap()
                .CloneWithScaleTo(448, 320, InterpolationMode.NearestNeighbor)
                .SaveTo("..\\..\\nnf.png", ImageFormat.Png);

            removeArea.CalculatePointsConfidence(1.5, 1.3)
                .ToBitmap(removeArea, image.Width, image.Height)
                .CloneWithScaleTo(448, 320, InterpolationMode.NearestNeighbor)
                .SaveTo(@"..\..\conf.png", ImageFormat.Png);

            nnf.ToRgbImage()
                .FromRgbToBitmap()
                .CloneWithScaleTo(448, 320, InterpolationMode.NearestNeighbor)
                .SaveTo(@"..\..\norm.png", ImageFormat.Png);

            image.FromLabToRgb()
                .FromRgbToBitmap()
                .CloneWithScaleTo(448, 320, InterpolationMode.NearestNeighbor)
                .SaveTo(@"..\..\inp.png", ImageFormat.Png)
                .ShowFile();
        }
    }
}
