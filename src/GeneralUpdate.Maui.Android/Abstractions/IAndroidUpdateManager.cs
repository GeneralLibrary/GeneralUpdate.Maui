using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Events;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Provides orchestration APIs for Android update workflows.
/// </summary>
public interface IAndroidUpdateManager
{
    event EventHandler<UpdateFoundEventArgs>? UpdateFound;

    event EventHandler<DownloadProgressChangedEventArgs>? DownloadProgressChanged;

    event EventHandler<UpdateCompletedEventArgs>? UpdateCompleted;

    event EventHandler<UpdateFailedEventArgs>? UpdateFailed;

    UpdateState CurrentState { get; }

    Task<UpdateCheckResult> CheckForUpdateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken);

    Task<UpdateExecutionResult> ExecuteUpdateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken);
}
