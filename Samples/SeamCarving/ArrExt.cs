namespace SeamCarving
{
    internal static class ArrExt
    {
        public static int[] ToOneDemensional(this int[][] arr)
        {
            if (arr == null)
                return null;

            int height = arr.Length;
            int width = arr[0].Length;

            int[] result = new int[height * width];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[y * width + x] = arr[y][x];
                }
            }

            return result;
        }

        public static void CopyFrom(this int[][] arr, int[] src)
        {
            if (arr == null || src == null)
                return;

            int height = arr.Length;
            int width = arr[0].Length;

            for (int i = 0; i < src.Length; i++)
            {
                int y = i / width;
                int x = i % width;
                arr[y][x] = src[i];
            }
        }
    }
}
