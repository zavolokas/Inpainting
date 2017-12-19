using System;

namespace SeamCarving
{
    public struct LabColor
   {
      public readonly double L;
      public readonly double a;
      public readonly double b;

      public readonly bool IsEmpty;
      public static readonly LabColor Empty = new LabColor(0, 0, 0, true);

      private LabColor(double L, double a, double b, bool isEmpty)
      {
         this.L = L;
         this.a = a;
         this.b = b;

         IsEmpty = isEmpty;
      }

      public LabColor(double L, double a, double b)
      {
         this.L = L;
         this.a = a;
         this.b = b;

         IsEmpty = false;
      }

      public const double BiggestDistance = 22044.69;

      public double CalculateSquareDistanceTo(LabColor color)
      {
         if (IsEmpty || color.IsEmpty)
            return BiggestDistance;

         var dL = L - color.L;
         var da = a - color.a;
         var db = b - color.b;
         return dL * dL + da * da + db * db;
      }

      public override string ToString()
      {
         return String.Format("({0},{1},{2})", L, a, b);
      }
   }
}
