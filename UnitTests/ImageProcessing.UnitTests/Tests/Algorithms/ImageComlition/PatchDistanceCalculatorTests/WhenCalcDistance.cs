//#if DEBUG
//using System.Drawing;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Zavolokas.GdiHelpers;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
//using Zavolokas.ImageProcessing.Primitives;
//using Point = Zavolokas.ImageProcessing.Primitives.Point;

//namespace Tests.Algorithms.ImageComlition.PatchDistanceCalculatorTests
//{
//    [TestFixture]
//    public class WhenCalcDistance
//    {
//        private MockRepository mocks;
//        private PatchSourceImageLight image;
//        private IPatchDistanceCalculator distanceCalculator;
//        private LabColor[][] lab;

//        [SetUp]
//        public void Setup()
//        {
//            distanceCalculator = new PatchDistanceCalculator();

//            mocks = new MockRepository();

//            int width = 10;
//            int height = 10;
//            Bitmap bitmap = new Bitmap(width, height);
//            using (Graphics g = Graphics.FromImage(bitmap))
//            {
//                g.FillRectangle(Brushes.Red, 0, 0, width, height);
//                g.FillRectangle(Brushes.Green, 3, 3, width - 3 * 2, height - 3 * 2);
//            }

//            RgbColor[][] rgbImage = Utils.Bitmap2RgbArray(bitmap);

//            image = mocks.DynamicMock<PatchSourceImageLight>(rgbImage, distanceCalculator);

//            lab = new LabColor[][]
//                                   {
//                                       new []{new LabColor(1,1,1), new LabColor(1,2,3), new LabColor(23,6,13),new LabColor(2,44,9),new LabColor(5,4,2),new LabColor(112,123,17),new LabColor(12,42,58)},
//                                       new []{new LabColor(10,14,1), new LabColor(85,1,1), new LabColor(14,13,1),new LabColor(34,1,2),new LabColor(23,1,90),new LabColor(12,17,21),new LabColor(1,84,13)},
//                                       new []{new LabColor(9,7,3), new LabColor(8,23,3), new LabColor(1,13,18),new LabColor(5,6,7), new LabColor(1,1,1), new LabColor(1,2,3), new LabColor(23,6,13)},
//                                       new []{new LabColor(2,44,9), new LabColor(5,4,2), new LabColor(1,1,1),new LabColor(4,5,2), new LabColor(10,14,1), new LabColor(85,1,1), new LabColor(14,13,1)},
//                                       new []{new LabColor(34,1,2), new LabColor(23,1,90), new LabColor(23,9,15),new LabColor(2,6,7),new LabColor(1,4,1),new LabColor(1,63,14),new LabColor(22,7,54)} 
//                                   };
//        }


//        [Test]
//        //This test makes sure that distance between two matching patches
//        //is bigger than zero when a part of dest patch is marked by a mask
//        public void ShouldReturnNonZeroForMatchingMarkedPatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{new LabColor(1,1,1),      new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)},
//                                       new []{new LabColor(10,14,1),    new LabColor(85,1,1),   new LabColor(44,215,5),  new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)},
//                                       new []{new LabColor(9,7,3),      new LabColor(12,74,45),   new LabColor(67,1,6),  new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7)} 
//                                   };

//            int[][] markupImage = new int[][]
//                        {
//                          new int[]{000,000,000,000,000,000,000},
//                          new int[]{000,000,001,000,000,000,000},  
//                          new int[]{000,140,140,000,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000},  
//                        };

//            var _markup = new PriorityPointsMarkup(markupImage);

//            const int patchSize = 3;
            
//            //calculate the distance for marked patch
//            //distance for one point
//            double maxDistance = LabColor.Empty.CalculateSquareDistanceTo(LabColor.Empty);
//            //3 points is marked
//            maxDistance *= 3;

//            Point pos1 = new Point(2, 2);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 2);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance1 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance1, Is.GreaterThan(0));

//            dest = dest - _markup;
//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.EqualTo(maxDistance));
//        }

