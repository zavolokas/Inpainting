using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Zavolokas;

namespace EnergyUpdate
{
    class Program
    {
        static void Main(string[] args)
        {
            int width = 10;
            int height = 4;
            int componentsAmount = 1;
            //Energy map
            double[] energyMap = new double[]
            {
                1, 2, 2, 2, 0, 1, 2, 1, 2, 1,
                3, 2, 2, 1, 2, 0, 2, 1, 3, 2,
                4, 4, 2, 2, 2, 2, 1, 1, 2, 4,
                6, 3, 2, 3, 4, 2, 3, 2, 1, 3
            };

            int[] seq = new int[] { 7, 7, 7, 8 };

            int[] xs = new int[3];
            //FillXs(xs,7,6); // 5,6,8
            //FillXs(xs,7,7); // 6,8,9
            //FillXs(xs,7,8); // 6,8,9

            Update(energyMap, seq, Area2D.Create(0, 0, width, height), width, height, componentsAmount);

            for (int y = 0; y < seq.Length; y++)
            {
                //RemoveElement(energyMap, seq[y], y, width, componentsAmount);

                Console.WriteLine();
                for (int x = 0; x < width; x++)
                {
                    Console.Write($"{energyMap[(y * width + x) * componentsAmount]} ");
                }
            }

            //int x = 8;
            //int y = 0;
            //RemoveElement(energyMap, x, y, width, componentsAmount);
            //for (int i = 0; i < width; i++)
            //{
            //    Console.Write($"{energyMap[(y * width + i) * componentsAmount]} ");
            //}
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
