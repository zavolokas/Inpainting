using System.Collections.Generic;

namespace SeamCarving
{
    internal interface IRemoveTypeResolver
    {
        IRemoveTarget DecideOn(List<RectArea> areas, int width, int height);
    }
}