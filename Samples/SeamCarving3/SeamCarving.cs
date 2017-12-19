using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
//using BenchmarkDotNet.Attributes;
using Zavolokas.GdiExtensions;
using Zavolokas.Structures;
using Zavolokas.Utils.Processes;

namespace SeamCarving2
{
    public class SeamCarving
    {
  //      [Benchmark]
        public static void Remove()
        {

            //using (var imageBitmap = new Bitmap(@"..\..\..\images\Valve_original.png"))
            using (var imageBitmap = new Bitmap(@"..\..\..\images\t009.jpg"))
            using (var protectBitmap = new Bitmap(@"..\..\..\images\p009.png"))
            using (var removeBitmap = new Bitmap(@"..\..\..\images\m009.png"))
            //using (var imageBitmap = new Bitmap(@"..\..\..\images\sc1.png"))
            {
                var imageArea = Area2D.Create(0, 0, imageBitmap.Width, imageBitmap.Height);
                var protectArea = protectBitmap.ToArea();
                var removeArea = removeBitmap.ToArea();

                // TODO: Sobel in one go
                var filterX = new double[]
                {
                    +1, 0, -1,
                    +2, 0, -2,
                    +1, 0, -1
                };

                var filterY = new double[]
                {
                    +1, +2, +1,
                    0,  0,  0,
                    -1, -2, -1
                };

                var image1 = imageBitmap
                    .ToRgbImage();

                var energyMap = imageBitmap
                    .ToRgbImage()
                    .FromRgbToGray();

                var image2 = energyMap.Clone();

                // Apply Sobol operator
                energyMap.Filter(imageArea, filterX, 3, 3);
                image2.Filter(imageArea, filterY, 3, 3);
                energyMap.MergeImage(image2);

                // adjust energy
                energyMap.NormalizeWeights(energyMap.Height, energyMap.Height * 2, imageArea);
                //energyMap.NormalizeWeights(0, 1, imageArea);
                //energyMap.NormalizeWeights(1, 3, imageArea);

                energyMap.SetComponentsValues(protectArea, new[] { 1.0 }, 0);
                energyMap.SetComponentsValues(removeArea, new[] { 0.0 }, 0);

                // build energy map
                energyMap.ConvertToEnergyMap(imageArea);
                //energyMap.NormalizeEnergyMap(imageArea);

                int[] xes = new int[energyMap.Height];

                // TODO: while not removed
                // TODO: Add component to the energy map or image with the markup
                // TODO: Convert the markup to Area and check whether it was reduced if it is not 3 times in a row - stop.
                for (int i = 0; i < 100; i++)
                {
                    // find min energy sequence
                    FillMinSequence(xes, energyMap, imageArea);
                    // remove max col / row from photo &energy map
                    Update(energyMap.PixelsData, xes, imageArea, image1.Width, image1.Height, 1);

                    //RemoveSequence(xes, energyMap);
                    RemoveSequence(xes, image1);

                    imageArea = Area2D.Create(imageArea.Bound.X, imageArea.Bound.Y, imageArea.Bound.Width - 1, imageArea.Bound.Height);
                }

                // TODO: restor with the protection
                // TODO: restor without the protection

                if (true)
                {
                    image1.FromRgbToBitmap()
                        .SaveTo("..\\..\\out2.png", ImageFormat.Png);

                    energyMap.FromGrayToRgb()
                        .FromRgbToBitmap()
                        .SaveTo("..\\..\\out1.png", ImageFormat.Png)
                        .ShowFile();
                }
            }
        }

        private static void RemoveSequence(int[] xIndecies, ZsImage image)
        {
            if (xIndecies == null)
                throw new ArgumentNullException(nameof(xIndecies));

            var numberOfComponents = image.NumberOfComponents;
            var stride = image.Stride;
            var pixelsData = image.PixelsData;
            var height = image.Height;
            var width = image.Width;

            for (int y = 0; y < height; y++)
            {
                var xStart = y * stride + xIndecies[y] * numberOfComponents;
                var sStart = y * stride + (xIndecies[y] + 1) * numberOfComponents;
                var len = (width - xIndecies[y] - 1) * numberOfComponents;

                Array.Copy(pixelsData, sStart, pixelsData, xStart, len);
            }
        }

        private static unsafe void FillMinSequence(int[] minSequence, ZsImage energyMap, Area2D imageArea)
        {
            if (energyMap == null)
                throw new ArgumentNullException();

            if (!IsRectangle(imageArea))
                throw new ArgumentException("imageArea that has not rectangular shape is not allowed");

            if (energyMap.Height != imageArea.Bound.Height)
                throw new ArgumentException("imageArea should occupay the whole height of the energyMap");

            double[] pixelsData = energyMap.PixelsData;

            int width = energyMap.Width;
            int height = energyMap.Height;
            byte cmpsAmount = energyMap.NumberOfComponents;

            int x = FindMinEnergySequenceStart(energyMap, imageArea);
            minSequence[height - 1] = x;

            var isValudIndex = CreateIndexValidator(imageArea, width);

            fixed (double* pixelsDataP = pixelsData)
            {
                for (int y = height - 2; y >= 0; y--)
                {
                    int tlIndex = y * width + (x - 1);
                    int trIndex = y * width + (x + 1);

                    double tl = x > 1 && isValudIndex(tlIndex) ? *(pixelsDataP + tlIndex * cmpsAmount + 0) : double.MaxValue;
                    double tc = *(pixelsDataP + (y * width + (x + 0)) * cmpsAmount + 0);
                    double tr = x < (width - 1) && isValudIndex(trIndex) ? *(pixelsDataP + trIndex * cmpsAmount + 0) : double.MaxValue;

                    if (tl < tc && tl < tr) x = x - 1;
                    else if (tr < tl && tr < tc) x = x + 1;
                    minSequence[y] = x;
                }
            }
        }

