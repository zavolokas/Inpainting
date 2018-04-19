using System.Collections.Generic;
using Zavolokas.ImageProcessing.Inpainting;
using Zavolokas.ImageProcessing.PatchMatch;

namespace InpaintService
{
    public interface ICreateNnfInput
    {
        string Container { get; set; }
        string Image { get; set; }
        string NnfName { get; set; }
        InpaintSettings Settings { get; set; }
    }

    public interface IScaleNnfInput : ICreateNnfInput
    {
        bool IsCie79Calc { get; set; }
        string[] MappingNames { get; set; }
        int LevelIndex { get; set; }
    }

    public interface IInpaintInput : ICreateNnfInput
    {
        string InpaintAreaName { get; set; }

        double K { get; set; }
        byte LevelIndex { get; set; }
        int IterationIndex { get; set; }
    }

    public interface INnfInitInput : ICreateNnfInput
    {
        string InpaintAreaName { get; set; }

        bool IsCie79Calc { get; set; }
        string[] MappingNames { get; set; }
        bool ExcludeInpaintArea { get; set; }
    }

    public interface INnfBuildInput : ICreateNnfInput
    {
        string InpaintAreaName { get; set; }

        bool IsCie79Calc { get; set; }
        string[] MappingNames { get; set; }
        bool ExcludeInpaintArea { get; set; }
        bool IsForward { get; set; }
    }

    public class NnfInputData : ICreateNnfInput, IInpaintInput
    {
        public string NnfName { get; set; }
        public string Image { get; set; }
        public string Container { get; set; }
        public InpaintSettings Settings { get; set; }
        public string Mapping { get; set; }
        public bool IsCie79Calc { get; set; }
        public bool IsForward { get; set; }
        public string InpaintAreaName { get; set; }
        public bool ExcludeInpaintArea { get; set; }
        public byte LevelIndex { get; set; }
        public double K { get; set; }
        public int IterationIndex { get; set; }
        public int PatchMatchIteration { get; set; }
        public string[] SplittedNnfNames { get; set; }

        public string[] Mappings { get; set; }

        public static NnfInputData From(string nnf, string container, string image, InpaintSettings settings,
            string mapping, string inpaintAreaName, bool isForward, byte levelIndex, double meanShiftK, string[] splittedNnfs,
            string[] mappings)
        {
            return new NnfInputData
            {
                NnfName = nnf,
                Container = container,
                Image = image,
                Settings = settings,
                Mapping = mapping,
                InpaintAreaName = inpaintAreaName,
                IsForward = isForward,
                IsCie79Calc = settings.PatchDistanceCalculator == ImagePatchDistance.Cie76,
                LevelIndex = levelIndex,
                K = meanShiftK,
                SplittedNnfNames = splittedNnfs,
                Mappings = mappings
            };
        }
    }
}