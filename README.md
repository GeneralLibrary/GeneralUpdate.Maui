# GeneralUpdate.Maui

[![GitHub Stars](https://img.shields.io/github/stars/GeneralLibrary/GeneralUpdate.Avalonia?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/GeneralLibrary/GeneralUpdate.Avalonia?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Avalonia/network/members)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat-square)](./LICENSE)
[![NuGet](https://img.shields.io/nuget/v/GeneralUpdate.Avalonia.Android?style=flat-square)](https://www.nuget.org/packages/GeneralUpdate.Avalonia.Android/)

[English](./README-EN.md)

---

## 项目简介

**GeneralUpdate.Maui** 是一个面向 .NET MAUI 生态的自动更新能力库，用于帮助项目实现标准化、可扩展且可维护的应用更新流程。

项目当前聚焦 Android 场景，提供无 UI 依赖的更新流程核心能力，可集成至企业应用、工具库、组件库或业务系统中。

## 核心特性

- **更新信息校验**：支持版本检测与更新条件验证。
- **断点续传下载**：基于 HTTP Range 的可恢复下载机制。
- **完整性校验**：内置 SHA256 校验，确保安装包安全可靠。
- **安装流程触发**：支持 Android `FileProvider` 与系统安装意图。
- **事件驱动设计**：提供更新生命周期与下载统计事件，便于监控与扩展。

## 快速开始

### 1. 环境准备

- .NET SDK：`{{.NET SDK Version}}`（建议 `10.0` 或更高版本）
- 目标平台：`{{Target Platform}}`（例如 `Android`）
- 可选工具：`{{IDE}}`（例如 Visual Studio / JetBrains Rider）

### 2. 安装步骤

```bash
dotnet add {{Project Path}} package {{Package Name}} --version {{Package Version}}
```

或使用你所在项目的标准依赖管理命令：

```bash
{{Installation Command}}
```

### 3. 基础使用示例

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
    Version = "{{Target Version}}",
    VersionName = "{{Target Version Name}}",
    ReleaseNotes = "{{Release Notes}}",
    DownloadUrl = "{{Download Url}}",
    Sha256 = "{{SHA256 Value}}",
    PackageSize = {{Package Size}}
};

var options = new UpdateOptions
{
    CurrentVersion = "{{Current Version}}",
    InstallOptions = new AndroidInstallOptions
    {
        FileProviderAuthority = "{{FileProvider Authority}}"
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
{{Repository Name}}/
├── README.md
├── README-EN.md
├── LICENSE
└── src/
    ├── GeneralUpdate.Maui.Android/
    └── GeneralUpdate.Maui.Android.Tests/
```

## 贡献指南

欢迎通过 GitHub 协作流程参与贡献：

1. Fork 本仓库。
2. 创建特性分支：`git checkout -b feature/{{feature-name}}`。
3. 遵循项目代码规范并完成必要测试。
4. 提交变更：`git commit -m "feat: {{change summary}}"`。
5. 推送分支并发起 Pull Request。

在提交 PR 前，请确保：

- 变更范围清晰且聚焦；
- 相关文档与示例已同步更新；
- 不引入新的安全风险或破坏性行为。

## 许可证

本项目采用 **Apache License 2.0** 开源协议。详情请参见 [LICENSE](./LICENSE)。

## 联系方式

- 维护者：`Juster Zhu`
- 问题反馈：<https://github.com/{{GitHub Owner}}/{{Repository Name}}/issues>