//        [Test]
//        //This test makes sure that distance to dest patch which has only
//        //empty colors is max possible distance
//        public void ShouldReturnMaxDistanceForEmptyColorDestPatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7)} 
//                                   };

//            const int patchSize = 3;

//            //calculate the distance for marked patch
//            //distance for one point
//            double maxDistance = LabColor.Empty.CalculateSquareDistanceTo(LabColor.Empty);
//            //9 points is marked
//            maxDistance *= 9;

//            Point pos1 = new Point(1, 1);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 3);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.EqualTo(maxDistance));
//        }

//        [Test]
//        //This test makes sure that distance to source patch which has only
//        //empty colors is max possible distance
//        public void ShouldReturnMaxDistanceForEmptyColorSourcePatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7)} 
//                                   };

//            const int patchSize = 3;

//            //calculate the distance for marked patch
//            //distance for one point
//            double maxDistance = LabColor.Empty.CalculateSquareDistanceTo(LabColor.Empty);
//            //9 points is marked
//            maxDistance *= 9;

//            Point pos1 = new Point(5, 3);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(1, 1);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.EqualTo(maxDistance));
//        }

//        [Test]
//        //This test makes sure that distance to matching source patch which has partially
//        //empty colors is beigger than zero
//        public void ShouldReturnNonZeroDistanceForPartiallyMatchingEmptySourcePatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1)} 
//                                   };

//            const int patchSize = 3;

//            //distance for one point
//            double maxDistance = LabColor.Empty.CalculateSquareDistanceTo(LabColor.Empty);
//            //6 points is empty
//            maxDistance *= 6;

//            Point pos1 = new Point(5, 3);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(1, 2);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.EqualTo(maxDistance));
//        }

//        [Test]
//        //This test makes sure that distance to matching dest patch which has partially
//        //empty colors is beigger than zero
//        public void ShouldReturnNonZeroDistanceForPartiallyMatchingEmptyDestPatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)},
//                                       new []{LabColor.Empty,           LabColor.Empty,         LabColor.Empty,         new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1)} 
//                                   };

//            const int patchSize = 3;

//            //calculate the distance for marked patch
//            //distance for one point
//            double maxDistance = LabColor.Empty.CalculateSquareDistanceTo(LabColor.Empty);
//            //6 points is empty
//            maxDistance *= 6;

//            Point pos1 = new Point(1, 2);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 3);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.EqualTo(maxDistance));
//        }
        
//        [Test]
//        //This test makes sure that distance between two matching patches
//        //is greater than zero when source patch is partially marked by a mask
//        public void ShouldReturnZeroForPartiallyMatchingMarkedPatch()
//        {
//            var lab1 = new LabColor[][]
//                                   {
//                                       new []{new LabColor(1,1,1),      new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9),   new LabColor(1,2,3),    new LabColor(23,6,13),  new LabColor(2,44,9)}, 
//                                       new []{new LabColor(10,14,1),    new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2),   new LabColor(85,1,1),   new LabColor(14,13,1),  new LabColor(34,1,2)}, 
//                                       new []{new LabColor(9,7,3),      new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7),    new LabColor(8,23,3),   new LabColor(1,13,18),  new LabColor(5,6,7)},  
//                                       new []{new LabColor(2,44,9),     new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2),    new LabColor(5,4,2),    new LabColor(1,1,1),    new LabColor(4,5,2)},  
//                                       new []{new LabColor(34,1,2),     new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7),    new LabColor(23,1,90),  new LabColor(23,9,15),  new LabColor(2,6,7)}  
//                                   };

//            int[][] markupImage = new int[][]
//                        {
//                          new int[]{000,000,000,000,000,000,000},
//                          new int[]{000,000,000,000,000,010,000},  
//                          new int[]{000,000,000,000,010,010,000},  
//                          new int[]{000,000,000,000,000,010,000},  
//                          new int[]{000,000,000,000,000,000,000},  
//                        };

//            var _markup = new PriorityPointsMarkup(markupImage);

//            const int patchSize = 3;

