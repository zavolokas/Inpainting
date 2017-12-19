namespace SeamCarving
{
    internal class ImageFilters : IImageFilters
    {
        public int[][] ApplyFilter(int[][] image, IFilter filter)
        {
            int height = image.Length;
            int width = image[0].Length;
            return ApplyFilter(image, width, height, filter);
        }

        public int[][] ApplyFilter(int[][] image, int width, int height, IFilter filter)
        {
            return filter.Apply(image, width, height);
        }
    }
}
