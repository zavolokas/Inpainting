using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Zavolokas.Math.Combinatorics;
using Zavolokas.Structures;

namespace ConsoleInpaintAreaDonors
{
    public static class Area2DIntersectTests
    {
        public static void RunAll()
        {
            AreaIntersectionTest(TestSet.Init256x128());
            AreaIntersectionTest(TestSet.Init1280x720());
        }

        public static void AreaIntersectionTest(TestSet ts)
        {
            string testName = nameof(AreaIntersectionTest);
            Console.WriteLine($"{testName} is running ...");
            bool testSuccess = false;
            bool noDiffs = true;
            try
            {
                var areas = new List<Area2D>();
                areas.Add(ts.Donors[0].ToArea());
                areas.Add(ts.Donors[2].ToArea());
                areas.Add(ts.Donors[3].ToArea());
                areas.Add(ts.Donors[4].ToArea());
                areas.Add(ts.RemoveMarkup.ToArea());

                var testCases = areas.GetAllCombinations()
                    .OrderBy(x => x.Count())
                    .ToArray();

                for (int i = 0; i < testCases.Length; i++)
                {
                    //Console.WriteLine($"\trun case #{i} from {testCases.Length}");
                    var resultArea = Area2D.Create(0, 0, ts.Picture.Width, ts.Picture.Height);

                    var testCase = testCases[i].ToArray();

                    for (int j = 0; j < testCase.Length; j++)
                    {
                        var area = testCase[j];
                        resultArea = resultArea.Intersect(area);
                    }

                    if (!resultArea.IsEmpty)
                    {
                        SaveToOutput(resultArea, $"intersection{i}", testName, ts.Path);
                    }
                }
                
                noDiffs = VerifyOutput(testName, ts.Path);
                testSuccess = true;
            }
            catch (Exception ex)
            {
                testSuccess = false;
                Console.WriteLine(ex);
            }

            TestUtils.PrintResult(testSuccess && noDiffs);
        }

        private static void SaveToOutput(Area2D area, string fileName, string testName, string testPath)
        {
            var bmp = area.ToBitmap(Color.Red);
            var dir = $"{testPath}\\{testName}\\output";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            var path = $"{dir}\\{fileName}.png";
            bmp.Save(path, ImageFormat.Png);
        }

        private static bool VerifyOutput(string testName, string testPath)
        {
            bool noDiffs = true;
            string[] files = Directory.GetFiles($"{testPath}\\{testName}\\refs", "*.*", SearchOption.TopDirectoryOnly);
            foreach (var refFilePath in files)
            {
                var refFileName = Path.GetFileName(refFilePath);
                var outFilePath = $"{testPath}\\{testName}\\output\\{refFileName}";

                if (!File.Exists(outFilePath))
                {
                    noDiffs = false;
                    continue;
                }

                var refArea = new Bitmap(refFilePath).ToArea();
                var outArea = new Bitmap(outFilePath).ToArea();

                if (!refArea.IsSameAs(outArea))
                {
                    noDiffs = false;
                }
            }

            return noDiffs;
        }
    }
}
