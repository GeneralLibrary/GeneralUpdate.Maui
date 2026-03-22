using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Enums;
using GeneralUpdate.Maui.Android.Events;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;
using Xunit;

namespace GeneralUpdate.Maui.Android.Tests;

public class AndroidUpdateManagerTests
{
    [Fact]
    public async Task CheckForUpdateAsync_Should_ReportUpdateAvailable_WhenVersionIsNewer()
    {
        var manager = CreateManager();
        var package = new UpdatePackageInfo
        {
            Version = "2.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "ABCDEF"
        };

        var raised = false;
        manager.UpdateFound += (_, args) => raised = args.PackageInfo.Version == "2.0.0";

        var result = await manager.CheckForUpdateAsync(package, new UpdateOptions { CurrentVersion = "1.0.0" }, CancellationToken.None);

        Assert.True(result.IsUpdateAvailable);
        Assert.True(raised);
        Assert.Equal(UpdateState.UpdateAvailable, manager.CurrentState);
    }

    [Fact]
    public async Task CheckForUpdateAsync_Should_ReturnNoUpdate_WhenVersionIsNotNewer()
    {
        var manager = CreateManager();
        var package = new UpdatePackageInfo
        {
            Version = "1.0.0",
            DownloadUrl = "https://example.com/app.apk",
            Sha256 = "ABCDEF"
        };

        var result = await manager.CheckForUpdateAsync(package, new UpdateOptions { CurrentVersion = "1.0.0" }, CancellationToken.None);

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

        var manager = new AndroidUpdateManager(
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
        manager.UpdateFailed += (_, e) => failedRaised = e.Reason == UpdateFailureReason.IntegrityCheckFailed;

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

    private static AndroidUpdateManager CreateManager()
    {
        return new AndroidUpdateManager(
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
}
