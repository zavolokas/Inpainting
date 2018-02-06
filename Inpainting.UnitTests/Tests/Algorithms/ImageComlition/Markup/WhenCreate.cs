#if UNIT_TESTS
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;

namespace Tests
{
    [TestFixture]
    class WhenCreate
    {
        private int[][] _rgbImage;
        private int[][] _onesMatrix;
        private int[][] _counturMatrix;
        private double[][] _confidenceMap;

        [SetUp]
        public void Setup()
        {
            _rgbImage = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,020,100,000,200,100,000,000,000,000},  
                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            _onesMatrix = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,001,001,000,001,001,000,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,000,001,001,001,001,000,000,000,000},  
                          new int[10]{000,000,000,001,001,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            _confidenceMap = new double[10][]
                        {
                          new double[10]{1,1,1,1,1,1,1,1,1,1},  
                          new double[10]{1,1,1,1,1,1,1,1,1,1},  
                          new double[10]{1,0,0,1,0,0,1,1,1,1},  
                          new double[10]{1,0,0,0,0,0,0,1,1,1},  
                          new double[10]{1,0,0,0,0,0,0,1,1,1},  
                          new double[10]{1,0,0,0,0,0,0,1,1,1},  
                          new double[10]{1,1,0,0,0,0,1,1,1,1},  
                          new double[10]{1,1,1,0,0,1,1,1,1,1},  
                          new double[10]{1,1,1,1,1,1,1,1,1,1},  
                          new double[10]{1,1,1,1,1,1,1,1,1,1}
                        };

            _counturMatrix = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,001,001,000,001,001,000,000,000,000},  
                          new int[10]{000,001,000,001,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,000,001,000,000,001,000,000,000,000},  
                          new int[10]{000,000,000,001,001,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };
        }

        [Test]
        public void DataShouldContainOnes()
        {
            PriorityPointsMarkup markup = new PriorityPointsMarkup(_rgbImage);
            bool allEqual = true;
            for (int y = 0; y < _onesMatrix.Length; y++)
            {
                for (int x = 0; x < _onesMatrix[y].Length; x++)
                {
                    var elmt = _onesMatrix[y][x];
                    if (elmt != markup.Data[y][x])
                        allEqual = false;
                }
            }

            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

        [Test]
        public void ShouldFormConfidenceMap()
        {
            PriorityPointsMarkup markup = new PriorityPointsMarkup(_rgbImage);
            bool allEqual = true;
            for (int y = 0; y < _confidenceMap.Length; y++)
            {
                for (int x = 0; x < _confidenceMap[y].Length; x++)
                {
                    var elmt = _confidenceMap[y][x];
                    if (elmt != markup.ConfidenceMap[y][x])
                        allEqual = false;
                }
            }

            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

        [Test]
        public void PixelsShouldBe28()
        {
            PriorityPointsMarkup markup = new PriorityPointsMarkup(_rgbImage);
            Assert.AreEqual(28, markup.PixelsAmount);
        }


        [Test]
        public void PixelsShouldBe36()
        {
            int[][] mrk = new int[10][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,001,001,001,001,001,001,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(mrk);
            Assert.AreEqual(36, markup.PixelsAmount);
        }


        [Test]
        public void ShouldFormFrontierMatrix()
        {
            PriorityPointsMarkup markup = new PriorityPointsMarkup(_rgbImage);
            bool allEqual = true;
            for (int y = 0; y < _counturMatrix.Length; y++)
            {
                for (int x = 0; x < _counturMatrix[y].Length; x++)
                {
                    var elmt = _counturMatrix[y][x];
                    if (elmt != markup.Frontier[y][x])
                        allEqual = false;
                }
            }

            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

        [Test]
        public void ShouldFormFrontierMatrixTopBound()
        {
            int[][] rgbImage = new int[8][]
                        {
                          new int[10]{000,020,100,020,200,100,000,000,000,000},  
                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            int[][] counturMatrix = new int[8][]
                        {
                          new int[10]{000,001,001,001,001,001,000,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,000,001,000,000,001,000,000,000,000},  
                          new int[10]{000,000,000,001,001,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000}
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(rgbImage);
            bool allEqual = true;
            for (int y = 0; y < counturMatrix.Length; y++)
            {
                for (int x = 0; x < counturMatrix[y].Length; x++)
                {
                    var elmt = counturMatrix[y][x];
                    if (elmt != markup.Frontier[y][x])
                        allEqual = false;
                }
            }

            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

        [Test]
        public void ShouldFormFrontierMatrixBottomBound()
        {
            int[][] rgbImage = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{000,020,100,020,200,100,000,000,000,000},  
                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
                        };

            int[][] counturMatrix = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{000,001,001,001,001,001,000,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,001,000,000,000,000,001,000,000,000},  
                          new int[10]{000,000,001,000,000,001,000,000,000,000},  
                          new int[10]{000,000,000,001,001,000,000,000,000,000},  
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(rgbImage);
            bool allEqual = true;
            for (int y = 0; y < counturMatrix.Length; y++)
            {
                for (int x = 0; x < counturMatrix[y].Length; x++)
                {
                    var elmt = counturMatrix[y][x];
                    if (elmt != markup.Frontier[y][x])
                        allEqual = false;
                }
            }

            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }


        [Test]
        public void ShouldFormFrontierMatrixLeftBound()
        {
            int[][] rgbImage = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{020,100,020,200,100,000,000,000,000,000},  
                          new int[10]{100,100,120,090,001,034,000,000,000,000},  
                          new int[10]{001,230,230,140,140,080,000,000,000,000},  
                          new int[10]{001,200,010,010,100,100,000,000,000,000},  
                          new int[10]{000,100,230,100,020,000,000,000,000,000},  
                          new int[10]{000,000,120,010,000,000,000,000,000,000},  
                        };

            int[][] counturMatrix = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{001,001,001,001,001,000,000,000,000,000},  
                          new int[10]{001,000,000,000,000,001,000,000,000,000},  
                          new int[10]{001,000,000,000,000,001,000,000,000,000},  
                          new int[10]{001,000,000,000,000,001,000,000,000,000},  
                          new int[10]{000,001,000,000,001,000,000,000,000,000},  
                          new int[10]{000,000,001,001,000,000,000,000,000,000},  
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(rgbImage);
            bool allEqual = true;
            for (int y = 0; y < counturMatrix.Length; y++)
            {
                for (int x = 0; x < counturMatrix[y].Length; x++)
                {
                    var elmt = counturMatrix[y][x];
                    if (elmt != markup.Frontier[y][x])
                        allEqual = false;

                }
            }
            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

        [Test]
        public void ShouldFormFrontierMatrixRightBound()
        {
            int[][] rgbImage = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{000,000,000,000,020,100,020,200,100,000},  
                          new int[10]{000,000,000,000,100,100,120,090,001,034},  
                          new int[10]{000,000,000,000,001,230,230,140,140,080},  
                          new int[10]{000,000,000,000,001,200,010,010,100,100},  
                          new int[10]{000,000,000,000,000,100,230,100,020,000},  
                          new int[10]{000,000,000,000,000,000,120,010,000,000},  
                        };

            int[][] counturMatrix = new int[8][]
                        {
                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
                          new int[10]{000,000,000,000,000,000,000,000,000,000},
                          new int[10]{000,000,000,000,001,001,001,001,001,000},  
                          new int[10]{000,000,000,000,001,000,000,000,000,001},  
                          new int[10]{000,000,000,000,001,000,000,000,000,001},  
                          new int[10]{000,000,000,000,001,000,000,000,000,001},  
                          new int[10]{000,000,000,000,000,001,000,000,001,000},  
                          new int[10]{000,000,000,000,000,000,001,001,000,000},  
                        };

            PriorityPointsMarkup markup = new PriorityPointsMarkup(rgbImage);
            bool allEqual = true;
            for (int y = 0; y < counturMatrix.Length; y++)
            {
                for (int x = 0; x < counturMatrix[y].Length; x++)
                {
                    var elmt = counturMatrix[y][x];
                    if (elmt != markup.Frontier[y][x])
                        allEqual = false;
                }
            }
            Assert.IsTrue(allEqual, "Not all elements are the same as in ethalone");
        }

    }
}
#endif