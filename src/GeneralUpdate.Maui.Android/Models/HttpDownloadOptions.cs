using System.Net;
using GeneralUpdate.Maui.Android.Abstractions;

namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Configures HTTP transport behavior for update downloads:
/// SSL/TLS certificate validation, proxy, timeouts, and authentication.
/// <para>
/// When provided to <see cref="Services.GeneralUpdateBootstrap.CreateDefault"/>,
/// the library constructs an internal <see cref="HttpClient"/> from these settings.
/// When null, the existing behavior is preserved (bare HttpClient, no auth, system SSL).
/// </para>
/// </summary>
public sealed record HttpDownloadOptions
{
    /// <summary>
    /// Custom SSL/TLS certificate validation policy.
    /// Defaults to null, which uses the system's default certificate validation.
    /// Set to <see cref="Services.AllowAllSslValidationPolicy"/> for self-signed certificates
    /// in development environments only.
    /// </summary>
    public ISslValidationPolicy? SslValidationPolicy { get; init; }

    /// <summary>
    /// Overall timeout for the entire download operation.
    /// Default is 10 minutes.
    /// </summary>
    public TimeSpan DownloadTimeout { get; init; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Optional web proxy for HTTP requests.
    /// When set, <see cref="UseProxy"/> must also be true for the proxy to take effect.
    /// </summary>
    public IWebProxy? Proxy { get; init; }

    /// <summary>
    /// Whether to use the configured <see cref="Proxy"/>.
    /// Default is false.
    /// </summary>
    public bool UseProxy { get; init; }

    /// <summary>
    /// Global authentication provider applied to all download requests.
    /// Per-package authentication on <see cref="UpdatePackageInfo"/> takes precedence.
    /// When <see cref="UpdatePackageInfo.AuthScheme"/> is explicitly set,
    /// per-package credentials are used exclusively (no fallback to global).
    /// </summary>
    public IHttpAuthProvider? AuthProvider { get; init; }

    /// <summary>
    /// Builds an <see cref="HttpClientHandler"/> from the configured options.
    /// Applies SSL validation policy and proxy settings.
    /// </summary>
    internal HttpClientHandler BuildHandler()
    {
        var handler = new HttpClientHandler();

        if (SslValidationPolicy != null)
        {
            handler.ServerCertificateCustomValidationCallback =
                (_, cert, chain, errors) => SslValidationPolicy.ValidateCertificate(cert, chain, errors);
        }

        if (UseProxy && Proxy != null)
        {
            handler.Proxy = Proxy;
            handler.UseProxy = true;
        }
        else
        {
            handler.UseProxy = false;
        }

        return handler;
    }
}
