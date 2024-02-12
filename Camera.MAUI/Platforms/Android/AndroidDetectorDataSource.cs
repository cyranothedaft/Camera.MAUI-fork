using System.Text;
using Android.Graphics;
using Android.Hardware.Lights;

namespace Camera.MAUI.Platforms.Android;

internal static class AndroidDetectorDataSource 
{
   public static DetectorDataSource Create(Bitmap bitmap)
   {
      var pixels = new int[bitmap.Width * bitmap.Height];
      bitmap.GetPixels(pixels, 0, bitmap.Width, 0, 0, bitmap.Width, bitmap.Height);
      var pixelBytes = new byte[pixels.Length * 4];
      Buffer.BlockCopy(pixels, 0, pixelBytes, 0, pixelBytes.Length);
      byte[] luminances = new byte[bitmap.Width * bitmap.Height];
      if (bitmap.HasAlpha)
      {
         RGBLuminanceSource.CalculateLuminance(luminances, bitmap.Width,bitmap.Height, pixelBytes, RGBLuminanceSource.BitmapFormat.RGBA32);
      }
      else
      {
         RGBLuminanceSource.CalculateLuminance(luminances, bitmap.Width,bitmap.Height, pixelBytes, RGBLuminanceSource.BitmapFormat.RGB32);
      }

      return new DetectorDataSource(bitmap.Width, bitmap.Height, luminances);
   }
}


internal static class RGBLuminanceSource
{
    public enum BitmapFormat
    {
        Unknown,
        Gray8,
        Gray16,
        RGB24,
        RGB32,
        ARGB32,
        BGR24,
        BGR32,
        BGRA32,
        RGB565,
        RGBA32,
        UYVY,
        YUYV
    }

