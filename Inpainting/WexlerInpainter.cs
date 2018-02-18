using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public static class WexlerInpainter
    {
        private static Result _result;

        public static Result Inpaint(ZsImage image, Area2D removeArea, Nnf nnf, int patchSize, ColorResolver colorResolver, double k)
        {
            // Since the nnf was normalized, the sigma now is normalized as well and it
            // has not any more some specific value.
            //const double naturalLogBase = System.Math.E;
            //const double minusSigmaCube2 = -0.91125;//-2 * sigma * sigma; sigma = 0.675 = 75percentil
            //double NaturalLogBase2 = System.Math.Pow(System.Math.E, 1.0 / minusSigmaCube2);
            const double naturalLogBasePowMinusSigmaCube2 = 0.33373978049163078;

            const double pixelChangeTreshold = 0.00003;
            const double maxSquareDistanceInLab = 32668.1151;

            // Get points' indexes that that needs to be inpainted.
            var pointIndexes = new int[removeArea.ElementsCount];
            removeArea.FillMappedPointsIndexes(pointIndexes, image.Width);

            var patchOffset = (patchSize - 1) / 2;

            var imageWidth = image.Width;
            var nnfWidth = nnf.DstWidth;
            var nnfHeight = nnf.DstHeight;
            var srcWidth = nnf.SourceWidth;
            var srcHeight = nnf.SourceHeight;

            var nnfdata = nnf.GetNnfItems();
            var cmpts = image.NumberOfComponents;

            // Weighted color is color's components values + weight of the color.
            var weightedColors = new double[patchSize * patchSize * (cmpts + 1)];
            var confidence = removeArea.CalculatePointsConfidence(1.5, 1.3);
            var resolvedColor = new double[cmpts];

            var totalDifference = 0.0;
            var changedPixelsAmount = 0;
            var changedPixelsDifference = 0.0;

            for (int ii = 0; ii < pointIndexes.Length; ii++)
            {
                var pointIndex = pointIndexes[ii];

                int pty = pointIndex / imageWidth;
                int ptx = pointIndex % imageWidth;

                int colorsCount = 0;

                //go thru the patch
                for (int y = pty - patchOffset, j = 0; j < patchSize; j++, y++)
                {
                    for (int x = ptx - patchOffset, i = 0; i < patchSize; i++, x++)
                    {
                        if (0 <= x && x < nnfWidth && 0 <= y && y < nnfHeight)
                        {
                            int destPatchPointIndex = y * nnfWidth + x;
                            int srcPatchPointIndex = (int)nnfdata[destPatchPointIndex * 2];

                            int srcPatchPointX = srcPatchPointIndex % nnfWidth;
                            int srcPatchPointY = srcPatchPointIndex / nnfWidth;

                            int srcPixelX = srcPatchPointX - i + patchOffset;
                            int srcPixelY = srcPatchPointY - j + patchOffset;

                            if (0 <= srcPixelX && srcPixelX < srcWidth && 0 <= srcPixelY && srcPixelY < srcHeight)
                            {
                                int srcPixelIndex = srcPixelY * imageWidth + srcPixelX;

                                for (int ci = 0; ci < cmpts; ci++)
                                {
                                    weightedColors[colorsCount * (cmpts + 1) + ci] = image.PixelsData[srcPixelIndex * cmpts + ci];
                                }
                                weightedColors[colorsCount * (cmpts + 1) + cmpts] =
                                    System.Math.Pow(naturalLogBasePowMinusSigmaCube2, nnfdata[destPatchPointIndex * 2 + 1]) *
                                    confidence[ii];

                                colorsCount++;
                            }
                        }
                    }
                }
                // calculate color
                colorResolver.Resolve(weightedColors, 0, colorsCount, cmpts, k, resolvedColor, 0);

                // how different is resolvedColor from the old color?
                var labL = resolvedColor[0] - image.PixelsData[pointIndex * cmpts + 0];
                var labA = resolvedColor[1] - image.PixelsData[pointIndex * cmpts + 1];
                var labB = resolvedColor[2] - image.PixelsData[pointIndex * cmpts + 2];
                var pixelsDiff = (labL*labL + labA*labA + labB*labB) / maxSquareDistanceInLab;
                totalDifference += pixelsDiff;

                if (pixelsDiff > pixelChangeTreshold)
                {
                    changedPixelsAmount ++;
                    changedPixelsDifference += pixelsDiff;
                }

                for (var i = 0; i < cmpts; i++)
                {
                    image.PixelsData[pointIndex * cmpts + i] = resolvedColor[i];
                }
            }

            totalDifference /= pointIndexes.Length;
            _result.TotalDifference = totalDifference / pointIndexes.Length;
            _result.PixelsChangedAmount = changedPixelsAmount;
            _result.PixelsToInpaintAmount = pointIndexes.Length;
            _result.ChangedPixelsDifference = changedPixelsDifference;

            return _result;
        }

        public struct Result
        {
            public int PixelsToInpaintAmount { get; set; }
            public int PixelsChangedAmount { get; set; }
            public double TotalDifference { get; set; }
            public double ChangedPixelsDifference { get; set; }

            public double ChangedPixelsPercent => (double)PixelsChangedAmount / (double)PixelsToInpaintAmount;
        }
    }
}