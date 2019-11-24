using System.Collections.Generic;
using Shouldly;
using Xunit;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    public class WhenGetLevelsAmount : PyramidTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(7)]
        public void Should_Return_Correct_Amount(byte levels)
        {
            // Arrange
            const int size = 256;
            var images = new List<ZsImage>();
            PopulateImages(images, levels, size, size);
            var areas = new List<Area2D>();
            PopulateAreas(areas, levels, size, size);
            var mappings = new List<Area2DMap>();
            PopulateMappings(mappings, levels, size, size);

            // Act
            var pyramid = new Pyramid(images, areas, mappings);

            // Assert
            pyramid.LevelsAmount.ShouldBe(levels);
        }
    }
}
