#if UNIT_TESTS
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests
{
    [TestFixture]
    class WhenGetPatchAt
    {
        private PatchableImage _patchableImage;
        private IPatchDistanceCalculator distanceCalculator;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

            int[][] image = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,020,100,000,200,100,000,000,000,000},  
                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            RgbColor[][] rgbImage = new RgbColor[image.Length][];

            for (int y = 0; y < image.Length; y++)
            {
                rgbImage[y] = new RgbColor[image[y].Length];
                for (int x = 0; x < image[y].Length; x++)
                {
                    int rgb = image[y][x];
                    rgbImage[y][x] = new RgbColor(rgb, rgb, rgb);
                }
            }

            _patchableImage = new PatchableImage(rgbImage, distanceCalculator);
        }

        [Test]
        public void ShouldBeNotNull()
        {
            int patchSize = 3;
            var pos = new Point(2, 3);
            ImagePatch patch = _patchableImage.GetPatchAt(pos, patchSize);            
        }

        [Test]
        public void Foo()
        {
             distanceCalculator = new PatchDistanceCalculator();

            int[][] destImage = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,020,100,000,200,100,000,000,000,000},  
                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            RgbColor[][] destRgbImage = new RgbColor[destImage.Length][];

            for (int y = 0; y < destImage.Length; y++)
            {
                destRgbImage[y] = new RgbColor[destImage[y].Length];
                for (int x = 0; x < destImage[y].Length; x++)
                {
                    int rgb = destImage[y][x];
                    destRgbImage[y][x] = new RgbColor(rgb, rgb, rgb);
                }
            }

            int[][] sourceImage = new int[5][]
                        {
                          new int[5]{000,000,000,000,000},  
                          new int[5]{000,000,000,000,000},  
                          new int[5]{020,100,200,100, 56},  
                          new int[5]{100,120,090,001,034},  
                          new int[5]{001,230,230,140,080},  
                        };

            RgbColor[][] sourceRgbImage = new RgbColor[sourceImage.Length][];

            for (int y = 0; y < sourceImage.Length; y++)
            {
                sourceRgbImage[y] = new RgbColor[sourceImage[y].Length];
                for (int x = 0; x < sourceImage[y].Length; x++)
                {
                    int rgb = sourceImage[y][x];
                    sourceRgbImage[y][x] = new RgbColor(rgb, rgb, rgb);
                }
            }

            _destPatchableImage = new PatchableImage(destRgbImage, distanceCalculator);
            var _sourcePatchableImage = new PatchableImage(sourceRgbImage, distanceCalculator);

            int patchSize = 3;
            var position = new Point(1, 9);
            var position2 = new Point(1, 1);
            ImagePatch destPatch = _destPatchableImage.GetPatchAt(position, patchSize);
            ImagePatch sourcePatch = _sourcePatchableImage.GetPatchAt(position2, patchSize);

            ImagePatch destPatch2 = _sourcePatchableImage.GetPatchAt(position2, patchSize);
            ImagePatch sourcePatch2 = _destPatchableImage.GetPatchAt(position, patchSize);

            var res = destPatch - sourcePatch;
            var res2 = sourcePatch - destPatch;

            res = destPatch - sourcePatch2;
            res2 = sourcePatch - destPatch2;

            //var pos = destPatch.GetPosition();

            //Assert.AreEqual(1, pos.X);
            //Assert.AreEqual(8, pos.Y);
            //Assert.AreEqual(7, destPatch.Top);
            //Assert.AreEqual(9, destPatch.Bottom);
            //Assert.AreEqual(0, destPatch.Left);
            //Assert.AreEqual(2, destPatch.Right);
        }

        [Test]
        public void ShouldCorrectlyTakePatchAtBottom()
        {
            int patchSize = 3;
            var position = new Point(1, 9);
            ImagePatch patch = _patchableImage.GetPatchAt(position, patchSize);

            var pos = patch.GetPosition();

            Assert.AreEqual(1, pos.X);
            Assert.AreEqual(8, pos.Y);
            Assert.AreEqual(7, patch.Top);
            Assert.AreEqual(9, patch.Bottom);
            Assert.AreEqual(0, patch.Left);
            Assert.AreEqual(2, patch.Right);
        }

        [Test]
        public void ShouldCorrectlyTakePatchAtTop()
        {
            int patchSize = 5;
            var position = new Point(4, 0);
            ImagePatch patch = _patchableImage.GetPatchAt(position, patchSize);

            var pos = patch.GetPosition();

            Assert.AreEqual(4, pos.X);
            Assert.AreEqual(2, pos.Y);
            Assert.AreEqual(0, patch.Top);
            Assert.AreEqual(4, patch.Bottom);
            Assert.AreEqual(2, patch.Left);
            Assert.AreEqual(6, patch.Right);
        }

        [Test]
        public void ShouldCorrectlyTakePatchAtLeft()
        {
            int patchSize = 3;
            var position = new Point(0, 2);
            ImagePatch patch = _patchableImage.GetPatchAt(position, patchSize);

            var pos = patch.GetPosition();

            Assert.AreEqual(1, pos.X);
            Assert.AreEqual(2, pos.Y);
            Assert.AreEqual(1, patch.Top);
            Assert.AreEqual(3, patch.Bottom);
            Assert.AreEqual(0, patch.Left);
            Assert.AreEqual(2, patch.Right);
        }

        [Test]
        public void ShouldCorrectlyTakePatchAtRight()
        {
            int patchSize = 3;
            var position = new Point(9, 2);
            ImagePatch patch = _patchableImage.GetPatchAt(position, patchSize);

            var pos = patch.GetPosition();

            Assert.AreEqual(8, pos.X);
            Assert.AreEqual(2, pos.Y);
            Assert.AreEqual(1, patch.Top);
            Assert.AreEqual(3, patch.Bottom);
            Assert.AreEqual(7, patch.Left);
            Assert.AreEqual(9, patch.Right);
        }

        //[Test]
        //public void GrayscaleElementsShouldBeNotGraterThan255()
        //{
        //    PatchableImage image = new PatchableImage(_rgbImage);
        //    int[][] grayscale = image.GetGrayscale();

        //    bool notGreater = true;
        //    for (int y = 0; y < grayscale.Length; y++)
        //    {
        //        for (int x = 0; x < grayscale[y].Length; x++)
        //        {
        //            if (grayscale[y][x] < 0 || 255 < grayscale[y][x])
        //            {
        //                notGreater = false;
        //                break;
        //            }
        //        }
        //    }
        //    Assert.IsTrue(notGreater);
        //}

        //[Test]
        //public void LabShouldBeNotNull()
        //{
        //    PatchableImage image = new PatchableImage(_rgbImage);
        //    Assert.NotNull(image.GetLab());
        //}

        //[Test]
        //public void RgbShouldBeNotNull()
        //{
        //    PatchableImage image = new PatchableImage(_rgbImage);
        //    Assert.NotNull(image.GetRgb());
        //}

        public PatchableImage _destPatchableImage { get; set; }
    }
}
#endif