using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;

namespace Inpaint
{
    class Program
    {
        static void Main(string[] args)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();

            const string imagesPath = @"../../../images";

            var donorNames = new string[0];

            //const string imageName = "t009.jpg";
            //const string markupName = "m009.png";

            const string imageName = "t061.jpg";
            const string markupName = "m061.png";
            donorNames = new[] {"d0611.png", "d0612.png"};

            //const string imageName = "t048.png";
            //const string markupName = "m048.png";

            //const string imageName = "t023.jpg";
            //const string markupName = "m023.png";

            //const string imageName = "t097.png";
            //const string markupName = "m097.png";

            //const string imageName = "t096.png";
            //const string markupName = "m099.png";

            //const string imageName = "t090.png";
            //const string markupName = "m090.png";

            //const string imageName = "t097.png";
            //const string markupName = "m097.png";

            //const string imageName = "t058.jpg";
            ////const string markupName = "m058_2.png";
            //const string markupName = "m058_1.png";

            //const string imageName = "t102.jpg";
            //const string markupName = "m102.png";

            //const string imageName = "t016.jpg";
            //const string markupName = "m016.png";

            //const string imageName = "t007.jpg";
            //const string markupName = "m007.png";

            //const string imageName = "t003.jpg";
            //const string markupName = "m003.png";

            //const string imageName = "t101.jpg";
            //const string markupName = "m101.png";

            //const string imageName = "t027.jpg";
            //const string markupName = "m027.png";

            // open an image and an image with a marked area to inpaint
            var imageArgb = OpenArgbImage(Path.Combine(imagesPath, imageName));
            var w = imageArgb.Width;
            var h = imageArgb.Height;
            var markupArgb = OpenArgbImage(Path.Combine(imagesPath, markupName));
            var donors = new List<ZsImage>();
            if (donorNames.Any())
            {
                donors.AddRange(donorNames.Select(donorName => OpenArgbImage(Path.Combine(imagesPath, donorName))));
            }
            var inpainter = new Inpainter();


            inpainter.IterationFinished += (sender, eventArgs) =>
            {
                var inpaintResult = eventArgs.InpaintResult;
                Console.WriteLine($"Changed pix%:{inpaintResult.ChangedPixelsPercent:F8}, ChangedPixels: {inpaintResult.PixelsChangedAmount}, PixDiff: {inpaintResult.ChangedPixelsDifference}");
                File.AppendAllLines($"../../out/{eventArgs.LevelIndex}.txt", new[] { $"{inpaintResult.ChangedPixelsPercent:F8}" });

                eventArgs.InpaintedLabImage
                    .FromLabToRgb()
                    .FromRgbToBitmap()
                    .CloneWithScaleTo(w, h, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//out//r{eventArgs.LevelIndex}_{eventArgs.InpaintIteration}_CPP{inpaintResult.ChangedPixelsPercent:F8}_CPA{inpaintResult.PixelsChangedAmount}.png", ImageFormat.Png);
            };

            //TODO: make sure the inputs are got cloned inside.
            var result = inpainter.Inpaint(imageArgb, markupArgb, donors);
            result
                .FromArgbToBitmap()
                .SaveTo($"..//..//out//result.png", ImageFormat.Png);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Done in {elapsedMs}ms");
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
