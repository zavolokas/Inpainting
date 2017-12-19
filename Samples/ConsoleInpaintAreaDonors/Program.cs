
namespace ConsoleInpaintAreaDonors
{
    class Program
    {
        /// <summary>
        /// Mains the specified arguments.
        /// </summary>
        /// <param name="args">The arguments.</param>
        static void Main(string[] args)
        {
            //fast check
            //Area2DJoinTests.AreaJoinTest(TestSet.Init256x128());
            //Area2DSubstractTests.AreaSubstractionTest(TestSet.Init256x128());
            //Area2DIntersectTests.AreaIntersectionTest(TestSet.Init256x128());
            //MapBuilderTests.TestMapBuilder(TestSet.Init256x128());

            Area2DJoinTests.RunAll();
            Area2DSubstractTests.RunAll();
            Area2DIntersectTests.RunAll();
            MapBuilderTests.RunAll();
        }
    }
}
