# GeneralUpdate.Maui

[![GitHub Stars](https://img.shields.io/github/stars/GeneralLibrary/GeneralUpdate.Maui?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Maui/stargazers)
[![GitHub Forks](https://img.shields.io/github/forks/GeneralLibrary/GeneralUpdate.Maui?style=flat-square)](https://github.com/GeneralLibrary/GeneralUpdate.Maui/network/members)
[![License](https://img.shields.io/badge/license-Apache%202.0-blue.svg?style=flat-square)](./LICENSE)
[![NuGet](https://img.shields.io/nuget/v/GeneralUpdate.Maui.Android?style=flat-square)](https://www.nuget.org/packages/GeneralUpdate.Maui.Android/)

[简体中文](./README.md)

---

## Introduction

`GeneralUpdate.Maui` is an update capabilities repository for .NET MAUI applications. Its current core module, `GeneralUpdate.Maui.Android`, provides a UI-free Android auto-update orchestration pipeline targeting `net10.0-android` for .NET MAUI apps.

The project breaks the update workflow into composable abstractions, so you can replace version comparison, downloading, hash validation, installer launching, logging, and event dispatching based on your architecture.

## Core Features

- **UI-free Android update core**: Host applications fully control dialogs, progress, and error presentation.  
- **End-to-end update flow**: validation → resumable download → SHA-256 verification → installer launch.  
- **Extensible architecture**: `IVersionComparer`, `IUpdateDownloader`, `IHashValidator`, `IApkInstaller`, and more are replaceable.  
- **Resumable downloading**: sidecar metadata + streaming writes for better reliability on unstable networks.  
- **Unified event model**: built-in validation, progress, completion, and failure events for UI/log integration.  

## Quick Start

### Prerequisites

- .NET SDK: `10.0+`
- Platform: `Android (net10.0-android)`
- .NET MAUI: `10.0+`
- Git: `2.30+`

### Installation

1. Clone the repository

```bash
git clone https://github.com/GeneralLibrary/GeneralUpdate.Maui.git
cd GeneralUpdate.Maui
```

2. Install dependencies (NuGet package consumption)

```bash
dotnet add package GeneralUpdate.Maui.Android
```

3. Build and test locally (repository development)

```bash
dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

### Basic Usage

```csharp
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Services;

var bootstrap = GeneralUpdateBootstrap.CreateDefault();

bootstrap.AddListenerValidate += (_, e) =>
{
    Console.WriteLine($"New version detected: {e.PackageInfo.Version}");
};

var package = new UpdatePackageInfo
{
    Version = "2.3.0",
    VersionName = "2.3.0",
    ReleaseNotes = "Fixes known issues and improves stability.",
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

## Directory Structure

```text
GeneralUpdate.Maui/
├── src/
│   ├── GeneralUpdate.Maui.Android/       # Android auto-update core library
│   └── GeneralUpdate.Maui.Android.Tests/ # Unit tests
├── README.md
├── README-EN.md
└── LICENSE
```

## Contributing

Contributions are welcome through the standard GitHub workflow:

1. Fork this repository and create a branch from `main`: `feature/your-change`.  
2. Keep changes focused and follow existing style and naming conventions.  
3. Run the existing tests before submitting:
   ```bash
   dotnet test src/GeneralUpdate.Maui.Android.Tests/GeneralUpdate.Maui.Android.Tests.csproj -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
   ```
4. Open a Pull Request describing motivation, implementation details, and compatibility impact.  
5. Iterate based on review feedback, then merge and delete the branch.  

## License

This project is licensed under the **Apache License 2.0**. See [LICENSE](./LICENSE) for details.

## Contact

- Repository: https://github.com/GeneralLibrary/GeneralUpdate.Maui
- Issue Tracker: https://github.com/GeneralLibrary/GeneralUpdate.Maui/issues
- Discussions: https://github.com/GeneralLibrary/GeneralUpdate.Maui/discussions
