using System;
using System.Linq;

// Accord doesn't have a support for .NET Standard
//using Accord.MachineLearning;
//using Accord.Statistics.Distributions.DensityKernels;

namespace Zavolokas.ImageProcessing.Inpainting
{
    [Obsolete("Use SimpleColorResolver", true)]
    public class MeanShiftColorResolver : ColorResolver
    {
        private readonly ColorResolver _colorResolver;
        public MeanShiftColorResolver()
        {
            _colorResolver = new SimpleColorResolver();
        }

        public override void Resolve(double[] weightedColors, int startIndex, int colorsAmount, byte componentsAmount, double k,
            double[] resultColor, int resultIndex)
        {
            throw new NotImplementedException();
            //var els = componentsAmount + 1;
            //var offs = startIndex * els;

            //var colors = weightedColors
            //    .Skip(offs)
            //    .Take(colorsAmount * els)
            //    .ToArray();

            //var observations = new double[colorsAmount][];
            //for (int colorIndex = 0; colorIndex < colorsAmount; colorIndex++)
            //{
            //    var weightedColor = new double[els];
            //    observations[colorIndex] = weightedColor;

            //    for (int i = 0; i < els; i++)
            //    {
            //        weightedColor[i] = weightedColors[offs + colorIndex * els + i];
            //    }
            //}

            //// Create a uniform kernel density function
            //UniformKernel kernel = new UniformKernel();

            ////calculate sigma
            //double sigma = CalculateSigma(colors, (byte)(componentsAmount + 1));

            ////calc bandwidth
            //double bandwidth = k * sigma;

            //// Create a new Mean-Shift algorithm for 4 dimensional samples
            //MeanShift meanShift = new MeanShift(kernel: kernel, bandwidth: bandwidth);

            //// Compute the algorithm, retrieving an integer array
            ////  containing the labels for each of the observations
            //int[] labels = meanShift.Compute(observations);
            //var mostCommonColor = labels
            //    .GroupBy(item => item)
            //    .OrderByDescending(gg => gg.Count())
            //    .Select(gg => gg.Key)
            //    .First();

            //var labeledWeightedColors = labels
            //    .Where(l => l == mostCommonColor)
            //    .SelectMany((label, index) => observations[index])
            //    .ToArray();


            //_colorResolver.Resolve(labeledWeightedColors, 0, labeledWeightedColors.Length / els, componentsAmount, k, resultColor, resultIndex);
        }

        private static double CalculateSigma(double[] info, byte cmp)
        {
            //calculate mean
            var mean = new double[cmp];
            int colorsAmount = info.Length;
            for (int index = 0; index < info.Length; index += cmp)
            {
                for (int j = 0; j < cmp; j++)
                {
                    mean[j] += info[index + j];
                }
            }

            mean = mean
                .Select(a => a / colorsAmount)
                .ToArray();

            //calculate sigma
            //calculate sum of square distances to mean
            double sumOfSquareDistances = 0.0;
            for (int index = 0; index < info.Length; index += cmp)
            {
                for (int i = 0; i < cmp; i++)
                {
                    double d = mean[0] - info[index + i];
                    sumOfSquareDistances += d * d;
                }
            }

            return System.Math.Sqrt(sumOfSquareDistances / (colorsAmount - 1));
        }
    }
}