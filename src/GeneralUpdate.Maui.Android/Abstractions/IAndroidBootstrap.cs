using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Events;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Provides orchestration APIs for Android update workflows.
/// </summary>
public interface IAndroidBootstrap
{
    event EventHandler<ValidateEventArgs>? AddListenerValidate;

    event EventHandler<DownloadProgressChangedEventArgs>? AddListenerDownloadProgressChanged;

    event EventHandler<UpdateCompletedEventArgs>? AddListenerUpdateCompleted;

    event EventHandler<UpdateFailedEventArgs>? AddListenerUpdateFailed;

    UpdateState CurrentState { get; }

    Task<UpdateCheckResult> ValidateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken);

    Task<UpdateExecutionResult> ExecuteUpdateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken);
}
