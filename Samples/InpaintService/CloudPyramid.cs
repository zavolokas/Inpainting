using Zavolokas.Structures;

namespace InpaintService
{
    public class CloudPyramid
    {
        public string[] ImageNames { get; set; }
        public string[] InpaintAreas { get; set; }
        public string[] Mappings { get; set; }

        public string GetImageName(byte levelIndex)
        {
            return ImageNames[levelIndex];
        }

        public string GetInpaintArea(byte levelIndex)
        {
            return InpaintAreas[levelIndex];
        }

        public string GetMapping(byte levelIndex)
        {
            return Mappings[levelIndex];
        }

        public byte LevelsAmount => (byte) ImageNames.Length;
    }
}