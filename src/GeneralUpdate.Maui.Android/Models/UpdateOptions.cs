namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Provides options for update workflow behavior.
/// </summary>
public sealed class UpdateOptions
{
    public string CurrentVersion { get; init; } = string.Empty;

    public string? DownloadDirectory { get; init; }

    public string TemporaryFileExtension { get; init; } = ".downloading";

    public bool DeleteCorruptedPackageOnFailure { get; init; } = true;

    public TimeSpan ProgressReportInterval { get; init; } = TimeSpan.FromMilliseconds(500);

    public AndroidInstallOptions InstallOptions { get; init; } = new();
}