//            Point pos1 = new Point(2, 2);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 2);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab1).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance1 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance1, Is.EqualTo(0));

//            source = source - _markup;
//            double distance2 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance2, Is.GreaterThan(0));
//        }

//        [Test]
//        //this test makes sure that a patch that fully marked by mask
//        //has a biggest possible distance
//        public void ShouldReturnMaxDistanceForMarkedPatch()
//        {
//            int[][] markupImage = new int[][]
//                        {
//                          new int[]{001,200,100,000,000,000,000},
//                          new int[]{120,090,001,000,000,000,000},  
//                          new int[]{230,140,140,000,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000},  
//                          new int[]{000,000,000,000,000,000,000},  
//                        };

//            var _markup = new PriorityPointsMarkup(markupImage);

//            const int patchSize = 3;

//            double maxDistance = LabColor.BiggestDistance;
//            maxDistance = maxDistance * patchSize * patchSize;

//            Point pos1 = new Point(1, 1);
//            ImagePatch dest = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(4, 1);
//            ImagePatch source = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab).Repeat.AtLeastOnce();
//            mocks.ReplayAll();

//            double distance1 = distanceCalculator.CalcDistance(ref dest,ref source);
//            Assert.That(distance1, Is.LessThan(maxDistance));

//            dest = dest - _markup;
//            double distance2 = distanceCalculator.CalcDistance(ref dest, ref source);
//            Assert.That(distance2, Is.GreaterThanOrEqualTo(maxDistance));
//        }


//        [Test]
//        //Distance between simular patches should be big
//        //when source is not full
//        public void DistanceShouldBeBigWhenSourceNotFull_LeftDown()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(0, 4);
//            ImagePatch source = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(3, 1);
//            ImagePatch dest = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double realDistance = 0;

//            realDistance += lab[0][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[0][3].CalculateSquareDistanceTo(lab[3][0]);
//            realDistance += lab[0][4].CalculateSquareDistanceTo(lab[3][1]);

//            realDistance += lab[1][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[1][3].CalculateSquareDistanceTo(lab[4][0]);
//            realDistance += lab[1][4].CalculateSquareDistanceTo(lab[4][1]);

//            realDistance += lab[2][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[2][3].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[2][4].CalculateSquareDistanceTo(LabColor.Empty);

//            double distance = distanceCalculator.CalcDistance(ref dest,ref source);

//            Assert.That(distance, Is.GreaterThan(0));
//            Assert.That(distance, Is.EqualTo(realDistance));
//        }

//        [Test]
//        //when dest is not full, the unpresented part count as Empty colors
//        //so the distance between simular patches should be bigger than zero
//        //even if the dest patch is not full
//        public void DistanceShouldBeBigWhenDestNotFull_LeftDown()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(0, 4);
//            ImagePatch patch1 = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(3, 1);
//            ImagePatch patch2 = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double realDistance = 0;

//            realDistance += lab[0][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[3][0].CalculateSquareDistanceTo(lab[0][3]);
//            realDistance += lab[3][1].CalculateSquareDistanceTo(lab[0][4]);
            
//            realDistance += lab[1][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[4][0].CalculateSquareDistanceTo(lab[1][3]);
//            realDistance += lab[4][1].CalculateSquareDistanceTo(lab[1][4]);

//            realDistance += lab[2][2].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[2][3].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[2][4].CalculateSquareDistanceTo(LabColor.Empty);

//            double distance = distanceCalculator.CalcDistance(ref patch1,ref patch2);

//            Assert.That(distance, Is.EqualTo(realDistance));
//        }


////====================================================================================================
//        [Test]
//        //Distance between simular patches should be big
//        //when source is not full
//        public void DistanceShouldBeBigWhenSourceNotFull_Top()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(1, 0);
//            ImagePatch patch1 = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 2);
//            ImagePatch patch2 = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double realDistance = 0;

//            realDistance += lab[1][4].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[1][5].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[1][6].CalculateSquareDistanceTo(LabColor.Empty);

