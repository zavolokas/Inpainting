namespace SeamCarving
{
    internal interface IFilter
    {
        int[][] Apply(int[][] input, int width, int height);
    }
}