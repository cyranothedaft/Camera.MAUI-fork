using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Imaging;

namespace Camera.MAUI.Platforms.Windows;

internal class WindowsDetectorDataSource : DetectorDataSource
{
   public WindowsDetectorDataSource(int width, int height, byte[] luminances) 
      : base(width, height, luminances) { }

   public static DetectorDataSource Create(SoftwareBitmap softwareBitmap)
    {
      // TODO
      // softwareBitmap.PixelWidth, softwareBitmap.PixelHeight
       // if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Gray8)
        // {
        //     using SoftwareBitmap convertedSoftwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Gray8);
        //     convertedSoftwareBitmap.CopyToBuffer(luminances.AsBuffer());
        // }
        // else
        // {
        //     softwareBitmap.CopyToBuffer(luminances.AsBuffer());
        // }
        throw new NotImplementedException();
    }
}
