using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramidBuilder
{
    [TestFixture]
    public class WhenBuild
    {
        [Test]
        public void Should_Throw_ImageNotProvidedException_When_No_Image_Was_Added()
        {
            // Arrange
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Build();

            // Act & Assert
            act.ShouldThrow<ImageNotProvidedException>();
        }
    }
}
