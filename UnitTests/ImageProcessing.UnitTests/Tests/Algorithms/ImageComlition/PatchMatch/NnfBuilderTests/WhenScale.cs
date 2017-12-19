//#if DEBUG
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using Rhino.Mocks;
//using Zavolokas.ImageProcessing.Algorithms;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PatchMatch;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;
//using Zavolokas.ImageProcessing.Primitives;

//namespace Zavolokas.ImageProcessing.UnitTests.NnfBuilderTests
//{
//    [TestFixture]
//    public class WhenScale
//    {
//        private NnfItem[][] _vectors;
//        private Size _1xSize;
//        private Size _2xSize;
//        private Size _2xSize_evenWidth;
//        private Size _2xSize_evenHeight;
//        private Size _2xSize_even;
//        private INnfBuilder _builder;

//        private PatchSourceImageLight _source;
//        private PatchSourceImageLight _dest;
//        private IPatchDistanceCalculator distanceCalculator;

//        private Markup _markup;

//        [SetUp]
//        public void Setup()
//        {
//            var setting = new PatchMatchSettings();
//            var execControl = new ProcessExecutionControl();
//            _builder = new NnfBuilder(setting, execControl);

//            int width = 5;
//            int height = 3;
//            _vectors = new NnfItem[height][];
//            for (int y = 0; y < height; y++)
//            {
//                _vectors[y] = new NnfItem[width];
//                for (int x = 0; x < width; x++)
//                {
//                    _vectors[y][x] = new NnfItem(x, y, 0.0);
//                }
//            }

//            _1xSize = new Size(width, height);
//            _2xSize = new Size(width * 2, height * 2);
//            _2xSize_evenWidth = new Size(_2xSize.Width + 1, _2xSize.Height);
//            _2xSize_evenHeight = new Size(_2xSize.Width, _2xSize.Height + 1);
//            _2xSize_even = new Size(_2xSize.Width + 1, _2xSize.Height + 1);

//            var mocks = new MockRepository();
//            distanceCalculator = mocks.DynamicMock<IPatchDistanceCalculator>();
//            var pp = ImagePatch.Empty;
//            Expect.Call(distanceCalculator.CalcDistance(ref pp,ref pp)).IgnoreArguments().Return(0);

//            int[][] image = new int[10][]
//                        {
//                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
//                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
//                          new int[10]{000,020,100,000,200,100,000,000,000,000},  
//                          new int[10]{000,100,100,120,090,001,034,000,000,000},  
//                          new int[10]{000,001,230,230,140,140,080,000,000,000},  
//                          new int[10]{000,001,200,010,010,100,100,000,000,000},  
//                          new int[10]{000,000,100,230,100,020,000,000,000,000},  
//                          new int[10]{000,000,000,120,010,000,000,000,000,000},  
//                          new int[10]{000,000,000,000,000,000,000,000,000,000},  
//                          new int[10]{000,000,000,000,000,000,000,000,000,000}
//                        };

//            var labImage = new LabColor[image.Length][];

//            for (int y = 0; y < image.Length; y++)
//            {
//                labImage[y] = new LabColor[image[y].Length];
//                for (int x = 0; x < image[y].Length; x++)
//                {
//                    int rgb = image[y][x];
//                    labImage[y][x] = new LabColor(rgb, rgb, rgb);
//                }
//            }

//            _source = new PatchSourceImageLight(labImage);
//            _dest = new PatchSourceImageLight(labImage);

//            var mrkp = new[]
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

//            _markup = new Markup(mrkp);

//            mocks.ReplayAll();
//        }

//        [Test]
//        [ExpectedException(typeof(ArgumentOutOfRangeException))]
//        public void ShouldThrowExceptionWhenScaleLessThan2()
//        {
//            int patchSize = 5;
//            var vectors = new NnfItem[10][];
//            Nnf nnf = new Nnf(vectors, patchSize);

//            Nnf scaled = _builder.Scale(nnf, _1xSize.Width, _1xSize.Height, _dest, _source, _markup, null);
//        }

