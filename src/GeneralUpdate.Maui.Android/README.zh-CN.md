# GeneralUpdate.Maui.Android

[English README](./README.md)

面向 .NET MAUI 的 **无 UI** Android 自动更新核心组件，强调可复用、低耦合的更新编排能力：

- 更新发现与校验
- 支持断点续传的 APK 下载（HTTP Range）
- SHA256 完整性校验
- Android 安装触发（`FileProvider` + 系统安装器 Intent）
- 生命周期与状态事件通知

> 目标框架：`net10.0;net10.0-android`  
> C# 语言版本：`latest`

## 安装与接入

在 MAUI 应用中引用该项目/包，并配置 Android `FileProvider` authority。

## 快速开始

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"发现新版本: {e.PackageInfo.Version}");
};

bootstrap.AddListenerDownloadProgressChanged += (_, e) =>
{
    var s = e.Statistics;
    Console.WriteLine(
        $"{s.ProgressPercentage:F2}% | {s.DownloadedBytes}/{s.TotalBytes} | " +
        $"剩余: {s.RemainingBytes} | 速度: {s.BytesPerSecond:F0} B/s");
};

bootstrap.AddListenerUpdateCompleted += (_, e) =>
{
    Console.WriteLine($"阶段={e.Stage}, 文件={e.PackagePath}");
};

bootstrap.AddListenerUpdateFailed += (_, e) =>
{
    Console.WriteLine($"失败: {e.Reason}, {e.Message}");
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
    Console.WriteLine(result.IsSuccess ? "更新流程完成。" : $"更新失败: {result.Message}");
}
```

## 事件模型

`IAndroidBootstrap` 暴露以下事件：

- `AddListenerValidate`：检测到更高版本时触发。
- `AddListenerDownloadProgressChanged`：周期性上报下载统计（`BytesPerSecond`、`DownloadedBytes`、`RemainingBytes`、`TotalBytes`、`ProgressPercentage`）及状态描述。
- `AddListenerUpdateCompleted`：更新流程里程碑事件：
  - `DownloadCompleted`
  - `VerificationCompleted`
  - `InstallationTriggered`
  - `WorkflowCompleted`
- `AddListenerUpdateFailed`：失败原因、消息、异常信息。

## 流程状态

`UpdateState` 包含：

`None`、`Checking`、`UpdateAvailable`、`Downloading`、`Verifying`、`ReadyToInstall`、`Installing`、`Completed`、`Failed`、`Canceled`。

## 线程安全与执行保障

- 同一个 bootstrap 实例一次仅允许一个 `ExecuteUpdateAsync` 执行
- 状态切换为原子操作（`CurrentState` 可线程安全读取）
- 事件监听器异常会被隔离并记录日志，不会中断主流程
- 支持从校验/下载/验签/触发安装全过程的取消传播

## Android 平台要求

1. 在 `AndroidManifest.xml` 中配置 `FileProvider`。
2. `AndroidInstallOptions.FileProviderAuthority` 必须与 manifest 中配置一致。
3. Android 8+ 需允许“安装未知应用”（可通过 `CanRequestPackageInstalls()` 检测）。

项目内包含 Android 模板文件：

- `Platforms/Android/AndroidManifest.xml`
- `Platforms/Android/XML/provider_paths.xml`

## 测试

建议使用以下命令运行更新器测试：

```bash
dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

## 许可证

参见仓库根目录 [LICENSE](../../LICENSE)。
