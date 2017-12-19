using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.Structures;

namespace ConsoleInpaintAreaDonors
{
    public static class MapBuilderTests
    {
        public static void RunAll()
        {
            MapBuilderTests.TestMapBuilder(TestSet.Init256x128());
            MapBuilderTests.TestMapBuilder(TestSet.Init1280x720());
        }

        public static void TestMapBuilder(TestSet ts)
        {
            Console.WriteLine($"{nameof(TestMapBuilder)} is running ...");
            var sw = new Stopwatch();

            // create map 
            var mapBuilder = new InpaintMapBuilder();

            var imageArea = Area2D.Create(0, 0, ts.Picture.Width, ts.Picture.Height);
            mapBuilder.InitNewMap(imageArea);

            //Console.WriteLine("Convert remove markup to area2d");
            sw.Start();
            var inpaintArea = ts.RemoveMarkup.ToArea();
            sw.Stop();
            //Console.WriteLine($"Finished in {sw.Elapsed}");
            mapBuilder.SetInpaintArea(inpaintArea);

            for (int i = 0; i < ts.Donors.Count; i++)
            {
                //Console.WriteLine($"Convert donor markup {i} to area2d");
                sw.Restart();
                var donorArea = ts.Donors[i].ToArea();
                sw.Stop();
                //Console.WriteLine($"Finished in {sw.Elapsed}");
                mapBuilder.AddDonor(donorArea);
            }

            //Console.WriteLine("\n\nStart map building...");
            sw.Restart();

            var mapping = mapBuilder.Build();

            sw.Stop();
            Console.WriteLine($"Map is built is: {sw.Elapsed}");

            // convert mapping to areas
            //Console.WriteLine("\n\nStart areas extraction from the mapping");
            sw.Restart();
            var areaPairs = (mapping as IAreasMapping).AssociatedAreasAsc;
            sw.Stop();
            //Console.WriteLine($"Elapsed time: {sw.Elapsed}");

            //Console.WriteLine("\n\nStart saving output");
            sw.Restart();

            var testName = nameof(TestMapBuilder);
            for (int i = 0; i < areaPairs.Length; i++)
            {
                var areaPair = areaPairs[i];

                SaveToOutput(areaPair.Item1, $"dest{i}", testName, ts.Path, Color.Red);
                SaveToOutput(areaPair.Item2, $"src{i}", testName, ts.Path, Color.Green);
            }
            sw.Stop();
            //Console.WriteLine($"Elapsed time: {sw.Elapsed}");

            // compare output and references
            string[] files = Directory.GetFiles($"{ts.Path}\\{nameof(TestMapBuilder)}\\refs", "*.*", SearchOption.TopDirectoryOnly);
            bool testSuccess = true;
            foreach (var refFilePath in files)
            {
                var refFileName = Path.GetFileName(refFilePath);
                var outFilePath = $"{ts.Path}\\{nameof(TestMapBuilder)}\\output\\{refFileName}";

                if (!File.Exists(outFilePath))
                {
                    testSuccess = false;
                    var color = Console.ForegroundColor;
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write("ERROR:");
                    Console.ForegroundColor = color;
                    Console.WriteLine($" {outFilePath} doesn't exist. ");

                    continue;
                }

                var refArea = new Bitmap(refFilePath).ToArea();
                var outArea = new Bitmap(outFilePath).ToArea();

                if (!refArea.IsSameAs(outArea))
                {
                    testSuccess = false;
                    //var color = Console.ForegroundColor;
                    //Console.ForegroundColor = ConsoleColor.Red;
                    //Console.Write("ERROR:");
                    //Console.ForegroundColor = color;
                    //Console.WriteLine($" '{outFilePath}' differs from '{refFilePath}'.");
                }
            }

            TestUtils.PrintResult(testSuccess);
        }

        private static void SaveToOutput(Area2D area, string fileName, string testName, string testPath, Color color)
        {
            var bmp = area.ToBitmap(color);
            var dir = $"{testPath}\\{testName}\\output";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = $"{dir}\\{fileName}.bmp";
            bmp.Save(path, ImageFormat.Png);
        }
    }
}