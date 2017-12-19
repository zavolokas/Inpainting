using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PatchMatch;
using Zavolokas.ImageProcessing.Primitives;

namespace Zavolokas.ImageProcessing.UnitTests.Tests.Algorithms.ImageComlition.PatchMatch.NnfTests
{
    [TestFixture]
    public class WhenCreate
    {
        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShouldThrowExceptionWhenPatchSizeEven()
        {
            int patchSize = 4;
            var vectors = new NnfItem[10][];
            Nnf nnf = new Nnf(vectors, patchSize);
        }

        [Test]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ShouldThrowExceptionWhenPatchSizeLessThan3()
        {
            int patchSize = 1;
            var vectors = new NnfItem[10][];
            Nnf nnf = new Nnf(vectors, patchSize);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ShouldThrowExceptionWhenVectorsNull()
        {
            int patchSize = 3;
            Nnf nnf = new Nnf(null, patchSize);
        }

        [Test]
        public void ShouldSetPatchSize()
        {
            int patchSize = 3;
            var vectors = new NnfItem[10][];
            Nnf nnf = new Nnf(vectors, patchSize);

            Assert.That(nnf.PatchSize, Is.EqualTo(patchSize));
        }

        [Test]
        public void ShouldSetNnfData()
        {
            int patchSize = 5;
            var vectors = new NnfItem[10][];
            Nnf nnf = new Nnf(vectors, patchSize);

            Assert.That(nnf.NnfData, Is.EqualTo(vectors));
        }
    }
}
