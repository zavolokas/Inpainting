//#if UNIT_TESTS
//using System;
//using System.Drawing;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Zavolokas.GdiHelpers;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
//using Zavolokas.ImageProcessing.Primitives;
//using Point = Zavolokas.ImageProcessing.Primitives.Point;

//namespace Tests
//{
//    [TestFixture]
//    class WhenUnmark
//    {
//        private PatchSourceImageLight patchableImage;
//        private IPatchDistanceCalculator distanceCalculator;

//        [SetUp]
//        public void Setup()
//        {
//            var mocks = new MockRepository();
//            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();

//            int width = 10;
//            int height = 10;
//            Bitmap bitmap = new Bitmap(width, height);
//            using (Graphics g = Graphics.FromImage(bitmap))
//            {
//                g.FillRectangle(Brushes.Red, 0, 0, width, height);
//                g.FillRectangle(Brushes.Green, 3, 3, width - 3 * 2, height - 3 * 2);
//            }

//            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

//            patchableImage = mocks.DynamicMock<PatchSourceImageLight>(rgbImage, distanceCalculator);
//        }

//        [Test]
//        public void ShouldAlertMarkupData1()
//        {
//            int[][] image = new[]
//                         {
//                          new int[]{000,000,000,000,000,000,000,000},  
//                          new int[]{000,020,100,000,200,100,000,000},  
//                          new int[]{000,100,100,120,090,001,034,000},  
//                          new int[]{000,001,230,230,140,140,080,000},  
//                          new int[]{000,001,200,010,010,100,100,000},  
//                          new int[]{000,000,100,230,100,020,000,000},  
//                          new int[]{000,000,000,120,010,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000,000}
//                        };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);

//            Assert.AreEqual(0, markup.Data[0][0]);
//            Assert.AreEqual(0, markup.Data[0][1]);
//            Assert.AreEqual(0, markup.Data[0][2]);
//            Assert.AreEqual(0, markup.Data[1][0]);
//            Assert.AreEqual(1, markup.Data[1][1]);
//            Assert.AreEqual(1, markup.Data[1][2]);
//            Assert.AreEqual(0, markup.Data[2][0]);
//            Assert.AreEqual(1, markup.Data[2][1]);
//            Assert.AreEqual(1, markup.Data[2][2]);

//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);

//            markup.Unmark(patch);

//            Assert.AreEqual(0, markup.Data[0][0]);
//            Assert.AreEqual(0, markup.Data[0][1]);
//            Assert.AreEqual(0, markup.Data[0][2]);
//            Assert.AreEqual(0, markup.Data[1][0]);
//            Assert.AreEqual(0, markup.Data[1][1]);
//            Assert.AreEqual(0, markup.Data[1][2]);
//            Assert.AreEqual(0, markup.Data[2][0]);
//            Assert.AreEqual(0, markup.Data[2][1]);
//            Assert.AreEqual(0, markup.Data[2][2]);
//        }

//        [Test]
//        public void ShouldAlertMarkupData2()
//        {
//            int[][] image = new[]
//                         {
//                          new int[]{000,000,000,000,000,000,000,000},  
//                          new int[]{000,020,100,000,200,100,000,000},  
//                          new int[]{000,100,100,120,090,001,034,000},  
//                          new int[]{000,001,230,230,140,140,080,000},  
//                          new int[]{000,001,200,010,010,100,100,000},  
//                          new int[]{000,000,100,230,100,020,000,000},  
//                          new int[]{000,000,000,120,010,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000,000}
//                        };

//            int[][] imageAfter = new[]
//                         {
//                          new int[]{0,0,0,0,0,0,0,0},  
//                          new int[]{0,0,0,0,0,0,0,0},  
//                          new int[]{0,0,0,0,0,0,1,0},  
//                          new int[]{0,0,0,0,0,0,1,0},  
//                          new int[]{0,0,0,0,0,0,1,0},  
//                          new int[]{0,0,0,0,0,0,0,0},  
//                          new int[]{0,0,0,1,1,0,0,0},  
//                          new int[]{0,0,0,0,0,0,0,0}
//                        };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(3, 3), 5, patchableImage, distanceCalculator);

//            markup.Unmark(patch);

//            for (int y = 0; y < markup.Data.Length; y++)
//            {
//                for (int x = 0; x < markup.Data[y].Length; x++)
//                {
//                    Assert.AreEqual(imageAfter[y][x], markup.Data[y][x]);
//                }
//            }
//        }

//        [Test]
//        public void ShouldAlertPixels()
//        {
//            int[][] image = new[]
//                         {
//                          new int[]{000,000,000,000,000,000,000,000},  
//                          new int[]{000,020,100,000,200,100,000,000},  
//                          new int[]{000,100,100,120,090,001,034,000},  
//                          new int[]{000,001,230,230,140,140,080,000},  
//                          new int[]{000,001,200,010,010,100,100,000},  
//                          new int[]{000,000,100,230,100,020,000,000},  
//                          new int[]{000,000,000,120,010,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000,000}
//                        };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);

