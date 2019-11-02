using System;
using System.Linq;
using Shouldly;
using Xunit;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenLevelsDetector
{
    public class WhenCalculateLevelsAmount
    {
        [Fact]
        public void Should_Throw_ArgumentNullException_When_Image_IsNull()
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = null;
            ZsImage markup = CreateImage(100, 100);
            byte patchSize = 7;

            Action act = () => detector.CalculateLevelsAmount(image, markup, patchSize);

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_When_RemoveMarkup_IsNull()
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = CreateImage(100, 100);
            ZsImage markup = null;
            byte patchSize = 7;

            Action act = () => detector.CalculateLevelsAmount(image, markup, patchSize);

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        public void Should_Throw_ArgumentOutOfRangeException_When_PatchSize_Less_Than_2(byte patchSize)
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = CreateImage(100, 100);
            ZsImage markup = CreateImage(50, 100);

            Action act = () => detector.CalculateLevelsAmount(image, markup, patchSize);

            // Act & Assert
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(200, 150, 200, 150)]
        [InlineData(230, 130, 230, 130)]
        [InlineData(200, 130, 201, 130)]
        [InlineData(200, 130, 200, 131)]
        public void Should_Throw_AreaRemovedException_When_Markup_Covers_Image(int iw, int ih, int mw, int mh)
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = CreateImage(iw, ih);
            ZsImage markup = CreateImage(mw, mh);
            byte patchSize = 7;

            Action act = () => detector.CalculateLevelsAmount(image, markup, patchSize);

            // Act & Assert
            act.ShouldThrow<AreaRemovedException>();
        }

        [Fact]
        public void Should_Return_1_When_Markup_IsEmpty()
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = CreateImage(100, 100);
            ZsImage markup = CreateImage(100, 100, 0.0);
            byte patchSize = 7;

            // Act
            var levelsAmount = detector.CalculateLevelsAmount(image, markup, patchSize);

            // Assert
            levelsAmount.ShouldBe((byte)1);
        }

        [Fact]
        public void Should_Return_1_When_Markup_IsEmpty_Inside_Image_Area()
        {
            // Arrange
            var detector = new PyramidLevelsDetector();
            ZsImage image = CreateImage(100, 100);
            ZsImage markup = Create3pixBiggerMarkupNotEmptyOutsideOfTheImage(100, 100);
            byte patchSize = 7;

            // Act
            var levelsAmount = detector.CalculateLevelsAmount(image, markup, patchSize);

            // Assert
            levelsAmount.ShouldBe((byte)1);
        }

        private static ZsImage CreateImage(int w, int h, double vals = 1.0)
        {
            var pixels = Enumerable.Repeat(vals, w * h * 4).ToArray();
            var image = new ZsImage(pixels, w, h, 4);
            return image;
        }

        private static ZsImage Create3pixBiggerMarkupNotEmptyOutsideOfTheImage(int imageWidth, int imageHeight)
        {
            int mw = imageWidth + 3;
            int mh = imageHeight + 3;
            var pixels = Enumerable.Repeat<double>(0.0, mw * mh * 4).ToArray();
            var x1 = imageWidth + 0;
            var y1 = imageHeight + 0;

            var x2 = imageWidth + 1;
            var y2 = imageHeight + 1;

            var x3 = imageWidth + 2;
            var y3 = imageHeight + 2;

            pixels[(y1 * mw + x1) * 4 + 0] = 1.0;
            pixels[(y1 * mw + x1) * 4 + 1] = 1.0;
            pixels[(y1 * mw + x1) * 4 + 2] = 1.0;
            pixels[(y2 * mw + x2) * 4 + 0] = 1.0;
            pixels[(y2 * mw + x2) * 4 + 1] = 1.0;
            pixels[(y2 * mw + x2) * 4 + 2] = 1.0;
            pixels[(y3 * mw + x3) * 4 + 0] = 1.0;
            pixels[(y3 * mw + x3) * 4 + 1] = 1.0;
            pixels[(y3 * mw + x3) * 4 + 2] = 1.0;
            var markup = new ZsImage(pixels, mw, mh, 4);
            return markup;
        }
    }
}
