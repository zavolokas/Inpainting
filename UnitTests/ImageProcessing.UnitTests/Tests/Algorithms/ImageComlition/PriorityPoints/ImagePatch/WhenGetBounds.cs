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
    class WhenGetBounds
    {
        private PatchSourceImageBase image;
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
        }

        [Test]
        public void ShouldReturnCorrectPosition1()
        {
            Point position = new Point(2, 3);
            int patchSize = 3;
            ImagePatch patch = new ImagePatch(position, patchSize, image, distanceCalculator);

            Assert.AreEqual(1, patch.Left);
            Assert.AreEqual(3, patch.Right);
            Assert.AreEqual(2, patch.Top);
            Assert.AreEqual(4, patch.Bottom);
        }

        [Test]
        public void ShouldReturnCorrectPosition2()
        {
            Point position = new Point(23, 89);
            int patchSize = 9;
            ImagePatch patch = new ImagePatch(position, patchSize, image, distanceCalculator);

            Assert.AreEqual(19, patch.Left);
            Assert.AreEqual(27, patch.Right);
            Assert.AreEqual(85, patch.Top);
            Assert.AreEqual(93, patch.Bottom);
        }
    }
}
#endif