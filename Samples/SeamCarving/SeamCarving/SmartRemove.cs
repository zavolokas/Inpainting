using System;

namespace SeamCarving
{

    internal class SmartRemove
    {
        private const int Boost = 1000;

        private readonly int _vboost;

        public SmartRemove(int width, int height)
        {
            _vboost = (int)(((double)height / (double)width) * Boost);
        }

        public void CorrectEnergy(int[] energyMap, int[] removeMap, int[] preserveMap, int[] edges, int currentWidth, int originalWidth, int[] col)
        {
            int currentStart = int.MaxValue;
            int currentEnd = int.MinValue;

            for (int y = 0; y < col.Length; y++)
            {
                //correct the x values which need to be checked in the vertical energy map
                int nextStart = int.MaxValue;
                int nextEnd = int.MinValue;

                for (int x = currentStart; x <= currentEnd; x++)
                {
                    int currentPixelEnergy = edges[y * originalWidth + x] + _vboost;
                    int preserveWeight = 0;
                    //if the current pixel is in the remove map then its energy is 0 in order to be removed
                    if (removeMap != null && removeMap[y * originalWidth + x] != 0)
                        currentPixelEnergy = 0;
                    else if (preserveMap != null)
                        //if the pixel is in the Preserve map then it will get the additional energy
                        preserveWeight = preserveMap[y * originalWidth + x];

                    //now we will find the adjacent pixel with the minimal energy
                    int minAdjacentEnergy = int.MaxValue;

                    bool conj = false;
                    if (y > 0 && x > 0)
                    {
                        conj = true;

                        int topLeftPixelEnergy = energyMap[(y - 1) * originalWidth + (x - 1)];
                        if (topLeftPixelEnergy < minAdjacentEnergy)
                            minAdjacentEnergy = topLeftPixelEnergy;
                    }

                    if (minAdjacentEnergy > 0)
                    {
                        if (y > 0)
                        {
                            conj = true;
                            int topPixelEnergy = energyMap[(y - 1) * originalWidth + x];
                            if (topPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topPixelEnergy;
                        }

                        if (minAdjacentEnergy > 0 && y > 0 && x < currentWidth - 1)
                        {
                            conj = true;
                            int topRightPixelEnergy = energyMap[(y - 1) * originalWidth + (x + 1)];
                            if (topRightPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topRightPixelEnergy;
                        }
                    }

                    int min = currentPixelEnergy + preserveWeight + (conj ? minAdjacentEnergy : 0);

                    //if the current value is changed - add its adjancents to the list
                    if (min != energyMap[y * originalWidth + x])
                    {
                        //update the value
                        energyMap[y * originalWidth + x] = min;

                        //add the adjancents
                        for (int j = 0; j < 3; j++)
                        {
                            int xx = x - 1 + j;
                            if (xx >= 0 && xx <= currentWidth - 1)
                            {
                                if (nextEnd < xx)
                                    nextEnd = xx;

                                if (xx < nextStart)
                                    nextStart = xx;
                            }
                        }
                    }
                }

                //take deleted element and add adjanced elements
                int nextX = col[y];
                for (int dx = -1; dx <= 1; dx++)
                {
                    int x = nextX + dx;
                    if (0 <= x && x < currentWidth)
                    {
                        if (nextEnd < x)
                            nextEnd = x;
                        if (x < nextStart)
                            nextStart = x;
                    }
                }
                currentEnd = nextEnd;
                currentStart = nextStart;
            }
        }

        internal void ConvertToVerticalEnergyMap(int[] energyMap, int[] removeMap, int[] preserveMap, int[] edges, int startRow, int currentWidth, int originalWidth, int height)
        {
            for (int y = startRow; y < height; y++)
            {
                for (int x = 0; x < currentWidth; x++)
                {
                    int currentPixelEnergy = edges[y * originalWidth + x] + _vboost;
                    int preserveWeight = 0;
                    //if the current pixel is in the remove map then its energy is 0 in order to be removed
                    if (removeMap[y * originalWidth + x] != 0)
                        currentPixelEnergy = 0;
                    else if (preserveMap!=null)
                        //if the pixel is in the Preserve map then it will get the additional energy
                        preserveWeight = preserveMap[y * originalWidth + x];

                    int minAdjacentEnergy = int.MaxValue;

                    bool conj = false;
                    if (y > 0 && x > 0)
                    {
                        conj = true;

                        int topLeftPixelEnergy = energyMap[(y - 1) * originalWidth + (x - 1)];
                        if (topLeftPixelEnergy < minAdjacentEnergy)
                            minAdjacentEnergy = topLeftPixelEnergy;
                    }

                    if (minAdjacentEnergy > 0)
                    {
                        if (y > 0)
                        {
                            conj = true;
                            int topPixelEnergy = energyMap[(y - 1) * originalWidth + x];
                            if (topPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topPixelEnergy;
                        }

                        if (minAdjacentEnergy > 0 && y > 0 && x < currentWidth - 1)
                        {
                            conj = true;
                            int topRightPixelEnergy = energyMap[(y - 1) * originalWidth + (x + 1)];
                            if (topRightPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topRightPixelEnergy;
                        }
                    }

                    int min = currentPixelEnergy + preserveWeight + (conj ? minAdjacentEnergy : 0);

                    if (min != energyMap[y * originalWidth + x])
                    {
                        //update the value
                        energyMap[y * originalWidth + x] = min;
                    }

                    ////if (conj)
                    //    energyMap[y * originalWidth + x] = currentPixelEnergy + preserveWeight + (conj ? minAdjacentEnergy : 0);
                }
            }
        }

        internal void ConvertToVerticalEnergyMap(int[] energyMap, int[] preserveMap, int[] edges, int startRow, int currentWidth, int originalWidth, int height)
        {
            for (int y = startRow; y < height; y++)
            {
                for (int x = 0; x < currentWidth; x++)
                {
                    int result = edges[y * originalWidth + x] + _vboost;
                    int preserveWeight = 0;
                    if (preserveMap != null)
                        preserveWeight = preserveMap[y * originalWidth + x];

                    int minAdjacentEnergy = int.MaxValue;

                    bool conj = false;
                    if (y > 0 && x > 0)
                    {
                        conj = true;

                        int topLeftPixelEnergy = energyMap[(y - 1) * originalWidth + (x - 1)];
                        if (topLeftPixelEnergy < minAdjacentEnergy)
                            minAdjacentEnergy = topLeftPixelEnergy;
                    }

                    if (minAdjacentEnergy > 0)
                    {
                        if (y > 0)
                        {
                            conj = true;
                            int topPixelEnergy = energyMap[(y - 1) * originalWidth + x];
                            if (topPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topPixelEnergy;
                        }

                        if (minAdjacentEnergy > 0 && y > 0 && x < currentWidth - 1)
                        {
                            conj = true;
                            int topRightPixelEnergy = energyMap[(y - 1) * originalWidth + (x + 1)];
                            if (topRightPixelEnergy < minAdjacentEnergy)
                                minAdjacentEnergy = topRightPixelEnergy;
                        }
                    }

                    //if (conj)
                    energyMap[y * originalWidth + x] = result + preserveWeight + (conj ? minAdjacentEnergy : 0);
                }
            }
        }

        public void FindMinCol(int[] energeyMap, int width, int originalWidth, int height, int[] minCol)
        {
            int minImportance = int.MaxValue;
            int y = height - 1;
            for (int x = 0; x < width; x++)
            {
                int importance = energeyMap[y * originalWidth + x];
                if (importance < minImportance)
                {
                    minImportance = importance;
                    minCol[height - 1] = x;
                }
            }

            int cx = minCol[height - 1];
            for (y = height - 2; y >= 0; y--)
            {
                int l = (cx > 0) ? energeyMap[y * originalWidth + (cx - 1)] : int.MaxValue;
                int c = energeyMap[y * originalWidth + cx];
                int r = (cx < width - 1) ? energeyMap[y * originalWidth + (cx + 1)] : int.MaxValue;
                int min = l < c ? l : c;
                min = r < min ? r : min;
                int pos = min == c ? 0 : (min == l) ? -1 : 1;
                cx += pos;
                minCol[y] = cx;
            }
        }

        public void StreightenCol(int[] edges, int[] col, int originalWidth, int height)
        {
            for (int y = 0; y < height; y++)
            {
                edges[y * originalWidth + col[y]] += 500;
            }
        }

        public bool IsValidVEnergyMap(int[] energyMap, int[] removeMap, int[] protectMap, int[] edges, int width, int originalWidth, int height)
        {
            int[] newMap = new int[edges.Length];
            edges.CopyTo(newMap, 0);

            ConvertToVerticalEnergyMap(newMap, removeMap, protectMap, edges, 0, width, originalWidth, height);

            for (int i = 0; i < newMap.Length; i++)
            {
                if (newMap[i] != energyMap[i])
                    return false;
            }

            return true;
        }

        public void RecalculateCurrentMinCol(int[] energyMap, int width, int originalWidth, int height, int[] minCol)
        {
            int minImportance = int.MaxValue;
            int y = height - 1;
            for (int x = 0; x < width; x++)
            {
                int importance = energyMap[y * originalWidth + x];
                if (importance < minImportance)
                {
                    minImportance = importance;
                    minCol[height - 1] = x;
                }
            }

            int cx = minCol[height - 1];
            for (y = height - 2; y >= 0; y--)
            {
                int l = (cx > 0) ? energyMap[y * originalWidth + (cx - 1)] : int.MaxValue;
                int c = energyMap[y * originalWidth + cx];
                int r = (cx < width - 1) ? energyMap[y * originalWidth + (cx + 1)] : int.MaxValue;

                int min = l < c ? l : c;
                min = r < min ? r : min;

                int pos = min == c ? 0 : (min == l) ? -1 : 1;
                cx += pos;
                minCol[y] = cx;
            }
        }

        public void RemoveCol(RemoveUnit removeUnit, int[][] inputs, int width, int originalWidth, int height, int[] minCol)
        {
            int removedCount = 0;

            int imageCount = inputs.Length;
            for (int y = 0; y < height; y++)
            {
                for (int imageIndex = 0; imageIndex < imageCount; imageIndex++)
                {
                    int x = minCol[y];
                    //current is removeMap?
                    if (imageIndex == 1)
                    {
                        //increase counter when a pixel from a marked object is removed.
                        removedCount += inputs[imageIndex][y * originalWidth + x] != 0 ? 1 : 0;
                    }
                    Array.Copy(inputs[imageIndex], y * originalWidth + (x + 1), inputs[imageIndex], y * originalWidth + x, width - x - 1);
                    inputs[imageIndex][y * originalWidth + (width - 1)] = 0;
                }
            }
            removeUnit.removedPixelsInSessionCount = removedCount;
            removeUnit.removeItarationsCount++;
            removeUnit.Width--;

            removeUnit.faildAttemptCount = removeUnit.removedPixelsInSessionCount > 0
                                            ? 0
                                            : removeUnit.faildAttemptCount + 1;
        }

        public void CloneCol(RemoveUnit removeUnit, int[][] photo, int width, int originalWidth, int height, int[] col)
        {
            int photosCount = photo.Length;
            for (int y = 0; y < height; y++)
            {
                for (int photoIndex = 0; photoIndex < photosCount; photoIndex++)
                {
                    if (photo[photoIndex] == null)
                        continue;

                    int x = col[y];
                    Array.Copy(photo[photoIndex], y * originalWidth + x, photo[photoIndex], y * originalWidth + (x + 1), width - x);
                }
            }

            removeUnit.Width++;
            removeUnit.restoreIteration++;
        }
    }
}
