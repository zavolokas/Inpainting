using System;
using System.Collections.Generic;
using Shouldly;
using Zavolokas.Structures;
using Xunit;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    public class WhenGetInpaintArea : PyramidTestBase
    {
        [Theory]
        [InlineData(1, 1)]
        [InlineData(1, 2)]
        [InlineData(2, 2)]
        [InlineData(2, 5)]
        [InlineData(3, 3)]
        [InlineData(3, 4)]
        [InlineData(4, 4)]
        public void Should_Throw_ArgumentOutOfRangeException_When_LevelIndex_Greater_Then_LevelsAmount(byte levelsAmount, byte levelIndex)
        {
            // Arrange
            const int imageSize = 64;
            var pyramid = CreatePyramid(imageSize, imageSize, levelsAmount);

            Action act = () => pyramid.GetInpaintArea(levelIndex);

            // Act & Assert
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Theory]
        [InlineData(3, 1)]
        [InlineData(3, 0)]
        [InlineData(3, 2)]
        [InlineData(2, 0)]
        [InlineData(2, 1)]
        [InlineData(1, 0)]
        [InlineData(5, 4)]
        [InlineData(5, 3)]
        public void Should_Return_Area2D_From_Proper_Level(byte levelsAmount, byte levelIndex)
        {
            // Arrange
            const int size = 128;
            var images = new List<ZsImage>(levelsAmount);
            PopulateImages(images, levelsAmount, size, size);
            var areas = new List<Area2D>(levelsAmount);
            PopulateAreas(areas, levelsAmount, size, size);
            var mappings = new List<Area2DMap>(levelsAmount);
            PopulateMappings(mappings, levelsAmount, size, size);

            var pyramid = new Pyramid(images, areas, mappings);

            // Act
            var inpaintArea = pyramid.GetInpaintArea(levelIndex);

            // Assert
            inpaintArea.ShouldBe(areas[levelsAmount - 1 - levelIndex]);
        }
    }
}
