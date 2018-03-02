using System;
using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    [TestFixture]
    public class WhenGetMapping : PyramidTestBase
    {
        [TestCase(1, 1)]
        [TestCase(1, 2)]
        [TestCase(2, 2)]
        [TestCase(2, 5)]
        [TestCase(3, 3)]
        [TestCase(3, 4)]
        [TestCase(4, 4)]
        public void Should_Throw_ArgumentOutOfRangeException_When_LevelIndex_Greater_Then_LevelsAmount(byte levelsAmount, byte levelIndex)
        {
            // Arrange
            const int imageSize = 64;
            var pyramid = CreatePyramid(imageSize, imageSize, levelsAmount);

            Action act = () => pyramid.GetMapping(levelIndex);

            // Act & Assert
            act.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [TestCase(3, 1)]
        [TestCase(3, 0)]
        [TestCase(3, 2)]
        [TestCase(2, 0)]
        [TestCase(2, 1)]
        [TestCase(1, 0)]
        [TestCase(5, 4)]
        [TestCase(5, 3)]
        public void Should_Return_Mapping_From_Proper_Level(byte levelsAmount, byte levelIndex)
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
            var mapping = pyramid.GetMapping(levelIndex);

            // Assert
            mapping.ShouldBe(mappings[levelIndex]);
        }
    }
}
