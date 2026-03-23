# GeneralUpdate.Maui

[![GitHub Stars](https://img.shields.io/github/stars/GeneralLibrary/GeneralUpdate.Maui?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Maui/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/GeneralLibrary/GeneralUpdate.Maui?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Maui/network/members)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat-square)](./LICENSE)
[![NuGet](https://img.shields.io/nuget/v/GeneralUpdate.Maui.Android?style=flat-square)](https://www.nuget.org/packages/GeneralUpdate.Maui.Android/)

[English](./README-EN.md)

---

## 项目简介

`GeneralUpdate.Maui` 是面向 .NET MAUI 应用的更新能力仓库。当前核心模块为 `GeneralUpdate.Maui.Android`，提供 Android 平台自动更新流程编排能力（无 UI），适配 `net10.0-android`，适用于 .NET MAUI 应用。

项目将更新流程拆分为可组合的抽象接口，便于在不同业务场景下替换版本比较、下载、哈希校验、安装拉起、日志与事件分发实现。

## 核心特性

- **Android 更新核心（无 UI）**：宿主应用可完全控制弹窗、进度和错误展示。  
- **完整更新链路**：版本校验 → 断点续传下载 → SHA-256 校验 → 安装器拉起。  
- **可扩展架构**：`IVersionComparer`、`IUpdateDownloader`、`IHashValidator`、`IApkInstaller` 等均可替换。  
- **断点续传下载**：支持 sidecar 元数据与流式写入，提升弱网场景稳定性。  
- **统一事件通知**：提供验证、进度、完成、失败等事件用于 UI/日志集成。  

## 快速开始

### 环境准备

- .NET SDK：`10.0+`
- 平台：`Android (net10.0-android)`
- .NET MAUI：`10.0+`
- Git：`2.30+`

### 安装步骤

1. 克隆仓库

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate.Maui.git
cd GeneralUpdate.Maui
```

2. 安装依赖（以 NuGet 包方式使用）

```bash
dotnet add package GeneralUpdate.Maui.Android
```

3. 本地构建与测试（仓库开发）

```bash
dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

### 基本使用示例

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"发现新版本: {e.PackageInfo.Version}");
};

var package = new UpdatePackageInfo
{
    Version = "2.3.0",
    VersionName = "2.3.0",
    ReleaseNotes = "修复已知问题并优化稳定性。",
    DownloadUrl = "https://example.com/app-release.apk",
    Sha256 = "REPLACE_WITH_ACTUAL_SHA256_HASH",
    PackageSize = 1024 * 1024 * 50
};

var options = new UpdateOptions
{
    CurrentVersion = "2.2.1",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "com.example.app.generalupdate.fileprovider"
    }
};

var check = await bootstrap.ValidateAsync(package, options, CancellationToken.None);
if (check.IsUpdateAvailable)
{
    await bootstrap.ExecuteUpdateAsync(package, options, CancellationToken.None);
}
```

## 目录结构

```text
GeneralUpdate.Maui/
├── src/
│   ├── GeneralUpdate.Maui.Android/       # Android 自动更新核心库
│   └── GeneralUpdate.Maui.Android.Tests/ # 单元测试
├── README.md
├── README-EN.md
└── LICENSE
```

## 贡献指南

欢迎通过 GitHub 协作流程参与贡献：

1. Fork 本仓库并从 `main` 创建分支：`feature/your-change`。  
2. 保持变更聚焦，并遵循现有代码风格与命名规范。  
3. 提交前运行现有测试：
   ```bash
   dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
   ```
4. 提交 Pull Request，说明动机、实现方案和兼容性影响。  
5. 根据评审反馈迭代，合并后删除分支。  

## 许可证

本项目采用 **Apache License 2.0**。详情请见 [LICENSE](./LICENSE)。

## 联系方式

- 仓库地址：https://github.com/GeneralLibrary/GeneralUpdate.Maui
- 问题反馈：https://github.com/GeneralLibrary/GeneralUpdate.Maui/issues
- 讨论交流：https://github.com/GeneralLibrary/GeneralUpdate.Maui/discussions
