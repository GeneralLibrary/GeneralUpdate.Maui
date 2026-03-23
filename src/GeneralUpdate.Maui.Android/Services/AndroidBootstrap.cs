using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Events;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// Orchestrates update discovery, package download, integrity validation, and APK installation triggering.
/// </summary>
public sealed class AndroidBootstrap : IAndroidBootstrap
{
    private readonly IUpdateDownloader _downloader;
    private readonly IHashValidator _hashValidator;
    private readonly IApkInstaller _apkInstaller;
    private readonly IUpdateStorageProvider _storageProvider;
    private readonly IUpdateLogger _logger;

    public AndroidBootstrap(
        IUpdateDownloader downloader,
        IHashValidator hashValidator,
        IApkInstaller apkInstaller,
        IUpdateStorageProvider storageProvider,
        IUpdateLogger? logger = null)
    {
        _downloader = downloader ?? throw new ArgumentNullException(nameof(downloader));
        _hashValidator = hashValidator ?? throw new ArgumentNullException(nameof(hashValidator));
        _apkInstaller = apkInstaller ?? throw new ArgumentNullException(nameof(apkInstaller));
        _storageProvider = storageProvider ?? throw new ArgumentNullException(nameof(storageProvider));
        _logger = logger ?? NullUpdateLogger.Instance;
    }

    public event EventHandler<ValidateEventArgs>? AddListenerValidate;

    public event EventHandler<DownloadProgressChangedEventArgs>? AddListenerDownloadProgressChanged;

    public event EventHandler<UpdateCompletedEventArgs>? AddListenerUpdateCompleted;

    public event EventHandler<UpdateFailedEventArgs>? AddListenerUpdateFailed;

    public UpdateState CurrentState { get; private set; }

    public Task<UpdateCheckResult> ValidateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ChangeState(UpdateState.Checking);

