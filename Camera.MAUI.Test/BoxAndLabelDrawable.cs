using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Font = Microsoft.Maui.Graphics.Font;

namespace Camera.MAUI.Test;

internal class BoxAndLabelDrawable : IDrawable
{
   private const float BoxStrokeSize = 6;

    internal bool DrawEnabled = false;
    internal (PointF from, PointF to) RectCorners = default;
    internal string Text;

    public void Draw(ICanvas canvas, RectF dirtyRect)
    {
       if (!DrawEnabled) return;

       setForBox(canvas);
       drawBox(canvas, RectCorners);
       setForLabel(canvas);
       drawLabel(canvas, RectCorners, Text);
    }


    private void drawBox(ICanvas canvas, (PointF from, PointF to) rectCorners)
    {
       canvas.DrawRectangle(rectCorners.from.RectFTo(rectCorners.to));
    }

    private void drawLabel(ICanvas canvas, (PointF from, PointF to) rectCorners, string text)
    {
       canvas.DrawString(text, rectCorners.from.X, rectCorners.to.Y + 16, HorizontalAlignment.Left);
    }

    private static void setForBox(ICanvas canvas)
    {
       canvas.StrokeSize = BoxStrokeSize;
       canvas.StrokeColor = Colors.Red;
    }

    private static void setForLabel(ICanvas canvas)
    {
       canvas.StrokeSize = 1;
       canvas.StrokeColor = Colors.Red;

       canvas.Font = Font.Default;
       // canvas.FontSize = 14;
       canvas.FontColor = Colors.Red;
    }


    public void Show((PointF from, PointF to) rectCorners, string text)
    {
       DrawEnabled = true;
       RectCorners = rectCorners;
       Text = text;
    }

    public void Hide()
    {
       DrawEnabled = false;
       RectCorners = default;
       Text = string.Empty;
    }
 }

internal static class GraphicsExtensions
{
   public static RectF RectFTo(this PointF from, PointF to)
      => RectF.FromLTRB(left: from.X, top: from.Y,
         right: to.X, bottom: to.Y);
}
