using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point = System.Drawing.Point;

namespace Camera.MAUI
{
   public record DetectorResultPoint(int X, int Y)
   {
      public DetectorResultPoint(Point asPoint):this(asPoint.X, asPoint.Y) { }

      public Point AsPoint()
         => new Point(X, Y);

      public override string ToString()
         => $"({X},{Y})";

   }
}
