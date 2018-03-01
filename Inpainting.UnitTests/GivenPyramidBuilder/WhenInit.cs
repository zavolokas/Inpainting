using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramidBuilder
{
    [TestFixture]
    public class WhenInit
    {
        [Test]
        public void Should_Throw_ArgumentNullException_When_Image_Is_Null()
        {
            // Arrange
            var markupImage = CreateImage(128, 128);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(null, markupImage);
            Action act = () => pyramidBuilder.Build(2);

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Markup_Is_Null()
        {
            // Arrange
            var image = CreateImage(128, 128);
            var pyramidBuilder = new PyramidBuilder();
            pyramidBuilder.Init(image, null);
            Action act = () => pyramidBuilder.Build(2);

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        public ZsImage CreateImage(int width, int height, double val = 1.0)
        {
            var pixels = Enumerable.Repeat(val, width * height * 4).ToArray();
            return new ZsImage(pixels, width, height, 4);
        }
    }
}
