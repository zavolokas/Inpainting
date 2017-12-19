using System.Collections.Generic;
using Zavolokas;

namespace SeamCarving
{
    public sealed class Markup
    {
        private const int MinMarkedPixelsAmount = 10;

        public readonly int Width;
        public readonly int Height;
        public readonly Point TopLeft;
        public readonly Point BottomRight;
        internal readonly Point[] PixelPositions;

        internal byte[][] Data;

        //public NewMarkup(Image markupImage)
        //{
        //    this.markupImage = markupImage;
        //}

        //private Markup(byte[][] data, Point[] pixelPositions)
        //{
        //    Width = data[0].Length;
        //    Height = data.Length;

        //    PixelPositions = pixelPositions;
        //    Data = data;
        //}

        public Markup(byte[][] image)
            : this(image, true)
        {
        }

        public Markup(byte[][] image, bool cloneData)
        {
            Width = image[0].Length;
            Height = image.Length;
            var pixelsIndexes = new List<Point>(Width * Height);
            Data = cloneData ? ArrayHelper.CloneJagged(image) : image;

            int left = Width;
            int right = 0;
            int top = Height;
            int bottom = 0;

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (Data[y][x] != 0)
                    {
                        pixelsIndexes.Add(new Point(x, y));

                        if (x < left) left = x;
                        if (x > right) right = x;
                        if (y < top) top = y;
                        if (y > bottom) bottom = y;
                    }
                }
            }

            TopLeft = new Point(left, top);
            BottomRight = new Point(right, bottom);

            PixelPositions = pixelsIndexes.ToArray();
        }

        public bool IsEmpty
        {
            get
            {
                return PixelPositions.Length < MinMarkedPixelsAmount; 
            }
        }

        public Markup Clone()
        {
            //var pointsClone = new Point[PixelPositions.Length];
            //PixelPositions.CopyTo(pointsClone, 0);
            return new Markup(Data, true);
        }
    }
}