//        [Test]
//        public void ShouldReturnNotNull()
//        {
//            int patchSize = 5;
//            Nnf nnf = new Nnf(_vectors, patchSize);
//            nnf = _builder.Scale(nnf, _2xSize.Width, _2xSize.Height, _dest, _source, _markup, null);
//            Assert.That(nnf, Is.Not.Null);
//        }

//        [Test]
//        public void ShouldDoubleTheSize()
//        {
//            int patchSize = 5;
//            Nnf nnf = new Nnf(_vectors, patchSize);

//            var newNnf = _builder.Scale(nnf, _2xSize.Width, _2xSize.Height, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            Assert.That(newVectors.Length, Is.EqualTo(_vectors.Length * 2));
//            Assert.That(newVectors[0].Length, Is.EqualTo(_vectors[0].Length * 2));
//        }

//        [Test]
//        public void ShouldDoubleTheSizeWithEvenWidth()
//        {
//            int patchSize = 5;
//            Nnf nnf = new Nnf(_vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, _2xSize_evenWidth.Width, _2xSize_evenWidth.Height, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            Assert.That(newVectors.Length, Is.EqualTo(_2xSize_evenWidth.Height));
//            Assert.That(newVectors[0].Length, Is.EqualTo(_2xSize_evenWidth.Width));
//        }

//        [Test]
//        public void ShouldDoubleTheSizeWithEvenHeight()
//        {
//            int patchSize = 5;
//            Nnf nnf = new Nnf(_vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, _2xSize_evenHeight.Width, _2xSize_evenHeight.Height, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            Assert.That(newVectors.Length, Is.EqualTo(_2xSize_evenHeight.Height));
//            Assert.That(newVectors[0].Length, Is.EqualTo(_2xSize_evenHeight.Width));
//        }

//        [Test]
//        public void ShouldDoubleTheSizeWithEvenSizes()
//        {
//            int patchSize = 5;
//            Nnf nnf = new Nnf(_vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, _2xSize_even.Width, _2xSize_even.Height, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            Assert.That(newVectors.Length, Is.EqualTo(_2xSize_even.Height));
//            Assert.That(newVectors[0].Length, Is.EqualTo(_2xSize_even.Width));
//        }

//        [Test]
//        public void ShouldIncreaseNnf()
//        {
//            int patchSize = 5;

//            var a1 = new NnfItem(1, 1, 0.0);
//            var a2 = new NnfItem(2, 1, 0.0);
//            var a3 = new NnfItem(3, 1, 0.0);

//            var b1 = new NnfItem(1, 2, 0.0);
//            var b2 = new NnfItem(2, 2, 0.0);
//            var b3 = new NnfItem(3, 2, 0.0);

//            var vectors = new NnfItem[][] { new[] { a1, a2, a3 }, new[] { b1, b2, b3 } };

//            Nnf nnf = new Nnf(vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, 6, 4, _dest, _source, _markup, null);

//            //1,1 ; 2,1 ; 3,1
//            //1,2 ; 2,2 ; 3,2

//            //     0     1     2     3     4     5 
//            //0 = 2,2 ; 3,2 ; 4,2 ; 5,2 ; 6,2 ; 7,2
//            //1 = 2,3 ; 3,3 ; 4,3 ; 5,3 ; 6,3 ; 7,3
//            //2 = 2,4 ; 3,4 ; 4,4 ; 5,4 ; 6,4 ; 7,4
//            //3 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5

//            var newVectors = newNnf.NnfData;

//            Assert.That(newVectors[0][0].P, Is.EqualTo(new Point(2, 2)));
//            Assert.That(newVectors[0][1].P, Is.EqualTo(new Point(3, 2)));
//            Assert.That(newVectors[1][0].P, Is.EqualTo(new Point(2, 3)));
//            Assert.That(newVectors[1][1].P, Is.EqualTo(new Point(3, 3)));

//            Assert.That(newVectors[0][2].P, Is.EqualTo(new Point(4, 2)));
//            Assert.That(newVectors[0][3].P, Is.EqualTo(new Point(5, 2)));
//            Assert.That(newVectors[1][2].P, Is.EqualTo(new Point(4, 3)));
//            Assert.That(newVectors[1][3].P, Is.EqualTo(new Point(5, 3)));

