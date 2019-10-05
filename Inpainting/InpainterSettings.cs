using Zavolokas.ImageProcessing.PatchMatch;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public interface IInpaintSettings
    {
        byte PatchSize { get; }
        double PixelChangeTreshold { get; }
        ColorResolver ColorResolver { get; }
    }

    public class InpaintSettings: IInpaintSettings
    {
        public InpaintSettings()
        {
            MeanShift = new MeanShiftSettings();
            PatchDistanceCalculator = ImagePatchDistance.Cie2000;
            PatchMatch = new PatchMatchSettings { PatchSize = 11 };
        }
        
        public double ChangedPixelsPercentTreshold = 0.005;
        public int MaxInpaintIterations = 50;
        public ImagePatchDistanceCalculator PatchDistanceCalculator;

        public MeanShiftSettings MeanShift { get; }

        public PatchMatchSettings PatchMatch { get; }
        public ColorResolver ColorResolver { get; set; } = ColorResolver.Simple;
        public double PixelChangeTreshold { get; set; } = 0.00003;
        public bool IgnoreInpaintedPixelsOnFirstIteration { get; set; } = true;

        public byte PatchSize
        {
            get { return PatchMatch.PatchSize; }
            set { PatchMatch.PatchSize = value; }
        }
    }

    public class MeanShiftSettings
    {
        /// <summary>
        /// Determines window size.
        /// </summary>
        public double K = 3.0;
        /// <summary>
        /// Determines the minimum K(window size).
        /// </summary>
        public double MinK = 0.02;
        /// <summary>
        /// Determines how fast K(window size) decreases.
        /// </summary>
        public double KDecreaseStep = 0.001;
    }
}