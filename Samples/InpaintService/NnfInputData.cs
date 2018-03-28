using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;

namespace InpaintService
{
    public class NnfInputData
    {
        public string NnfName { get; set; }
        public string Image { get; set; }
        public string Container { get; set; }
        public InpaintSettings Settings { get; set; }
        public string[] MappingNames { get; set; }
        public bool IsCie79Calc { get; set; }
        public bool IsForward { get; set; }
        public string InpaintAreaName { get; set; }
        public bool ExcludeInpaintArea { get; set; }
        public byte LevelIndex { get; set; }
        public double K { get; set; }
        public int IterationIndex { get; set; }
        public int PatchMatchIteration { get; set; }

        public static NnfInputData From(string nnf, string container, string image, InpaintSettings settings,
            string[] mappings, string inpaintAreaName, bool isForward, byte levelIndex, double meanShiftK)
        {
            return new NnfInputData
            {
                NnfName = nnf,
                Container = container,
                Image = image,
                Settings =settings,
                MappingNames = mappings,
                InpaintAreaName = inpaintAreaName,
                IsForward = isForward,
                IsCie79Calc = settings.PatchDistanceCalculator == ImagePatchDistance.Cie76,
                LevelIndex = levelIndex,
                K = meanShiftK
            };
        }
    }
}