using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Events;

/// <summary>
/// Provides data for update completion notifications.
/// </summary>
public sealed class UpdateCompletedEventArgs : EventArgs
{
    public UpdateCompletedEventArgs(UpdatePackageInfo packageInfo, UpdateCompletionStage stage, string? packagePath = null)
    {
        PackageInfo = packageInfo;
        Stage = stage;
        PackagePath = packagePath;
    }

    public UpdatePackageInfo PackageInfo { get; }

    public UpdateCompletionStage Stage { get; }

    public string? PackagePath { get; }
}
