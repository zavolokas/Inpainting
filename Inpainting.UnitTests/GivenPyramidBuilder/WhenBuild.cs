using System;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramidBuilder
{
    [TestFixture]
    public class WhenBuild
    {
        [Test]
        public void Should_Throw_InitializationException_When_No_Image_Was_Added()
        {
            // Arrange
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build(2);

            // Act & Assert
            act.ShouldThrow<InitializationException>();
        }

        [Test]
        public void Should_Throw_ArgumentOutOfRangeException_When_Levels_Amount_Less_Then_1()
        {
            // Arrange
            byte levelsAmount = 0;
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestCase(500, 400, 5)]
        [TestCase(500, 400, 4)]
        [TestCase(256, 127, 2)]
        [TestCase(256, 128, 9)]
        [TestCase(256, 128, 9)]
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

        [TestCase(500, 400, 3)]
        [TestCase(256, 127, 1)]
        [TestCase(257, 127, 1)]
        [TestCase(256, 128, 8)]
        public void Should_Not_Throw_When_Can_Be_Divided_Levels_Amount_Times(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldNotThrow();
        }

        [TestCase(2, 2, 1)]
        [TestCase(11, 11, 1)]
        [TestCase(20, 20, 2)]
        [TestCase(500, 400, 3)]
        [TestCase(256, 128, 8)]
        public void Should_Build_Pyramid_Of_Required_Level_High(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height);
            var markup = CreateImage(width, height);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);

            // Act 
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe<byte>(levelsAmount);
        }

        [TestCase(500, 400, 256, 400, 5)]
        [TestCase(256, 127, 400, 130, 2)]
        public void Should_Use_Last_Set_Image(int width1, int height1, int width2, int height2, byte levelsAmount)
        {
            // Arrange
            var image1 = CreateImage(width1, height1);
            var image2 = CreateImage(width2, height2);
            var markup1 = CreateImage(width1, height1);
            var markup2 = CreateImage(width2, height2);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image1, markup1);
            pyramidBuilder.Init(image2, markup2);

            // Act 
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe<byte>(levelsAmount);
        }

        [Test]
        public void Should_Throw_NoAreaToInpaintException_When_Markup_In_Image_Area_Empty()
        {
            // Arrange
            int width = 128;
            int height = 128;
            byte levelsAmount = 3;

            var image = CreateImage(width, height);
            int mw = width + 3;
            int mh = height + 3;
            var pixels = Enumerable.Repeat<double>(0.0, mw * mh * 4).ToArray();
            var x1 = width + 0;
            var y1 = height + 0;

            var x2 = width + 1;
            var y2 = height + 1;

            var x3 = width + 2;
            var y3 = height + 2;

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
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, markup);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<NoAreaToInpaintException>();
        }

        [Test]
        public void Should_Use_Last_Set_Markup()
        {
            // Arrange
            int width = 128;
            int height = 128;
            byte levelsAmount = 3;

            var image = CreateImage(width, height);
            int mw = width + 3;
            int mh = height + 3;
            var pixels = Enumerable.Repeat<double>(0.0, mw * mh).ToArray();
            pixels[(height + 0) * mw * 4 + width * 4 + 0] = 1.0;
            pixels[(height + 0) * mw * 4 + width * 4 + 1] = 1.0;
            pixels[(height + 0) * mw * 4 + width * 4 + 2] = 1.0;
            pixels[(height + 1) * mw * 4 + width * 4 + 0] = 1.0;
            pixels[(height + 1) * mw * 4 + width * 4 + 1] = 1.0;
            pixels[(height + 1) * mw * 4 + width * 4 + 2] = 1.0;
            pixels[(height + 2) * mw * 4 + width * 4 + 0] = 1.0;
            pixels[(height + 2) * mw * 4 + width * 4 + 1] = 1.0;
            pixels[(height + 2) * mw * 4 + width * 4 + 2] = 1.0;
            var markup1 = new ZsImage(pixels, mw, mh, 4);
            var pyramidBuilder = new PyramidBuilder();

            var markup2 = CreateImage(width, height);

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
    }
}
