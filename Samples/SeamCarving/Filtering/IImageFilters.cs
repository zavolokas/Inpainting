namespace SeamCarving
{
    internal interface IImageFilters
    {
        int[][] ApplyFilter(int[][] image, IFilter filter);
        int[][] ApplyFilter(int[][] image, int width, int height, IFilter filter);
    }
}
