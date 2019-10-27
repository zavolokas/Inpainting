using System;
using Moq;
using NUnit.Framework;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    [TestFixture]
    public class WhenInitNewMap
    {
        [Test]
        public void Should_Throw_ArgumentNullException_When_ImageArea_Is_Null()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintingMapBuilder = new InpaintMapBuilder(mapBuilder);
            Area2D area = null;
            Assert.Throws<ArgumentNullException>(() => inpaintingMapBuilder.InitNewMap(area));
        }

        [Test]
        public void Should_Throw_EmptyAreaException_When_Area_IsEmpty()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintingMapBuilder = new InpaintMapBuilder(mapBuilder);
            Area2D area = Area2D.Empty;
            Assert.Throws<EmptyAreaException>(() => inpaintingMapBuilder.InitNewMap(area));
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Mapping_Is_Null()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintingMapBuilder = new InpaintMapBuilder(mapBuilder);
            IAreasMapping mapping = null;
            Assert.Throws<ArgumentNullException>(() => inpaintingMapBuilder.InitNewMap(mapping));
        }

        [Test]
        public void Should_Delegate_Map_Initialization_To_MapBuilder_When_Initialized_With_ImageArea()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintingMapBuilder = new InpaintMapBuilder(mapBuilder);
            Area2D area = Area2D.Create(0, 0, 20, 20);

            inpaintingMapBuilder.InitNewMap(area);

            mock.Verify(x=>x.InitNewMap(area, area));
        }
    }
}
