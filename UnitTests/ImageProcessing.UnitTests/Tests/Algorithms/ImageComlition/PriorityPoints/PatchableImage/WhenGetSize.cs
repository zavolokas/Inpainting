#if UNIT_TESTS
using System.Drawing;
using NUnit.Framework;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Size = Zavolokas.ImageProcessing.Primitives.Size;
using Rhino.Mocks;

namespace Tests.PatchableImageTest
{
    [TestFixture]
    class WhenGetSize
    {
        private IPatchDistanceCalculator distanceCalculator;

        [SetUp]
        public void Setup()
        {
            var mocks = new MockRepository();
            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();
        }

        [Test]
        public void ShouldBe10x10()
        {
            int width = 10;
            int height = 10;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Red, 0, 0, width, height);
                g.FillRectangle(Brushes.Green, 3, 3, width - 3 * 2, height - 3 * 2);
            }

            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

            PatchableImage image = new PatchableImage(rgbImage, distanceCalculator);

            Assert.AreEqual(new Size(width,height), image.Size);
        }

        [Test]
        public void ShouldBe12x15()
        {
            int width = 12;
            int height = 15;
            Bitmap bitmap = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.FillRectangle(Brushes.Red, 0, 0, width, height);
                g.FillRectangle(Brushes.Green, 3, 3, width - 3 * 2, height - 3 * 2);
            }

            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

            PatchableImage image = new PatchableImage(rgbImage, distanceCalculator);

            Assert.AreEqual(new Size(width, height), image.Size);
        }
    }
}
#endif