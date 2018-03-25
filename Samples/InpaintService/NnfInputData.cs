using System.Security.Cryptography;
using Zavolokas.ImageProcessing.PatchMatch;
using Zavolokas.Structures;

namespace InpaintService
{
    public class NnfInputData
    {
        public NnfState NnfState { get; set; }
        public string Image { get; set; }
        public string Container { get; set; }
        //public PatchMatchSettings PmSettings { get; set; }
        public Area2DMapState Area2DMapState { get; set; }
        public Area2DState PixelsAreaState { get; set; }
        public bool IsCie79Calc { get; set; }
        public bool IsForward { get; set; }

        public static NnfInputData From(Nnf nnf, string container, string image, PatchMatchSettings pmSettings, 
            ImagePatchDistanceCalculator calculator, Area2DMap mapping, Area2D pixelsArea, bool isForward)
        {
            return new NnfInputData
            {
                NnfState = nnf.GetState(),
                Container = container,
                Image = image,
          //      PmSettings = pmSettings,
                Area2DMapState = mapping.GetState(),
                PixelsAreaState = pixelsArea.GetState(),
                IsForward = isForward,
                IsCie79Calc = calculator == ImagePatchDistance.Cie76
            };
        }
    }
}