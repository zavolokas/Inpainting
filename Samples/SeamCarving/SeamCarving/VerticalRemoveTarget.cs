using System.Collections.Generic;

namespace SeamCarving
{
    internal class VerticalRemoveTarget : IRemoveTarget
    {
        private readonly List<RectArea> _areas;

        public VerticalRemoveTarget(List<RectArea> areas)
        {
            _areas = areas;
        }

        public RemoveUnit CreateRemoveUnit(RemoveUnitFactory removeUnitFactory, int[] photo, int[] removeMarkup, int[] preserveMarkup, int originalWidth, int originalHeight)
        {
            int pixelsToRemove = 0;
            for (int areaIndex = 0; areaIndex < _areas.Count; areaIndex++)
            {
                pixelsToRemove += _areas[areaIndex].PixAmount;
            }

            RemoveUnit removeUnit = removeUnitFactory.Create(photo, removeMarkup, preserveMarkup, originalWidth, originalHeight);
            removeUnit.Creator = this;

            return removeUnit;
        }

        public void FinalPhase(RemoveUnit unit)
        {
        }
    }
}
