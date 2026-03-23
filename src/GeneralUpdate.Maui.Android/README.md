# GeneralUpdate.Maui.Android

[中文文档 (README.zh-CN.md)](./README.zh-CN.md)

UI-less Android auto-update core for .NET MAUI, focused on reusable update orchestration:

- update discovery/validation
- resumable APK download (HTTP Range)
- SHA256 integrity verification
- Android installer triggering (`FileProvider` + system installer intent)
- lifecycle/status event notifications

> Target frameworks: `net10.0;net10.0-android`  
> C# language version: `latest`

## Installation

Reference the project/package in your MAUI app and configure Android `FileProvider` authority.

## Quick Start

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"New version: {e.PackageInfo.Version}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine(
        $"{s.ProgressPercentage:F2}% | {s.DownloadedBytes}/{s.TotalBytes} | " +
        $"remaining: {s.RemainingBytes} | speed: {s.BytesPerSecond:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"Stage={e.Stage}, File={e.PackagePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"Failed: {e.Reason}, {e.Message}");
};

var package = new UpdatePackageInfo
{
    Version = "2.0.0",
    VersionName = "2.0",
    ReleaseNotes = "Performance and stability improvements",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "3A0D...F9C2",
    PackageSize = 52_428_800
};

var options = new UpdateOptions
{
    CurrentVersion = "1.5.0",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = $"{AppInfo.PackageName}.fileprovider"
    }
};

var check = await bootstrap.ValidateAsync(package, options, CancellationToken.None);
if (check.IsUpdateAvailable)
{
    var result = await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
    Console.WriteLine(result.IsSuccess ? "Update workflow completed." : $"Update failed: {result.Message}");
}
```

## Event Model

`IAndroidBootstrap` exposes:

- `AddListenerValidate`: raised when a higher version is validated.
- `AddListenerDownloadProgressChanged`: periodic statistics (`BytesPerSecond`, `DownloadedBytes`, `RemainingBytes`, `TotalBytes`, `ProgressPercentage`) and status.
- `AddListenerUpdateCompleted`: workflow milestones:
  - `DownloadCompleted`
  - `VerificationCompleted`
  - `InstallationTriggered`
  - `WorkflowCompleted`
- `AddListenerUpdateFailed`: typed failure reason + message + exception.

## Workflow States

`UpdateState` includes:

`None`, `Checking`, `UpdateAvailable`, `Downloading`, `Verifying`, `ReadyToInstall`, `Installing`, `Completed`, `Failed`, `Canceled`.

## Thread-Safety and Execution Guarantees

- only one `ExecuteUpdateAsync` can run at a time per bootstrap instance
- state transitions are atomic (`CurrentState` is thread-safe to read)
- listener exceptions are isolated and logged, and do not stop workflow execution
- cancellation is honored across validation/download/verification/install trigger stages

## Android Requirements

1. Configure `FileProvider` in `AndroidManifest.xml`.
2. Provide a matching authority in `AndroidInstallOptions.FileProviderAuthority`.
3. For Android 8+, ensure app is allowed to install unknown apps (`CanRequestPackageInstalls()`).

Template files are included in this project:

- `Platforms/Android/AndroidManifest.xml`
- `Platforms/Android/XML/provider_paths.xml`

## Testing

Run updater tests with:

```bash
dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

## License

See repository [LICENSE](../../LICENSE).
