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
    class WhenCreateImagePatch
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
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException_WhenSourceImageNull()
        {
            ImagePatch patch = new ImagePatch(new Point(2, 2), 3, null, distanceCalculator);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowException_WhenDistanceCalculatorNull()
        {
            ImagePatch patch = new ImagePatch(new Point(2, 2), 3, image, null);
        }

        //Now patch doesn't care about the area. 
        //A particular implementation of the PatchSourceImageBase takes care of it.
        //[Test]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void ShouldThrowExceptionWhenPointOutsideImage()
        //{
        //    ImagePatch patch = new ImagePatch(new PatchPoint() { Pos = new Point(1, 1), PatchSize = 5 }, image);
        //}
    }
}
#endif