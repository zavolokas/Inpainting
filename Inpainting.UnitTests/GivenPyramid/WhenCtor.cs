using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Shouldly;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    [TestFixture]
    public class WhenCtor
    {
        [Test]
        public void Should_Throw_ArgumentNullException_When_Images_Null()
        {
            // Arrange
            IList<ZsImage> images = null;
            var inpaintAreas = new List<Area2D>();
            PopulateAreas(inpaintAreas, 3, 32, 32);
            var mappings = new List<Area2DMap>();
            PopulateMappings(mappings, 3, 32, 32);

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_InpaintAreas_Null()
        {
            // Arrange
            var images = new List<ZsImage>();
            PopulateImages(images, 3, 32, 32);
            List<Area2D> inpaintAreas = null;
            var mappings = new List<Area2DMap>();
            PopulateMappings(mappings, 3, 32, 32);

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [Test]
        public void Should_Throw_ArgumentNullException_When_Mappings_Null()
        {
            // Arrange
            var images = new List<ZsImage>();
            PopulateImages(images, 3, 32, 32);
            var inpaintAreas = new List<Area2D>();
            PopulateAreas(inpaintAreas, 3, 32, 32);
            List<Area2DMap> mappings = null;

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<ArgumentNullException>();
        }

        [TestCase(32, 32, 16, 16, 8, 7)]
        [TestCase(32, 32, 16, 17, 8, 8)]
        [TestCase(32, 32, 10, 16, 8, 8)]
        [TestCase(32, 32, 17, 16, 8, 8)]
        [TestCase(32, 32, 16, 15, 8, 8)]
        [TestCase(32, 32, 16, 16, 8, 9)]
        [TestCase(32, 32, 16, 16, 9, 8)]
        [TestCase(32, 32, 16, 16, 7, 8)]
        public void Should_Throw_WrongImageSizeException_When_Images_Not_2_Times_Smaller(int w1, int h1, int w2, int h2, int w3, int h3)
        {
            // Arrange
            IList<ZsImage> images = new List<ZsImage>();
            images.Add(CreateImage(w1, h1));
            images.Add(CreateImage(w2, h2));
            images.Add(CreateImage(w3, h3));

            var inpaintAreas = new List<Area2D>();
            PopulateAreas(inpaintAreas, 3, 32, 32);
            var mappings = new List<Area2DMap>();
            PopulateMappings(mappings, 3, 32, 32);

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<WrongImageSizeException>();
        }

        [TestCase(1, 1, 32, 32, 0, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(2, 0, 32, 32, 0, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 1, 1, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 2, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 5, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 0, 16, 16, 2, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 0, 16, 16, 0, 3, 8, 8)]
        public void Should_Throw_WrongMappingSizeException_When_Mapping_Outside_Of_Image(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, int x3, int y3, int w3, int h3)
        {
            // Arrange
            IList<ZsImage> images = new List<ZsImage>();
            PopulateImages(images, 3, 32, 32);
            var inpaintAreas = new List<Area2D>();
            PopulateAreas(inpaintAreas, 3, 32, 32);
            var mappings = new List<Area2DMap>();
            mappings.Add(CreateMapping(x1, y1, w1, h1));
            mappings.Add(CreateMapping(x2, y2, w2, h2));
            mappings.Add(CreateMapping(x3, y3, w3, h3));

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<WrongMappingSizeException>();
        }

        [TestCase(1, 1, 32, 32, 0, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(2, 0, 32, 32, 0, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 1, 1, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 2, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 5, 0, 16, 16, 0, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 0, 16, 16, 2, 0, 8, 8)]
        [TestCase(0, 0, 32, 32, 0, 0, 16, 16, 0, 3, 8, 8)]
        public void Should_Throw_WrongInpaintAreaSizeException_When_Area_Outside_Of_Image(int x1, int y1, int w1, int h1, int x2, int y2, int w2, int h2, int x3, int y3, int w3, int h3)
        {
            // Arrange
            IList<ZsImage> images = new List<ZsImage>();
            PopulateImages(images, 3, 32, 32);
            var inpaintAreas = new List<Area2D>();
            inpaintAreas.Add(Area2D.Create(x1,y1,w1,h1));
            inpaintAreas.Add(Area2D.Create(x2,y2,w2,h2));
            inpaintAreas.Add(Area2D.Create(x3,y3,w3,h3));

            var mappings = new List<Area2DMap>();
            PopulateMappings(mappings, 3, 32, 32);

            Action act = () => { new Pyramid(images, inpaintAreas, mappings); };

            // Act & Assert
            act.ShouldThrow<WrongInpaintAreaSizeException>();
        }

        private void PopulateImages(IList<ZsImage> images, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                var image = CreateImage(w, h);
                images.Add(image);
                w /= 2;
                h /= 2;
            }
        }

        private static ZsImage CreateImage(int w, int h)
        {
            var pixels = Enumerable.Repeat(0.0, w * h * 4).ToArray();
            var image = new ZsImage(pixels, w, h, 4);
            return image;
        }

        private void PopulateAreas(IList<Area2D> areas, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                areas.Add(Area2D.Create(0, 0, w, h));
                w /= 2;
                h /= 2;
            }
        }

        private void PopulateMappings(IList<Area2DMap> mappings, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                var mapping = CreateMapping(0, 0, w, h);
                mappings.Add(mapping);
                w /= 2;
                h /= 2;
            }
        }

        private static Area2DMap CreateMapping(int x, int y, int w, int h)
        {
            var builder = new Area2DMapBuilder();
            builder.InitNewMap(Area2D.Create(x, y, w, h), Area2D.Create(x, y, w, h));
            var mapping = builder.Build();
            return mapping;
        }
    }
}