//            markup.Unmark(patch);
//            Assert.AreEqual(24, markup.Pixels);
//        }

//        [Test]
//        public void ShouldAlertPixels2()
//        {
//            int[][] image = new[]
//                         {
//                          new int[]{000,000,000,000,000,000,000,000},  
//                          new int[]{000,020,100,000,200,100,000,000},  
//                          new int[]{000,100,100,120,090,001,034,000},  
//                          new int[]{000,001,230,230,140,140,080,000},  
//                          new int[]{000,001,200,010,010,100,100,000},  
//                          new int[]{000,000,100,230,100,020,000,000},  
//                          new int[]{000,000,000,120,010,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000,000}
//                        };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            patch = new ImagePatch(new Point(1, 4), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            Assert.AreEqual(19, markup.Pixels);
//        }

//        [Test]
//        public void ShouldAlertMarkupContour()
//        {
//            int[][] image = new[]
//                             {
//                              new int[]{000,000,000,000,000,000,000,000},  
//                              new int[]{000,020,100,000,200,100,000,000},  
//                              new int[]{000,100,100,120,090,001,034,000},  
//                              new int[]{000,001,230,230,140,140,080,000},  
//                              new int[]{000,001,200,010,010,100,100,000},  
//                              new int[]{000,000,100,230,100,020,000,000},  
//                              new int[]{000,000,000,120,010,000,000,000},  
//                              new int[]{000,000,000,000,000,000,000,000}
//                            };

//            int[][] imageAfter = new[]
//                             {             
//                              new int[] {0,0,0,0,0,0,0,0}, 
//                              new int[] {0,0,0,0,1,1,0,0}, 
//                              new int[] {0,0,0,1,1,1,1,0}, 
//                              new int[] {0,1,1,1,1,1,1,0}, 
//                              new int[] {0,1,1,1,1,1,1,0}, 
//                              new int[] {0,0,1,1,1,1,0,0}, 
//                              new int[] {0,0,0,1,1,0,0,0}, 
//                              new int[] {0,0,0,0,0,0,0,0} 
//                            };

//            Point[] result = new Point[]{new Point(1,3), new Point(1,4), new Point(2,5), new Point(3,6),new Point(4,6), new Point(5,5),
//                                           new Point(6,4), new Point(6,3), new Point(6,2), new Point(5,1), new Point(4,1),
//                                           new Point(3,2), new Point(2,3)};

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);
//            Point[][] contour = markup.GetContours();

//            Assert.IsTrue(AreEqual(result, contour[0]));
//        }

//        [Test]
//        public void ShouldAlertConfidenceMap()
//        {
//            int[][] image = new[]
//                             {
//                              new int[]{000,000,000,000,000,000,000,000},  
//                              new int[]{000,020,100,000,200,100,000,000},  
//                              new int[]{000,100,100,120,090,001,034,000},  
//                              new int[]{000,001,230,230,140,140,080,000},  
//                              new int[]{000,001,200,010,010,100,100,000},  
//                              new int[]{000,000,100,230,100,020,000,000},  
//                              new int[]{000,000,000,120,010,000,000,000},  
//                              new int[]{000,000,000,000,000,000,000,000}
//                            };

//            double[][] confidenceMapAfter = new[]
//                             {             
//                              new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000}, 
//                              new [] {1.000,0.556,0.556,1.000,0.000,0.000,1.000,1.000}, 
//                              new [] {1.000,0.556,0.556,0.000,0.000,0.000,0.000,1.000}, 
//                              new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//                              new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//                              new [] {1.000,1.000,0.000,0.000,0.000,0.000,1.000,1.000}, 
//                              new [] {1.000,1.000,1.000,0.000,0.000,1.000,1.000,1.000}, 
//                              new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000} 
//                            };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            bool allEqual = true;
//            for (int y = 0; y < confidenceMapAfter.Length; y++)
//            {
//                for (int x = 0; x < confidenceMapAfter[y].Length; x++)
//                {
//                    if (Math.Round(confidenceMapAfter[y][x], 3) != Math.Round(markup.ConfidenceMap[y][x], 3))
//                    {
//                        allEqual = false;
//                        break;
//                    }
//                }
//            }

//            Assert.IsTrue(allEqual);
//        }

//        [Test]
//        public void ShouldAlertConfidenceMap2()
//        {
//            int[][] image = new[]
//                             {
//                              new int[]{000,000,000,000,000,000,000,000},  
//                              new int[]{000,020,100,000,200,100,000,000},  
//                              new int[]{000,100,100,120,090,001,034,000},  
//                              new int[]{000,001,230,230,140,140,080,000},  
//                              new int[]{000,001,200,010,010,100,100,000},  
//                              new int[]{000,000,100,230,100,020,000,000},  
//                              new int[]{000,000,000,120,010,000,000,000},  
//                              new int[]{000,000,000,000,000,000,000,000}
//                            };

