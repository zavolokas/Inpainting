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
        public void Should_Throw_ImageNotProvidedException_When_No_Image_Was_Added()
        {
            // Arrange
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build(2);

            // Act & Assert
            act.ShouldThrow<ImageNotProvidedException>();
        }

        [TestCase(0)]
        [TestCase(-1)]
        [TestCase(-2)]
        public void Should_Throw_ArgumentOutOfRangeException_When_Levels_Amount_Less_Then_1(byte levelsAmount)
        {
            // Arrange
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
            var image = CreateImage(width, height, 1);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.SetImage(image);
            Action act = () => pyramidBuilder.Build(levelsAmount);

            // Act & Assert
            act.ShouldThrow<WrongImageSizeException>();
        }

        [TestCase(2, 2, 1)]
        [TestCase(11, 11, 1)]
        [TestCase(20, 20, 2)]
        [TestCase(500, 400, 3)]
        [TestCase(256, 128, 8)]
        public void Should_Build_Pyramid_Of_Required_Level_High(int width, int height, byte levelsAmount)
        {
            // Arrange
            var image = CreateImage(width, height, 1);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.SetImage(image);

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
            var image1 = CreateImage(width1, height1, 1);
            var image2 = CreateImage(width2, height2, 1);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.SetImage(image1);
            pyramidBuilder.SetImage(image2);

            // Act 
            var pyramid = pyramidBuilder.Build(levelsAmount);

            // Assert
            pyramid.LevelsAmount.ShouldBe<byte>(levelsAmount);
        }

        public ZsImage CreateImage(int width, int height, byte numberOfComponents)
        {
            var pixels = Enumerable.Repeat(0.0, width * height * numberOfComponents).ToArray();
            return new ZsImage(pixels, width, height, numberOfComponents);
        }
    }
}
