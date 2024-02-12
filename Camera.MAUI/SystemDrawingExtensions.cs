using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Point=System.Drawing.Point;

namespace Camera.MAUI
{
   internal static class SystemDrawingExtensions
   {
      public static Point Offset2(this Point point, int offset_x, int offset_y)
         => new Point(point.X + offset_x, point.Y + offset_y);
   }
}
