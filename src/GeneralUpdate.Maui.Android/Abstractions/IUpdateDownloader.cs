using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Downloads update packages using resumable transfer semantics.
/// </summary>
public interface IUpdateDownloader
{
    Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        string targetFilePath,
        string temporaryFilePath,
        TimeSpan progressReportInterval,
        IProgress<DownloadStatistics>? progress,
        CancellationToken cancellationToken);
}