//            Assert.That(newVectors[0][4].P, Is.EqualTo(new Point(6, 2)));
//            Assert.That(newVectors[0][5].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[1][4].P, Is.EqualTo(new Point(6, 3)));
//            Assert.That(newVectors[1][5].P, Is.EqualTo(new Point(7, 3)));


//            Assert.That(newVectors[2][0].P, Is.EqualTo(new Point(2, 4)));
//            Assert.That(newVectors[2][1].P, Is.EqualTo(new Point(3, 4)));
//            Assert.That(newVectors[3][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[3][1].P, Is.EqualTo(new Point(3, 5)));

//            Assert.That(newVectors[2][2].P, Is.EqualTo(new Point(4, 4)));
//            Assert.That(newVectors[2][3].P, Is.EqualTo(new Point(5, 4)));
//            Assert.That(newVectors[3][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[3][3].P, Is.EqualTo(new Point(5, 5)));

//            Assert.That(newVectors[2][4].P, Is.EqualTo(new Point(6, 4)));
//            Assert.That(newVectors[2][5].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[3][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[3][5].P, Is.EqualTo(new Point(7, 5)));
//        }

//        [Test]
//        public void ShouldIncreaseNnfWithEvenWidth()
//        {
//            int patchSize = 5;

//            var a1 = new NnfItem(1, 1, 0.0);
//            var a2 = new NnfItem(2, 1, 0.0);
//            var a3 = new NnfItem(3, 1, 0.0);

//            var b1 = new NnfItem(1, 2, 0.0);
//            var b2 = new NnfItem(2, 2, 0.0);
//            var b3 = new NnfItem(3, 2, 0.0);

//            var vectors = new NnfItem[][] { new[] { a1, a2, a3 }, new[] { b1, b2, b3 } };

//            Nnf nnf = new Nnf(vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, 7, 4, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            //1,1 ; 2,1 ; 3,1
//            //1,2 ; 2,2 ; 3,2

//            //     0     1     2     3     4     5     6
//            //0 = 2,2 ; 3,2 ; 4,2 ; 5,2 ; 6,2 ; 7,2 ; 7,2
//            //1 = 2,3 ; 3,3 ; 4,3 ; 5,3 ; 6,3 ; 7,3 ; 7,3
//            //2 = 2,4 ; 3,4 ; 4,4 ; 5,4 ; 6,4 ; 7,4 ; 7,4
//            //3 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5 ; 7,5

//            Assert.That(newVectors[0][0].P, Is.EqualTo(new Point(2, 2)));
//            Assert.That(newVectors[0][1].P, Is.EqualTo(new Point(3, 2)));
//            Assert.That(newVectors[1][0].P, Is.EqualTo(new Point(2, 3)));
//            Assert.That(newVectors[1][1].P, Is.EqualTo(new Point(3, 3)));

//            Assert.That(newVectors[0][2].P, Is.EqualTo(new Point(4, 2)));
//            Assert.That(newVectors[0][3].P, Is.EqualTo(new Point(5, 2)));
//            Assert.That(newVectors[1][2].P, Is.EqualTo(new Point(4, 3)));
//            Assert.That(newVectors[1][3].P, Is.EqualTo(new Point(5, 3)));

//            Assert.That(newVectors[0][4].P, Is.EqualTo(new Point(6, 2)));
//            Assert.That(newVectors[0][5].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[0][6].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[1][4].P, Is.EqualTo(new Point(6, 3)));
//            Assert.That(newVectors[1][5].P, Is.EqualTo(new Point(7, 3)));
//            Assert.That(newVectors[1][6].P, Is.EqualTo(new Point(7, 3)));


//            Assert.That(newVectors[2][0].P, Is.EqualTo(new Point(2, 4)));
//            Assert.That(newVectors[2][1].P, Is.EqualTo(new Point(3, 4)));
//            Assert.That(newVectors[3][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[3][1].P, Is.EqualTo(new Point(3, 5)));

//            Assert.That(newVectors[2][2].P, Is.EqualTo(new Point(4, 4)));
//            Assert.That(newVectors[2][3].P, Is.EqualTo(new Point(5, 4)));
//            Assert.That(newVectors[3][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[3][3].P, Is.EqualTo(new Point(5, 5)));

