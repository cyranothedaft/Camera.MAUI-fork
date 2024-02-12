using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera.MAUI
{
   internal class DetectorGeneric
   {
      private IsItBrightDetector _actualDetector = new IsItBrightDetector();

      public DetectorResult Decode(DetectorDataSource source)
      {
         DetectorResult detectedResult = _actualDetector.GetDetectedResult(source);
         return detectedResult;
      }

      public DetectorResult[] DecodeMultiple(DetectorDataSource source)
      {
         throw new NotImplementedException();
      }
   }
}
