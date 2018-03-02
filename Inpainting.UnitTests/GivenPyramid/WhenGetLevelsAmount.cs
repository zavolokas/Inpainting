using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    [TestFixture]
    public class WhenGetLevelsAmount : PyramidTestBase
    {
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(7)]
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
