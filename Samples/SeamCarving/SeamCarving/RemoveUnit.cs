using System.Collections.Generic;
using System.Linq;

namespace SeamCarving
{
    internal class RemoveUnit
    {
        private const byte MaxRemoveAttempts = 3;

        public int[] Photo;
        public int[] RemoveMap;
        public int[] PreserveMap;
        public int[] Edges;
        public int[] EnergyMap;
        public int[][] All;// = new[] { photo, remve };
        public int[][] ToRestore;

        public int Width;
        public int Height;
        public int[] minCol;
        public int faildAttemptCount = 0;
        public int removedPixelsInSessionCount = 0;
        public int removeItarationsCount = 0;
        public int restoreIteration = 0;
        public IRemoveTarget Creator;
        public bool IsRotated;

        public RemoveUnit(int[] photo,
                               int[] removeMap,
                               int[] preserveMap,
                               int[] edges,
                               int[] vEMap,
                               int width,
                               int height)
        {
            Photo = photo;
            RemoveMap = removeMap;
            PreserveMap = preserveMap;
            Edges = edges;
            EnergyMap = vEMap;
            All = new[] { Photo, RemoveMap, PreserveMap, Edges, EnergyMap };
            ToRestore = new[] { Photo, EnergyMap, Edges, PreserveMap };

            Width = width;
            Height = height;
            IsPreserveMapEmpty = preserveMap?.All(i => i == 0) == true;
            minCol = new int[height];
        }

        public bool IsPreserveMapEmpty { get; }

        public bool IsStuck => faildAttemptCount > MaxRemoveAttempts;

        public int[] GetPhoto(int originalWidth)
        {
            if (Width - originalWidth < 0)
            {
                Photo = ArrayHelper.ChangeWidth(Photo, originalWidth, Width - originalWidth, 1);
            }

            Creator.FinalPhase(this);
            return Photo;
        }

        public static List<RemoveUnit> Clone(List<RemoveUnit> removeUnits)
        {
            var result = new List<RemoveUnit>();
            for (int i = 0; i < removeUnits.Count; i++)
            {
                result.Add(removeUnits[i]);
            }
            return result;
        }
    }
}
