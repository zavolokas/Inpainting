//using System;

//namespace SeamCarving
//{
//    internal partial class SmartRemove
//    {
//        public void CorrectEnergy(int[] energyMap, int[] removeMap, int[] preserveMap, int[] edges, int currentWidth, int originalWidth, int[] col)
//        {
//            int currentStart = int.MaxValue;
//            int currentEnd = int.MinValue;

//            for (int currentY = 0; currentY < col.Length; currentY++)
//            {
//                //correct the x values which need to be checked in the vertical energy map
//                int nextStart = int.MaxValue;
//                int nextEnd = int.MinValue;

//                for (int currentX = currentStart; currentX <= currentEnd; currentX++)
//                {
//                    int currentPixelEnergy = edges[currentY * originalWidth + currentX] + _vboost;
//                    int preserveWeight = 0;
//                    //if the current pixel is in the remove map then its energy is 0 in order to be removed
//                    if (removeMap != null && removeMap[currentY * originalWidth + currentX] != 0)
//                        currentPixelEnergy = 0;
//                    else if (preserveMap != null)
//                        //if the pixel is in the Preserve map then it will get the additional energy
//                        preserveWeight = preserveMap[currentY * originalWidth + currentX];

//                    //now we will find the adjacent pixel with the minimal energy
//                    int minAdjacentEnergy = int.MaxValue;

//                    bool conj = false;
//                    if (currentY > 0 && currentX > 0)
//                    {
//                        conj = true;

//                        int topLeftPixelEnergy = energyMap[(currentY - 1) * originalWidth + (currentX - 1)];
//                        if (topLeftPixelEnergy < minAdjacentEnergy)
//                            minAdjacentEnergy = topLeftPixelEnergy;
//                    }

//                    if (minAdjacentEnergy > 0)
//                    {
//                        if (currentY > 0)
//                        {
//                            conj = true;
//                            int topPixelEnergy = energyMap[(currentY - 1) * originalWidth + currentX];
//                            if (topPixelEnergy < minAdjacentEnergy)
//                                minAdjacentEnergy = topPixelEnergy;
//                        }

//                        if (minAdjacentEnergy > 0 && currentY > 0 && currentX < currentWidth - 1)
//                        {
//                            conj = true;
//                            int topRightPixelEnergy = energyMap[(currentY - 1) * originalWidth + (currentX + 1)];
//                            if (topRightPixelEnergy < minAdjacentEnergy)
//                                minAdjacentEnergy = topRightPixelEnergy;
//                        }
//                    }

//                    int min = currentPixelEnergy + preserveWeight + (conj ? minAdjacentEnergy : 0);

//                    //if the current value is changed - add its adjancents to the list
//                    if (min != energyMap[currentY * originalWidth + currentX])
//                    {
//                        //update the value
//                        energyMap[currentY * originalWidth + currentX] = min;

//                        //add the adjancents
//                        for (int j = 0; j < 3; j++)
//                        {
//                            int x = currentX - 1 + j;
//                            if (x >= 0 && x <= currentWidth - 1)
//                            {
//                                if (nextEnd < x)
//                                    nextEnd = x;

//                                if (x < nextStart)
//                                    nextStart = x;
//                            }
//                        }
//                    }
//                }

//                //take deleted element and add adjanced elements
//                int nextX = col[currentY];
//                for (int j = 0; j < 3; j++)
//                {
//                    int x = nextX - 1 + j;
//                    if (x >= 0 && x <= currentWidth - 1)
//                    {
//                        if (nextEnd < x)
//                            nextEnd = x;
//                        if (x < nextStart)
//                            nextStart = x;
//                    }
//                }
//                currentEnd = nextEnd;
//                currentStart = nextStart;
//            }
//        }
//    }
//}
