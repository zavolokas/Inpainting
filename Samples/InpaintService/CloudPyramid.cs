using Zavolokas.Structures;

namespace InpaintService
{
    public struct CloudPyramidLevel
    {
        public string ImageName { get; set; }
        public string InpaintArea { get; set; }
        public string[] Mappings { get; set; }
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

        public string[] GetMapping(byte levelIndex)
        {
            return Levels[levelIndex].Mappings;
        }

        public byte LevelsAmount => (byte) Levels.Length;
    }
}