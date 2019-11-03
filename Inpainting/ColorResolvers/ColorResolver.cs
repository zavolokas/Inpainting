namespace Zavolokas.ImageProcessing.Inpainting
{
    public abstract class ColorResolver
    {
        /// <summary>
        /// Calculates the color using the information about colors around.
        /// </summary>
        /// <param name="weightedColors">Array of colors' components + it's weight.</param>
        /// <param name="startIndex">Index of the first color.</param>
        /// <param name="colorsAmount">Amount of colors to use for calculation(not components).</param>
        /// <param name="componentsAmount">Amount of components in a coolor.</param>
        /// <param name="k">The k.</param>
        /// <param name="resultColor">Destination, where to write the result color.</param>
        /// <param name="resultIndex">Index of where to write the result.</param>
        public abstract void Resolve(double[] weightedColors, int startIndex, int colorsAmount, byte componentsAmount, double k, double[] resultColor, int resultIndex);

        /// <summary>
        /// Fast method that uses averaging in order to calculate a result color.
        /// </summary>
        public static readonly ColorResolver Simple = new SimpleColorResolver();

        // TODO: uncomment this when Accord have .NET Standard support. This method it more precise.
        ///// <summary>
        ///// Uses mean shift method in order to calculate a result color. 
        ///// </summary>
        //public static readonly ColorResolver MeanShift = new MeanShiftColorResolver();
    }
}
