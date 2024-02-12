using CommunityToolkit.Maui.Views;
using System.Diagnostics;
using System.Net.Mime;
#if ANDROID
using Microsoft.Maui.Graphics.Platform;
#elif WINDOWS
using Microsoft.Maui.Graphics.Win2D;
#endif

namespace Camera.MAUI.Test;

public partial class MainPage : ContentPage
{

    public static readonly BindableProperty StreamProperty = BindableProperty.Create(nameof(Stream), typeof(Stream), typeof(MainPage), null, propertyChanged: SetStream);

// TODO: thread safety?
//    private byte[][] _barCodePixels;
//    private RectF _barCodeLoc;
    private readonly BoxAndLabelDrawable _boxAndLabelDrawable;

    private static void SetStream(BindableObject bindable, object oldValue, object newValue)
    {
        if (newValue != oldValue && newValue is Stream str)
        {
            var control = bindable as MainPage;
            str.Position = 0;
            control.snapPreview.RemoveBinding(Image.SourceProperty);
            control.snapPreview.Source = ImageSource.FromStream(() => str);
        }
    }
    public string DetectionCount { get; set; } = "-";
    public string DetectorText { get; set; } = "No barcode detected";
    public Stream Stream {
        get { return (Stream)GetValue(StreamProperty); }
        set { SetValue(StreamProperty, value); } 
    }

    private static readonly Size EmptyDims = new Size(double.NaN);

    private Size _cameraDims = EmptyDims;
    private Size CameraDims => (_cameraDims == EmptyDims ? queryDims(cameraView, validDims => _cameraDims = validDims) : _cameraDims);

    private Size _cameraOverlayDims = EmptyDims;
    private Size CameraOverlayDims => (_cameraOverlayDims == EmptyDims ? queryDims(cameraViewOverlay, validDims => _cameraOverlayDims = validDims) : _cameraOverlayDims);

    public MainPage()
	{
		InitializeComponent();
        cameraView.CamerasLoaded += CameraView_CamerasLoaded;
        cameraView.TargetDetectionAttempted += CameraView_TargetDetectionAttempted;
        cameraView.TargetDetected += CameraView_TargetDetected;
        cameraView.DetectorOptions = new DetectorTargetImageDecodeOptions
        {
// TODO
        };
        BindingContext = cameraView;

        _boxAndLabelDrawable = new BoxAndLabelDrawable();
        cameraViewOverlay.Drawable = _boxAndLabelDrawable;
   }

    private static Size queryDims(IView view, Func<Size, Size> setAction)
    {
       double w = view.Width, h = view.Height;
       return !double.IsNaN(w) && !double.IsNaN(h)
          ? setAction(new Size(w, h))
          : EmptyDims;
    }


    private void CameraView_TargetDetectionAttempted(object sender, DetectionAttemptedEventArgs args)
    {
       string resultCount = args.ResultCount;
       //Debug.WriteLine($"Setting {nameof(SizedPage)}.{nameof(DetectionCount)} = {resultCount}");
       DetectionCount = resultCount;
       OnPropertyChanged(nameof(DetectionCount));
    }

    private void CameraView_TargetDetected(object sender, DetectorEventArgs2 args)
    {
       DetectorResult result = args.DetectorResultDatum[0];
       string text = result.Text;
       //Debug.WriteLine($"Setting {nameof(SizedPage)}.{nameof(DetectorText)} = {text}");
       DetectorText = "Detected: " + text;
       OnPropertyChanged(nameof(DetectorText));

       convertAndSaveResultPoints(result.ResultPoints, result.Text, args.Metadata.NormalizeBitmapCoords);
        cameraViewOverlay.Invalidate();
    }

