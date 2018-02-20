using Zavolokas.Structures;

namespace Inpaint
{
    public interface IPyramid
    {
        ZsImage GetNextImage();
        Area2DMap GetNextMapping();
        Area2D GetNextInpaintArea();
    }
}