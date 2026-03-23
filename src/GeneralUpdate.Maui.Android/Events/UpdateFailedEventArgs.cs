using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Events;

/// <summary>
/// Provides data for update failure notifications.
/// </summary>
public sealed class UpdateFailedEventArgs : EventArgs
{
    public UpdateFailedEventArgs(UpdateFailureReason reason, string message, Exception? exception = null, UpdatePackageInfo? packageInfo = null)
    {
        Reason = reason;
        Message = message;
        Exception = exception;
        PackageInfo = packageInfo;
    }

    public UpdateFailureReason Reason { get; }

    public string Message { get; }

    public Exception? Exception { get; }

    public UpdatePackageInfo? PackageInfo { get; }
}
