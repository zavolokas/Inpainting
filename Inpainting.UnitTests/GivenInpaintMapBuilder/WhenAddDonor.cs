using System;
using NUnit.Framework;
using Rhino.Mocks;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    [TestFixture]
    public class WhenAddDonor
    {
        [Test]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var donorArea = Area2D.Create(3, 3, 5, 5);
            Assert.Throws<MapIsNotInitializedException>(()=>inpaintMapBuilder.AddDonor(donorArea));
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_DonorArea_IsNull()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<ArgumentNullException>(() => inpaintMapBuilder.AddDonor(donorArea));
        }

        [Test]
        public void Should_Throw_EmptyAreaException_When_DonorArea_IsEmpty()
        {
            var mocks = new MockRepository();
            var mapBuilder = mocks.Stub<IArea2DMapBuilder>();

            mocks.ReplayAll();

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<EmptyAreaException>(()=>inpaintMapBuilder.AddDonor(donorArea));
        }
    }
}
