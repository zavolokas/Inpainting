using System;
using Moq;
using Shouldly;
using Xunit;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenInpaintMapBuilder
{
    public class WhenAddDonor
    {
        [Fact]
        public void Shoud_Throw_MapIsNotInitializedException_When_Called_Before_InitMap_Call()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            var donorArea = Area2D.Create(3, 3, 5, 5);
            Should.Throw<MapIsNotInitializedException>(() => inpaintMapBuilder.AddDonor(donorArea));
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_When_DonorArea_IsNull()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = null;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Should.Throw<ArgumentNullException>(() => inpaintMapBuilder.AddDonor(donorArea));
        }

        [Fact]
        public void Should_Throw_EmptyAreaException_When_DonorArea_IsEmpty()
        {
            var mock = new Mock<IArea2DMapBuilder>();
            var mapBuilder = mock.Object;

            var inpaintMapBuilder = new InpaintMapBuilder(mapBuilder);

            Area2D donorArea = Area2D.Empty;
            inpaintMapBuilder.InitNewMap(Area2D.Create(0, 0, 15, 15));
            Should.Throw<EmptyAreaException>(() => inpaintMapBuilder.AddDonor(donorArea));
        }
    }
}
