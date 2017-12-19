#if UNIT_TESTS
using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Point = Zavolokas.ImageProcessing.Primitives.Point;

namespace Tests.PatchFinders
{
    [TestFixture]
    class WhenFind
    {
        private ImagePatch destPatch;
        private PatchableImage image;
        private PriorityPointsMarkup markup;
        private IPatchDistanceCalculator distanceCalculator;
        private MockRepository mocks;

        private IPatchFinder patchFinder;

        [SetUp]
        public void Setup()
        {
            int[][] markupArray = new int[10][]
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

            mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

            int width = 10;
            int height = 10;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Red, 0, 0, width, height);
                g.FillRectangle(Brushes.Green, 2, 2, width - 2 * 2, height - 2 * 2);
            }

            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

            image = new PatchableImage(rgbImage, distanceCalculator);
            markup = new PriorityPointsMarkup(markupArray);

            destPatch = new ImagePatch(new Point(1, 1), 3, image, distanceCalculator);

            patchFinder = new SimplePatchFinder();
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException1()
        {
            patchFinder.Find(null, null, null, destPatch);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException2()
        {
            patchFinder.Find(image, null, null, destPatch);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException3()
        {
            patchFinder.Find(null, null, markup, destPatch);
        }

        [Test]
        public void ShouldReturnPatch()
        {
            Expect.Call(distanceCalculator.CalcDistance(new ImagePatch(), new ImagePatch()))
                .IgnoreArguments()
                .Return(0.0)
                .Repeat.AtLeastOnce();
            mocks.ReplayAll();

            ImagePatch source = patchFinder.Find(image, markup, markup, destPatch);

            mocks.VerifyAll();
        }

        [Test]
        public void ShouldHaveSameSizeAsDestPatch()
        {
            Expect.Call(distanceCalculator.CalcDistance(new ImagePatch(), new ImagePatch()))
                .IgnoreArguments()
                .Return(0.0)
                .Repeat.AtLeastOnce();
            
            mocks.ReplayAll();

            ImagePatch source = patchFinder.Find(image, markup, markup, destPatch);
            Assert.AreEqual(destPatch.GetSize(), source.GetSize());

            mocks.VerifyAll();
        }

    }
}
#endif