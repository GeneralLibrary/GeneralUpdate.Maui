using System.Net;
using System.Net.Http.Headers;
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Utilities;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// HTTP downloader that supports range-based resume, authentication, retry, and progress statistics.
/// </summary>
public sealed class HttpRangeDownloader : IUpdateDownloader, IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly HttpDownloadOptions? _httpOptions;
    private readonly IHttpAuthProvider? _globalAuthProvider;
    private readonly bool _ownsClient;
    private bool _disposed;

    /// <summary>
    /// Creates a downloader with an externally-provided HttpClient.
    /// No authentication or custom HTTP options are applied.
    /// </summary>
    public HttpRangeDownloader(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpOptions = null;
        _globalAuthProvider = null;
        _ownsClient = false;
    }

    /// <summary>
    /// Creates a downloader with HTTP options that configure SSL, proxy, auth, and timeouts.
    /// </summary>
    internal HttpRangeDownloader(HttpClient httpClient, HttpDownloadOptions httpOptions)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _httpOptions = httpOptions ?? throw new ArgumentNullException(nameof(httpOptions));
        _globalAuthProvider = httpOptions.AuthProvider;
        _ownsClient = true;
    }

    public async Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        string targetFilePath,
        string temporaryFilePath,
        TimeSpan progressReportInterval,
        IProgress<DownloadStatistics>? progress,
        CancellationToken cancellationToken)
    {
        if (packageInfo is null)
            throw new ArgumentNullException(nameof(packageInfo));

        if (!Uri.TryCreate(packageInfo.DownloadUrl, UriKind.Absolute, out var requestUri))
            throw new ArgumentException("The download url is invalid.", nameof(packageInfo));

        // Resolve download timeout
        using var timeoutCts = _httpOptions != null
            ? new CancellationTokenSource(_httpOptions.DownloadTimeout)
            : null;
        using var linkedCts = timeoutCts != null
            ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token)
            : null;
        var effectiveCt = linkedCts?.Token ?? cancellationToken;

        var existingLength = File.Exists(temporaryFilePath) ? new FileInfo(temporaryFilePath).Length : 0L;
        var fallbackToFullDownload = false;

        while (true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (existingLength > 0 && !fallbackToFullDownload)
            {
                request.Headers.Range = new RangeHeaderValue(existingLength, null);
            }

            // Apply authentication
            await ApplyAuthAsync(request, packageInfo, effectiveCt).ConfigureAwait(false);

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, effectiveCt).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.RequestedRangeNotSatisfiable && existingLength > 0 && !fallbackToFullDownload)
            {
                fallbackToFullDownload = true;
                continue;
            }

            response.EnsureSuccessStatusCode();

            var resumeAccepted = response.StatusCode == HttpStatusCode.PartialContent && existingLength > 0 && !fallbackToFullDownload;
            if (!resumeAccepted && existingLength > 0)
            {
                File.Delete(temporaryFilePath);
                existingLength = 0;
            }

            var totalBytes = ResolveTotalBytes(response, existingLength, packageInfo.PackageSize);
            var mode = resumeAccepted ? FileMode.Append : FileMode.Create;
            var speedCalculator = new SpeedCalculator();
            var nextReportAt = DateTimeOffset.UtcNow;
            var downloadedBytes = existingLength;

            await using var networkStream = await response.Content.ReadAsStreamAsync(effectiveCt).ConfigureAwait(false);
            await using var fileStream = new FileStream(temporaryFilePath, mode, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            var buffer = new byte[81920];
            int read;

            while ((read = await networkStream.ReadAsync(buffer.AsMemory(0, buffer.Length), effectiveCt).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), effectiveCt).ConfigureAwait(false);
                downloadedBytes += read;
                speedCalculator.AddSample(downloadedBytes);

                var now = DateTimeOffset.UtcNow;
                if (now >= nextReportAt)
                {
                    progress?.Report(CreateStatistics(downloadedBytes, totalBytes, speedCalculator));
                    nextReportAt = now + progressReportInterval;
                }
            }

            progress?.Report(CreateStatistics(downloadedBytes, totalBytes, speedCalculator));

            return new DownloadResult
            {
                FilePath = targetFilePath,
                TemporaryFilePath = temporaryFilePath,
                TotalBytes = totalBytes,
                UsedResumableDownload = resumeAccepted
            };
        }
    }

    /// <summary>
    /// Applies authentication to the HTTP request.
    /// Per-package auth takes precedence over global auth.
    /// </summary>
    private async Task ApplyAuthAsync(HttpRequestMessage request, UpdatePackageInfo packageInfo, CancellationToken cancellationToken)
    {
        IHttpAuthProvider? provider = null;

        if (packageInfo.AuthScheme.HasValue)
        {
            provider = HttpAuthProviderFactory.Create(
                packageInfo.AuthScheme.Value,
                packageInfo.AuthToken,
                packageInfo.AuthSecretKey,
                packageInfo.BasicUsername,
                packageInfo.BasicPassword);
        }

        if ((provider is null || provider is NoOpAuthProvider) && _globalAuthProvider != null)
        {
            provider = _globalAuthProvider;
        }

        if (provider != null)
        {
            await provider.ApplyAuthAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private static long ResolveTotalBytes(HttpResponseMessage response, long existingLength, long? fallbackTotal)
    {
        if (response.Content.Headers.ContentRange?.Length is long contentRangeLength)
            return contentRangeLength;

        if (response.Content.Headers.ContentLength is long contentLength)
            return response.StatusCode == HttpStatusCode.PartialContent
                ? existingLength + contentLength
                : contentLength;

        return Math.Max(existingLength, fallbackTotal ?? 0L);
    }

    private static DownloadStatistics CreateStatistics(long downloadedBytes, long totalBytes, SpeedCalculator speedCalculator)
    {
        var remaining = totalBytes > 0 ? Math.Max(0, totalBytes - downloadedBytes) : 0;
        var percentage = totalBytes > 0 ? (double)downloadedBytes / totalBytes * 100D : 0D;

        return new DownloadStatistics
        {
            DownloadedBytes = downloadedBytes,
            TotalBytes = totalBytes,
            RemainingBytes = remaining,
            ProgressPercentage = percentage,
            BytesPerSecond = speedCalculator.GetBytesPerSecond()
        };
    }

    public void Dispose()
    {
        if (_disposed) return;
        if (_ownsClient)
        {
            _httpClient.Dispose();
        }
        _disposed = true;
    }
}
