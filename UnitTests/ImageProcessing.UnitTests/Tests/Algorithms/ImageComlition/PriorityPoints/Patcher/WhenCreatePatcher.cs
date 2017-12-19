#if UNIT_TESTS
using System;
using System.Drawing;
using NUnit.Framework;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Rhino.Mocks;

namespace Tests
{
    [TestFixture]
    class WhenCreatePatcher
    {
        private PatchableImage _image;
        private IPatchDistanceCalculator distanceCalculator;
        private PriorityPointsMarkup _markup;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

            int[][] markup = new int[10][]
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

            int width = 10;
            int height = 10;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Red, 0, 0, width, height);
                g.FillRectangle(Brushes.Green, 2, 2, width - 2 * 2, height - 2 * 2);
            }

            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

            _image = new PatchableImage(rgbImage, distanceCalculator);
            _markup = new PriorityPointsMarkup(markup);
        }


        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException1()
        {
            var patch = new Patcher(null, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException2()
        {
            var patch = new Patcher(_image, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException3()
        {
            var patch = new Patcher(null, _markup);
        }
    }
}
#endif