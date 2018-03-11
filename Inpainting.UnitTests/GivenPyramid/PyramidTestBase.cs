using System.Collections.Generic;
using System.Linq;
using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting.UnitTests.GivenPyramid
{
    public class PyramidTestBase
    {
        internal Pyramid CreatePyramid(int width, int height, byte levelsAmount)
        {
            IList<ZsImage> images = new List<ZsImage>();
            PopulateImages(images, levelsAmount, width, height);
            IList<Area2D> areas = new List<Area2D>();
            PopulateAreas(areas, levelsAmount, width, height);
            IList<Area2DMap> mappings = new List<Area2DMap>();
            PopulateMappings(mappings, levelsAmount, width, height);
            var pyramid = new Pyramid(images, areas, mappings);
            return pyramid;
        }

        protected void PopulateImages(IList<ZsImage> images, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                var image = CreateImage(w, h);
                images.Add(image);
                w /= 2;
                h /= 2;
            }
        }

        protected static ZsImage CreateImage(int w, int h)
        {
            var pixels = Enumerable.Repeat(0.0, w * h * 4).ToArray();
            var image = new ZsImage(pixels, w, h, 4);
            return image;
        }

        protected void PopulateAreas(IList<Area2D> areas, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                areas.Add(Area2D.Create(0, 0, w, h));
                w /= 2;
                h /= 2;
            }
        }

        protected void PopulateMappings(IList<Area2DMap> mappings, byte levels, int w, int h)
        {
            for (int i = 0; i < levels; i++)
            {
                var mapping = CreateMapping(0, 0, w, h);
                mappings.Add(mapping);
                w /= 2;
                h /= 2;
            }
        }

        protected static Area2DMap CreateMapping(int x, int y, int w, int h)
        {
            var builder = new Area2DMapBuilder();
            builder.InitNewMap(Area2D.Create(x, y, w, h), Area2D.Create(x, y, w, h));
            var mapping = builder.Build();
            return mapping;
        }
    }
}