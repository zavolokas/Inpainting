namespace SeamCarving
{
    internal class RemoveUnitFactory
    {
        private readonly SmartRemove _remover;
        private readonly IFilter _edgeDetector;
        private readonly IImageFilters _imageFilters;

        public RemoveUnitFactory(SmartRemove remover, IFilter filter, IImageFilters filters)
        {
            _remover = remover;
            _edgeDetector = filter;
            _imageFilters = filters;
        }

        public RemoveUnit Create(int[] photo, int[] removeMap, int[] preserveMap, int width, int height)
        {
            var photo2 = ArrayHelper.CreateJagged<int>(width, height);
            photo2.CopyFrom(photo);

            var grayScalePhoto = ImageUtils.RgbToGrayScale(photo2);
            var edges = _imageFilters.ApplyFilter(grayScalePhoto, width, height, _edgeDetector).ToOneDemensional();
            var vEMap = new int[edges.Length];
            edges.CopyTo(vEMap, 0);

            SmartRemove remover = new SmartRemove(width, height);
            remover.ConvertToVerticalEnergyMap(vEMap, removeMap, preserveMap, edges, 0, width, width, height);

            return new RemoveUnit(photo, removeMap, preserveMap, edges, vEMap, width, height);
        }

        public RemoveUnit Clone(RemoveUnit removeUnit, bool cloneRemoveMask, bool clonePreserveMask, int originalWidth)
        {
            int[] photo = new int[removeUnit.Photo.Length];
            removeUnit.Photo.CopyTo(photo, 0);

            int[] removeMask = null;
            if (clonePreserveMask)
            {
                removeMask = new int[removeUnit.RemoveMap.Length];
                removeUnit.RemoveMap.CopyTo(removeMask, 0);
            }
            int[] preserveMask = null;
            if (clonePreserveMask)
            {
                preserveMask = new int[removeUnit.PreserveMap.Length];
                removeUnit.PreserveMap.CopyTo(preserveMask, 0);
            }

            var edges = new int[removeUnit.Edges.Length];
            removeUnit.Edges.CopyTo(edges, 0);

            var energyMap = new int[edges.Length];
            edges.CopyTo(energyMap,0);

            _remover.ConvertToVerticalEnergyMap(energyMap, preserveMask, edges, 0, removeUnit.Width, originalWidth,  removeUnit.Height);
            var unit = new RemoveUnit(photo, removeMask, preserveMask, edges, energyMap, removeUnit.Width, removeUnit.Height);

            unit.removeItarationsCount = removeUnit.removeItarationsCount;
            unit.faildAttemptCount = removeUnit.faildAttemptCount;
            unit.Creator = removeUnit.Creator;
            unit.IsRotated = removeUnit.IsRotated;

            return unit;
        }

        public RemoveUnit CreateRemoveUnit(IRemoveTarget removeTarget, int[] photo, int[] removeMarkup, int[] preserveMarkup, int width, int height)
        {
            return removeTarget.CreateRemoveUnit(this, photo, removeMarkup, preserveMarkup, width, height);
        }
    }
}
