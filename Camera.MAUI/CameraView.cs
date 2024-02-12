using System.Collections.ObjectModel;
using System.Diagnostics;
#if ANDROID
using Camera.MAUI.Platforms.Android;
using DecodeDataType = Android.Graphics.Bitmap;
#elif WINDOWS
using Camera.MAUI.Platforms.Windows;
using DecodeDataType = Windows.Graphics.Imaging.SoftwareBitmap;
#else
using DecodeDataType = System.Object;
#endif

namespace Camera.MAUI
{
    public class CameraView : View, ICameraView
   {
      public static readonly BindableProperty SelfProperty = BindableProperty.Create(nameof(Self), typeof(CameraView), typeof(CameraView), null, BindingMode.OneWayToSource);
      public static readonly BindableProperty TorchEnabledProperty = BindableProperty.Create(nameof(TorchEnabled), typeof(bool), typeof(CameraView), false);
      public static readonly BindableProperty CamerasProperty = BindableProperty.Create(nameof(Cameras), typeof(ObservableCollection<CameraInfo>), typeof(CameraView), new ObservableCollection<CameraInfo>());
      public static readonly BindableProperty NumCamerasDetectedProperty = BindableProperty.Create(nameof(NumCamerasDetected), typeof(int), typeof(CameraView), 0);
      public static readonly BindableProperty CameraProperty = BindableProperty.Create(nameof(Camera), typeof(CameraInfo), typeof(CameraView), null, propertyChanged:CameraChanged);
      public static readonly BindableProperty MirroredImageProperty = BindableProperty.Create(nameof(MirroredImage), typeof(bool), typeof(CameraView), false);
      public static readonly BindableProperty TargetDetectorEnabledProperty = BindableProperty.Create(nameof(TargetDetectorEnabled), typeof(bool), typeof(CameraView), false);
      public static readonly BindableProperty TargetDetectorFrameRateProperty = BindableProperty.Create(nameof(TargetDetectorFrameRate), typeof(int), typeof(CameraView), 10);
      public static readonly BindableProperty DetectorOptionsProperty = BindableProperty.Create(nameof(DetectorOptions), typeof(DetectorTargetImageDecodeOptions), typeof(CameraView), new DetectorTargetImageDecodeOptions(), propertyChanged:TargetImageDecodeOptionsChanged);
      public static readonly BindableProperty DetectionCountProperty = BindableProperty.Create(nameof(DetectionCount), typeof(string), typeof(CameraView), "-");
      public static readonly BindableProperty DetectorResultsProperty = BindableProperty.Create(nameof(DetectorResults), typeof(DetectorResult[]), typeof(CameraView), null);
      public static readonly BindableProperty ZoomFactorProperty = BindableProperty.Create(nameof(ZoomFactor), typeof(float), typeof(CameraView), 1f);
      public static readonly BindableProperty AutoSnapShotSecondsProperty = BindableProperty.Create(nameof(AutoSnapShotSeconds), typeof(float), typeof(CameraView), 0f);
      public static readonly BindableProperty AutoSnapShotFormatProperty = BindableProperty.Create(nameof(AutoSnapShotFormat), typeof(ImageFormat), typeof(CameraView), ImageFormat.PNG);
      public static readonly BindableProperty SnapShotProperty = BindableProperty.Create(nameof(SnapShot), typeof(ImageSource), typeof(CameraView), null, BindingMode.OneWayToSource);
      public static readonly BindableProperty SnapShotStreamProperty = BindableProperty.Create(nameof(SnapShotStream), typeof(Stream), typeof(CameraView), null, BindingMode.OneWayToSource);
      public static readonly BindableProperty TakeAutoSnapShotProperty = BindableProperty.Create(nameof(TakeAutoSnapShot), typeof(bool), typeof(CameraView), false, propertyChanged:TakeAutoSnapShotChanged);
      public static readonly BindableProperty AutoSnapShotAsImageSourceProperty = BindableProperty.Create(nameof(AutoSnapShotAsImageSource), typeof(bool), typeof(CameraView), false);
      public static readonly BindableProperty AutoStartPreviewProperty = BindableProperty.Create(nameof(AutoStartPreview), typeof(bool), typeof(CameraView), false, propertyChanged: AutoStartPreviewChanged);
      public static readonly BindableProperty MetadataProperty = BindableProperty.Create(nameof(Metadata), typeof(CameraViewMetadata), typeof(CameraView), null);

