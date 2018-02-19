using System;
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

            const string imageName = "t009.jpg";
            const string markupName = "m009.png";

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
            var markupArgb = OpenArgbImage(Path.Combine(imagesPath, markupName));
            var inpainter = new Inpainter();

            inpainter.IterationFinished += (sender, eventArgs) =>
            {
                var inpaintResult = eventArgs.InpaintResult;
                Console.WriteLine($"Changed pix%:{inpaintResult.ChangedPixelsPercent:F8}, ChangedPixels: {inpaintResult.PixelsChangedAmount}, PixDiff: {inpaintResult.ChangedPixelsDifference}");
                File.AppendAllLines($"../../out/{eventArgs.LevelIndex}.txt", new[] { $"{inpaintResult.ChangedPixelsPercent:F8}" });

                eventArgs.InpaintedLabImage
                    .FromLabToRgb()
                    .FromRgbToBitmap()
                    .CloneWithScaleTo(imageArgb.Width, imageArgb.Height, InterpolationMode.HighQualityBilinear)
                    .SaveTo($"..//..//out//r{eventArgs.LevelIndex}_{eventArgs.InpaintIteration}_CPP{inpaintResult.ChangedPixelsPercent:F8}_CPA{inpaintResult.PixelsChangedAmount}.png", ImageFormat.Png);
            };

            var result = inpainter.Inpaint(imageArgb, markupArgb);
            result
                .FromArgbToBitmap()
                .SaveTo($"..//..//out//result.png", ImageFormat.Png);

            watch.Stop();
            var elapsedMs = watch.ElapsedMilliseconds;

            Console.WriteLine($"Done in {elapsedMs}ms");
        }

        private static void SaveNnf(Nnf nnf, int width, byte levelIndex, int inpaintIteration)
        {
            var nnfdata = nnf
                .GetNnfItems()
                .Where((d, i) => i % 2 == 0)
                .Select((d, ind) =>
                {
                    var i = ind + 1;
                    var x = d % width;
                    var y = d / width;

                    var line = $"{x:00}:{y:00} ";
                    if (i % width == 0)
                        line += "\n";
                    return line;
                })
                .Aggregate("", (s1, s2) => s1 + s2);

            File.AppendAllText($"..//..//n{levelIndex}_{inpaintIteration}.txt", nnfdata);
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
