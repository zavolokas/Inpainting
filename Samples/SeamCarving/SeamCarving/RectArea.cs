namespace SeamCarving
{
    internal class RectArea
    {
        public int Left;
        public int Top;
        public int Width;
        public int Height;
        public int PixAmount;

        public RectArea(int x, int y, int widht, int height)
        {
            Left = x;
            Top = y;

            Width = widht;
            Height = height;
        }
    }
}