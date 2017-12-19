namespace SeamCarving
{
    internal static class ColorsHelper
    {
        public static LabColor RgbToLab(RgbColor rgb)
        {
            return RgbToLab(rgb.R, rgb.G, rgb.B);
        }

        public static LabColor RgbToLab(int rgbR, int rgbG, int rgbB)
        {
            //R from 0 to 255
            //G from 0 to 255
            //B from 0 to 255
            double r = (double)(rgbR) / 255.0;
            double g = (double)(rgbG) / 255.0;
            double b = (double)(rgbB) / 255.0;

            r = (r > 0.04045) ? System.Math.Pow((r + 0.055) / 1.055, 2.4) : (r / 12.92);
            g = (g > 0.04045) ? System.Math.Pow((g + 0.055) / 1.055, 2.4) : (g / 12.92);
            b = (b > 0.04045) ? System.Math.Pow((b + 0.055) / 1.055, 2.4) : (b / 12.92);

            r *= 100;
            g *= 100;
            b *= 100;

            //Observer. = 2°, Illuminant = D65
            double x = r * 0.4124 + g * 0.3576 + b * 0.1805;
            double y = r * 0.2126 + g * 0.7152 + b * 0.0722;
            double z = r * 0.0193 + g * 0.1192 + b * 0.9505;

            x = x / 95.047; // Observer= 2°, Illuminant= D65
            y = y / 100.000;
            z = z / 108.883;

            const double oneThird = 1.0 / 3.0;
            const double ss = 16.0 / 116.0;

            x = (x > 0.008856) ? System.Math.Pow(x, oneThird) : (7.787 * x) + ss;
            y = (y > 0.008856) ? System.Math.Pow(y, oneThird) : (7.787 * y) + ss;
            z = (z > 0.008856) ? System.Math.Pow(z, oneThird) : (7.787 * z) + ss;

            double L = (116.0 * y) - 16.0;
            double A = 500.0 * (x - y);
            double B = 200.0 * (y - z);

            var lab = new LabColor(L, A, B);
            return lab;
        }
    }
}