//            Assert.That(newVectors[2][4].P, Is.EqualTo(new Point(6, 4)));
//            Assert.That(newVectors[2][5].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[2][6].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[3][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[3][5].P, Is.EqualTo(new Point(7, 5)));
//            Assert.That(newVectors[3][6].P, Is.EqualTo(new Point(7, 5)));
//        }

//        [Test]
//        public void ShouldIncreaseNnfWithEvenHeight()
//        {
//            int patchSize = 5;

//            var a1 = new NnfItem(1, 1, 0.0);
//            var a2 = new NnfItem(2, 1, 0.0);
//            var a3 = new NnfItem(3, 1, 0.0);

//            var b1 = new NnfItem(1, 2, 0.0);
//            var b2 = new NnfItem(2, 2, 0.0);
//            var b3 = new NnfItem(3, 2, 0.0);

//            var vectors = new NnfItem[][] { new[] { a1, a2, a3 }, new[] { b1, b2, b3 } };

//            Nnf nnf = new Nnf(vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, 6, 5, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            //1,1 ; 2,1 ; 3,1
//            //1,2 ; 2,2 ; 3,2

//            //0 = 2,2 ; 3,2 ; 4,2 ; 5,2 ; 6,2 ; 7,2 
//            //1 = 2,3 ; 3,3 ; 4,3 ; 5,3 ; 6,3 ; 7,3
//            //2 = 2,4 ; 3,4 ; 4,4 ; 5,4 ; 6,4 ; 7,4
//            //3 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5
//            //4 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5

//            Assert.That(newVectors[0][0].P, Is.EqualTo(new Point(2, 2)));
//            Assert.That(newVectors[0][1].P, Is.EqualTo(new Point(3, 2)));
//            Assert.That(newVectors[1][0].P, Is.EqualTo(new Point(2, 3)));
//            Assert.That(newVectors[1][1].P, Is.EqualTo(new Point(3, 3)));

//            Assert.That(newVectors[0][2].P, Is.EqualTo(new Point(4, 2)));
//            Assert.That(newVectors[0][3].P, Is.EqualTo(new Point(5, 2)));
//            Assert.That(newVectors[1][2].P, Is.EqualTo(new Point(4, 3)));
//            Assert.That(newVectors[1][3].P, Is.EqualTo(new Point(5, 3)));

//            Assert.That(newVectors[0][4].P, Is.EqualTo(new Point(6, 2)));
//            Assert.That(newVectors[0][5].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[1][4].P, Is.EqualTo(new Point(6, 3)));
//            Assert.That(newVectors[1][5].P, Is.EqualTo(new Point(7, 3)));


//            Assert.That(newVectors[2][0].P, Is.EqualTo(new Point(2, 4)));
//            Assert.That(newVectors[2][1].P, Is.EqualTo(new Point(3, 4)));
//            Assert.That(newVectors[3][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[3][1].P, Is.EqualTo(new Point(3, 5)));
//            Assert.That(newVectors[4][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[4][1].P, Is.EqualTo(new Point(3, 5)));

//            Assert.That(newVectors[2][2].P, Is.EqualTo(new Point(4, 4)));
//            Assert.That(newVectors[2][3].P, Is.EqualTo(new Point(5, 4)));
//            Assert.That(newVectors[3][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[3][3].P, Is.EqualTo(new Point(5, 5)));
//            Assert.That(newVectors[4][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[4][3].P, Is.EqualTo(new Point(5, 5)));

//            Assert.That(newVectors[2][4].P, Is.EqualTo(new Point(6, 4)));
//            Assert.That(newVectors[2][5].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[3][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[3][5].P, Is.EqualTo(new Point(7, 5)));
//            Assert.That(newVectors[4][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[4][5].P, Is.EqualTo(new Point(7, 5)));
//        }

//        [Test]
//        public void ShouldIncreaseNnfWithEvenSides()
//        {
//            int patchSize = 5;

//            var a1 = new NnfItem(1, 1, 0.0);
//            var a2 = new NnfItem(2, 1, 0.0);
//            var a3 = new NnfItem(3, 1, 0.0);

