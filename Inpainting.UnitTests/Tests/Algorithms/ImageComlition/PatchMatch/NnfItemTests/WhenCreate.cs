using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PatchMatch;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests.Algorithms.ImageComlition.PatchMatch.NnfItemTests
{
    [TestFixture]
    public class WhenCreate
    {
        [Test]
        public void ShouldSetFields()
        {
            var pos = new Point(20,10);
            var distance = 345.6;
            NnfItem item = new NnfItem(20,10, distance);

            Assert.That(item.DistanceToOriginal, Is.EqualTo(distance));
            Assert.That(item.X.Equals(pos.X), Is.True);
            Assert.That(item.Y.Equals(pos.Y), Is.True);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShouldThrowExceptionWhenPosNegative()
        {
            var pos = new Point(20, -10);
            var distance = 345.6;
            NnfItem item = new NnfItem(20,-10, distance);

            Assert.That(item.DistanceToOriginal, Is.EqualTo(distance));
            Assert.That(item.X.Equals(pos.X), Is.True);
            Assert.That(item.Y.Equals(pos.Y), Is.True);
        }
    }
}
