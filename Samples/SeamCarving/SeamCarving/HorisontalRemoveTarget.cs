using System.Collections.Generic;

namespace SeamCarving
{
    internal class HorisontalRemoveTarget : IRemoveTarget
    {
        private readonly List<RectArea> _areas;

        public HorisontalRemoveTarget(List<RectArea> areas)
        {
            _areas = areas;
        }

        public RemoveUnit CreateRemoveUnit(RemoveUnitFactory removeUnitFactory, int[] photo2D, int[] removeMap, int[] preserveMap, int originalWidth, int originalHeight)
        {
            int pixelsToRemove = 0;
            for (int areaIndex = 0; areaIndex < _areas.Count; areaIndex++)
            {
                pixelsToRemove += _areas[areaIndex].PixAmount;
            }

            photo2D = ArrayHelper.Rotate90(photo2D, originalWidth, originalHeight);
            removeMap = ArrayHelper.Rotate90(removeMap, originalWidth, originalHeight);
            preserveMap = ArrayHelper.Rotate90(preserveMap, originalWidth, originalHeight);

            RemoveUnit removeUnit = removeUnitFactory.Create(photo2D, removeMap, preserveMap, originalWidth, originalHeight);
            removeUnit.Creator = this;
            removeUnit.IsRotated = true;

            return removeUnit;
        }

        public void FinalPhase(RemoveUnit unit)
        {
            if (unit.IsRotated)
            {
                unit.Photo = ArrayHelper.Rotate90(unit.Photo, unit.Width, unit.Height);
            }
        }
    }
}
