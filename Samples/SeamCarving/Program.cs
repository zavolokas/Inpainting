using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using Zavolokas.Structures;

namespace SeamCarving
{
    class Program
    {
        private const byte MinImageSideSize = 50;
        private const double AllowedMarkupCoverage = 0.30;

        static void Main(string[] args)
        {
            var sca = new SeamCarvingAlgorithm();


            using (var imageBitmap = new Bitmap(@"..\..\images\t009.jpg"))
            using (var protectBitmap = new Bitmap(@"..\..\images\p009.png"))
            using (var removeBitmap = new Bitmap(@"..\..\images\m009.png"))
            //using (var imageBitmap = new Bitmap(@"..\..\images\t019.jpg"))
            //using (var protectBitmap = new Bitmap(@"..\..\images\p019.png"))
            //using (var removeBitmap = new Bitmap(@"..\..\images\m019.png"))
            {
                var settings = new SeamCarvingSettings { ProvideNotRestored = true, ProvideRestored = true, ProvideRestoredWithMask = true };

                var img = imageBitmap.ToRgbImage();
                var removeArea = removeBitmap.ToArea();
                var protectArea = protectBitmap.ToArea();
                CheckParams(img, removeArea, protectArea, settings);

                var image = new GdiImage(imageBitmap);
                var protect = new GdiImage(protectBitmap);
                var remove = new GdiImage(removeBitmap);


                var results = sca.InpaintImage(image, remove, protect, settings);
                for (var i = 0; i < results.Length; i++)
                {
                    var result = results[i];
                    var gdiResult = new GdiImage(result);
                    var filename = $"..\\..\\{i}.bmp";
                    gdiResult.Bitmap.Save(filename, ImageFormat.Bmp);
                    if (i == results.Length - 1)
                        Process.Start(filename);
                }
            }
        }

        private static void CheckParams(ZsImage image, Area2D removeArea, Area2D protectArea, SeamCarvingSettings settings)
        {
            //if (!settings.ProvideNotRestored && !settings.ProvideRestored && (protectArea == null || !settings.ProvideRestoredWithMask))
            //{
            //    //return new Image[0];
            //    //todo: throw smth
            //}

            //if (image == null)
            //{
            //    throw new ImageNullException();
            //}

            //if (removeMarkupImage == null)
            //{
            //    throw new MarkupNullException();
            //}

            //if (image.Width < MinImageSideSize && image.Height < MinImageSideSize)
            //{
            //    throw new ImageBadSizeException(image.Width, image.Height);
            //}

            //if (image.Width != removeMarkupImage.Width || image.Height != removeMarkupImage.Height)
            //{
            //    throw new MarkupSizeNotMatchException();
            //}

            //if (protectMarkupImage != null && (image.Width != protectMarkupImage.Width || image.Height != protectMarkupImage.Height))
            //{
            //    throw new MarkupSizeNotMatchException();
            //}

            ////make sure that the markup is not empty
            //if (markup.IsEmpty)
            //{
            //    throw new BadMarkupException();
            //}

            //////make sure that the markup area doesn't cover all the image
            ////if (markup.PixelPositions.Length / (double)(imageCopy.Width * imageCopy.Height) > AllowedMarkupCoverage)
            ////{
            ////    throw new BadMarkupException();
            ////}
        }

    }
}