        try
        {
            ValidateInputs(packageInfo, options);

            var currentVersion = new Version(options.CurrentVersion);
            var latestVersion = new Version(packageInfo.Version);
            if (latestVersion <= currentVersion)
            {
                ChangeState(UpdateState.None);
                return Task.FromResult(UpdateCheckResult.NoUpdate());
            }

            ChangeState(UpdateState.UpdateAvailable);
            AddListenerValidate?.Invoke(this, new ValidateEventArgs(packageInfo));
            return Task.FromResult(UpdateCheckResult.UpdateAvailable(packageInfo));
        }
        catch (Exception ex)
        {
            ChangeState(UpdateState.Failed);
            NotifyFailure(UpdateFailureReason.InvalidInput, ex.Message, ex, packageInfo);
            return Task.FromResult(UpdateCheckResult.NoUpdate(ex.Message));
        }
    }

    public async Task<UpdateExecutionResult> ExecuteUpdateAsync(UpdatePackageInfo packageInfo, UpdateOptions options, CancellationToken cancellationToken)
    {
        try
        {
            ValidateInputs(packageInfo, options);
            var (targetFilePath, temporaryFilePath) = _storageProvider.GetPackagePaths(packageInfo, options);

            ChangeState(UpdateState.Downloading);
            _logger.LogInfo($"Starting update package download from '{packageInfo.DownloadUrl}'.");

            var downloadProgress = new Progress<DownloadStatistics>(stats =>
            {
                AddListenerDownloadProgressChanged?.Invoke(this, new DownloadProgressChangedEventArgs(packageInfo, stats, "Downloading update package."));
            });

            await _downloader.DownloadAsync(
                packageInfo,
                targetFilePath,
                temporaryFilePath,
                options.ProgressReportInterval,
                downloadProgress,
                cancellationToken).ConfigureAwait(false);

            _storageProvider.ReplaceTemporaryWithFinal(temporaryFilePath, targetFilePath);
            AddListenerUpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(packageInfo, UpdateCompletionStage.DownloadCompleted, targetFilePath));

            ChangeState(UpdateState.Verifying);
            _logger.LogInfo("Starting SHA256 verification for downloaded update package.");

            var hashResult = await _hashValidator.ValidateSha256Async(targetFilePath, packageInfo.Sha256, progress: null, cancellationToken).ConfigureAwait(false);
            if (!hashResult.IsSuccess)
            {
                if (options.DeleteCorruptedPackageOnFailure)
                {
                    _storageProvider.DeleteFileIfExists(targetFilePath);
                }

                throw new InvalidDataException(hashResult.FailureReason ?? "Integrity check failed.");
            }

            AddListenerUpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(packageInfo, UpdateCompletionStage.VerificationCompleted, targetFilePath));

            ChangeState(UpdateState.ReadyToInstall);

            ChangeState(UpdateState.Installing);
            _logger.LogInfo("Triggering Android package installer.");
            await _apkInstaller.TriggerInstallAsync(targetFilePath, options.InstallOptions, cancellationToken).ConfigureAwait(false);
            AddListenerUpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(packageInfo, UpdateCompletionStage.InstallationTriggered, targetFilePath));

            ChangeState(UpdateState.Completed);
            AddListenerUpdateCompleted?.Invoke(this, new UpdateCompletedEventArgs(packageInfo, UpdateCompletionStage.WorkflowCompleted, targetFilePath));

            return UpdateExecutionResult.Success(targetFilePath);
        }
        catch (OperationCanceledException ex)
        {
            ChangeState(UpdateState.Canceled);
            NotifyFailure(UpdateFailureReason.Canceled, "Update operation was canceled.", ex, packageInfo);
            return UpdateExecutionResult.Failure(UpdateFailureReason.Canceled, "Update operation was canceled.");
        }
        catch (Exception ex)
        {
            ChangeState(UpdateState.Failed);
            var reason = MapFailureReason(ex);
            NotifyFailure(reason, ex.Message, ex, packageInfo);
            return UpdateExecutionResult.Failure(reason, ex.Message);
        }
    }

    private void ChangeState(UpdateState state)
    {
        CurrentState = state;
    }

    private static void ValidateInputs(UpdatePackageInfo packageInfo, UpdateOptions options)
    {
        ArgumentNullException.ThrowIfNull(packageInfo);
        ArgumentNullException.ThrowIfNull(options);

        if (string.IsNullOrWhiteSpace(options.CurrentVersion))
        {
            throw new ArgumentException("Current version cannot be null or empty.", nameof(options));
        }

        if (string.IsNullOrWhiteSpace(packageInfo.Version))
        {
            throw new ArgumentException("Update package version cannot be null or empty.", nameof(packageInfo));
        }

        if (string.IsNullOrWhiteSpace(packageInfo.DownloadUrl))
        {
            throw new ArgumentException("Update package download url cannot be null or empty.", nameof(packageInfo));
        }

        if (string.IsNullOrWhiteSpace(packageInfo.Sha256))
        {
            throw new ArgumentException("Update package SHA256 cannot be null or empty.", nameof(packageInfo));
        }
    }

    private void NotifyFailure(UpdateFailureReason reason, string message, Exception? ex, UpdatePackageInfo packageInfo)
    {
        _logger.LogError(message, ex);
        AddListenerUpdateFailed?.Invoke(this, new UpdateFailedEventArgs(reason, message, ex, packageInfo));
    }

    private static UpdateFailureReason MapFailureReason(Exception ex)
    {
        return ex switch
        {
            ArgumentException => UpdateFailureReason.InvalidInput,
            HttpRequestException => UpdateFailureReason.Network,
            InvalidDataException => UpdateFailureReason.IntegrityCheckFailed,
            IOException => UpdateFailureReason.FileAccess,
            UnauthorizedAccessException => UpdateFailureReason.InstallPermissionDenied,
            OperationCanceledException => UpdateFailureReason.Canceled,
            _ => UpdateFailureReason.Unknown
        };
    }
}
