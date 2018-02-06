#if UNIT_TESTS
using System;
using NUnit.Framework;
using Zavolokas.ImageProcessing.Algorithms.ImageComplition.PriorityPoints;

namespace Tests
{
   [TestFixture]
   public class WhenCreatePatchPointsFinder
   {
      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void ShouldThrowExceptionWhenImageEmpty()
      {
         PatchPointsFinder finder = new PatchPointsFinder(new int[10][]);
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void ShouldThrowExceptionWhenImageIsNull()
      {
         PatchPointsFinder finder = new PatchPointsFinder(null);
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void ShouldThrowExceptionWhenImageSizeIsSmall()
      {
         PatchPointsFinder finder = new PatchPointsFinder(new int[1][]);
      }

      [Test]
      [ExpectedException(typeof(ArgumentOutOfRangeException))]
      public void ShouldThrowExceptionWhenImageSizeIsSmall2()
      {
         int[][] image = new int[5][] 
         {new int[1]{0}, 
         new int[1]{2}, 
         new int[1]{2}, 
         new int[1]{2}, 
         new int[1]{2}
         };

         PatchPointsFinder finder = new PatchPointsFinder(image);
      }
   }
}
#endif