#if UNIT_TESTS
using System;
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests
{
    [TestFixture]
    class WhenCountEmptyPixels
    {
        private int[][] image;
        private PriorityPointsMarkup markup;

        [SetUp]
        public void Setup()
        {
            image = new[]
                         {
                          new int[]{000,000,000,000,000,000,000,000},  
                          new int[]{000,020,100,000,200,100,000,000},  
                          new int[]{000,100,100,120,090,001,034,000},  
                          new int[]{000,001,230,230,140,140,080,000},  
                          new int[]{000,001,200,010,010,100,100,000},  
                          new int[]{000,000,100,230,100,020,000,000},  
                          new int[]{000,000,000,120,010,000,000,000},  
                          new int[]{000,000,000,000,000,000,000,000}
                        };

            markup = new PriorityPointsMarkup(image);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven()
        {
            markup.CountEmptyPixels(new Point(1,1), 4);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven2()
        {
            markup.CountEmptyPixels(new Point(1, 1), 6);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void ShouldThrowExceptionWhenPatchSizeEven3()
        {
            markup.CountEmptyPixels(new Point(1, 1), 10);
        }

        [Test]
        public void ShouldBe4()
        {
            int count = markup.CountEmptyPixels(new Point(6, 2), 3);

            Assert.AreEqual(4,count);
        }

        [Test]
        public void ShouldBe3()
        {
            int count = markup.CountEmptyPixels(new Point(0, 0), 3);

            Assert.AreEqual(3, count);
        }

        [Test]
        public void ShouldBe7()
        {
            int count = markup.CountEmptyPixels(new Point(2, 3), 5);

            Assert.AreEqual(7, count);
        }
    }
}
#endif