        private static Func<int, bool> CreateIndexValidator(Area2D area, int mappedWidth)
        {
            int[] allowedIndexes = new int[area.ElementsCount];
            area.FillMappedPointsIndexes(allowedIndexes, mappedWidth);
            var allowedIndexesSet = new HashSet<int>(allowedIndexes);
            Func<int, bool> isValudIndex = index => allowedIndexesSet.Contains(index);
            return isValudIndex;
        }

        private static unsafe int FindMinEnergySequenceStart(ZsImage energyMap, Area2D imageArea)
        {
            if (energyMap == null)
                throw new ArgumentNullException();

            if (!IsRectangle(imageArea))
                throw new ArgumentException("imageArea that has not rectangular shape is not allowed");

            if (energyMap.Height != imageArea.Bound.Height)
                throw new ArgumentException("imageArea should occupay the whole height of the energyMap");

            double[] pixelsData = energyMap.PixelsData;

            int width = energyMap.Width;
            byte cmpsAmount = energyMap.NumberOfComponents;

            int startX = imageArea.Bound.X;
            int endX = imageArea.Bound.Width + startX;

            int minEnergyIndex = startX;
            int y = energyMap.Height - 1;
            double currentEnergy;
            double minEnergy = double.MaxValue;

            fixed (double* pixelsDataP = pixelsData)
            {
                for (int x = startX; x < endX; x++)
                {
                    currentEnergy = *(pixelsDataP + (y * width + x) * cmpsAmount + 0);
                    if (currentEnergy < minEnergy)
                    {
                        minEnergy = currentEnergy;
                        minEnergyIndex = x;
                    }
                }
            }

            return minEnergyIndex;
        }

        private static bool IsRectangle(Area2D imageArea)
        {
            return (imageArea.Bound.Width - imageArea.Bound.X) * (imageArea.Bound.Height - imageArea.Bound.Y) == imageArea.ElementsCount;
        }

        private static void Update(double[] energyMap, int[] xsToRemove, Area2D area, int width, int height, int componentsAmount)
        {
            const byte neighboursAmount = 3;
            int[] neighboursX = new int[neighboursAmount];

            var xsToUpdateNow = new Dictionary<int, double>();
            var xsToUpdateLater = new Dictionary<int, double>();

            var isValidIndex = GetIndexValidator(area, width);

            for (int currentY = 0; currentY <= xsToRemove.Length; currentY++)
            {
                // remove the element at xsToRemove[y-1] position from the TopRow if exists
                int topRowY = currentY - 1;
                if (topRowY >= 0)
                {
                    RemoveElement(energyMap, xsToRemove[topRowY], topRowY, width, componentsAmount);
                }

                if (currentY >= height)
                    continue;

                var xToRemove = xsToRemove[currentY];

                if (!xsToUpdateNow.ContainsKey(xToRemove))
                {
                    xsToUpdateNow.Add(xToRemove, energyMap[(currentY * width + xToRemove) * componentsAmount]);
                }

                var nextRowY = currentY + 1;
                var moreRowsBelow = nextRowY < height;
                if (moreRowsBelow)
                {
                    // Collect all the x coordinates from the next row that will might be affected
                    // by the elements that are going to be updated now (xsToUpdateNow)
                    Func<int, bool> isValidX = x => isValidIndex(nextRowY * width + x);
                    FillXsToUpdateLater(xsToUpdateLater, xsToUpdateNow, isValidX);
                    FillValues(xsToUpdateLater, energyMap, width, nextRowY, componentsAmount);

                    // restore initial energy value of the points that will need to be updated later
                    var xs = xsToUpdateLater
                        .Select(kvp => kvp.Key)
                        .OrderBy(v => v).ToArray();

                    for (int xi = 0; xi < xs.Length; xi++)
                    {
                        int x = xs[xi];
                        neighboursX[0] = x - 1;
                        neighboursX[1] = x;
                        neighboursX[2] = x + 1;
                        var minContributer = FindMinContributeValue(neighboursX, energyMap, width, componentsAmount, currentY, isValidIndex);
                        xsToUpdateLater[x] = xsToUpdateLater[x] - minContributer;
                    }
                }

                // Since the impact of the current row to the previous values 
                // is already taken into account, we can update the energy
                Func<int, int> xToAbsIndex = x => (currentY * width + x) * componentsAmount;
                UpdateEnergy(energyMap, xsToUpdateNow, xToAbsIndex);

                // Finally, let's calculate the values 
                if (moreRowsBelow)
                {
                    var xsNotAffected = new List<int>();
                    for (var pairIndex = 0; pairIndex < xsToUpdateLater.Count; pairIndex++)
                    {
                        var pair = xsToUpdateLater.ElementAt(pairIndex);
                        var x = pair.Key;

                        FillXs(neighboursX, xToRemove, x);
                        var minContributeValue = FindMinContributeValue(neighboursX, energyMap, width, componentsAmount, currentY, isValidIndex);

                        // calculate a new value and if it differs from the current one - store it to update
                        var newValue = pair.Value + minContributeValue;
                        var curValue = energyMap[((currentY + 1) * width + x) * componentsAmount];

                        if (curValue == newValue)
                        {
                            xsNotAffected.Add(x);
                        }
                        else
                        {
                            xsToUpdateLater[x] = newValue;
                        }
                    }

                    for (var i = 0; i < xsNotAffected.Count; i++)
                    {
                        xsToUpdateLater.Remove(xsNotAffected[i]);
                    }

                    // swap
                    var tmp = xsToUpdateNow;
                    xsToUpdateNow = xsToUpdateLater;
                    xsToUpdateLater = tmp;
                    xsToUpdateLater.Clear();
                }
            }
        }

