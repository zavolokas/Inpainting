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
using System.Threading;
//using Zavolokas.Utils.Processes;

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
                Bitmap BitmapMask;

                byte[] ByteImg = new byte[this.Request.Files.First().Value.Length];
                this.Request.Files.First().Value.Read(ByteImg, 0, (int)this.Request.Files.First().Value.Length);
                using (MemoryStream ms = new MemoryStream(ByteImg))
                {
                    BitmapImg = new Bitmap(ms);
                }
                byte[] ByteMask = new byte[this.Request.Files.Last().Value.Length];
                this.Request.Files.Last().Value.Read(ByteMask, 0, (int)this.Request.Files.Last().Value.Length);
                using (MemoryStream ms = new MemoryStream(ByteMask))
                {
                    BitmapMask = new Bitmap(ms);
                }

                var imageArgb = ConvertToArgbImage((Bitmap)BitmapImg);
                var markupArgb = ConvertToArgbImage((Bitmap)BitmapMask);

                var inpainter = new Inpainter();
                var settings = new InpaintSettings
                {
                    //MaxInpaintIterations = 15,
                    MaxInpaintIterations = 3, //less iterations for debugging
                    PatchDistanceCalculator = ImagePatchDistance.Cie76
                };

                Image LastImageResult = null;

                inpainter.IterationFinished += (sender, eventArgs) =>
                {
                    var ImageResult = eventArgs.InpaintedLabImage
                        .FromLabToRgb()
                        .FromRgbToBitmap();
                    LastImageResult = ImageResult;
                    Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + "] call on inpainter.IterationFinished"); //Debugging
                };

                await Task.Factory.StartNew(() => inpainter.Inpaint(imageArgb, markupArgb, settings));

                Console.WriteLine("[" + DateTime.Now.ToString("HH:mm:ss.fffff") + "] Processing finished");

                LastImageResult.Save(@"..\..\TESTAPP.PNG"); //Debugging
                    
                Stream stream = new MemoryStream(LastImageResult.GetBytes());
                return this.Response.FromStream(stream, "image/jpg");
            });

            Get("/", _ => View["TestWebsite/index"]);

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
