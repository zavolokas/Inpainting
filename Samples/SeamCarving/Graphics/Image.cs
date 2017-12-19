using System;

namespace SeamCarving
{
    public abstract class Image : IDisposable
    {
        public abstract void Dispose();

        public int Width;
        public int Height;

        protected bool IsPixelsInit { get; private set; }

        protected Image(int width, int height)
        {
            Width = width;
            Height = height;
        }

        public abstract void FillComponentsFrom(int x, int y, out byte alpha, out byte red, out byte green, out byte blue);

        private LabColor[][] _lab;
        private RgbColor[][] _pixels;

        public RgbColor[][] Pixels
        {
            get
            {
                if (_pixels == null)
                {
                    _pixels = new RgbColor[Height][];

                    for (int y = 0; y < _pixels.Length; y++)
                    {
                        var row = new RgbColor[Width];
                        _pixels[y] = row;
                        for (int x = 0; x < row.Length; x++)
                        {
                            byte alpha;
                            byte red;
                            byte green;
                            byte blue;
                            FillComponentsFrom(x, y, out alpha, out red, out green, out blue);
                            row[x] = alpha == 0 ? RgbColor.Empty : new RgbColor(red, green, blue);
                        }
                    }
                    IsPixelsInit = true;
                }
                return _pixels;
            }
        }

        internal LabColor[][] Lab
        {
            get
            {
                if (_lab == null)
                {
                    _lab = new LabColor[Height][];

                    if (_pixels != null)
                    {
                        for (int y = 0; y < _lab.Length; y++)
                        {
                            var row = new LabColor[Width];
                            _lab[y] = row;
                            for (int x = 0; x < row.Length; x++)
                            {
                                var rgb = _pixels[y][x];
                                row[x] = rgb.IsEmpty ? LabColor.Empty : ColorsHelper.RgbToLab(rgb);
                            }
                        }
                    }
                    else
                    {
                        _pixels = new RgbColor[Height][];

                        for (int y = 0; y < _lab.Length; y++)
                        {
                            var rgbRow = new RgbColor[Width];
                            var labRow = new LabColor[Width];
                            _lab[y] = labRow;
                            _pixels[y] = rgbRow;
                            for (int x = 0; x < labRow.Length; x++)
                            {
                                byte alpha;
                                byte red;
                                byte green;
                                byte blue;
                                FillComponentsFrom(x, y, out alpha, out red, out green, out blue);

                                rgbRow[x] = alpha == 0 ? RgbColor.Empty : new RgbColor(red, green, blue);
                                labRow[x] = alpha == 0 ? LabColor.Empty : ColorsHelper.RgbToLab(red, green, blue);
                            }
                        }
                    }
                }
                return _lab;
            }
        }
    }
}