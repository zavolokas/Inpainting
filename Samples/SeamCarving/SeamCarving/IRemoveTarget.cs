
namespace SeamCarving
{
    internal interface IRemoveTarget
    {
        RemoveUnit CreateRemoveUnit(RemoveUnitFactory removeUnitFactory, int[] photo, int[] removeMarkup, int[] preserveMarkup, int originalWidth, int originalHeight);
        void FinalPhase(RemoveUnit unit);
    }
}