using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;

namespace Inpaint
{
    internal interface IInpaintSettings
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
        public MeanShiftSettings MeanShift;
        public ImagePatchDistanceCalculator PatchDistanceCalculator;

        public PatchMatchSettings PatchMatch { get; }
        public ColorResolver ColorResolver { get; set; } = ColorResolver.MeanShift;
        public double PixelChangeTreshold { get; set; } = 0.00003;

        public byte PatchSize
        {
            get { return PatchMatch.PatchSize; }
            set { PatchMatch.PatchSize = value; }
        }
    }

    public class MeanShiftSettings
    {
        public double InitK = 3.0;
        public double MinK = 3.0;
        public double DeltaK = 0.001;
    }
}