//            var b1 = new NnfItem(1, 2, 0.0);
//            var b2 = new NnfItem(2, 2, 0.0);
//            var b3 = new NnfItem(3, 2, 0.0);

//            var vectors = new NnfItem[][] { new[] { a1, a2, a3 }, new[] { b1, b2, b3 } };

//            Nnf nnf = new Nnf(vectors, patchSize);
//            var newNnf = _builder.Scale(nnf, 7, 5, _dest, _source, _markup, null);

//            var newVectors = newNnf.NnfData;

//            //1,1 ; 2,1 ; 3,1
//            //1,2 ; 2,2 ; 3,2

//            //0 = 2,2 ; 3,2 ; 4,2 ; 5,2 ; 6,2 ; 7,2 ; 7,2
//            //1 = 2,3 ; 3,3 ; 4,3 ; 5,3 ; 6,3 ; 7,3 ; 7,3
//            //2 = 2,4 ; 3,4 ; 4,4 ; 5,4 ; 6,4 ; 7,4 ; 7,4
//            //3 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5 ; 7,5
//            //4 = 2,5 ; 3,5 ; 4,5 ; 5,5 ; 6,5 ; 7,5 ; 7,5

//            Assert.That(newVectors[0][0].P, Is.EqualTo(new Point(2, 2)));
//            Assert.That(newVectors[0][1].P, Is.EqualTo(new Point(3, 2)));
//            Assert.That(newVectors[1][0].P, Is.EqualTo(new Point(2, 3)));
//            Assert.That(newVectors[1][1].P, Is.EqualTo(new Point(3, 3)));

//            Assert.That(newVectors[0][2].P, Is.EqualTo(new Point(4, 2)));
//            Assert.That(newVectors[0][3].P, Is.EqualTo(new Point(5, 2)));
//            Assert.That(newVectors[1][2].P, Is.EqualTo(new Point(4, 3)));
//            Assert.That(newVectors[1][3].P, Is.EqualTo(new Point(5, 3)));

//            Assert.That(newVectors[0][4].P, Is.EqualTo(new Point(6, 2)));
//            Assert.That(newVectors[0][5].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[0][6].P, Is.EqualTo(new Point(7, 2)));
//            Assert.That(newVectors[1][4].P, Is.EqualTo(new Point(6, 3)));
//            Assert.That(newVectors[1][5].P, Is.EqualTo(new Point(7, 3)));
//            Assert.That(newVectors[1][6].P, Is.EqualTo(new Point(7, 3)));

//            Assert.That(newVectors[2][0].P, Is.EqualTo(new Point(2, 4)));
//            Assert.That(newVectors[2][1].P, Is.EqualTo(new Point(3, 4)));
//            Assert.That(newVectors[3][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[3][1].P, Is.EqualTo(new Point(3, 5)));
//            Assert.That(newVectors[4][0].P, Is.EqualTo(new Point(2, 5)));
//            Assert.That(newVectors[4][1].P, Is.EqualTo(new Point(3, 5)));

//            Assert.That(newVectors[2][2].P, Is.EqualTo(new Point(4, 4)));
//            Assert.That(newVectors[2][3].P, Is.EqualTo(new Point(5, 4)));
//            Assert.That(newVectors[3][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[3][3].P, Is.EqualTo(new Point(5, 5)));
//            Assert.That(newVectors[4][2].P, Is.EqualTo(new Point(4, 5)));
//            Assert.That(newVectors[4][3].P, Is.EqualTo(new Point(5, 5)));

//            Assert.That(newVectors[2][4].P, Is.EqualTo(new Point(6, 4)));
//            Assert.That(newVectors[2][5].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[2][6].P, Is.EqualTo(new Point(7, 4)));
//            Assert.That(newVectors[3][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[3][5].P, Is.EqualTo(new Point(7, 5)));
//            Assert.That(newVectors[3][6].P, Is.EqualTo(new Point(7, 5)));
//            Assert.That(newVectors[4][4].P, Is.EqualTo(new Point(6, 5)));
//            Assert.That(newVectors[4][5].P, Is.EqualTo(new Point(7, 5)));
//            Assert.That(newVectors[4][6].P, Is.EqualTo(new Point(7, 5)));
//        }
//    }
//}
//#endif