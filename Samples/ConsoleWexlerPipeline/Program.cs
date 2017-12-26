using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;
using Zavolokas.Utils.Processes;

namespace ConsoleWexlerPipeline
{
    class Program
    {
        static void Main(string[] args)
        {
            var sw = new Stopwatch();
            sw.Start();

            const string basePath = "..\\..\\..\\images";

            // black parts bug
            var imageName = "t009.jpg";
            var markupImageName = "m009.png";
            //var imageName = "t101.jpg";
            //var markupImageName = "m101.png";
            //var imageName = "t102.jpg";
            //var markupImageName = "m102.png";

            ////colors smashed bug
            //var imageName = "t003.jpg";
            //var markupImageName = "m003.png";

            ////first choice importance bug
            //var imageName = "t016.jpg";
            //var markupImageName = "m016.png";

            //// shift bug
            //// inner patches cogerence importance bug (white window doesn't propagate)
            //var imageName = "t023.jpg";
            //var markupImageName = "m023.png";

            //// shift bug
            //var imageName = "t058.jpg";
            //var markupImageName = "m058_2.png";

            ZsImage image, markupImage;
            using (var imageBitmap = new Bitmap(Path.Combine(basePath, imageName)))
            using (var removeMarkupBitmap = new Bitmap(Path.Combine(basePath, markupImageName)))
            {
                image = imageBitmap
                    .ToRgbImage();

                markupImage = removeMarkupBitmap
                    .ToArgbImage();
            }

            var inpaintedImage = Inpainter.Inpaint(image, markupImage);

            // Save and display the result
            inpaintedImage
                .FromLabToRgb()
                .FromRgbToBitmap()
                .SaveTo(@"..\..\inpainted.png", ImageFormat.Png)
                .ShowFile();

            sw.Stop();

            Console.WriteLine($"Elapsed time: {sw.Elapsed}");
            Console.WriteLine("WexlerPipeline processing is finished.");
        }
    }
}