      /// <summary>
      /// Binding property for use this control in MVVM.
      /// </summary>
      public CameraView Self
      {
         get { return (CameraView)GetValue(SelfProperty); }
         set { SetValue(SelfProperty, value); }
      }
      /// <summary>
      /// Turns the camera torch on and off if available. This is a bindable property.
      /// </summary>
      public bool TorchEnabled
      {
         get { return (bool)GetValue(TorchEnabledProperty); }
         set { SetValue(TorchEnabledProperty, value); }
      }
      /// <summary>
      /// List of available cameras in the device. This is a bindable property.
      /// </summary>
      public ObservableCollection<CameraInfo> Cameras
      {
         get { return (ObservableCollection<CameraInfo>)GetValue(CamerasProperty); }
         set { SetValue(CamerasProperty, value); }
      }
      /// <summary>
      /// Indicates the number of available cameras in the device.
      /// </summary>
      public int NumCamerasDetected
      {
         get { return (int)GetValue(NumCamerasDetectedProperty); }
         set { SetValue(NumCamerasDetectedProperty, value); }
      }
      /// <summary>
      /// Set the camera to use by the controler. This is a bindable property.
      /// </summary>
      public CameraInfo Camera
      {
         get { return (CameraInfo)GetValue(CameraProperty); }
         set { SetValue(CameraProperty, value); }
      }
      /// <summary>
      /// Turns a mirror image of the camera on and off. This is a bindable property.
      /// </summary>
      public bool MirroredImage
      {
         get { return (bool)GetValue(MirroredImageProperty); }
         set { SetValue(MirroredImageProperty, value); }
      }
      /// <summary>
      /// Turns on and off the barcode detection. This is a bindable property.
      /// </summary>
      public bool TargetDetectorEnabled
      {
         get { return (bool)GetValue(TargetDetectorEnabledProperty); }
         set { SetValue(TargetDetectorEnabledProperty, value); }
      }
      /// <summary>
      /// Indicates every how many frames the control tries to detect a barcode in the image. This is a bindable property.
      /// </summary>
      public int TargetDetectorFrameRate
      {
         get { return (int)GetValue(TargetDetectorFrameRateProperty); }
         set { SetValue(TargetDetectorFrameRateProperty, value); }
      }
      /// <summary>
      /// Indicates the maximun number of simultaneous running threads for barcode detection
      /// </summary>
      public int TargetDetectorMaxThreads { get; set; } = 3;
      internal int currentThreads = 0;
      internal object currentThreadsLocker = new();
      /// <summary>
      /// Options for the barcode detection. This is a bindable property.
      /// </summary>
      public DetectorTargetImageDecodeOptions DetectorOptions
      {
         get { return (DetectorTargetImageDecodeOptions)GetValue(DetectorOptionsProperty); }
         set { SetValue(DetectorOptionsProperty, value); }
      }

      public string DetectionCount
      {
         get { return (string)GetValue(DetectionCountProperty); }
         set { SetValue(DetectionCountProperty, value); }
      }

