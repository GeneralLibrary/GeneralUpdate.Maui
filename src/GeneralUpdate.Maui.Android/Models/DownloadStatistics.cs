namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents download statistics emitted during package download.
/// </summary>
public sealed class DownloadStatistics
{
    public long DownloadedBytes { get; init; }

    public long TotalBytes { get; init; }

    public long RemainingBytes { get; init; }

    public double ProgressPercentage { get; init; }

    public double BytesPerSecond { get; init; }
}
