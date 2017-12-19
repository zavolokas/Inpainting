
namespace SeamCarving
{
    public class RgbImage : Image
    {
        private readonly RgbColor[][] _rgbColors;

        public RgbImage(RgbColor[][] rgbColors)
            : this(rgbColors, false)
        { }

        public RgbImage(RgbColor[][] rgbColors, bool clone)
            : base(rgbColors[0].Length, rgbColors.Length)
        {
            _rgbColors = clone ? ArrayHelper.CloneJagged(rgbColors) : rgbColors;
        }

        public override void Dispose()
        {

        }

        public override void FillComponentsFrom(int x, int y, out byte alpha, out byte red, out byte green, out byte blue)
        {
            var color = IsPixelsInit ? Pixels[y][x] : _rgbColors[y][x];
            alpha = 255;
            red = color.R;
            green = color.G;
            blue = color.B;
        }
    }
}