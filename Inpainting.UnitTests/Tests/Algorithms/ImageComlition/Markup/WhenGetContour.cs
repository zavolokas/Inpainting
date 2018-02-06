#if UNIT_TESTS
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;

namespace Tests
{
    [TestFixture]
    class WhenGetContour
    {
        [Test]
        public void ShouldReturnContour()
        {
            int[][] image = new[]
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

            Point[] result = new Point[]{new Point(1,1), new Point(1,2), new Point(1,3), new Point(1,4),new Point(2,5), new Point(3,6),
                                       new Point(4,6), new Point(5,5), new Point(6,4), new Point(6,3), new Point(6,2),
                                       new Point(5,1), new Point(4,1), new Point(3,2), new Point(2,1)};

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            Point[][] contours = markup.GetContours();

            Assert.AreEqual(1, contours.Length);
            Assert.IsTrue(AreEqual(result, contours[0]));
        }

        [Test]
        public void ShouldReturnContour2()
        {
            int[][] image = new[]
                         {
                          new int[]{000,000,000,000,000,000,000,000},  
                          new int[]{000,000,000,020,000,000,000,000},  
                          new int[]{000,000,100,120,090,000,000,000},  
                          new int[]{000,001,230,230,140,140,000,000},  
                          new int[]{000,001,200,010,010,100,000,000},  
                          new int[]{000,000,100,230,100,020,000,000},  
                          new int[]{000,000,000,120,010,000,000,000},  
                          new int[]{000,000,000,000,000,000,000,000}
                        };

            Point[] result = new Point[]{new Point(3,1), new Point(2,2), new Point(1,3), new Point(1,4),new Point(2,5), new Point(3,6),
                                       new Point(4,6), new Point(5,5), new Point(5,4), new Point(5,3), new Point(4,2)};

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            Point[][] contours = markup.GetContours();

            Assert.AreEqual(1, contours.Length);
            Assert.IsTrue(AreEqual(result, contours[0]));
        }

        [Test]
        public void ShouldReturnContour3()
        {
            int[][] image = new[]
                         {
                          new int[]{000,000,000,000,000,000,000,000},  
                          new int[]{000,000,100,000,200,100,000,000},  
                          new int[]{000,000,100,000,090,001,034,000},  
                          new int[]{000,001,230,230,140,140,080,000},  
                          new int[]{000,001,200,010,010,100,100,000},  
                          new int[]{000,000,100,230,100,020,000,000},  
                          new int[]{000,000,000,120,010,000,000,000},  
                          new int[]{000,000,000,000,000,000,000,000}
                        };

            Point[] result = new Point[]
                                 {
                                     new Point(2, 1), new Point(2, 2),
                                     new Point(1, 3), new Point(1, 4), new Point(2, 5), new Point(3, 6), new Point(4, 6),
                                     new Point(5, 5), new Point(6, 4), new Point(6, 3), new Point(6, 2), new Point(5, 1),
                                     new Point(4, 1), new Point(4, 2), new Point(3, 3) 
                                 };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            Point[][] contours = markup.GetContours();

            Assert.AreEqual(1, contours.Length);
            Assert.IsTrue(AreEqual(result, contours[0]));
        }

        [Test]
        public void ShouldReturnContour4()
        {
            int[][] image = new[]
                         {
                          new int[]{000,000,100,000,000,000,000,000},  
                          new int[]{000,000,100,000,200,000,000,000},  
                          new int[]{000,000,100,000,090,000,034,000},  
                          new int[]{000,001,230,230,140,140,080,000},  
                          new int[]{000,001,200,010,010,100,100,000},  
                          new int[]{000,000,100,230,100,020,000,000},  
                          new int[]{000,000,000,120,010,000,000,000},  
                          new int[]{000,000,000,000,000,000,000,000}
                        };

            Point[] result = new Point[]
                                 {
                                     new Point(2, 0), new Point(2, 1), new Point(2, 2), new Point(1, 3), 
                                     new Point(1, 4), new Point(2, 5), new Point(3, 6), new Point(4, 6),
                                     new Point(5, 5), new Point(6, 4), new Point(6, 3), new Point(6, 2),
                                     new Point(5, 3), new Point(4, 2), new Point(4, 1), new Point(3, 3)
                                 };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            Point[][] contours = markup.GetContours();

            Assert.AreEqual(1, contours.Length);
            Assert.IsTrue(AreEqual(result, contours[0]));
        }

