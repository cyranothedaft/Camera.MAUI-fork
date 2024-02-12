using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera.MAUI
{
   public class DetectorResult
   {
      public string Text { get; init; }
      public (DetectorResultPoint, DetectorResultPoint) ResultPoints { get; init; }

      public override string ToString()
         => $"{Text} {ResultPoints.Item1}-{ResultPoints.Item2}";
   }
}
