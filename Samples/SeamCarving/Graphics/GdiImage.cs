using System;
using System.Drawing;
using System.Drawing.Imaging;

namespace SeamCarving
{
    public sealed class GdiImage : Image
    {
        private bool _disposed;
        private Bitmap _bitmap;
        private byte[][][] _components;

        public Bitmap Bitmap
        {
            get
            {
                if (_bitmap == null)
                    _bitmap = CreateBitmap();
                return _bitmap;
            }
        }

        public GdiImage(Bitmap bitmap)
            : base(bitmap.Width, bitmap.Height)
        {
            _bitmap = bitmap;
            init(bitmap);
        }

        private unsafe void init(Bitmap bitmap)
        {
            _components = new byte[4][][];
            _components[3] = new byte[Height][];
            _components[2] = new byte[Height][];
            _components[1] = new byte[Height][];
            _components[0] = new byte[Height][];

            BitmapData bd = bitmap.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);
            try
            {
                byte* curpos;
                for (int h = 0; h < Height; h++)
                {
                    _components[3][h] = new byte[Width];
                    _components[2][h] = new byte[Width];
                    _components[1][h] = new byte[Width];
                    _components[0][h] = new byte[Width];

                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < Width; w++)
                    {
                        _components[3][h][w] = *(curpos++);
                        _components[2][h][w] = *(curpos++);
                        _components[1][h][w] = *(curpos++);
                        _components[0][h][w] = *(curpos++); //(_components[1][h][w] > 0 || _components[2][h][w] > 0 || _components[3][h][w] > 0) ? (byte)255 : (byte)0;
                    }
                }
            }
            finally
            {
                bitmap.UnlockBits(bd);
            }
        }

        public GdiImage(Image image)
            : base(image.Width, image.Height)
        {
            const int componentsAmout = 4;
            _components = new byte[componentsAmout][][];
            _components[3] = new byte[Height][];
            _components[2] = new byte[Height][];
            _components[1] = new byte[Height][];
            _components[0] = new byte[Height][];

            RgbColor[] colorsRow;
            RgbColor color;

            for (int y = 0; y < Height; y++)
            {
                colorsRow = image.Pixels[y];
                _components[0][y] = new byte[Width];
                _components[1][y] = new byte[Width];
                _components[2][y] = new byte[Width];
                _components[3][y] = new byte[Width];

                for (int x = 0; x < Width; x++)
                {
                    //byte a, r, g, b;
                    //image.FillComponentsFrom(x, y, out a, out r, out g, out b);
                    color = colorsRow[x];
                    _components[0][y][x] = (byte)(color.IsEmpty ? 0 : 255);
                    _components[1][y][x] = color.R;
                    _components[2][y][x] = color.G;
                    _components[3][y][x] = color.B;
                }
            }
        }

        public GdiImage(byte[] binaryData, int width, int height)
            : base(width, height)
        {

            for (int i = 0; i < binaryData.Length; i++)
            {
                binaryData[i] *= 255;
            }

            const int componentsAmout = 4;
            _components = new byte[componentsAmout][][];

            for (int cIndex = 0; cIndex < componentsAmout; cIndex++)
            {
                _components[cIndex] = new byte[Height][];
                for (int y = 0; y < Height; y++)
                {
                    _components[cIndex][y] = new byte[Width];
                    if (cIndex == 0)
                        Array.Copy(binaryData, width * y, _components[cIndex][y], 0, width);
                }
            }
        }

        public override void FillComponentsFrom(int x, int y, out byte alpha, out byte red, out byte green, out byte blue)
        {
            alpha = _components[0][y][x];
            red = _components[1][y][x];
            green = _components[2][y][x];
            blue = _components[3][y][x];
        }

        private unsafe Bitmap CreateBitmap()
        {
            Bitmap result = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);

            BitmapData bd = result.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            try
            {
                byte* curpos;
                for (int h = 0; h < Height; h++)
                {
                    curpos = ((byte*)bd.Scan0) + h * bd.Stride;
                    for (int w = 0; w < Width; w++)
                    {
                        *(curpos++) = _components[3][h][w];
                        *(curpos++) = _components[2][h][w];
                        *(curpos++) = _components[1][h][w];
                        *(curpos++) = _components[0][h][w];
                    }
                }
            }
            finally
            {
                result.UnlockBits(bd);
            }

            return result;
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources.
                    if (_bitmap != null)
                        _bitmap.Dispose();
                }

                // There are no unmanaged resources to release, but
                // if we add them, they need to be released here.
            }
            _disposed = true;
        }
    }
}