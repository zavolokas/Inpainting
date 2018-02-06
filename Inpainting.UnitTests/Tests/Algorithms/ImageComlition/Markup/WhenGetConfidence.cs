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
//    class WhenGetConfidence
//    {
//        private int[][] image;
//        private PriorityPointsMarkup markup;
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

//            image = new[]
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

//            markup = new PriorityPointsMarkup(image);
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentException))]
//        public void ShouldThrowExceptionWhenPatchSizeEven()
//        {
//            markup.GetConfidence(new Point(1, 1), 4);
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentException))]
//        public void ShouldThrowExceptionWhenPatchSizeEven2()
//        {
//            markup.GetConfidence(new Point(1, 1), 6);
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentException))]
//        public void ShouldThrowExceptionWhenPatchSizeEven3()
//        {
//            markup.GetConfidence(new Point(1, 1), 10);
//        }

//        [Test]
//        public void ShouldBe4()
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

//            //double[][] confidenceMapAfter = new[]
//            //                 {             
//            //                  new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000}, 
//            //                  new [] {1.000,0.556,0.556,1.000,0.235,0.000,1.000,1.000}, 
//            //                  new [] {1.000,0.556,0.556,0.235,0.235,0.000,0.000,1.000}, 
//            //                  new [] {1.000,0.000,0.235,0.235,0.235,0.000,0.000,1.000}, 
//            //                  new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//            //                  new [] {1.000,1.000,0.000,0.000,0.000,0.000,1.000,1.000}, 
//            //                  new [] {1.000,1.000,1.000,0.000,0.000,1.000,1.000,1.000}, 
//            //                  new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000} 
//            //                };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3 , patchableImage, distanceCalculator);
//            markup.Unmark(patch);

//            patch = new ImagePatch(new Point(3, 2), 3, patchableImage,distanceCalculator);
//            markup.Unmark(patch);

//            double confidence = markup.GetConfidence(new Point(1, 3), 3);

//            Assert.AreEqual(0.483,Math.Round(confidence,3));
//        }

//        [Test]
//        public void ShouldBe0235()
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

//            //double[][] confidenceMapAfter = new[]
//            //                 {             
//            //                  new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000}, 
//            //                  new [] {1.000,0.556,0.556,1.000,0.000,0.000,1.000,1.000}, 
//            //                  new [] {1.000,0.556,0.556,0.000,0.000,0.000,0.000,1.000}, 
//            //                  new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//            //                  new [] {1.000,0.000,0.000,0.000,0.000,0.000,0.000,1.000}, 
//            //                  new [] {1.000,1.000,0.000,0.000,0.000,0.000,1.000,1.000}, 
//            //                  new [] {1.000,1.000,1.000,0.000,0.000,1.000,1.000,1.000}, 
//            //                  new [] {1.000,1.000,1.000,1.000,1.000,1.000,1.000,1.000} 
//            //                };

//            PriorityPointsMarkup markup = new PriorityPointsMarkup(image);
//            ImagePatch patch = new ImagePatch(new Point(1, 1), 3, patchableImage,distanceCalculator);
//            markup.Unmark(patch);

//            double confidence = markup.GetConfidence(new Point(3, 2), 3);

//            Assert.AreEqual(0.235, Math.Round(confidence, 3));
//        }
//    }
//}
//#endif