      /// <summary>
      /// It refresh each time a barcode is detected if TargetDetectorEnabled porperty is true
      /// </summary>
      public DetectorResult[] DetectorResults
      {
         get { return (DetectorResult[])GetValue(DetectorResultsProperty); }
         set { SetValue(DetectorResultsProperty, value); }
      }
      /// <summary>
      /// The zoom factor for the current camera in use. This is a bindable property.
      /// </summary>
      public float ZoomFactor
      {
         get { return (float)GetValue(ZoomFactorProperty); }
         set { SetValue(ZoomFactorProperty, value); }
      }
      /// <summary>
      /// Indicates the minimum zoom factor for the camera in use. This property is refreshed when the "Camera" property change.
      /// </summary>
      public float MinZoomFactor
      {
         get
         {
            if (Camera != null)
               return Camera.MinZoomFactor;
            else
               return 1f;
         }
      }
      /// <summary>
      /// Indicates the maximum zoom factor for the camera in use. This property is refreshed when the "Camera" property change.
      /// </summary>
      public float MaxZoomFactor
      {
         get
         {
            if (Camera != null)
               return Camera.MaxZoomFactor;
            else
               return 1f;
         }
      }
      /// <summary>
      /// Sets how often the SnapShot property is updated in seconds.
      /// Default 0: no snapshots are taken
      /// WARNING! A low frequency directly impacts over control performance and memory usage (with AutoSnapShotAsImageSource = true)
      /// </summary>
      public float AutoSnapShotSeconds
      {
         get { return (float)GetValue(AutoSnapShotSecondsProperty); }
         set { SetValue(AutoSnapShotSecondsProperty, value); }
      }
      /// <summary>
      /// Sets the snaphost image format
      /// </summary>
      public ImageFormat AutoSnapShotFormat
      {
         get { return (ImageFormat)GetValue(AutoSnapShotFormatProperty); }
         set { SetValue(AutoSnapShotFormatProperty, value); }
      }
      /// <summary>
      /// Refreshes according to the frequency set in the AutoSnapShotSeconds property (if AutoSnapShotAsImageSource is set to true)
      /// or when GetSnapShot is called or TakeAutoSnapShot is set to true
      /// </summary>
      public ImageSource SnapShot
      {
         get { return (ImageSource)GetValue(SnapShotProperty); }
         private set { SetValue(SnapShotProperty, value); }
      }
      /// <summary>
      /// Refreshes according to the frequency set in the AutoSnapShotSeconds property or when GetSnapShot is called.
      /// WARNING. Each time a snapshot is made, the previous stream is disposed.
      /// </summary>
      public Stream SnapShotStream
      {
         get { return (Stream)GetValue(SnapShotStreamProperty); }
         internal set { SetValue(SnapShotStreamProperty, value); }
      }
      /// <summary>
      /// Change from false to true refresh SnapShot property
      /// </summary>
      public bool TakeAutoSnapShot
      {
         get { return (bool)GetValue(TakeAutoSnapShotProperty); }
         set { SetValue(TakeAutoSnapShotProperty, value); }
      }
      /// <summary>
      /// If true SnapShot property is refreshed according to the frequency set in the AutoSnapShotSeconds property
      /// </summary>
      public bool AutoSnapShotAsImageSource
      {
         get { return (bool)GetValue(AutoSnapShotAsImageSourceProperty); }
         set { SetValue(AutoSnapShotAsImageSourceProperty, value); }
      }
      /// <summary>
      /// Starts/Stops the Preview if camera property has been set
      /// </summary>
      public bool AutoStartPreview
      {
         get { return (bool)GetValue(AutoStartPreviewProperty); }
         set { SetValue(AutoStartPreviewProperty, value); }
      }

      public record CameraViewMetadata(Func<System.Drawing.Point, Point> NormalizeBitmapCoords);

      public CameraViewMetadata Metadata
      {
         get { return (CameraViewMetadata)GetValue(MetadataProperty); }
         set { SetValue(MetadataProperty, value); }
      }


      /// <summary>
      /// If true BarcodeDetected event will invoke only if a Results is diferent from preview Results
      /// </summary>
      public bool ControlDetectorResultDuplicate { get; set; } = false;
      public delegate void DetectorResultHandler(object sender, DetectorEventArgs2 args);

      public event EventHandler<DetectionAttemptedEventArgs> TargetDetectionAttempted;

      /// <summary>
      /// Event launched every time a code is detected in the image if "TargetDetectorEnabled" is set to true.
      /// </summary>
      public event DetectorResultHandler TargetDetected;
      /// <summary>
      /// Event launched when "Cameras" property has been loaded.
      /// </summary>
      public event EventHandler CamerasLoaded;
      /// <summary>
      /// A static reference to the last CameraView created.
      /// </summary>
      public static CameraView Current { get; set; }

