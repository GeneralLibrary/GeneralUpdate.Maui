using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Events;

/// <summary>
/// Provides data for update discovery notifications.
/// </summary>
public sealed class UpdateFoundEventArgs : EventArgs
{
    public UpdateFoundEventArgs(UpdatePackageInfo packageInfo)
    {
        PackageInfo = packageInfo;
    }

    public UpdatePackageInfo PackageInfo { get; }
}
