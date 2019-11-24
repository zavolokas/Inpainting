using System;
using System.Linq;
using Shouldly;
using Zavolokas.Structures;
using Xunit;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramidBuilder
{
    public class WhenBuild
    {
        [Fact]
        public void Should_Throw_InitializationException_When_No_Image_Was_Added()
        {
            // Arrange
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build(2);

            // Act & Assert
            act.ShouldThrow<InitializationException>();
        }

        [Fact]
        public void Should_Throw_ArgumentOutOfRangeException_When_Levels_Amount_Less_Then_1()
        {
            // Arrange
            byte levelsAmount = 0;
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(500, 400, 5)]
        [InlineData(500, 400, 4)]
        [InlineData(256, 127, 2)]
        [InlineData(256, 128, 9)]
        public void Should_Throw_WrongImageSizeException_When_Cant_Be_Divided_Levels_Amount_Times(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<WrongImageSizeException>();
        }

        [Theory]
        [InlineData(500, 1, 1)]
        [InlineData(1, 127, 1)]
        [InlineData(256, 128, 8)]
        public void Should_Throw_WrongImageSizeException_When_Image_Downscaled_To_1px_On_Any_Side(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width / 2, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<WrongImageSizeException>();
        }

        [Theory]
        [InlineData(500, 400, 3)]
        [InlineData(256, 127, 1)]
        [InlineData(257, 127, 1)]
        [InlineData(256, 128, 7)]
        public void Should_Not_Throw_When_Can_Be_Divided_Levels_Amount_Times(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width / 2, height / 2);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldNotThrow();
        }

        [Theory]
        [InlineData(2, 2, 1)]
        [InlineData(11, 11, 1)]
        [InlineData(20, 20, 2)]
        [InlineData(500, 400, 3)]
        public void Should_Build_Pyramid_Of_Required_Level_High(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width / 2, height / 2);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);

            // Act 
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe<byte>(levelsAmount);
        }

        [Theory]
        [InlineData(500, 400, 256, 400, 5)]
        [InlineData(256, 127, 400, 130, 2)]
        public void Should_Use_Last_Set_Image(int width1, int height1, int width2, int height2, byte levelsAmount)
        {
            // Arrange
            var image1 = CreateImage(width1, height1);
            var image2 = CreateImage(width2, height2);
            var markup1 = CreateImage(width1 / 2, height1 / 2);
            var markup2 = CreateImage(width2 / 2, height2 / 2);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image1, markup1);
            pyramidBuilder.Init(image2, markup2);

            // Act 
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe<byte>(levelsAmount);
        }

        [Fact]
        public void Should_Throw_NoAreaToInpaintException_When_Markup_In_Image_Area_Empty()
        {
            // Arrange
            int width = 128;
            int height = 128;
            byte levelsAmount = 3;

            var image = CreateImage(width, height);
            var markup = Create3pixBiggerMarkupNotEmptyOutsideOfTheImage(width, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<NoAreaToInpaintException>();
        }

        [Fact]
        public void Should_Throw_AreaRemovedException_When_Markup_Covers_Whole_Image()
        {
            // Arrange
            int width = 128;
            int height = 128;
            byte levelsAmount = 3;

            var image = CreateImage(width, height);
            var markup = CreateImage(width, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<AreaRemovedException>();
        }

        [Fact]
        public void Should_Use_Last_Set_Markup()
        {
            // Arrange
            int width = 128;
            int height = 128;
            byte levelsAmount = 3;

            var image = CreateImage(width, height);
            var markup1 = Create3pixBiggerMarkupNotEmptyOutsideOfTheImage(width, height);
            var pyramidBuilder = new PyramidBuilder();

            var markup2 = CreateImage(width / 2, height / 2);

            // Act 
            pyramidBuilder.Init(image, markup1);
            pyramidBuilder.Init(image, markup2);
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe(levelsAmount);
        }

        public ZsImage CreateImage(int width, int height, double val = 1.0)
        {
            var pixels = Enumerable.Repeat(val, width * height * 4).ToArray();
            return new ZsImage(pixels, width, height, 4);
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