      private readonly DetectorGeneric Detector;
      internal DateTime lastSnapshot = DateTime.Now;
      internal Size PhotosResolution = new(0, 0);

      public CameraView()
      {
         Detector = new DetectorGeneric();
         HandlerChanged += CameraView_HandlerChanged;
         Current = this;
      }
      private void CameraView_HandlerChanged(object sender, EventArgs e)
      {
         if (Handler != null)
         {
            CamerasLoaded?.Invoke(this, EventArgs.Empty);
            Self = this;
         }
      }
      internal void RefreshSnapshot(ImageSource img)
      {
         if (AutoSnapShotAsImageSource)
         {
            SnapShot = img;
            OnPropertyChanged(nameof(SnapShot));
         }
         OnPropertyChanged(nameof(SnapShotStream));
         lastSnapshot = DateTime.Now;
      }

      internal void DecodeTargetImage(DecodeDataType sourceImageData)
      {
         //Debug.WriteLine("Decoding target image");

         DetectorDataSource source = default;
         try
         {
            long ticks = DateTime.Now.Ticks;

#if ANDROID
        source = AndroidDetectorDataSource.Create(sourceImageData);
#elif WINDOWS
        source = WindowsDetectorDataSource.Create(sourceImageData);
#endif

            TimeSpan elapsed = TimeSpan.FromTicks(DateTime.Now.Ticks - ticks);
            //Debug.WriteLine("Decoded target image in {0}", elapsed);
         }
         catch (Exception ex)
         {
            Debug.WriteLine("Exception!! " + ex);
         }
         try
         {
            DetectorResult[] results = null;
            // if (DetectorOptions.ReadMultipleCodes)
            //    results = Detector.DecodeMultiple(source);
            // else
            // {
            DetectorResult result2 = Detector.Decode(source);
            if (result2 != null) results = new DetectorResult[] { result2 };
            //Debug.WriteLine("Decode result: {0}", (object)(result2?.ToString() ?? "(null)"));
            // }

            TargetDetectionAttempted?.Invoke(this, new DetectionAttemptedEventArgs()
               {
                  ResultCount = (results?.Length ?? 0).ToString()
               }
            );

            if (results?.Length > 0)
            {
               bool refresh = true;
               if (ControlDetectorResultDuplicate)
               {
                  if (DetectorResults != null)
                  {
                     foreach (var result in results)
                     {
                        refresh = DetectorResults.FirstOrDefault((DetectorResult detectorResultDatum) =>
                           detectorResultDatum.Text == result.Text) == null;
                        if (refresh) break;
                     }
                  }
               }

               if (refresh)
               {
                  //Debug.WriteLine($"Setting {nameof(CameraView)}.{nameof(DetectorResults)} = {results?.FirstOrDefault()}");
                  DetectorResults = results;
                  OnPropertyChanged(nameof(DetectorResults));

                  TargetDetected?.Invoke(this, new DetectorEventArgs2()
                  {
                     DetectorResultDatum = results,
                     Metadata = this.Metadata
                  });
               }
            }
         }
         catch (Exception ex)
         {
            Debug.WriteLine("Exception: {0}", ex);
         }
      }

