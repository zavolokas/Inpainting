//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using NUnit.Framework;
//using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PatchMatch;
//using Zavolokas.ImageProcessing.Primitives;

//namespace Tests.Algorithms.ImageComlition.PatchMatch.NnfTests
//{
//    [TestFixture]
//    public class WhenGetAbsoulutePosition
//    {
//        [Test]
//        public void ShouldCalculate1()
//        {
//            int patchSize = 3;
//            var vectors = new NnfItem[10][];

//            int x = 3;
//            int y = 5;

//            var pos = new Point(10,4);

//            vectors[y] = new NnfItem[10];
//            vectors[y][x] = new NnfItem(pos, 104);

//            Nnf nnf = new Nnf(vectors, patchSize);

//            Assert.That(nnf.NnfData[y][x].P.Equals(pos), Is.True);
//        }

//        [Test]
//        public void ShouldCalculate2()
//        {
//            int patchSize = 3;
//            var vectors = new NnfItem[10][];

//            int x = 7;
//            int y = 2;

//            var pos = new Point(8, 13);

//            vectors[y] = new NnfItem[10];
//            vectors[y][x] = new NnfItem(pos, 104);

//            Nnf nnf = new Nnf(vectors, patchSize);

//            Assert.That(nnf.NnfData[y][x].P.Equals(pos), Is.True);
//        }
//    }
//}
