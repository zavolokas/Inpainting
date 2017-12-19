
using System;

namespace SeamCarving
{
    public static class ArrayHelper
    {

        public static T[][] CreateJagged<T>(int width, int height)
        {
            var jaggedArray = new T[height][];

            for (int y = 0; y < height; y++)
            {
                jaggedArray[y] = new T[width];
            }

            return jaggedArray;
        }

        public static T[][] CloneJagged<T>(T[][] source)
        {
            int height = source.Length;
            int width = source[0].Length;

            var clone = new T[height][];

            for (int y = 0; y < height; y++)
            {
                clone[y] = new T[width];
                source[y].CopyTo(clone[y], 0);
            }

            return clone;
        }

        public static void Multiply(int[] removeMarkup, int a)
        {
            for (int i = 0; i < removeMarkup.Length; i++)
            {
                removeMarkup[i] *= a;
            }
        }

        public static int[] ChangeWidth(int[] array, int currentWidth, int delta, int componentsAmount)
        {
            if (delta == 0)
                return array;

            var height = array.Length / (currentWidth * componentsAmount);
            int[] result = new int[height * (currentWidth + delta) * componentsAmount];
            int newWidth = currentWidth + delta;

            for (var y = height - 1; y >= 0; y--)
            {
                var spos = y * currentWidth * componentsAmount;
                var dpos = y * newWidth * componentsAmount;
                Array.Copy(array, spos, result, dpos, newWidth);
            }

            return result;
        }

        public static int[] Rotate90(int[] array, int width, int height)
        {
            int[] result = new int[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    result[x * width + y] = array[y * width + x];
                }
            }

            return result;
        }
    }
}
