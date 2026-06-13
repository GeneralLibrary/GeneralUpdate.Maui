using GeneralUpdate.Maui.Android.Enums;

namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents update package metadata used for update discovery and download.
/// </summary>
public sealed class UpdatePackageInfo
{
    public string Version { get; init; } = string.Empty;

    public string? VersionName { get; init; }

    public string? ReleaseNotes { get; init; }

    public string DownloadUrl { get; init; } = string.Empty;

    public long? PackageSize { get; init; }

    public string Sha256 { get; init; } = string.Empty;

    public DateTimeOffset? PublishTime { get; init; }

    public bool ForceUpdate { get; init; }

    public string? ApkFileName { get; init; }

    /// <summary>
    /// Per-package authentication scheme.
    /// When set, takes precedence over the global <see cref="HttpDownloadOptions.AuthProvider"/>.
    /// </summary>
    public AuthScheme? AuthScheme { get; init; }

    /// <summary>
    /// Token value used by Bearer or ApiKey authentication.
    /// For Bearer: the Bearer token string.
    /// For ApiKey: the API key value.
    /// </summary>
    public string? AuthToken { get; init; }

    /// <summary>
    /// Secret key used by HMAC-SHA256 signature authentication.
    /// </summary>
    public string? AuthSecretKey { get; init; }

    /// <summary>
    /// Username used by Basic authentication.
    /// </summary>
    public string? BasicUsername { get; init; }

    /// <summary>
    /// Password used by Basic authentication.
    /// </summary>
    public string? BasicPassword { get; init; }
}
