using System.Collections.Generic;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace ConsoleWexlerPipeline
{
    class WexlerLevelsData
    {
        public ZsImage CurrentPicture
        {
            get { return Pictures.Peek(); }
        }

        public Area2DMap CurrentMap
        {
            get { return Maps.Peek(); }
        }

        public double[] CurrentConfidenceMap
        {
            get { return ConfidenceMaps.Peek(); }
        }

        public Area2D CurrentRemoveArea
        {
            get { return RemoveAreas.Peek(); }
        }

        public Area2D CurrentPixelsArea
        {
            get { return PixelsAreas.Peek(); }
        }

        public PatchMatchSettings PatchMatchSettings
        {
            get { return PatchMatchSettingsQueue.Peek(); }
        }

        public double K
        {
            get { return KQueue.Peek(); }
        }

        public Nnf Nnf = null;
        public int OriginalImageWidth;
        public int OriginalImageHeight;
        public Nnf NormalizedNnf;
        public Queue<ZsImage> Pictures { get; private set; } = new Queue<ZsImage>();
        public Queue<Area2DMap> Maps { get; private set; } = new Queue<Area2DMap>();

        public Queue<double[]> ConfidenceMaps { get; private set; } = new Queue<double[]>();

        public Queue<Area2D> RemoveAreas { get; private set; } = new Queue<Area2D>();

        public Queue<Area2D> PixelsAreas { get; private set; } = new Queue<Area2D>();

        public Queue<double> KQueue { get; private set; } = new Queue<double>();

        public Queue<PatchMatchSettings> PatchMatchSettingsQueue { get; private set; } = new Queue<PatchMatchSettings>();
        public WexlerAlgorithmSettings Settings { get; private set; } = new WexlerAlgorithmSettings();
        

        public Area2D OriginalRemoveArea;
        public ZsImage OriginalImage;


        public void CreateEmptyNnf()
        {
            var image = CurrentPicture;
            var w = image.Width;
            var h = image.Height;
            Nnf = new Nnf(w, h, w, h, PatchMatchSettings.PatchSize);
        }

        public WexlerLevelsData MakeCopy()
        {
            var copy = new WexlerLevelsData();

            copy.Nnf = Nnf.Clone();
            copy.Pictures = Pictures;
            copy.RemoveAreas = RemoveAreas;
            copy.ConfidenceMaps = ConfidenceMaps;
            //copy.PatchMatchSettings = PatchMatchSettings;
            copy.PatchMatchSettingsQueue = PatchMatchSettingsQueue;
            copy.Settings = Settings;
            copy.OriginalImageHeight = OriginalImageHeight;
            copy.OriginalImageWidth = OriginalImageWidth;
            copy.OriginalImage = OriginalImage;
            copy.OriginalRemoveArea = OriginalRemoveArea;

            copy.Maps = new Queue<Area2DMap>();
            foreach (var map in Maps.ToArray())
            {
                copy.Maps.Enqueue(map);
            }

            copy.KQueue = new Queue<double>();
            foreach (var k in KQueue.ToArray())
            {
                copy.KQueue.Enqueue(k);
            }

            copy.PixelsAreas = new Queue<Area2D>();
            foreach (var pixelsArea in PixelsAreas.ToArray())
            {
                copy.PixelsAreas.Enqueue(pixelsArea);
            }

            return copy;
        }

        public void SubstituteMap(Area2DMap map)
        {
            Maps.Dequeue();
            var maps = Maps.ToArray();
            Maps.Clear();
            Maps.Enqueue(map);

            foreach (var m in maps)
            {
                Maps.Enqueue(m);
            }
        }
    }
}