        [Test]
        public void ShouldReturnTwoContours()
        {
            int[][] image = new[]
                         {
                          new int[]{000,000,000,000,000,000,000,000},  
                          new int[]{000,100,000,200,000,000,000,000},  
                          new int[]{000,100,020,090,100,000,000,010},  
                          new int[]{001,230,230,140,140,000,020,010},  
                          new int[]{001,200,010,010,100,000,020,010},  
                          new int[]{000,100,230,100,020,000,010,000},  
                          new int[]{000,000,120,010,000,000,000,000},  
                          new int[]{000,000,000,000,000,000,000,000}
                        };

            Point[] result = new Point[]
                                 {
                                     new Point(1,1), new Point(1,2), new Point(0,3),
                                     new Point(0,4), new Point(1,5), new Point(2,6),
                                     new Point(3,6), new Point(4,5), new Point(4,4),
                                     new Point(4,3), new Point(4,2), new Point(3,1),
                                     new Point(2,2),
                                 };

            Point[] result2 = new Point[]
                                 {
                                     new Point(6, 3),
                                     new Point(6, 4),
                                     new Point(6, 5),
                                     new Point(7, 4),
                                     new Point(7, 3),
                                     new Point(7, 2)
                                 };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            
            Point[][] contours = markup.GetContours();

            Assert.AreEqual(2, contours.Length);

            Assert.IsTrue(AreEqual(result, contours[0]));
            Assert.IsTrue(AreEqual(result2, contours[1]));
        }

        [Test]
        public void ShouldReturnEmptyContour()
        {
            int[][] image = new[]
                         {
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0},  
                          new int[]{0,0,0,0,0,0,0,0,0,0}
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
            Point[][] contour = markup.GetContours();
            Assert.AreEqual(0, contour.Length);
        }

        //[Test]
        public bool IsContourEqualityFuncValid()
        {
            Point[] c1 = new Point[] { new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };
            Point[] c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

            bool isCheckDifferentSizeCorrect = !AreEqual(c1, c2);

            c1 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };
            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

            bool isEqualArraysCompareCorrect = AreEqual(c1, c2);

            c1 = new Point[] { new Point(1, 1), new Point(2, 2), new Point(1, 3), new Point(5, 4), new Point(2, 5) };
            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(4, 3), new Point(1, 4), new Point(2, 5) };

            bool areTwoDifferentArraysEqualCorrent = !AreEqual(c1, c2);

            c1 = new Point[] { new Point(1, 3), new Point(1, 4), new Point(2, 5), new Point(1, 1), new Point(1, 2) };
            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

            bool isEqualWitchCyclicOffsetCorrect = AreEqual(c1, c2);

            c1 = new Point[] { new Point(1, 3), new Point(1, 4), new Point(2, 5), new Point(1, 3), new Point(1, 2) };
            c2 = new Point[] { new Point(1, 3), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

            bool isEqualWitchDoubleCyclicOffsetCorrect = AreEqual(c1, c2);

            bool result = isCheckDifferentSizeCorrect && isEqualArraysCompareCorrect && areTwoDifferentArraysEqualCorrent && isEqualWitchCyclicOffsetCorrect && isEqualWitchDoubleCyclicOffsetCorrect;

            Assert.IsTrue(result);

            return result;
        }

        private bool AreEqual(Point[] c1, Point[] c2)
        {
            if (c1.Length != c2.Length)
                return false;

            for (int i = 0; i < c1.Length; i++)
            {
                if (AreEqual(c1, c2, i))
                    return true;
            }

            return false;
        }

        private bool AreEqual(Point[] c1, Point[] c2, int index)
        {
            for (int i = 0; i < c2.Length; i++)
            {
                int i2 = i + index;
                if (i2 >= c1.Length)
                    i2 = i2 - c1.Length;

                if (c1[i2].X != c2[i].X || c1[i2].Y != c2[i].Y)
                    return false;
            }

            return true;
        }
    }
}
#endif