using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Triggers Android package installer for an APK file.
/// </summary>
public interface IApkInstaller
{
    Task TriggerInstallAsync(string apkFilePath, AndroidInstallOptions options, CancellationToken cancellationToken);

    bool CanRequestPackageInstalls();
}
