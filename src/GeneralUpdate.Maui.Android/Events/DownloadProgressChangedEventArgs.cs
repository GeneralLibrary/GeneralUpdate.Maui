using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Events;

/// <summary>
/// Provides data for update download progress notifications.
/// </summary>
public sealed class DownloadProgressChangedEventArgs : EventArgs
{
    public DownloadProgressChangedEventArgs(UpdatePackageInfo packageInfo, DownloadStatistics statistics, string statusDescription)
    {
        PackageInfo = packageInfo;
        Statistics = statistics;
        StatusDescription = statusDescription;
    }

    public UpdatePackageInfo PackageInfo { get; }

    public DownloadStatistics Statistics { get; }

    public string StatusDescription { get; }
}
