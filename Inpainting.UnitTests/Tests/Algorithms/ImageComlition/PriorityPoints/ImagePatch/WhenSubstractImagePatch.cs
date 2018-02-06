#if UNIT_TESTS
using System;
using System.Drawing;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.GdiHelpers;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
using Zavolokas.ImageProcessing.Primitives;
using Point = Zavolokas.ImageProcessing.Primitives.Point;

namespace Tests
{
    [TestFixture]
    class WhenSubstractImagePatch
    {
        private PatchSourceImageBase image;
        private IPatchDistanceCalculator distanceCalculator;
        private PriorityPointsMarkup markup;
        private MockRepository mocks;

        [SetUp]
        public void Setup()
        {
            mocks = new MockRepository();
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

            //image = mocks.DynamicMock<PatchSourceImageBase>(rgbImage, distanceCalculator);
            image = new PatchSourceImage(rgbImage, distanceCalculator);

            int[][] markupImage = new int[10][]
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

            markup = new PriorityPointsMarkup(markupImage);
        }

        //This test was commented out because now the responsibility for substraction
        //overtakes the distance calculator
        //[Test]
        //public void ShouldCalculateDifference()
        //{
        //    ImagePatch patch1 = new ImagePatch(new Point(1, 1), 3, image, distanceCalculator);
        //    ImagePatch patch2 = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);

        //    LabColor[][] lab = image.GetLab();
        //    LabColor lab1 = lab[1][1];
        //    LabColor lab2 = lab[5][4];

        //    double dist = lab1.CalculateSquareDistanceTo(lab2);

        //    double totalDistance = Math.Round(dist * 9, 4);

        //    //destPatch shouldn't contain the area from the markup
        //    //and shouldn't be taken into account during the distance calculation.
        //    //ImagePatch result = patch - markup;

        //    Assert.AreEqual(totalDistance, Math.Round(patch1 - patch2, 4));
        //}

        //This test was commented out because now the responsibility for substraction
        //overtakes the distance calculator
        //[Test]
        //public void ShouldCalculateDifferenceTakenMarkupIntoAccount1()
        //{
        //    ImagePatch patch1 = new ImagePatch(new Point(1, 1), 3, image, distanceCalculator);
        //    ImagePatch patch2 = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);

        //    LabColor[][] lab = image.GetLab();
        //    LabColor lab1 = lab[1][1];
        //    LabColor lab2 = lab[5][4];

        //    double dist = lab1.CalculateSquareDistanceTo(lab2);

        //    double totalDistance = Math.Round(dist * 7, 4);

        //    //patch1 shouldn't contain the area from the markup
        //    //and shouldn't be taken into account during the distance calculation.
        //    patch1 = patch1 - markup;

        //    Assert.AreEqual(totalDistance, Math.Round(patch1 - patch2, 4));
        //}


        //This test was commented out because now the responsibility for substraction
        //overtakes the distance calculator
        //[Test]
        //public void ShouldCalculateDifferenceTakenMarkupIntoAccount2()
        //{
        //    ImagePatch patch1 = new ImagePatch(new Point(1, 1), 3, image, distanceCalculator);
        //    ImagePatch patch2 = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);

        //    LabColor[][] lab = image.GetLab();
        //    LabColor lab1 = lab[1][1];
        //    LabColor lab2 = lab[5][4];

        //    double dist = lab1.CalculateSquareDistanceTo(lab2);

        //    double totalDistance = Math.Round(dist * 7, 4);

        //    //patch1 shouldn't contain the area from the markup
        //    //and shouldn't be taken into account during the distance calculation.
        //    patch1 = patch1 - markup;

        //    Assert.AreEqual(totalDistance, Math.Round(patch2 - patch1, 4));
        //}

        [Test]
        public void ShouldNotFailWhenDestNotFull()
        {
            ImagePatch dest = new ImagePatch(new Point(0, 0), 3, image, distanceCalculator);
            ImagePatch source = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);

            double distance = dest - source;
        }

        [Test]
        public void ShouldNotFailWhenSourceNotFull()
        {
            ImagePatch dest = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);
            ImagePatch source = new ImagePatch(new Point(0, 0), 3, image, distanceCalculator);

            double distance = dest - source;
        }

        [Test]
        public void ShouldUsePatchDistanceCalculatorWhenSubstract()
        {
            ImagePatch dest = new ImagePatch(new Point(4, 5), 3, image, distanceCalculator);
            ImagePatch source = new ImagePatch(new Point(0, 0), 3, image, distanceCalculator);

            Expect.Call(distanceCalculator.CalcDistance(dest, source)).Return(100.0).Repeat.Once();
            mocks.ReplayAll();

            double distance = dest - source;
            mocks.VerifyAll();
        }
    }
}
#endif