      private static void CameraChanged(BindableObject bindable, object oldValue, object newValue)
      {
         if (newValue != null && oldValue != newValue && bindable is CameraView cameraView && newValue is CameraInfo)
         {
            cameraView.OnPropertyChanged(nameof(MinZoomFactor));
            cameraView.OnPropertyChanged(nameof(MaxZoomFactor));
         }
      }
      private static void TakeAutoSnapShotChanged(BindableObject bindable, object oldValue, object newValue)
      {
         if ((bool)newValue && !(bool)oldValue && bindable is CameraView cameraView)
         {
            cameraView.RefreshSnapshot(cameraView.GetSnapShot(cameraView.AutoSnapShotFormat));
         }
      }
      private static async void AutoStartPreviewChanged(BindableObject bindable, object oldValue, object newValue)
      {
         if (oldValue != newValue && bindable is CameraView control)
         {
            try
            {
               if ((bool)newValue)
                  await control.StartCameraAsync();
               else
                  await control.StopCameraAsync();
            }
            catch { }
                    
         }
      }
      private static void TargetImageDecodeOptionsChanged(BindableObject bindable, object oldValue, object newValue)
      {
         if (newValue != null && oldValue != newValue && bindable is CameraView cameraView && newValue is DetectorTargetImageDecodeOptions options)
         {
            // TODO
         }
      }
      /// <summary>
      /// Start playback of the selected camera async. "Camera" property must not be null.
      /// <paramref name="Resolution"/> Indicates the resolution for the preview and photos taken with TakePhotoAsync (must be in Camera.AvailableResolutions). If width or height is 0, max resolution will be taken.
      /// </summary>
      public async Task<CameraResult> StartCameraAsync(Size Resolution = default)
      {
         CameraResult result = CameraResult.AccessError;
         if (Camera != null)
         {
            Debug.WriteLine("------------------------------------------------------------------------------------------------------");
            Debug.WriteLine("Camera started {0}", Resolution);
            PhotosResolution = Resolution;
            if (Resolution.Width != 0 && Resolution.Height != 0)
            {
               if (!Camera.AvailableResolutions.Any(r => r.Width == Resolution.Width && r.Height == Resolution.Height))
                  return CameraResult.ResolutionNotAvailable;
            }
            if (Handler != null && Handler is CameraViewHandler handler)
            {
               result = await handler.StartCameraAsync(Resolution);
               if (result == CameraResult.Success)
               {
                  DetectorResults = null;
                  OnPropertyChanged(nameof(MinZoomFactor));
                  OnPropertyChanged(nameof(MaxZoomFactor));
               }
            }
         }
         else
            result = CameraResult.NoCameraSelected;

         return result;
      }
      /// <summary>
      /// Stop playback of the selected camera async.
      /// </summary>
      public async Task<CameraResult> StopCameraAsync()
      {
         CameraResult result = CameraResult.AccessError;
         if (Handler != null && Handler is CameraViewHandler handler)
         {
            result = await handler.StopCameraAsync();
         }
         return result;
      }
      /// <summary>
      /// Takes a capture form the active camera playback.
      /// </summary>
      /// <param name="imageFormat">The capture image format</param>
      public ImageSource GetSnapShot(ImageFormat imageFormat = ImageFormat.PNG)
      {
         if (Handler != null && Handler is CameraViewHandler handler)
         {
            SnapShot = handler.GetSnapShot(imageFormat);
         }
         return SnapShot;
      }
      /// <summary>
      /// Saves a capture form the active camera playback in a file
      /// </summary>
      /// <param name="imageFormat">The capture image format</param>
      /// <param name="SnapFilePath">Full path for the file</param>
      public async Task<bool> SaveSnapShot(ImageFormat imageFormat, string SnapFilePath)
      {
         bool result = false;
         if (Handler != null && Handler is CameraViewHandler handler)
         {
            result = await handler.SaveSnapShot(imageFormat, SnapFilePath);
         }
         return result;
      }
      /// <summary>
      /// Force execute the camera autofocus trigger.
      /// </summary>
      public void ForceAutoFocus()
      {
         if (Handler != null && Handler is CameraViewHandler handler)
         {
            handler.ForceAutoFocus();
         }
      }
      /// <summary>
      /// Forces the device specific control dispose
      /// </summary>
      public void ForceDisposeHandler()
      {
         if (Handler != null && Handler is CameraViewHandler handler)
         {
            handler.ForceDispose();
         }
      }
      internal void RefreshDevices()
      {
         Task.Run(() => { 
            OnPropertyChanged(nameof(Cameras)); 
            NumCamerasDetected = Cameras.Count;
         });
      }
      public static async Task<bool> RequestPermissions(bool withStorageWrite = false)
      {
         var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
         if (status != PermissionStatus.Granted)
         {
            status = await Permissions.RequestAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted) return false;
         }
         if (withStorageWrite)
         {
            status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
            if (status != PermissionStatus.Granted)
            {
               status = await Permissions.RequestAsync<Permissions.StorageWrite>();
               if (status != PermissionStatus.Granted) return false;
            }
         }
         return true;
      }
   }
}
