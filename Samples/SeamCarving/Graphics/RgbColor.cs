using System;

namespace SeamCarving
{
    public struct RgbColor
    {
        public readonly byte R;
        public readonly byte G;
        public readonly byte B;

        public readonly bool IsEmpty;
        public static readonly RgbColor Empty = new RgbColor(0, 0, 0, true);

        public RgbColor(byte r, byte g, byte b, bool isEmpty)
        {
            R = r;
            G = g;
            B = b;
            IsEmpty = isEmpty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RgbColor"/> struct.
        /// Make sure that the components are within 0-255 range.
        /// </summary>
        /// <param name="r">The r.</param>
        /// <param name="g">The g.</param>
        /// <param name="b">The b.</param>
        public RgbColor(byte r, byte g, byte b)
        {
#if DEBUG
            if (r < 0) r = 0;
            if (r > 255) r = 255;
            if (g < 0) g = 0;
            if (g > 255) g = 255;
            if (b < 0) b = 0;
            if (b > 255) b = 255;
#endif
            R = r;
            G = g;
            B = b;

            IsEmpty = false;
        }

        public override string ToString()
        {
            return String.Format("({0}, {1}, {2})", R, G, B);
        }

        public const double BiggestDistance = 195075.0;

        public double CalculateSquareDistanceTo(RgbColor color)
        {
            if (IsEmpty || color.IsEmpty)
                return BiggestDistance;

            var dR = R - color.R;
            var dG = G - color.G;
            var dB = B - color.B;
            return dR * dR + dG * dG + dB * dB;
        }
    }
}