using System;
using Moq;
using NUnit.Framework;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    [TestFixture]
    public class WhenAddDonor
    {
        [Test]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var donorArea = Area2D.Create(3, 3, 5, 5);
            Assert.Throws<MapIsNotInitializedException>(()=>inpaintMapBuilder.AddDonor(donorArea));
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_DonorArea_IsNull()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<ArgumentNullException>(() => inpaintMapBuilder.AddDonor(donorArea));
        }

        [Test]
        public void Should_Throw_EmptyAreaException_When_DonorArea_IsEmpty()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Assert.Throws<EmptyAreaException>(()=>inpaintMapBuilder.AddDonor(donorArea));
        }
    }
}