        private static Func<int, bool> GetIndexValidator(Area2D area, int width)
        {
            var allowedIndecies = new int[area.ElementsCount];
            area.FillMappedPointsIndexes(allowedIndecies, width);
            var indeciesSet = new HashSet<int>(allowedIndecies);
            Func<int, bool> isValidIndex = (index) => indeciesSet.Contains(index);
            return isValidIndex;
        }

        private static void UpdateEnergy(double[] energyMap, Dictionary<int, double> xsToUpdateNow, Func<int, int> xToAbsIndex)
        {
            var xsToUpdateOrdered = xsToUpdateNow
                .OrderBy(v => v.Key);
            foreach (var pair in xsToUpdateOrdered)
            {
                int index = xToAbsIndex(pair.Key);
                energyMap[index] = pair.Value;
            }
        }

        private static double FindMinContributeValue(int[] nxs, double[] energyMap, int width, int componentsAmount, int y,
            Func<int, bool> isValidIndex)
        {
            double[] contributingValues = new double[3];

            for (int i = 0; i < nxs.Length; i++)
            {
                int nx = nxs[i];
                int bl = y * width + nx;
                if (!isValidIndex(bl))
                {
                    contributingValues[i] = double.MaxValue;
                }
                else
                {
                    contributingValues[i] = energyMap[bl * componentsAmount];
                }
            }
            var minContributer = contributingValues.Min();
            return minContributer;
        }

        private static void FillValues(Dictionary<int, double> xsToUpdateLater, double[] energyMap, int width, int y, int componentsAmount)
        {
            for (int i = 0; i < xsToUpdateLater.Count; i++)
            {
                var pair = xsToUpdateLater.ElementAt(i);
                xsToUpdateLater[pair.Key] = energyMap[(y * width + pair.Key) * componentsAmount];
            }
        }

        private static void FillXsToUpdateLater(Dictionary<int, double> xsToUpdateLater, Dictionary<int, double> xsToUpdateNow, Func<int, bool> isValidX)
        {
            var Xs = xsToUpdateNow
                .Select(kvp => kvp.Key)
                .OrderBy(v => v).ToArray();

            // Collect all the x coordinates from the next row that will might be affected
            // by the elements that are going to be updated now (xsToUpdateNow)
            for (int xi = 0; xi < Xs.Length; xi++)
            {
                for (int offs = -1; offs <= 1; offs++)
                {
                    int cur_x = Xs[xi] + offs;
                    if (!isValidX(cur_x) || xsToUpdateLater.ContainsKey(cur_x))
                        continue;

                    xsToUpdateLater.Add(cur_x, 0.0);
                }
            }
        }

        private static void FillXs(int[] nxs, int xToRemove, int x)
        {
            if (nxs.Length != 3)
                throw new ArgumentException();

            int startX = x - 1;
            int endX = startX + nxs.Length;
            if ((startX < xToRemove && endX < xToRemove) || (startX > xToRemove))
            {
                for (int i = 0, cur_x = startX; i < nxs.Length; i++, cur_x++)
                {
                    nxs[i] = cur_x;
                }
            }
            else
            {
                int cur_x = startX == xToRemove ? startX - 1 : startX;
                for (int i = 0; i < nxs.Length; i++)
                {
                    if (xToRemove == cur_x) cur_x++;
                    nxs[i] = cur_x++;
                }
            }
        }

        private static void RemoveElement(double[] energyMap, int x, int topRowY, int width, int componentsAmount)
        {
            int dstPointIndex = topRowY * width + x;
            int srcPointIndex = dstPointIndex + 1;
            int length = (width - x - 1) * componentsAmount;
            int dstIndex = dstPointIndex * componentsAmount;
            int srcIndex = srcPointIndex * componentsAmount;

            Array.Copy(energyMap, srcIndex, energyMap, dstIndex, length);
        }
    }
}