using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera.MAUI
{
   public class DetectorDataSource
   {
      public int Width { get; }
      public int Height { get; }
      public byte[] Luminances { get; }

      internal DetectorDataSource(int width, int height, byte[] luminances)
      {
         Width = width;
         Height = height;
         Luminances = luminances;
      }
   }
}
