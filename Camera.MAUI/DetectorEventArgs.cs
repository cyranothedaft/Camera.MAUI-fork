namespace Camera.MAUI;



public record DetectionAttemptedEventArgs
{
   public static DetectionAttemptedEventArgs Empty = new DetectionAttemptedEventArgs();
   public string ResultCount { get; set; }
}

public record DetectorEventArgs
{
    public DetectorResult[] DetectorResultDatum { get; init; }
}


public record DetectorEventArgs2
{
    public DetectorResult[] DetectorResultDatum { get; init; }
    public CameraView.CameraViewMetadata Metadata { get; init; }
}
