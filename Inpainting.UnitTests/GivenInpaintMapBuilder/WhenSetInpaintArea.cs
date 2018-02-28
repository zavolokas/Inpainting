using System;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    [TestFixture]
    public class WhenSetInpaintArea
    {
        [Test]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var inpaintArea = Area2D.Create(3, 3, 5, 5);
            Assert.Throws<MapIsNotInitializedException>(()=>inpaintMapBuilder.SetInpaintArea(inpaintArea));
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_InpaintArea_IsNull()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D inpaintArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<ArgumentNullException>(() => inpaintMapBuilder.SetInpaintArea(inpaintArea));
        }

        [Test]
        public void Should_Throw_EmptyAreaException_When_InpaintArea_IsEmpty()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D inpaintArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<EmptyAreaException>(()=>inpaintMapBuilder.SetInpaintArea(inpaintArea));
        }

        [Test]
        public void Should_Delegate_SetInpaintArea_To_MapBuilder()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var inpaintArea = Area2D.Create(3, 3, 5, 5);
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            inpaintMapBuilder.SetInpaintArea(inpaintArea);

            mapBuilder.AssertWasCalled(b => b.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x=>x.IsSameAs(inpaintArea))));
        }

        [Test]
        public void InpaintArea_Should_Be_Adjusted_To_Source_Area()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var inpaintArea = Area2D.Create(-3, -3, 10, 10);
            var imageArea = Area2D.Create(0, 0, 15, 15);
            inpaintMapBuilder.InitNewMap(imageArea);
            inpaintMapBuilder.SetInpaintArea(inpaintArea);

            var realArea = imageArea.Intersect(inpaintArea);

            mapBuilder.AssertWasCalled(b => b.SetIgnoredSourcedArea(Arg<Area2D>.Matches(x => x.IsSameAs(realArea))));
        }
    }
}
