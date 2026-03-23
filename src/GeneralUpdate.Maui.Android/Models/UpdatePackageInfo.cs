namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents update package metadata used for update discovery and download.
/// </summary>
public sealed class UpdatePackageInfo
{
    public string Version { get; init; } = string.Empty;

    public string? VersionName { get; init; }

    public string? ReleaseNotes { get; init; }

    public string DownloadUrl { get; init; } = string.Empty;

    public long? PackageSize { get; init; }

    public string Sha256 { get; init; } = string.Empty;

    public DateTimeOffset? PublishTime { get; init; }

    public bool ForceUpdate { get; init; }

    public string? ApkFileName { get; init; }
}
