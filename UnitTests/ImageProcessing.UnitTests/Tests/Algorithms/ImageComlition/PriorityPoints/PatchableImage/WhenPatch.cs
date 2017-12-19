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

namespace Tests
{
    [TestFixture]
    class WhenPatch
    {
        private PatchableImage _image;
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

            _image = new PatchableImage(rgbImage, distanceCalculator);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenDifferentSize()
        {
            int dPatchSize = 3;
            var dPos = new Point(3, 3);
            ImagePatch destPatch = new ImagePatch(dPos, dPatchSize, _image, distanceCalculator);

            int sPatchSize = 5;
            var sPos = new Point(5, 5);
            ImagePatch sourcePatch = new ImagePatch(sPos, sPatchSize, _image, distanceCalculator);

            _image.Patch(destPatch, sourcePatch);
        }

        [Test]
        public void ShouldChangeTheRgbLayer()
        {
            int dPatchSize = 3;
            var dPos = new Point(3, 3);
            ImagePatch destPatch = new ImagePatch(dPos, dPatchSize, _image, distanceCalculator);

            int sPatchSize = 3;
            var sPos = new Point(1, 1);
            ImagePatch sourcePatch = new ImagePatch(sPos, sPatchSize, _image, distanceCalculator);

            _image.Patch(destPatch, sourcePatch);

            RgbColor[][] rgbLayer = _image.GetRgb();
            for (int dy = 2, sy = 0; dy < 5; dy++, sy++)
            {
                for (int dx = 2, sx = 0; dx < 5; dx++, sx++)
                {
                    RgbColor sourceColor = rgbLayer[sy][sx];
                    RgbColor destColor = rgbLayer[dy][dx];

                    Assert.AreEqual(sourceColor, destColor);
                }
            }
        }

        [Test]
        public void ShouldChangeLabLayer()
        {
            int dPatchSize = 3;
            var dPos = new Point(3, 3);
            ImagePatch destPatch = new ImagePatch(dPos, dPatchSize, _image, distanceCalculator);

            int sPatchSize = 3;
            var sPos = new Point(1, 1);
            ImagePatch sourcePatch = new ImagePatch(sPos, sPatchSize, _image, distanceCalculator);

            _image.Patch(destPatch, sourcePatch);

            LabColor[][] labLayer = _image.GetLab();
            for (int dy = 2, sy = 0; dy < 5; dy++, sy++)
            {
                for (int dx = 2, sx = 0; dx < 5; dx++, sx++)
                {
                    LabColor sourceColor = labLayer[sy][sx];
                    LabColor destColor = labLayer[dy][dx];

                    Assert.AreEqual(sourceColor, destColor);
                }
            }
        }

        [Test]
        public void ShouldChangeGrayscaleLayer()
        {
            int dPatchSize = 3;
            var dPos = new Point(3, 3);
            ImagePatch destPatch = new ImagePatch(dPos, dPatchSize, _image, distanceCalculator);

            int sPatchSize = 3;
            var sPos = new Point(1, 1);
            ImagePatch sourcePatch = new ImagePatch(sPos, sPatchSize, _image, distanceCalculator);

            _image.Patch(destPatch, sourcePatch);

            int[][] grayscaleLayer = _image.GetGrayscale();
            for (int dy = 2, sy = 0; dy < 5; dy++, sy++)
            {
                for (int dx = 2, sx = 0; dx < 5; dx++, sx++)
                {
                    int sourceColor = grayscaleLayer[sy][sx];
                    int destColor = grayscaleLayer[dy][dx];

                    Assert.AreEqual(sourceColor, destColor);
                }
            }
        }
    }
}
#endif