    private static BitmapFormat DetermineBitmapFormat(byte[] rgbRawBytes, int width, int height)
    {
        var square = width * height;
        var byteperpixel = rgbRawBytes.Length / square;

        return byteperpixel switch
        {
            1 => BitmapFormat.Gray8,
            2 => BitmapFormat.RGB565,
            3 => BitmapFormat.RGB24,
            4 => BitmapFormat.RGB32,
            _ => throw new ArgumentException("The bitmap format could not be determined. Please specify the correct value."),
        };
    }
    public static void CalculateLuminance(byte[] luminances, int width, int height, byte[] rgbRawBytes, BitmapFormat bitmapFormat)
   {
      if (bitmapFormat == BitmapFormat.Unknown)
      {
            bitmapFormat = DetermineBitmapFormat(rgbRawBytes, width, height);
        }
        switch (bitmapFormat)
        {
            case BitmapFormat.Gray8:
                Buffer.BlockCopy(rgbRawBytes, 0, luminances, 0, rgbRawBytes.Length < luminances.Length ? rgbRawBytes.Length : luminances.Length);
                break;
            case BitmapFormat.Gray16:
                CalculateLuminanceGray16(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.RGB24:
                CalculateLuminanceRGB24(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.BGR24:
                CalculateLuminanceBGR24(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.RGB32:
                CalculateLuminanceRGB32(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.BGR32:
                CalculateLuminanceBGR32(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.RGBA32:
                CalculateLuminanceRGBA32(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.ARGB32:
                CalculateLuminanceARGB32(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.BGRA32:
                CalculateLuminanceBGRA32(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.RGB565:
                CalculateLuminanceRGB565(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.UYVY:
                CalculateLuminanceUYVY(luminances, width, height, rgbRawBytes);
                break;
            case BitmapFormat.YUYV:
                CalculateLuminanceYUYV(luminances, width, height, rgbRawBytes);
                break;
            default:
                throw new ArgumentException("The bitmap format isn't supported.", bitmapFormat.ToString());
        }
    }

    private static void CalculateLuminanceRGB565(byte[] luminances, int width, int height, byte[] rgb565RawData)
    {
        var luminanceIndex = 0;
        for (var index = 0; index < rgb565RawData.Length && luminanceIndex < luminances.Length; index += 2, luminanceIndex++)
        {
            var byte1 = rgb565RawData[index];
            var byte2 = rgb565RawData[index + 1];

            var b5 = byte1 & 0x1F;
            var g5 = (((byte1 & 0xE0) >> 5) | ((byte2 & 0x03) << 3)) & 0x1F;
            var r5 = (byte2 >> 2) & 0x1F;
            var r8 = (r5 * 527 + 23) >> 6;
            var g8 = (g5 * 527 + 23) >> 6;
            var b8 = (b5 * 527 + 23) >> 6;

            luminances[luminanceIndex] = (byte)((BaseLuminanceSource.RChannelWeight * r8 + BaseLuminanceSource.GChannelWeight * g8 + BaseLuminanceSource.BChannelWeight * b8) >> BaseLuminanceSource.ChannelWeight);
        }
    }

    private static void CalculateLuminanceRGB24(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            int r = rgbRawBytes[rgbIndex++];
            int g = rgbRawBytes[rgbIndex++];
            int b = rgbRawBytes[rgbIndex++];
            luminances[luminanceIndex] = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
        }
    }

    private static void CalculateLuminanceBGR24(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            int b = rgbRawBytes[rgbIndex++];
            int g = rgbRawBytes[rgbIndex++];
            int r = rgbRawBytes[rgbIndex++];
            luminances[luminanceIndex] = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
        }
    }

    private static void CalculateLuminanceRGB32(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            int r = rgbRawBytes[rgbIndex++];
            int g = rgbRawBytes[rgbIndex++];
            int b = rgbRawBytes[rgbIndex++];
            rgbIndex++;
            luminances[luminanceIndex] = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
        }
    }

    private static void CalculateLuminanceBGR32(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            int b = rgbRawBytes[rgbIndex++];
            int g = rgbRawBytes[rgbIndex++];
            int r = rgbRawBytes[rgbIndex++];
            rgbIndex++;
            luminances[luminanceIndex] = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
        }
    }

    private static void CalculateLuminanceBGRA32(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            var b = rgbRawBytes[rgbIndex++];
            var g = rgbRawBytes[rgbIndex++];
            var r = rgbRawBytes[rgbIndex++];
            var alpha = rgbRawBytes[rgbIndex++];
            var luminance = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
            luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
        }
    }

    private static void CalculateLuminanceRGBA32(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            var r = rgbRawBytes[rgbIndex++];
            var g = rgbRawBytes[rgbIndex++];
            var b = rgbRawBytes[rgbIndex++];
            var alpha = rgbRawBytes[rgbIndex++];
            var luminance = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
            luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
        }
    }

    private static void CalculateLuminanceARGB32(byte[] luminances, int width, int height, byte[] rgbRawBytes)
    {
        for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < rgbRawBytes.Length && luminanceIndex < luminances.Length; luminanceIndex++)
        {
            // Calculate luminance cheaply, favoring green.
            var alpha = rgbRawBytes[rgbIndex++];
            var r = rgbRawBytes[rgbIndex++];
            var g = rgbRawBytes[rgbIndex++];
            var b = rgbRawBytes[rgbIndex++];
            var luminance = (byte)((BaseLuminanceSource.RChannelWeight * r + BaseLuminanceSource.GChannelWeight * g + BaseLuminanceSource.BChannelWeight * b) >> BaseLuminanceSource.ChannelWeight);
            luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
        }
    }

    private static void CalculateLuminanceUYVY(byte[] luminances, int width, int height, byte[] uyvyRawBytes)
    {
        // start by 1, jump over first U byte
        for (int uyvyIndex = 1, luminanceIndex = 0; uyvyIndex < uyvyRawBytes.Length - 3 && luminanceIndex < luminances.Length;)
        {
            byte y1 = uyvyRawBytes[uyvyIndex];
            uyvyIndex += 2; // jump from 1 to 3 (from Y1 over to Y2)
            byte y2 = uyvyRawBytes[uyvyIndex];
            uyvyIndex += 2; // jump from 3 to 5

            luminances[luminanceIndex++] = y1;
            luminances[luminanceIndex++] = y2;
        }
    }

    private static void CalculateLuminanceYUYV(byte[] luminances, int width, int height, byte[] yuyvRawBytes)
    {
        // start by 0 not by 1 like UYUV
        for (int yuyvIndex = 0, luminanceIndex = 0; yuyvIndex < yuyvRawBytes.Length - 3 && luminanceIndex < luminances.Length;)
        {
            byte y1 = yuyvRawBytes[yuyvIndex];
            yuyvIndex += 2; // jump from 0 to 2 (from Y1 over over to Y2)
            byte y2 = yuyvRawBytes[yuyvIndex];
            yuyvIndex += 2; // jump from 2 to 4

            luminances[luminanceIndex++] = y1;
            luminances[luminanceIndex++] = y2;
        }
    }

    private static void CalculateLuminanceGray16(byte[] luminances, int width, int height, byte[] gray16RawBytes)
    {
        for (int grayIndex = 0, luminanceIndex = 0; grayIndex < gray16RawBytes.Length && luminanceIndex < luminances.Length; grayIndex += 2, luminanceIndex++)
        {
            byte gray8 = gray16RawBytes[grayIndex];

            luminances[luminanceIndex] = gray8;
        }
    }
}


/*
* Copyright 2012 ZXing.Net authors
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*      http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

    /// <summary>
    /// The base class for luminance sources which supports 
    /// cropping and rotating based upon the luminance values.
    /// </summary>
    public abstract class BaseLuminanceSource : LuminanceSource
    {
        // the following channel weights give nearly the same
        // gray scale picture as the java version with BufferedImage.TYPE_BYTE_GRAY
        // they are used in sub classes for luminance / gray scale calculation
        /// <summary>
        /// weight of the red channel for calculating a gray scale image
        /// </summary>
        public const int RChannelWeight = 19562;
        /// <summary>
        /// weight of the green channel for calculating a gray scale image
        /// </summary>
        public const int GChannelWeight = 38550;
        /// <summary>
        /// weight of the blue channel for calculating a gray scale image
        /// </summary>
        public const int BChannelWeight = 7424;
        /// <summary>
        /// numbers of bits which for right shifting
        /// </summary>
        public const int ChannelWeight = 16;

        /// <summary>
        /// 
        /// </summary>
        protected byte[] luminances;

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLuminanceSource"/> class.
        /// </summary>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected BaseLuminanceSource(int width, int height)
           : base(width, height)
        {
            luminances = new byte[width * height];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseLuminanceSource"/> class.
        /// </summary>
        /// <param name="luminanceArray">The luminance array.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        protected BaseLuminanceSource(byte[] luminanceArray, int width, int height)
           : base(width, height)
        {
            luminances = new byte[width * height];
            Buffer.BlockCopy(luminanceArray, 0, luminances, 0, width * height);
        }

        /// <summary>
        /// Fetches one row of luminance data from the underlying platform's bitmap. Values range from
        /// 0 (black) to 255 (white). It is preferable for implementations of this method
        /// to only fetch this row rather than the whole image, since no 2D Readers may be installed and
        /// getMatrix() may never be called.
        /// </summary>
        /// <param name="y">The row to fetch, 0 &lt;= y &lt; Height.</param>
        /// <param name="row">An optional preallocated array. If null or too small, it will be ignored.
        /// Always use the returned object, and ignore the .length of the array.</param>
        /// <returns>
        /// An array containing the luminance data.
        /// </returns>
        override public byte[] getRow(int y, byte[] row)
        {
            int width = Width;
            if (row == null || row.Length < width)
            {
                row = new byte[width];
            }
            for (int i = 0; i < width; i++)
                row[i] = luminances[y * width + i];
            return row;
        }

        /// <summary>
        /// gets the luminance matrix
        /// </summary>
        public override byte[] Matrix
        {
            get { return luminances; }
        }

        /// <summary>
        /// Returns a new object with rotated image data by 90 degrees counterclockwise.
        /// Only callable if {@link #isRotateSupported()} is true.
        /// </summary>
        /// <returns>
        /// A rotated version of this object.
        /// </returns>
        public override LuminanceSource rotateCounterClockwise()
        {
            var rotatedLuminances = new byte[Width * Height];
            var newWidth = Height;
            var newHeight = Width;
            var localLuminances = Matrix;
            for (var yold = 0; yold < Height; yold++)
            {
                for (var xold = 0; xold < Width; xold++)
                {
                    var ynew = newHeight - xold - 1;
                    var xnew = yold;
                    rotatedLuminances[ynew * newWidth + xnew] = localLuminances[yold * Width + xold];
                }
            }
            return CreateLuminanceSource(rotatedLuminances, newWidth, newHeight);
        }

        /// <summary>
        /// TODO: not implemented yet
        /// </summary>
        /// <returns>
        /// A rotated version of this object.
        /// </returns>
        public override LuminanceSource rotateCounterClockwise45()
        {
            // TODO: implement a good 45 degrees rotation without lost of information
            return base.rotateCounterClockwise45();
        }

        /// <summary>
        /// </summary>
        /// <returns> Whether this subclass supports counter-clockwise rotation.</returns>
        public override bool RotateSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Returns a new object with cropped image data. Implementations may keep a reference to the
        /// original data rather than a copy. Only callable if CropSupported is true.
        /// </summary>
        /// <param name="left">The left coordinate, 0 &lt;= left &lt; Width.</param>
        /// <param name="top">The top coordinate, 0 &lt;= top &lt;= Height.</param>
        /// <param name="width">The width of the rectangle to crop.</param>
        /// <param name="height">The height of the rectangle to crop.</param>
        /// <returns>
        /// A cropped version of this object.
        /// </returns>
        public override LuminanceSource crop(int left, int top, int width, int height)
        {
            if (left + width > Width || top + height > Height)
            {
                throw new ArgumentException("Crop rectangle does not fit within image data.");
            }
            var croppedLuminances = new byte[width * height];
            var oldLuminances = Matrix;
            var oldWidth = Width;
            var oldRightBound = left + width;
            var oldBottomBound = top + height;
            for (int yold = top, ynew = 0; yold < oldBottomBound; yold++, ynew++)
            {
                for (int xold = left, xnew = 0; xold < oldRightBound; xold++, xnew++)
                {
                    croppedLuminances[ynew * width + xnew] = oldLuminances[yold * oldWidth + xold];
                }
            }
            return CreateLuminanceSource(croppedLuminances, width, height);
        }

        /// <summary>
        /// </summary>
        /// <returns> Whether this subclass supports cropping.</returns>
        public override bool CropSupported
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>Whether this subclass supports invertion.</returns>
        public override bool InversionSupported
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// Should create a new luminance source with the right class type.
        /// The method is used in methods crop and rotate.
        /// </summary>
        /// <param name="newLuminances">The new luminances.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        /// <returns></returns>
        protected abstract LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height);
    }






    /// <summary>
    /// The purpose of this class hierarchy is to abstract different bitmap implementations across
    /// platforms into a standard interface for requesting greyscale luminance values. The interface
    /// only provides immutable methods; therefore crop and rotation create copies. This is to ensure
    /// that one Reader does not modify the original luminance source and leave it in an unknown state
    /// for other Readers in the chain.
    /// </summary>
    /// <author>dswitkin@google.com (Daniel Switkin)</author>
    public abstract class LuminanceSource
    {
        private int width;
        private int height;

        /// <summary>
        /// initializing constructor
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        protected LuminanceSource(int width, int height)
        {
            this.width = width;
            this.height = height;
        }

        /// <summary>
        /// Fetches one row of luminance data from the underlying platform's bitmap. Values range from
        /// 0 (black) to 255 (white). Because Java does not have an unsigned byte type, callers will have
        /// to bitwise and with 0xff for each value. It is preferable for implementations of this method
        /// to only fetch this row rather than the whole image, since no 2D Readers may be installed and
        /// getMatrix() may never be called.
        /// </summary>
        /// <param name="y">The row to fetch, which must be in [0, bitmap height)</param>
        /// <param name="row">An optional preallocated array. If null or too small, it will be ignored.
        /// Always use the returned object, and ignore the .length of the array.
        /// </param>
        /// <returns> An array containing the luminance data.</returns>
        public abstract byte[] getRow(int y, byte[] row);

        /// <summary>
        /// Fetches luminance data for the underlying bitmap. Values should be fetched using:
        /// <code>int luminance = array[y * width + x] &amp; 0xff</code>
        /// </summary>
        /// <returns>
        /// A row-major 2D array of luminance values. Do not use result.length as it may be
        /// larger than width * height bytes on some platforms. Do not modify the contents
        /// of the result.
        /// </returns>
        public abstract byte[] Matrix { get; }

        /// <returns> The width of the bitmap.</returns>
        public virtual int Width
        {
            get
            {
                return width;
            }
            protected set
            {
                width = value;
            }
        }

        /// <returns> The height of the bitmap.</returns>
        public virtual int Height
        {
            get
            {
                return height;
            }
            protected set
            {
                height = value;
            }
        }

        /// <returns> Whether this subclass supports cropping.</returns>
        public virtual bool CropSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary> 
        /// Returns a new object with cropped image data. Implementations may keep a reference to the
        /// original data rather than a copy. Only callable if CropSupported is true.
        /// </summary>
        /// <param name="left">The left coordinate, which must be in [0, Width)</param>
        /// <param name="top">The top coordinate, which must be in [0, Height)</param>
        /// <param name="width">The width of the rectangle to crop.</param>
        /// <param name="height">The height of the rectangle to crop.</param>
        /// <returns> A cropped version of this object.</returns>
        public virtual LuminanceSource crop(int left, int top, int width, int height)
        {
            throw new NotSupportedException("This luminance source does not support cropping.");
        }

        /// <returns> Whether this subclass supports counter-clockwise rotation.</returns>
        public virtual bool RotateSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Returns a new object with rotated image data by 90 degrees counterclockwise.
        /// Only callable if <see cref="RotateSupported"/> is true.
        /// </summary>
        /// <returns>A rotated version of this object.</returns>
        public virtual LuminanceSource rotateCounterClockwise()
        {
            throw new NotSupportedException("This luminance source does not support rotation.");
        }

        /// <summary>
        /// Returns a new object with rotated image data by 45 degrees counterclockwise.
        /// Only callable if <see cref="RotateSupported"/> is true.
        /// </summary>
        /// <returns>A rotated version of this object.</returns>
        public virtual LuminanceSource rotateCounterClockwise45()
        {
            throw new NotSupportedException("This luminance source does not support rotation by 45 degrees.");
        }

        /// <summary>
        /// </summary>
        /// <returns>Whether this subclass supports invertion.</returns>
        public virtual bool InversionSupported
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// inverts the luminance values, not supported here. has to implemented in sub classes
        /// </summary>
        /// <returns></returns>
        public virtual LuminanceSource invert()
        {
            throw new NotSupportedException("This luminance source does not support inversion.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            var row = new byte[width];
            var result = new StringBuilder(height * (width + 1));
            for (int y = 0; y < height; y++)
            {
                row = getRow(y, row);
                for (int x = 0; x < width; x++)
                {
                    int luminance = row[x] & 0xFF;
                    char c;
                    if (luminance < 0x40)
                    {
                        c = '#';
                    }
                    else if (luminance < 0x80)
                    {
                        c = '+';
                    }
                    else if (luminance < 0xC0)
                    {
                        c = '.';
                    }
                    else
                    {
                        c = ' ';
                    }
                    result.Append(c);
                }
                result.Append('\n');
            }
            return result.ToString();
        }
    }