//            double[][] confidenceMapAfter = new[]
//                             {             
//                              new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000}, 
//                              new [] {1.000,0.556,0.556,1.000,0.235,0.000,1.000,1.000}, 
//                              new [] {1.000,0.556,0.556,0.235,0.235,0.000,0.000,1.000}, 
//                              new [] {1.000,0.000,0.235,0.235,0.235,0.000,0.000,1.000}, 
//                              new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//                              new [] {1.000,1.000,0.000,0.000,0.000,0.000,1.000,1.000}, 
//                              new [] {1.000,1.000,1.000,0.000,0.000,1.000,1.000,1.000}, 
//                              new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000} 
//                            };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            patch = new ImagePatch(new Point(3, 2), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            bool allEqual = true;
//            for (int y = 0; y < confidenceMapAfter.Length; y++)
//            {
//                for (int x = 0; x < confidenceMapAfter[y].Length; x++)
//                {
//                    if (Math.Round(confidenceMapAfter[y][x], 3) != Math.Round(markup.ConfidenceMap[y][x], 3))
//                    {
//                        allEqual = false;
//                        break;
//                    }
//                }
//            }

//            Assert.IsTrue(allEqual);
//        }

//        [Test]
//        public void ShouldAlertMarkupFrontier()
//        {
//            int[][] image = new[]
//                             {
//                              new int[]{000,000,000,000,000,000,000,000},  
//                              new int[]{000,020,100,000,200,100,000,000},  
//                              new int[]{000,100,100,120,090,001,034,000},  
//                              new int[]{000,001,230,230,140,140,080,000},  
//                              new int[]{000,001,200,010,010,100,100,000},  
//                              new int[]{000,000,100,230,100,020,000,000},  
//                              new int[]{000,000,000,120,010,000,000,000},  
//                              new int[]{000,000,000,000,000,000,000,000}
//                            };

//            int[][] imageAfter = new[]
//                             {             
//                              new int[] {0,0,0,0,0,0,0,0}, 
//                              new int[] {0,0,0,0,1,1,0,0}, 
//                              new int[] {0,0,0,1,0,0,1,0}, 
//                              new int[] {0,1,1,0,0,0,1,0}, 
//                              new int[] {0,1,0,0,0,0,1,0}, 
//                              new int[] {0,0,1,0,0,1,0,0}, 
//                              new int[] {0,0,0,1,1,0,0,0}, 
//                              new int[] {0,0,0,0,0,0,0,0} 
//                            };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            for (int y = 0; y < markup.Frontier.Length; y++)
//            {
//                for (int x = 0; x < markup.Frontier[y].Length; x++)
//                {
//                    Assert.AreEqual(imageAfter[y][x], markup.Frontier[y][x]);
//                }
//            }
//        }

//        //[Test]
//        public bool IsContourEqualityFuncValid()
//        {
//            Point[] c1 = new Point[] { new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };
//            Point[] c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

//            bool isCheckDifferentSizeCorrect = !AreEqual(c1, c2);

//            c1 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };
//            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

//            bool isEqualArraysCompareCorrect = AreEqual(c1, c2);

//            c1 = new Point[] { new Point(1, 1), new Point(2, 2), new Point(1, 3), new Point(5, 4), new Point(2, 5) };
//            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(4, 3), new Point(1, 4), new Point(2, 5) };

//            bool areTwoDifferentArraysEqualCorrent = !AreEqual(c1, c2);

//            c1 = new Point[] { new Point(1, 3), new Point(1, 4), new Point(2, 5), new Point(1, 1), new Point(1, 2) };
//            c2 = new Point[] { new Point(1, 1), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

//            bool isEqualWitchCyclicOffsetCorrect = AreEqual(c1, c2);

//            c1 = new Point[] { new Point(1, 3), new Point(1, 4), new Point(2, 5), new Point(1, 3), new Point(1, 2) };
//            c2 = new Point[] { new Point(1, 3), new Point(1, 2), new Point(1, 3), new Point(1, 4), new Point(2, 5) };

//            bool isEqualWitchDoubleCyclicOffsetCorrect = AreEqual(c1, c2);

//            bool result = isCheckDifferentSizeCorrect && isEqualArraysCompareCorrect && areTwoDifferentArraysEqualCorrent && isEqualWitchCyclicOffsetCorrect && isEqualWitchDoubleCyclicOffsetCorrect;

//            Assert.IsTrue(result);

//            return result;
//        }

//        private bool AreEqual(Point[] c1, Point[] c2)
//        {
//            if (c1.Length != c2.Length)
//                return false;

//            for (int i = 0; i < c1.Length; i++)
//            {
//                if (AreEqual(c1, c2, i))
//                    return true;
//            }

//            return false;
//        }

//        private bool AreEqual(Point[] c1, Point[] c2, int index)
//        {
//            for (int i = 0; i < c2.Length; i++)
//            {
//                int i2 = i + index;
//                if (i2 >= c1.Length)
//                    i2 = i2 - c1.Length;

//                if (c1[i2].X != c2[i].X || c1[i2].Y != c2[i].Y)
//                    return false;
//            }

//            return true;
//        }
//    }
//}
//#endif