//            realDistance += lab[0][0].CalculateSquareDistanceTo(lab[2][4]);
//            realDistance += lab[0][1].CalculateSquareDistanceTo(lab[2][5]);
//            realDistance += lab[0][2].CalculateSquareDistanceTo(lab[2][6]);

//            realDistance += lab[1][0].CalculateSquareDistanceTo(lab[3][4]);
//            realDistance += lab[1][1].CalculateSquareDistanceTo(lab[3][5]);
//            realDistance += lab[1][2].CalculateSquareDistanceTo(lab[3][6]);

//            double distance = distanceCalculator.CalcDistance(ref patch2,ref patch1);

//            Assert.That(distance, Is.GreaterThan(0));
//            Assert.That(distance, Is.EqualTo(realDistance));
//        }

//        [Test]
//        //when dest is not full, the unpresented part count as Empty colors
//        //so the distance between simular patches should be bigger than zero
//        //even if the dest patch is not full
//        public void DistanceShouldBeBigWhenDestNotFull_Top()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(1, 0);
//            ImagePatch patch1 = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Point pos2 = new Point(5, 2);
//            ImagePatch patch2 = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double realDistance = 0;


//            realDistance += lab[1][4].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[1][5].CalculateSquareDistanceTo(LabColor.Empty);
//            realDistance += lab[1][6].CalculateSquareDistanceTo(LabColor.Empty);

//            realDistance += lab[0][0].CalculateSquareDistanceTo(lab[2][4]);
//            realDistance += lab[0][1].CalculateSquareDistanceTo(lab[2][5]);
//            realDistance += lab[0][2].CalculateSquareDistanceTo(lab[2][6]);

//            realDistance += lab[1][0].CalculateSquareDistanceTo(lab[3][4]);
//            realDistance += lab[1][1].CalculateSquareDistanceTo(lab[3][5]);
//            realDistance += lab[1][2].CalculateSquareDistanceTo(lab[3][6]);

//            double distance = distanceCalculator.CalcDistance(ref patch1, ref patch2);

//            Assert.That(distance, Is.EqualTo(realDistance));
//        }

//        [Test]
//        public void ShouldReturnZeroDistance()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(1, 1);
//            ImagePatch patch1 = new ImagePatch(pos1, patchSize, image, distanceCalculator);
//            ImagePatch patch2 = new ImagePatch(pos1, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double distance = distanceCalculator.CalcDistance(ref patch1, ref patch2);

//            Assert.That(distance, Is.EqualTo(0));
//        }

//        [Test]
//        public void ShouldReturnDistance()
//        {
//            const int patchSize = 3;

//            Point pos1 = new Point(1, 1);
//            ImagePatch patch1 = new ImagePatch(pos1, patchSize, image, distanceCalculator);
            
//            Point pos2 = new Point(2, 1);
//            ImagePatch patch2 = new ImagePatch(pos2, patchSize, image, distanceCalculator);

//            Expect.Call(image.Lab).Return(lab);
//            mocks.ReplayAll();

//            double realDistance = 0;

//            realDistance += lab[0][0].CalculateSquareDistanceTo(lab[0][1]);
//            realDistance += lab[0][1].CalculateSquareDistanceTo(lab[0][2]);
//            realDistance += lab[0][2].CalculateSquareDistanceTo(lab[0][3]);

//            realDistance += lab[1][0].CalculateSquareDistanceTo(lab[1][1]);
//            realDistance += lab[1][1].CalculateSquareDistanceTo(lab[1][2]);
//            realDistance += lab[1][2].CalculateSquareDistanceTo(lab[1][3]);

//            realDistance += lab[2][0].CalculateSquareDistanceTo(lab[2][1]);
//            realDistance += lab[2][1].CalculateSquareDistanceTo(lab[2][2]);
//            realDistance += lab[2][2].CalculateSquareDistanceTo(lab[2][3]);

//            double distance = distanceCalculator.CalcDistance(ref patch1, ref patch2);

//            Assert.That(distance, Is.EqualTo(realDistance));
//        }
//    }
//}
//#endif