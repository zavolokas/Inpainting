#if UNIT_TESTS
using System;
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests
{
    [TestFixture]
    public class WhenGetPatchPoints
    {
        private PatchPointsFinder finder;
        private PriorityPointsMarkup markup;

        [SetUp]
        public void Setup()
        {
            int[][] image = new int[5][] 
         {new int[3]{0,3,4}, 
         new int[3]{2,5,6}, 
         new int[3]{2,7,8}, 
         new int[3]{2,8,4}, 
         new int[3]{2,4,1}
         };

            int[][] mrkp = new int[5][] 
                            {new int[3]{0,0,0}, 
                             new int[3]{0,5,0}, 
                             new int[3]{0,7,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}
                             };

            markup = new PriorityPointsMarkup(mrkp);
            finder = new PatchPointsFinder(image);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven()
        {
            finder.GetPatchPoints(markup, 4);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven2()
        {
            finder.GetPatchPoints(markup, 6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven3()
        {
            finder.GetPatchPoints(markup, 10);
        }

        [Test]
        public void ShouldNoThrowException()
        {
            finder.GetPatchPoints(markup, 11);
        }

        [Test]
        public void ShouldSetCorrectPatchSize()
        {
            PatchPointSortedList patchPoints = finder.GetPatchPoints(markup, 11);
            for (int i = 0; i < patchPoints.Count; i++)
            {
                PriorityPoint point = patchPoints[i];
                Assert.AreEqual(11, point.PatchSize);
            }
        }

        [Test]
        public void ShouldSetCorrectPatchSize2()
        {
            PatchPointSortedList patchPoints = finder.GetPatchPoints(markup, 9);
            for (int i = 0; i < patchPoints.Count; i++)
            {
                PriorityPoint point = patchPoints[i];
                Assert.AreEqual(9, point.PatchSize);
            }
        }

        [Test]
        public void ShouldReturnOnePointWhenContourIsOnePoint()
        {
            int[][] mrkp = new int[5][] 
                            {new int[3]{0,0,0}, 
                             new int[3]{0,5,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}
                             };

            markup = new PriorityPointsMarkup(mrkp);

            PatchPointSortedList patchPoints = finder.GetPatchPoints(markup, 9);
            Assert.AreEqual(1, patchPoints.Count);
            Assert.AreEqual(new Point(1,1), patchPoints[0].Pos);
        }

        [Test]
        public void ShouldReturnEmptyContour()
        {
            int[][] mrkp = new int[5][] 
                            {new int[3]{0,0,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}, 
                             new int[3]{0,0,0}
                             };

            markup = new PriorityPointsMarkup(mrkp);

            PatchPointSortedList patchPoints = finder.GetPatchPoints(markup, 9);
            Assert.AreEqual(0, patchPoints.Count);
        }
    }
}
#endif