    private void convertAndSaveResultPoints((DetectorResultPoint, DetectorResultPoint) resultPoints, string detectorText,
       Func<System.Drawing.Point, Point> normalizeBitmapCoords)
    {
       (double srcWidth, double srcHeight) = CameraDims;
       (double destWidth, double destHeight) = CameraOverlayDims;
Debug.WriteLine("............................................................................");
Debug.WriteLine("CameraDims       : {0:F0}x{1:F0}",srcWidth,srcHeight);
Debug.WriteLine("CameraOverlayDims: {0:F0}x{1:F0}",destWidth,destHeight);

       Point convertBitmapCoordsToOverlay(System.Drawing.Point bitmapCoords)
       {
          Point normalized = normalizeBitmapCoords(bitmapCoords);
Debug.WriteLine("converted bitmap to norm      : {0} -> {1}", bitmapCoords, normalized);
          Point overlayCoords=convertNormalizedCoordsToOverlay(normalized);
Debug.WriteLine("converted norm to overlay     : {0} -> {1}", normalized, overlayCoords);
Debug.WriteLine("=> converted bitmap to overlay: {0} -> {1}", bitmapCoords, overlayCoords);
          return overlayCoords;
       }

       Point convertNormalizedCoordsToOverlay(Point normalizedCoords)
          => new(normalizedCoords.X * destWidth, normalizedCoords.Y * destHeight);

       (PointF from, PointF to) rectCorners = (
          convertAndScale(resultPoints.Item1),
          convertAndScale(resultPoints.Item2));

       _boxAndLabelDrawable.Show(rectCorners, detectorText);

       PointF convertAndScale(DetectorResultPoint resultPoint)
          => convertBitmapCoordsToOverlay(resultPoint.AsPoint());
    }

    private void CameraView_CamerasLoaded(object sender, EventArgs e)
    {
        cameraPicker.ItemsSource = cameraView.Cameras;
        cameraPicker.SelectedIndex = 0;
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        if (cameraPicker.SelectedItem != null && cameraPicker.SelectedItem is CameraInfo camera)
        {
            cameraLabel.BackgroundColor = Colors.White;
            cameraView.Camera = camera;
            var result = await cameraView.StartCameraAsync();
            Debug.WriteLine("Start camera result " + result);
            // var result2 = await startDuplicateViewAsync(cameraView.);
            // Debug.WriteLine("Start duplicate view result " + result2);
        }
        else
        {
            cameraLabel.BackgroundColor = Colors.Red;
        }
    }
    private async void OnStopClicked(object sender, EventArgs e)
    {
        var result = await cameraView.StopCameraAsync();
        Debug.WriteLine("Stop camera result " + result);
    }
    private void OnSnapClicked(object sender, EventArgs e)
    {        
        var result = cameraView.GetSnapShot(ImageFormat.PNG);
        if (result != null)
            snapPreview.Source = result;
    }

    private void CheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        cameraView.MirroredImage = e.Value;
    }
    private void CheckBox4_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        cameraView.TorchEnabled = e.Value;
    }
    private void CheckBox3_CheckedChanged(object sender, CheckedChangedEventArgs e)
    {
        cameraView.TargetDetectorEnabled = e.Value;
    }

    private void Stepper_ValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (cameraView != null) cameraView.ZoomFactor = (float)e.NewValue;
    }

    private void CameraPicker_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (cameraPicker.SelectedItem != null && cameraPicker.SelectedItem is CameraInfo camera)
        {
            torchLabel.IsEnabled = torchCheck.IsEnabled = camera.HasFlashUnit;
            if (camera.MaxZoomFactor > 1)
            {
                zoomLabel.IsEnabled = zoomStepper.IsEnabled = true;
                zoomStepper.Maximum = camera.MaxZoomFactor;
            }else
                zoomLabel.IsEnabled = zoomStepper.IsEnabled = true;
            cameraView.Camera = camera;
        }
    }

    private void Entry_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (float.TryParse(e.NewTextValue, out float value))
        {
            cameraView.AutoSnapShotSeconds = value;
            if (value <= 0)
                snapPreview.RemoveBinding(Image.SourceProperty);
            else
                snapPreview.SetBinding(Image.SourceProperty, nameof(cameraView.SnapShot));
        }
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        cameraView.ForceAutoFocus();
    }
}
