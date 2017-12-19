using System.Collections.Generic;

namespace SeamCarving
{
    internal class SimpleRemoveTypeResolver : IRemoveTypeResolver
    {
        public IRemoveTarget DecideOn(List<RectArea> areas, int width, int height)
        {
            //decide in which direction it is better to remove object - horisontal/vertical?
            IRemoveTarget removeTarget = null;

            int w = 0;
            int h = 0;
            for (int i = 0; i < areas.Count; i++)
            {
                var area = areas[i];
                w += area.Width;
                h += area.Height;
            }

            //which part of the photo occupates the object
            double objectLength = (((double)w) / (double)width);

            //how vertical is the object?
            double objectVerticalization = (((double)h) / (double)w);

            //look at the size of sides of the areas
            //if the object is very long && it is not vertical then
            //remove horisontally
            if (objectVerticalization < 0.7 && objectLength >= 0.5)
                removeTarget = new HorisontalRemoveTarget(areas);
            else
                removeTarget = new VerticalRemoveTarget(areas);
            //    removeTarget = new TwoRemoveTarget(areas);

            return removeTarget;
        }
    }
}
