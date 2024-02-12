using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Camera.MAUI
{
   internal class IsItBrightDetector
   {
      public DetectorResult GetDetectedResult(DetectorDataSource source)
      {
         (bool isDetected, DetectorResult detectionResult) = detect();
         return isDetected
            ? detectionResult
            : null;

         (bool isDetected, DetectorResult result) detect()
         {
            int foundBright = findBright(source.Luminances);
            bool found = (foundBright >= 0);
            if (found)
            {
               ((int x, int y) at, byte value) info = (at: getXYFromIndex(foundBright, source.Width, source.Height),
                  value: source.Luminances[foundBright]);
               System.Drawing.Point asPoint = new System.Drawing.Point(info.at.x, info.at.y);
               var result = new DetectorResult()
               {
                  Text = $"Bright: {info.value} @({info.at.x},{info.at.y})",
                  ResultPoints = (new DetectorResultPoint(asPoint),
                                  new DetectorResultPoint(asPoint.Offset2(6, 6))),
               };
               Debug.WriteLine("FOUND - {0}", result);
               return (true, result);
            }
            else
               return (false, null);
         }
      }

      private (int x, int y) getXYFromIndex(int index, int width, int height)
         => (index % width, index / height);


      private static int findBright(byte[] luminances)
      {
         // count the pixels that DON'T match the criteria - if none match, the count will be the array size
         int foundIndex = luminances.TakeWhile(isNotBright).Count();
         return (foundIndex < luminances.Length) ? foundIndex : -1;

         static bool isNotBright(byte b) => !isBright(b);
         static bool isBright(byte b) => (b >= 235);
      }
   }
}
