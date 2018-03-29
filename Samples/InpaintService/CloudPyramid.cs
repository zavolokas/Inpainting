using Zavolokas.Structures;

namespace InpaintService
{
    public struct CloudPyramidLevel
    {
        public string ImageName { get; set; }
        public string InpaintArea { get; set; }
        public string Mapping { get; set; }
        public string[] SplittedMappings { get; set; }
        public string Nnf { get; set; }
        public string[] SplittedNnfs { get; set; }
    }

    public class CloudPyramid
    {
        public CloudPyramidLevel[] Levels { get; set; }

        public string GetImageName(byte levelIndex)
        {
            return Levels[levelIndex].ImageName;
        }

        public string GetInpaintArea(byte levelIndex)
        {
            return Levels[levelIndex].InpaintArea;
        }

        public string GetMapping(byte levelIndex)
        {
            return Levels[levelIndex].Mapping;
        }

        public string GetNnf(byte levelIndex)
        {
            return Levels[levelIndex].Nnf;
        }

        public string[] GetSplittedMappings(byte levelIndex)
        {
            return Levels[levelIndex].SplittedMappings;
        }

        public string[] GetSplittedNnfs(byte levelIndex)
        {
            return Levels[levelIndex].SplittedNnfs;
        }

        public byte LevelsAmount => (byte) Levels.Length;
    }
}