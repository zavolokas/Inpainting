using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Zavolokas;
using Zavolokas.Arrays;
using Zavolokas.ImageProcessing;

namespace SeamCarving2
{
    public static class Extensions
    {
        public static unsafe void ConvertToEnergyMap(this ZsImage grayImage, Area2D imageArea)
        {
            if (grayImage == null)
                throw new ArgumentNullException();

            var pointIndecies = new int[imageArea.ElementsCount];
            imageArea.FillMappedPointsIndexes(pointIndecies, grayImage.Width);
            var pointIndeciesSet = new HashSet<int>(pointIndecies);

            double[] pixelsData1 = grayImage.PixelsData;

            int width = grayImage.Width;
            int height = grayImage.Height;
            byte cmpsAmount = 1;

            fixed (double* pixelsDataP1 = pixelsData1)
            {
                for (int i = 0; i < pointIndecies.Length; i++)
                {
                    int pointIndex = pointIndecies[i];

                    int y = pointIndex / width;
                    int x = pointIndex % width;

                    // components should be in the range [0.0 , 1.0]
                    int tlPointIndex = (y - 1) * width + (x - 1);
                    int tcPointIndex = (y - 1) * width + x;
                    int trPointIndex = (y - 1) * width + (x + 1);

                    double tl = pointIndeciesSet.Contains(tlPointIndex) ? *(pixelsDataP1 + tlPointIndex * cmpsAmount + 0) : double.MaxValue;
                    double tc = pointIndeciesSet.Contains(tcPointIndex) ? *(pixelsDataP1 + tcPointIndex * cmpsAmount + 0) : double.MaxValue;
                    double tr = pointIndeciesSet.Contains(trPointIndex) ? *(pixelsDataP1 + trPointIndex * cmpsAmount + 0) : double.MaxValue;

                    double minEnergy;
                    if (tr < tl && tr < tc) minEnergy = tr;
                    else if (tl < tc && tl < tr) minEnergy = tl;
                    else minEnergy = tc;

                    if (minEnergy == double.MaxValue) minEnergy = 0.0;

                    int absIndex = pointIndex * cmpsAmount;
                    double weight = *(pixelsDataP1 + absIndex + 0);

                    double totalWeight = weight + minEnergy;

                    *(pixelsDataP1 + absIndex + 0) = totalWeight;
                }
            }
        }

        public static unsafe void NormalizeEnergyMap(this ZsImage greyImage, Area2D imageArea)
        {
            if (greyImage == null)
                throw new ArgumentNullException();

            const byte cmptsAmount = 1;

            void EnergyNormalization(double[] pixelsData1, int width, int height, int[] indeciesToProcess, int firstY, int lastY, Func<int, bool> isValidIndex)
            { 
                fixed (double* pixelsDataP1 = pixelsData1)
                {
                    double maxEnergy;
                    double minEnergy;
                    for (int y = firstY; y <= lastY; y++)
                    {
                        maxEnergy = 0.0;
                        minEnergy = 1.0;
                        for (int x = 0; x < width; x++)
                        {
                            var pointIndex = y * width + x;
                            if (!isValidIndex(pointIndex))
                                continue;

                            int i = pointIndex * cmptsAmount;
                            double energy = *(pixelsDataP1 + i + 0);
                            if (energy > maxEnergy) maxEnergy = energy;
                            if (energy < minEnergy) minEnergy = energy;
                        }

                        var denergy = maxEnergy - minEnergy;
                        if (denergy < 1.0)
                        {
                            denergy = 1.0;
                            minEnergy = 0.0;
                        }

                        for (int x = 0; x < width; x++)
                        {
                            var pointIndex = y * width + x;
                            if (!isValidIndex(pointIndex))
                                continue;

                            int i = pointIndex * cmptsAmount;
                            double energy = *(pixelsDataP1 + i + 0);

                            *(pixelsDataP1 + i + 0) = (energy - minEnergy) / denergy;
                        }
                    }
                }
            }

            double[] pixelsData = greyImage.PixelsData;

            pixelsData.ProcessBlocksInParallel(greyImage.Width, greyImage.Height, EnergyNormalization, imageArea);
        }

        public static unsafe void NormalizeWeights(this ZsImage grayImage, double w1, double w2, Area2D imageArea)
        {
            if (grayImage == null)
                throw new ArgumentNullException();

            const int NotDividableMinAmountElements = 80;
            const byte componentsAmount = 1;

            double[] pixelsData1 = grayImage.PixelsData;

            int pointsAmount = imageArea.ElementsCount;
            var pointIndecies = new int[pointsAmount];
            imageArea.FillMappedPointsIndexes(pointIndecies, grayImage.Width);

            // Decide on how many partitions we should divade the processing
            // of the elements.
            var partsCount = pointsAmount > NotDividableMinAmountElements
                ? Environment.ProcessorCount
                : 1;

            var partSize = (int)(pointsAmount / partsCount);

            Parallel.For(0, partsCount, partIndex =>
            {
                var firstPointIndex = partIndex * partSize;
                var lastPointIndex = firstPointIndex + partSize - 1;
                if (partIndex == partsCount - 1) lastPointIndex = pointsAmount - 1;
                if (lastPointIndex > pointsAmount) lastPointIndex = pointsAmount - 1;

                fixed (double* pixelsDataP1 = pixelsData1)
                {
                    for (int j = lastPointIndex; j >= firstPointIndex; j--)
                    {
                        int absIndex = pointIndecies[j] * componentsAmount;

                        // components should be in the range [0.0 , 1.0]
                        double g = *(pixelsDataP1 + absIndex + 0);
                        g = g > 1.0 ? 1.0 : g < 0.0 ? 0.0 : g;

                        *(pixelsDataP1 + absIndex + 0) = (g + w1) / w2;
                    }
                }
            });
        }

        public static unsafe void MergeImage(this ZsImage grayImage1, ZsImage grayImage2)
        {
            if (grayImage1 == null)
                throw new ArgumentNullException();

            if (grayImage2 == null)
                throw new ArgumentNullException();

            const int NotDividableMinAmountElements = 80;

            double[] pixelsData1 = grayImage1.PixelsData;
            double[] pixelsData2 = grayImage2.PixelsData;

            int pointsAmount = grayImage1.Width * grayImage1.Height;

            // Decide on how many partitions we should divade the processing
            // of the elements.
            var partsCount = pointsAmount > NotDividableMinAmountElements
                ? Environment.ProcessorCount
                : 1;

            var partSize = (int)(pointsAmount / partsCount);

            Parallel.For(0, partsCount, partIndex =>
            {
                var firstPointIndex = partIndex * partSize;
                var lastPointIndex = firstPointIndex + partSize - 1;
                if (partIndex == partsCount - 1) lastPointIndex = pointsAmount - 1;
                if (lastPointIndex > pointsAmount) lastPointIndex = pointsAmount - 1;

                fixed (double* pixelsDataP1 = pixelsData1)
                fixed (double* pixelsDataP2 = pixelsData2)
                {
                    for (int pointIndex = lastPointIndex; pointIndex >= firstPointIndex; pointIndex--)
                    {
                        int i = pointIndex * 1;

                        // components should be in the range [0.0 , 1.0]
                        double g1 = *(pixelsDataP1 + i + 0);
                        double g2 = *(pixelsDataP2 + i + 0);

                        *(pixelsDataP1 + i + 0) = Math.Sqrt(g1 * g1 + g2 * g2);
                    }
                }
            });
        }
    }
}