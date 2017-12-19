using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Point = Zavolokas.ImageProcessing.Primitives.Point;

namespace Tests.ImagePatchTests
{
    [TestFixture]
    public class WhenClone
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
        public void ShoudClone()
        {
            ImagePatch patch = new ImagePatch(new Point(3,3),3,image, distanceCalculator );
            ImagePatch clone = patch.Clone();

            var clonePosition = clone.GetPosition();
            var patchPosition = patch.GetPosition();

            var cloneSize = clone.GetSize();
            var patchSize = patch.GetSize();

            Assert.That(clone.Left, Is.EqualTo(patch.Left));
            Assert.That(clone.Right, Is.EqualTo(patch.Right));
            Assert.That(clone.Top, Is.EqualTo(patch.Top));
            Assert.That(clone.Bottom, Is.EqualTo(patch.Bottom));
            Assert.That(clone.IsComplete, Is.EqualTo(patch.IsComplete));
            Assert.That(clone.IsEmpty, Is.EqualTo(patch.IsEmpty));
            Assert.That(cloneSize, Is.EqualTo(patchSize));
            Assert.That(clonePosition.X, Is.EqualTo(patchPosition.X));
            Assert.That(clonePosition.Y, Is.EqualTo(patchPosition.Y));
            Assert.That(clonePosition.IsEmpty, Is.EqualTo(patchPosition.IsEmpty));
        }
    }
}
