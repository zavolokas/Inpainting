namespace SeamCarving
{
    internal class SobolEdgeDetector : IFilter
    {
        public int[][] Apply(int[][] input, int width, int height)
        {
            int hght = height - 1;
            int wdth = width - 1;

            int[][] output = new int[height][];
            int P1, P2, P3, P4, P6, P7, P8, P9;

            for (int y = 0; y < height; y++)
            {
                int[] row = new int[width];
                for (int x = 0; x < width; x++)
                {
                    P1 = 0 < x && 0 < y ? input[y - 1][x - 1] : 0;
                    P2 = 0 < y ? input[y - 1][x] : 0;
                    P3 = x < wdth && 0 < y ? input[y - 1][x + 1] : 0;
                    P4 = 0 < x ? input[y][x - 1] : 0;
                    P6 = x < wdth ? input[y][x + 1] : 0;
                    P7 = 0 < x && y < hght ? input[y + 1][x - 1] : 0;
                    P8 = y < hght ? input[y + 1][x] : 0;
                    P9 = x < wdth && y < hght ? input[y + 1][x + 1] : 0;

                    int G = System.Math.Abs((P1 + 2 * P2 + P3) - (P7 + 2 * P8 + P9)) +
                            System.Math.Abs((P3 + 2 * P6 + P9) - (P1 + 2 * P4 + P7));

                    row[x] = G;
                }
                output[y] = row;
            }
            return output;
        }
    }
}
