using System.Collections.Generic;
using System.Drawing;

namespace ConsoleInpaintAreaDonors
{
    public class TestSet
    {
        public string Path;
        public Bitmap Picture;
        public Bitmap RemoveMarkup;
        public List<Bitmap> Donors;

        private TestSet(){}

        public static TestSet Init256x128()
        {
            TestSet ts = new TestSet();

            ts.Path = @"..\..\images\256x128";
            ts.Picture = new Bitmap($"{ts.Path}\\picture.png");
            ts.RemoveMarkup = new Bitmap($"{ts.Path}\\inpaintarea.bmp");
            var donors = new List<Bitmap>();
            donors.Add(new Bitmap($"{ts.Path}\\donor0.bmp"));
            donors.Add(new Bitmap($"{ts.Path}\\donor1.bmp"));
            donors.Add(new Bitmap($"{ts.Path}\\donor2.bmp"));
            donors.Add(new Bitmap($"{ts.Path}\\donor3.bmp"));
            donors.Add(new Bitmap($"{ts.Path}\\donor4.bmp"));
            ts.Donors = donors;
            return ts;
        }

        public static TestSet Init1280x720()
        {
            TestSet ts = new TestSet();
            ts.Path = @"..\..\images\1280x720";
            ts.Picture = new Bitmap($"{ts.Path}\\picture.jpg");
            ts.RemoveMarkup = new Bitmap($"{ts.Path}\\inapaintarea.png");
            var donors = new List<Bitmap>();
            donors.Add(new Bitmap($"{ts.Path}\\donor00.png"));
            donors.Add(new Bitmap($"{ts.Path}\\donor01.png"));
            donors.Add(new Bitmap($"{ts.Path}\\donor02.png"));
            donors.Add(new Bitmap($"{ts.Path}\\donor03.png"));
            donors.Add(new Bitmap($"{ts.Path}\\donor04.png"));
            ts.Donors = donors;
            return ts;
        }
    }
}