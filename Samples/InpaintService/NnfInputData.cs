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
        public string Area2DMapName { get; set; }
        public string PixelsAreaName { get; set; }
        public bool IsCie79Calc { get; set; }
        public bool IsForward { get; set; }

        public static NnfInputData From(string nnf, string container, string image, InpaintSettings settings, 
            ImagePatchDistanceCalculator calculator, string mapping, string pixelsArea, bool isForward)
        {
            return new NnfInputData
            {
                NnfName = nnf,
                Container = container,
                Image = image,
                Settings =settings,
                Area2DMapName = mapping,
                PixelsAreaName = pixelsArea,
                IsForward = isForward,
                IsCie79Calc = calculator == ImagePatchDistance.Cie76
            };
        }
    }
}