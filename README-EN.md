# GeneralUpdate.Maui

[![Stars](https://img.shields.io/github/stars/{{GitHub Owner}}/{{Repository Name}}?style=flat-square)](https://github.com/{{GitHub Owner}}/{{Repository Name}}/stargazers)
[![Forks](https://img.shields.io/github/forks/{{GitHub Owner}}/{{Repository Name}}?style=flat-square)](https://github.com/{{GitHub Owner}}/{{Repository Name}}/network/members)
[![License](https://img.shields.io/github/license/{{GitHub Owner}}/{{Repository Name}}?style=flat-square)](./LICENSE)
[![Version](https://img.shields.io/badge/version-{{Version}}-blue?style=flat-square)](https://github.com/{{GitHub Owner}}/{{Repository Name}}/releases)

[简体中文](./README.md)

---

## Introduction

**GeneralUpdate.Maui** is an auto-update capability library for the .NET MAUI ecosystem, designed to provide a standardized, extensible, and maintainable update workflow.

The project currently focuses on Android and provides a UI-less update orchestration core that can be integrated into enterprise apps, utility libraries, component libraries, and business systems.

## Core Features

- **Update validation**: Supports version checks and update eligibility validation.
- **Resumable download**: Implements resumable package downloads based on HTTP Range.
- **Integrity verification**: Built-in SHA256 verification for package integrity and safety.
- **Installer triggering**: Supports Android `FileProvider` and system installer intents.
- **Event-driven workflow**: Exposes lifecycle and download-statistics events for monitoring and extension.

## Quick Start

### 1. Prerequisites

- .NET SDK: `{{.NET SDK Version}}` (recommended `10.0` or later)
- Target platform: `{{Target Platform}}` (for example, `Android`)
- Optional IDE: `{{IDE}}` (for example, Visual Studio / JetBrains Rider)

### 2. Installation

```bash
dotnet add {{Project Path}} package {{Package Name}} --version {{Package Version}}
```

Or use your project's standard dependency management command:

```bash
{{Installation Command}}
```

### 3. Basic Usage

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

## Directory Structure

```text
{{Repository Name}}/
├── README.md
├── README-EN.md
├── LICENSE
└── src/
    ├── GeneralUpdate.Maui.Android/
    └── GeneralUpdate.Maui.Android.Tests/
```

## Contributing

Contributions are welcome through the standard GitHub collaboration workflow:

1. Fork this repository.
2. Create a feature branch: `git checkout -b feature/{{feature-name}}`.
3. Follow the project coding standards and complete required tests.
4. Commit your changes: `git commit -m "feat: {{change summary}}"`.
5. Push your branch and open a Pull Request.

Before submitting a PR, please make sure:

- your change scope is clear and focused;
- related docs and examples are updated;
- no new security risks or breaking behavior are introduced.

## License

This project is licensed under the **Apache License 2.0**. See [LICENSE](./LICENSE) for details.

## Contact

- Maintainer: `{{Maintainer Name}}`
- Email: `{{Contact Email}}`
- Issue Tracker: <https://github.com/{{GitHub Owner}}/{{Repository Name}}/issues>
