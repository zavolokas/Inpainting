#if DEBUG
using System.Linq;
using NUnit.Framework;

namespace Zavolokas.ImageProcessing.UnitTests.Tests.Algorithms.ImageComlition.Wexler.WexlerMarkupTests
{
    [TestFixture]
    public class WhenGetPoints
    {
        [Test]
        public void ShouldReturnEmptyCollection()
        {
            var image = new[]{new []{0,0,0,0,0},
                               new []{0,0,0,0,0},
                               new []{0,0,0,0,0},
                               new []{0,0,0,0,0}};

            ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup markup = new ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup(image, 1, 1);

            Assert.That(markup.Points.Count, Is.EqualTo(0));
        }

        [Test]
        public void ShouldHasSameAmountAsPixels()
        {
            var image = new[]{new []{0,0,0,0,0},
                               new []{0,0,0,0,0},
                               new []{0,0,0,0,0},
                               new []{0,0,0,0,0}};

            ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup markup = new ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup(image,1,1);

            Assert.That(markup.Points.Count, Is.EqualTo(markup.PixelsAmount));
        }

        [Test]
        public void ShouldHasSameAmountAsPixels2()
        {
            var image = new[]{new []{0,0,0,0,0},
                               new []{0,1,0,0,0},
                               new []{0,0,0,1,0},
                               new []{0,0,1,0,0}};

            ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup markup = new ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup(image,1,1);

            Assert.That(markup.Points.Count, Is.EqualTo(markup.PixelsAmount));
        }

        [Test]
        public void ShouldContainPointsForEveryOne()
        {
            var image = new[]{new []{0,0,0,0,0},
                               new []{1,1,0,0,0},
                               new []{0,1,1,1,0},
                               new []{0,0,0,1,0}};

            ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup markup = new ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup(image,1,1);

            for (int y = 0; y < image.Length; y++)
            {
                for (int x = 0; x < image[y].Length; x++)
                {
                    if (image[y][x] == 1)
                    {
                        var p = from d in markup.Points
                                where d.X == x && d.Y == y
                                select d;
                        Assert.That(p.Count(), Is.EqualTo(1));
                    }
                }
            }
        }

        [Test]
        public void ShouldNotContainExtraPoints()
        {
            var image = new[]{new []{0,0,0,1,0},
                               new []{1,1,0,1,0},
                               new []{0,1,1,1,0},
                               new []{0,0,0,1,0}};

            ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup markup = new ImageProcessing.Algorithms.ImageComplition.Wexler.WexlerMarkup(image,1,1);

            foreach (var point in markup.Points)
            {
                Assert.That(image[point.Y][point.X], Is.EqualTo(1));
            }
        }
    }
}
#endif