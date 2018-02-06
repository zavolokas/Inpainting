#if UNIT_TESTS
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Point = Zavolokas.ImageProcessing.Primitives.Point;

namespace Tests
{
    [TestFixture]
    class WhenIsComplete
    {
        private PatchSourceImageBase image;
        private PriorityPointsMarkup markup;
        private IPatchDistanceCalculator distanceCalculator;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

            int width = 10;
            int height = 10;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Red, 0, 0, width, height);
                g.FillRectangle(Brushes.Green, 3, 3, width - 3 * 2, height - 3 * 2);
            }

            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

            image = mocks.DynamicMock<PatchSourceImageBase>(rgbImage, distanceCalculator);

            int[][] markupImage = new int[10][]
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

            markup = new PriorityPointsMarkup(markupImage);
        }

        [Test]
        public void ShouldBeNotComplete()
        {
            ImagePatch patch = new ImagePatch(new Point(1,1),3,image, distanceCalculator );
            ImagePatch result = patch - markup;

            Assert.IsFalse(result.IsComplete);
        }

        [Test]
        public void ShouldBeComplite()
        {
            ImagePatch patch = new ImagePatch(new Point(8, 8), 3, image, distanceCalculator);
            ImagePatch result = patch - markup;

            Assert.IsTrue(result.IsComplete);
        }

        [Test]
        public void ShouldBeNotCompliteWhenOutsideImage1()
        {
            ImagePatch patch = new ImagePatch(new Point(0, 0), 3, image, distanceCalculator);
            ImagePatch result = patch - markup;

            Assert.IsFalse(result.IsComplete);
        }

        [Test]
        public void ShouldBeNotCompliteWhenOutsideImage2()
        {
            ImagePatch patch = new ImagePatch(new Point(1, 9), 3, image, distanceCalculator);
            ImagePatch result = patch - markup;

            Assert.IsFalse(result.IsComplete);
        }

        [Test]
        public void ShouldBeNotCompliteWhenOutsideImage3()
        {
            ImagePatch patch = new ImagePatch(new Point(9, 8), 3, image, distanceCalculator);
            ImagePatch result = patch - markup;

            Assert.IsFalse(result.IsComplete);
        }

        
    }
}
#endif