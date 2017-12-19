namespace Zavolokas.ImageProcessing.Inpainting
{
    public class SimpleColorResolver : ColorResolver
    {
        public override void Resolve(double[] weightedColors, int startIndex, int colorsAmount, byte componentsAmount, double k,
            double[] resultColor, int resultIndex)
        {
            double wieghtsSum = 0.0;
            var endIndex = startIndex + colorsAmount;

            var pos = resultIndex * componentsAmount;
            for (int i = 0; i < componentsAmount; i++)
            {
                resultColor[pos + i] = 0.0;
            }

            for (int i = startIndex; i < endIndex; i++)
            {
                var weightedColorIndex = i * (componentsAmount + 1);

                var weight = weightedColors[weightedColorIndex + componentsAmount];
                wieghtsSum += weight;

                for (int j = 0; j < componentsAmount; j++)
                {
                    resultColor[pos + j] += weightedColors[weightedColorIndex + j] * weight;
                }
            }

            for (int i = 0; i < componentsAmount; i++)
            {
                resultColor[pos + i] /= wieghtsSum;
            }
        }
    }
}