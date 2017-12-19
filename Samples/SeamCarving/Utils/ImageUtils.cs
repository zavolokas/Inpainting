using System.Collections.Generic;

namespace SeamCarving
{
    public static partial class ImageUtils
    {
        public static int[][] RgbToGrayScale(int[][] rgbImage)
        {
            return RgbToGrayScale(rgbImage, rgbImage[0].GetLength(0), rgbImage.GetLength(0));
        }

        private static int[][] RgbToGrayScale(int[][] rgbImage, int width, int height)
        {
            int[][] grayScaleImage = new int[height][];

            for (int y = 0; y < height; y++)
            {
                int[] grayColorsRow = new int[width];
                int[] rgbColorsRow = rgbImage[y];
                for (int x = 0; x < width; x++)
                {
                    int color = rgbColorsRow[x];
                    int r = (color & 0x00ff0000) >> 16;
                    int g = (color & 0x0000ff00) >> 8;
                    int b = (color & 0x000000ff);
                    grayColorsRow[x] = (int)(0.21f * r + 0.72 * g + 0.07f * b);
                }
                grayScaleImage[y] = grayColorsRow;
            }
            return grayScaleImage;
        }

        internal static List<RectArea> GetAllObjects(int[] image, RectArea a, int width)
        {
            var labels = new int[image.Length];
            var equivalencies = new List<List<int>> { new List<int>() };
            var label = 0;

            var height = image.Length / width;

            for (int y = a.Top >= 0 ? a.Top : 0; y < (a.Top + a.Height) && y < height; y++)
            {
                for (int x = a.Left >= 0 ? a.Left : 0; x < (a.Left + a.Width) && x < width; x++)
                {
                    if (image[y * width + x] != 0)
                    {
                        int topLeft = (y > a.Top && x > a.Left && x > 0) ? labels[(y - 1) * width + (x - 1)] : 0;
                        int top = (y > a.Top) ? labels[(y - 1) * width + x] : 0;
                        int topRight = (y > a.Top && x < a.Left + a.Width - 1 && x < width - 1) ? labels[(y - 1) * width + (x + 1)] : 0;
                        int left = (x > a.Left && x > 0) ? labels[y * width + (x - 1)] : 0;

                        //is there at least one already labeled neighbour?
                        if (topLeft != 0 || top != 0 || topRight != 0 || left != 0)
                        {
                            //is left labeled?
                            if (left != 0)
                            {
                                //then lebel 
                                labels[y * width + x] = left;
                            }

                            //is top lebeled?
                            TryLabel(x, y, labels, top, equivalencies, width, height);

                            //is topLeft lebeled?
                            TryLabel(x, y, labels, topLeft, equivalencies, width, height);

                            //is topRight lebeled?
                            TryLabel(x, y, labels, topRight, equivalencies, width, height);
                        }
                        else
                        {
                            labels[y * width + x] = ++label;
                            equivalencies.Add(new List<int>());
                            equivalencies[label].Add(label);
                        }
                    }
                }
            }

            //sort equivalencies
            for (int classIndex = 0; classIndex < equivalencies.Count; classIndex++)
            {
                List<int> list = equivalencies[classIndex];
                list.Sort();
            }

            //relabel the components and collect Areas information
            IDictionary<int, RectArea> areas = new Dictionary<int, RectArea>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y * width + x] != 0)
                    {
                        int realLabel = equivalencies[labels[y * width + x]][0];
                        labels[y * width + x] = realLabel;
                        RectArea area;
                        //lookup the area related to this label
                        if (!areas.ContainsKey(realLabel))
                        {
                            //area related to this label doesn't exist, 
                            //so lets create it and put it to the areas
                            area = new RectArea(x, y, 0, 0) { PixAmount = 1 };
                            areas.Add(realLabel, area);
                        }
                        else
                        {
                            //area is in the areas collection
                            area = areas[realLabel];
                            if (x < area.Left)
                                area.Left = x;

                            if (y < area.Top)
                                area.Top = y;

                            area.PixAmount++;
                        }
                    }
                }
            }

            var result = new List<RectArea>();
            result.AddRange(areas.Values);
            return result;
        }

        private static void TryLabel(int x, int y, int[] labels, int newLabel, List<List<int>> equivalencies, int width, int height)
        {
            if (newLabel != 0)
            {
                //is current labeled but it's label not the same as top have?
                if (labels[y * width + x] != 0 && labels[y * width + x] != newLabel)
                {
                    //store equivalence between top & labels[y][x]
                    int oldLabel = labels[y * width + x];
                    AddNewLabel(equivalencies[newLabel], oldLabel, equivalencies);
                    if (labels[y * width + x] > newLabel)
                        labels[y * width + x] = newLabel;
                }
                else
                {
                    labels[y * width + x] = newLabel;
                }
            }
        }

        private static void AddNewLabel(List<int> sequence, int label, List<List<int>> equivalencies)
        {
            if (!sequence.Contains(label))
            {
                sequence.Add(label);
                for (int i = 0; i < sequence.Count; i++)
                {
                    AddNewLabel(equivalencies[sequence[i]], label, equivalencies);
                    AddNewLabel(equivalencies[label], sequence[i], equivalencies);
                }
            }
        }

        public static int[][] ConvertToBinaryIntJaggedArray(Image originalImage)
        {
            var result = new int[originalImage.Height][];

            for (int y = 0; y < originalImage.Height; y++)
            {
                var row = new int[originalImage.Width];
                for (int x = 0; x < originalImage.Width; x++)
                {
                    byte alpha;
                    byte red;
                    byte green;
                    byte blue;
                    originalImage.FillComponentsFrom(x, y, out alpha, out red, out green, out blue);
                    row[x] = (int)(alpha > 0 ? 1 : 0);
                }
                result[y] = row;
            }
            return result;
        }

        public static byte[][] ConvertToBinaryJaggedArray(Image originalImage)
        {
            var result = new byte[originalImage.Height][];

            for (int y = 0; y < originalImage.Height; y++)
            {
                var row = new byte[originalImage.Width];
                for (int x = 0; x < originalImage.Width; x++)
                {
                    byte alpha;
                    byte red;
                    byte green;
                    byte blue;
                    originalImage.FillComponentsFrom(x, y, out alpha, out red, out green, out blue);
                    row[x] = (byte)(alpha > 0 ? 1 : 0);
                }
                result[y] = row;
            }
            return result;
        }
    }
}
