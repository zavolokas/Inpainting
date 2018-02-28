using Zavolokas.Structures;

namespace Zavolokas.ImageProcessing.Inpainting
{
    public interface IPyramid
    {
        ZsImage GetNextImage();
        Area2DMap GetNextMapping();
        Area2D GetNextInpaintArea();
    }
}