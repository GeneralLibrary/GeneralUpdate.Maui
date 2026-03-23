using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Events;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;
using Xunit;

namespace GeneralUpdate.Maui.Android.Tests;

public class AndroidBootstrapTests
{
    [Fact]
    public async Task ValidateAsync_Should_ReportUpdateAvailable_WhenVersionIsNewer()
    {
        var manager = CreateManager();
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "ABCDEF"
        };

        var raised = false;
        manager.AddListenerValidate += (_, args) => raised = args.PackageInfo.Version == "2.0.0";

        var result = await manager.ValidateAsync(package, new UpdateOptions { CurrentVersion = "1.0.0" }, CancellationToken.None);

        Assert.True(result.IsUpdateAvailable);
        Assert.True(raised);
        Assert.Equal(UpdateState.UpdateAvailable, manager.CurrentState);
    }

    [Fact]
    public async Task ValidateAsync_Should_ReturnNoUpdate_WhenVersionIsNotNewer()
    {
        var manager = CreateManager();
        var package = new UpdatePackageInfo
        {
            Version = "1.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "ABCDEF"
        };

        var result = await manager.ValidateAsync(package, new UpdateOptions { CurrentVersion = "1.0.0" }, CancellationToken.None);

        Assert.False(result.IsUpdateAvailable);
        Assert.Equal(UpdateState.None, manager.CurrentState);
    }

    [Fact]
    public async Task ExecuteUpdateAsync_Should_FailWithIntegrityReason_WhenHashValidationFails()
    {
        var fakeDownloader = new FakeDownloader();
        var fakeValidator = new FakeValidator(new HashValidationResult
        {
            IsSuccess = false,
            ExpectedHash = "A",
            ActualHash = "B",
            FailureReason = "SHA mismatch"
        });

        var manager = new AndroidBootstrap(
            fakeDownloader,
            fakeValidator,
            new FakeInstaller(),
            new UpdateFileStore());

        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "A"
        };

        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        var failedRaised = false;
        manager.AddListenerUpdateFailed += (_, e) => failedRaised = e.Reason == UpdateFailureReason.IntegrityCheckFailed;

        var result = await manager.ExecuteUpdateAsync(package, new UpdateOptions
        {
            CurrentVersion = "1.0.0",
            DownloadDirectory = tempDir,
            InstallOptions = new AndroidInstallOptions { FileProviderAuthority = "com.test.fileprovider" }
        }, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(UpdateFailureReason.IntegrityCheckFailed, result.FailureReason);
        Assert.True(failedRaised);
    }

    [Fact]
    public async Task ExecuteUpdateAsync_Should_ReturnFailure_WhenConcurrentExecutionIsRequested()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var slowDownloader = new SlowDownloader(started, gate.Task);
        var manager = new AndroidBootstrap(
            slowDownloader,
            new FakeValidator(new HashValidationResult { IsSuccess = true, ExpectedHash = "A", ActualHash = "A" }),
            new FakeInstaller(),
            new UpdateFileStore());

        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "A"
        };

        var options = new UpdateOptions
        {
            CurrentVersion = "1.0.0",
            DownloadDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")),
            InstallOptions = new AndroidInstallOptions { FileProviderAuthority = "com.test.fileprovider" }
        };

        var firstExecution = manager.ExecuteUpdateAsync(package, options, CancellationToken.None);
        await started.Task;

        var secondExecution = await manager.ExecuteUpdateAsync(package, options, CancellationToken.None);

        gate.SetResult();
        await firstExecution;

        Assert.False(secondExecution.IsSuccess);
        Assert.Equal(UpdateFailureReason.AlreadyInProgress, secondExecution.FailureReason);
        Assert.Equal("An update execution is already in progress.", secondExecution.Message);
    }

    [Fact]
    public async Task ValidateAsync_Should_NotBreak_WhenListenerThrows()
    {
        var logger = new RecordingLogger();
        var manager = new AndroidBootstrap(
            new FakeDownloader(),
            new FakeValidator(new HashValidationResult
            {
                IsSuccess = true,
                ExpectedHash = "A",
                ActualHash = "A"
            }),
            new FakeInstaller(),
            new UpdateFileStore(),
            logger);
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "ABCDEF"
        };

        var secondListenerCalled = false;
        manager.AddListenerValidate += (_, _) => throw new InvalidOperationException("test listener fault");
        manager.AddListenerValidate += (_, _) => secondListenerCalled = true;

        var result = await manager.ValidateAsync(package, new UpdateOptions { CurrentVersion = "1.0.0" }, CancellationToken.None);

        Assert.True(result.IsUpdateAvailable);
        Assert.True(secondListenerCalled);
        Assert.Contains(logger.Errors, message => message.Contains("AddListenerValidate listener", StringComparison.Ordinal));
    }

    private static AndroidBootstrap CreateManager()
    {
        return new AndroidBootstrap(
            new FakeDownloader(),
            new FakeValidator(new HashValidationResult
            {
                IsSuccess = true,
                ExpectedHash = "A",
                ActualHash = "A"
            }),
            new FakeInstaller(),
            new UpdateFileStore());
    }

    private sealed class FakeDownloader : IUpdateDownloader
    {
        public Task<DownloadResult> DownloadAsync(UpdatePackageInfo packageInfo, string targetFilePath, string temporaryFilePath, TimeSpan progressReportInterval, IProgress<DownloadStatistics>? progress, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(temporaryFilePath)!);
            File.WriteAllBytes(temporaryFilePath, [1, 2, 3]);

            progress?.Report(new DownloadStatistics
            {
                DownloadedBytes = 3,
                TotalBytes = 3,
                RemainingBytes = 0,
                ProgressPercentage = 100,
                BytesPerSecond = 100
            });

            return Task.FromResult(new DownloadResult
            {
                FilePath = targetFilePath,
                TemporaryFilePath = temporaryFilePath,
                TotalBytes = 3,
                UsedResumableDownload = false
            });
        }
    }

    private sealed class FakeValidator(HashValidationResult result) : IHashValidator
    {
        public Task<HashValidationResult> ValidateSha256Async(string filePath, string expectedSha256, IProgress<double>? progress, CancellationToken cancellationToken)
            => Task.FromResult(result);
    }

    private sealed class FakeInstaller : IApkInstaller
    {
        public bool CanRequestPackageInstalls() => true;

        public Task TriggerInstallAsync(string apkFilePath, AndroidInstallOptions options, CancellationToken cancellationToken)
            => Task.CompletedTask;
    }

    private sealed class SlowDownloader(TaskCompletionSource started, Task waitTask) : IUpdateDownloader
    {
        public async Task<DownloadResult> DownloadAsync(UpdatePackageInfo packageInfo, string targetFilePath, string temporaryFilePath, TimeSpan progressReportInterval, IProgress<DownloadStatistics>? progress, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(temporaryFilePath)!);
            started.TrySetResult();
            await waitTask;
            await File.WriteAllBytesAsync(temporaryFilePath, [1, 2, 3], cancellationToken);
            return new DownloadResult
            {
                FilePath = targetFilePath,
                TemporaryFilePath = temporaryFilePath,
                TotalBytes = 3,
                UsedResumableDownload = false
            };
        }
    }

    private sealed class RecordingLogger : IUpdateLogger
    {
        public List<string> Errors { get; } = [];

        public void LogError(string message, Exception? exception = null) => Errors.Add(message);

        public void LogInfo(string message) { }

        public void LogWarning(string message) { }
    }
}
