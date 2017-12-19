#if UNIT_TESTS
using System;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests
{
    [TestFixture]
    class WhenCreatePatchableImage
    {
        private RgbColor[][] _rgbImage;
        private IPatchDistanceCalculator distanceCalculator;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

            int[][] rgbImage = new int[10][]
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

            _rgbImage = new RgbColor[rgbImage.Length][];

            for (int y = 0; y < rgbImage.Length; y++)
            {
                _rgbImage[y] = new RgbColor[rgbImage[y].Length];
                for (int x = 0; x < rgbImage[y].Length; x++)
                {
                    int rgb = rgbImage[y][x];
                    _rgbImage[y][x] = new RgbColor(rgb,rgb,rgb);
                }
            }

        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException()
        {
            PatchableImage image = new PatchableImage(null, distanceCalculator);
        }

        [Test]
        public void GrayscaleShouldBeNotNull()
        {
            PatchableImage image = new PatchableImage(_rgbImage, distanceCalculator);
            Assert.NotNull(image.GetGrayscale());
        }

        [Test]
        public void GrayscaleElementsShouldBeNotGraterThan255()
        {
            PatchableImage image = new PatchableImage(_rgbImage, distanceCalculator);
            int[][] grayscale = image.GetGrayscale();

            bool notGreater = true;
            for (int y = 0; y < grayscale.Length; y++)
            {
                for (int x = 0; x < grayscale[y].Length; x++)
                {
                    if (grayscale[y][x] < 0 || 255 < grayscale[y][x])
                    {
                        notGreater = false;
                        break;
                    }
                }
            }
            Assert.IsTrue(notGreater);
        }

        [Test]
        public void LabShouldBeNotNull()
        {
            PatchableImage image = new PatchableImage(_rgbImage, distanceCalculator);
            Assert.NotNull(image.GetLab());
        }

        [Test]
        public void RgbShouldBeNotNull()
        {
            PatchableImage image = new PatchableImage(_rgbImage,distanceCalculator);
            Assert.NotNull(image.GetRgb());
        }
    }
}
#endif