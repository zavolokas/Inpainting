namespace Zavolokas.ImageProcessing.Inpainting
{
    public struct InpaintingResult
    {
        public int PixelsToInpaintAmount { get; set; }
        public int PixelsChangedAmount { get; set; }
        public double TotalDifference { get; set; }
        public double ChangedPixelsDifference { get; set; }

        public double ChangedPixelsPercent => (double)PixelsChangedAmount / (double)PixelsToInpaintAmount;
    }
}