using System;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    [TestFixture]
    public class WhenReduceDestArea
    {
        [Test]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var reduceArea = Area2D.Create(2, 2, 10, 10);
            Assert.Throws<MapIsNotInitializedException>(()=>inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Area_IsNull()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D reduceArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<ArgumentNullException>(() => inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Test]
        public void Should_Throw_EmptyAreaException_When_Area_IsEmpty()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D reduceArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<EmptyAreaException>(()=>inpaintMapBuilder.ReduceDestArea(reduceArea));
        }

        [Test]
        public void Should_Delegate_ReduceDestArea_To_MapBuilder()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var reduceArea = Area2D.Create(2, 2, 10, 10);
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            inpaintMapBuilder.ReduceDestArea(reduceArea);

            mapBuilder.AssertWasCalled(x => x.ReduceDestArea(reduceArea));
        }
    }
}
