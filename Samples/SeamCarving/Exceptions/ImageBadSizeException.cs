using System;

namespace SeamCarving
{
    public class ImageBadSizeException : Exception
    {
        public readonly int Width;
        public readonly int Height;

        public ImageBadSizeException(int width, int height)
        {
            Width = width;
            Height = height;
        }
    }
}