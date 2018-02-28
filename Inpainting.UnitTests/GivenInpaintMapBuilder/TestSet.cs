using System;
using System.Collections.Generic;
using System.Drawing;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    internal class TestSet
    {
        public string Path;
        public Bitmap Picture;
        public Bitmap RemoveMarkup;
        public List<Bitmap> Donors;

        private TestSet(){}

        public static TestSet Init(string size)
        {
            TestSet ts = new TestSet();
            ts.Path = System.IO.Path.GetFullPath(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, $@"..\..\images\{size}"));
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