using System;
using Nancy;
using Nancy.ErrorHandling;
using System.Linq;
using Zavolokas.GdiExtensions;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;
using System.Threading.Tasks;
using Zavolokas.ImageProcessing.PatchMatch;
using System.Drawing;
using System.IO;

//TODO: Cleanup the mess above & unused References

namespace InpaintHTTP
{
    public class MainMod : NancyModule
    {
        public MainMod()
        {
            Post("/api/inpaint", async x =>
            {
                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + "] Incomming request from " + this.Request.UserHostAddress);
                if (this.Request.Files.Count() < 2)
                {
                    Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + $"] Error, {this.Request.Files.Count()} files found");
                    return "Err";
                }
                  
                Bitmap BitmapImg;
                var imageFile = this.Request.Files.First();
                byte[] ByteImg = new byte[imageFile.Value.Length];
                imageFile.Value.Read(ByteImg, 0, (int)imageFile.Value.Length);
                using (MemoryStream ms = new MemoryStream(ByteImg))
                    BitmapImg = new Bitmap(ms);

                Bitmap BitmapMask;
                var maskFile = this.Request.Files.Last();
                byte[] ByteMask = new byte[maskFile.Value.Length];
                maskFile.Value.Read(ByteMask, 0, (int)maskFile.Value.Length);
                using (MemoryStream ms = new MemoryStream(ByteMask))
                    BitmapMask = new Bitmap(ms);

                var imageArgb = ConvertToArgbImage(BitmapImg);
                var markupArgb = ConvertToArgbImage(BitmapMask);

                var inpainter = new Inpainter();
                var settings = new InpaintSettings
                {
                    //MaxInpaintIterations = 15,
                    MaxInpaintIterations = 5, //less iterations for debugging
                    PatchDistanceCalculator = ImagePatchDistance.Cie76
                };

                Image finalResult = null;

                inpainter.IterationFinished += (sender, eventArgs) =>
                {
                    Bitmap iterationResult = eventArgs.InpaintedLabImage
                        .FromLabToRgb()
                        .FromRgbToBitmap();
                    finalResult = iterationResult;
                    Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + "] call on inpainter.IterationFinished"); //Debugging
                };

                await Task.Factory.StartNew(() => inpainter.Inpaint(imageArgb, markupArgb, settings));

                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + "] Processing finished");

                finalResult.Save($"TESTAPP_{DateTime.Now.ToString("HH:mm:ss.fffff")}.PNG"); //Debugging
                    
                Stream stream = new MemoryStream(finalResult.GetBytes());
                //return this.Response.FromStream(stream, "image/png");
                return Convert.ToBase64String(finalResult.GetBytes()); //this does the job ¯\_(ツ)_/¯
            });

            Get("/ping", _ => "Ping is successful");

        }

        private static ZsImage ConvertToArgbImage(Bitmap imageBitmap)
        {
            const double maxSize = 2048.0;

            if (imageBitmap.Width > maxSize || imageBitmap.Height > maxSize)
            {
                var tmp = imageBitmap;
                double percent = imageBitmap.Width > imageBitmap.Height
                    ? maxSize / imageBitmap.Width
                    : maxSize / imageBitmap.Height;
                imageBitmap =
                    imageBitmap.CloneWithScaleTo((int)(imageBitmap.Width * percent), (int)(imageBitmap.Height * percent));
                tmp.Dispose();
            }

            var imageArgb = imageBitmap.ToArgbImage();
            return imageArgb;
        }

    }

    public class MyStatusHandler : IStatusCodeHandler
    {
        //TODO: return json error message?
        public bool HandlesStatusCode(global::Nancy.HttpStatusCode statusCode, NancyContext context)
        {
            return true;
        }

        public void Handle(global::Nancy.HttpStatusCode statusCode, NancyContext context)
        {
            return;
        }
    }
}
