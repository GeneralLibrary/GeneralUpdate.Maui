namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents the download output of an APK package.
/// </summary>
public sealed class DownloadResult
{
    public required string FilePath { get; init; }

    public required string TemporaryFilePath { get; init; }

    public required long TotalBytes { get; init; }

    public required bool UsedResumableDownload { get; init; }
}
