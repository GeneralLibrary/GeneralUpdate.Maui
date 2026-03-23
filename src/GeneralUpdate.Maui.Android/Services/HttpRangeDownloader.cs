using System.Net;
using System.Net.Http.Headers;
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Utilities;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// HTTP downloader that supports range-based resume and progress statistics.
/// </summary>
public sealed class HttpRangeDownloader(HttpClient httpClient) : IUpdateDownloader
{
    private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

    public async Task<DownloadResult> DownloadAsync(
        UpdatePackageInfo packageInfo,
        string targetFilePath,
        string temporaryFilePath,
        TimeSpan progressReportInterval,
        IProgress<DownloadStatistics>? progress,
        CancellationToken cancellationToken)
    {
        if (packageInfo is null)
        {
            throw new ArgumentNullException(nameof(packageInfo));
        }

        if (!Uri.TryCreate(packageInfo.DownloadUrl, UriKind.Absolute, out var requestUri))
        {
            throw new ArgumentException("The download url is invalid.", nameof(packageInfo));
        }

        var existingLength = File.Exists(temporaryFilePath) ? new FileInfo(temporaryFilePath).Length : 0L;
        var fallbackToFullDownload = false;

        while (true)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            if (existingLength > 0 && !fallbackToFullDownload)
            {
                request.Headers.Range = new RangeHeaderValue(existingLength, null);
            }

            using var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

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

            await using var networkStream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var fileStream = new FileStream(temporaryFilePath, mode, FileAccess.Write, FileShare.None, 81920, useAsync: true);

            var buffer = new byte[81920];
            int read;

            while ((read = await networkStream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
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

    private static long ResolveTotalBytes(HttpResponseMessage response, long existingLength, long? fallbackTotal)
    {
        if (response.Content.Headers.ContentRange?.Length is long contentRangeLength)
        {
            return contentRangeLength;
        }

        if (response.Content.Headers.ContentLength is long contentLength)
        {
            return response.StatusCode == HttpStatusCode.PartialContent
                ? existingLength + contentLength
                : contentLength;
        }

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
}
