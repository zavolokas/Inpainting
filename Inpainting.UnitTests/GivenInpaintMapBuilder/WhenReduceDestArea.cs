using System;
using Moq;
using Shouldly;
using Xunit;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    public class WhenReduceDestArea
    {
        [Fact]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var reduceArea = Area2D.Create(2, 2, 10, 10);
            Should.Throw<MapIsNotInitializedException>(() => inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_When_Area_IsNull()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D reduceArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Should.Throw<ArgumentNullException>(() => inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Fact]
        public void Should_Throw_EmptyAreaException_When_Area_IsEmpty()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D reduceArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Should.Throw<EmptyAreaException>(() => inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Fact]
        public void Should_Delegate_ReduceDestArea_To_MapBuilder()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var reduceArea = Area2D.Create(2, 2, 10, 10);
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            inpaintMapBuilder.ReduceDestArea(reduceArea);

            mock.Verify(x => x.ReduceDestArea(reduceArea, false));
        }
    }
}
