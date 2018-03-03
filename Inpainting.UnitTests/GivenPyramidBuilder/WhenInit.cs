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
            
            Action act = () => pyramidBuilder.Init(null, markupImage); 

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Markup_Is_Null()
        {
            // Arrange
            var image = CreateImage(128, 128);
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Init(image, null);

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(6)]
        public void Should_Throw_WrongImageFormatException_When_Not_Argb_Image(byte componenetsAmount)
        {
            // Arrange
            var image = CreateImage(128, 128, componenetsAmount);
            var markupImage = CreateImage(128, 128, 4);
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Init(image, markupImage);

            // Act & Assert
            act.ShouldThrow<WrongImageFormatException>();
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        [TestCase(6)]
        public void Should_Throw_WrongImageFormatException_When_Not_Argb_Markup(byte componenetsAmount)
        {
            // Arrange
            var image = CreateImage(128, 128, 4);
            var markupImage = CreateImage(128, 128, componenetsAmount); 
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Init(image, markupImage);

            // Act & Assert
            act.ShouldThrow<WrongImageFormatException>();
        }

        [Test]
        public void Should_Not_Throw_When_Images_Correct()
        {
            // Arrange
            var image = CreateImage(128, 128);
            var markupImage = CreateImage(128, 128);
            var pyramidBuilder = new PyramidBuilder();
            Action act = () => pyramidBuilder.Init(image, markupImage);

            // Act & Assert
            act.ShouldNotThrow();
        }

        public ZsImage CreateImage(int width, int height, byte componentsAmount = 4, double val = 1.0)
        {
            var pixels = Enumerable.Repeat(val, width * height * componentsAmount).ToArray();
            return new ZsImage(pixels, width, height, componentsAmount);
        }
    }
}

