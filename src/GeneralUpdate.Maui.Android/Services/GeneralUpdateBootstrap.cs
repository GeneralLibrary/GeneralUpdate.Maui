using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;
using GeneralUpdate.Maui.Android.Platform.Android;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// Creates default <see cref="IAndroidBootstrap"/> instances with built-in service implementations.
/// </summary>
public static class GeneralUpdateBootstrap
{
    public static IServiceCollection AddGeneralUpdateMauiAndroid(
        this IServiceCollection services,
        HttpClient? httpClient = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (httpClient is not null)
        {
            services.TryAddSingleton<IUpdateDownloader>(new HttpRangeDownloader(httpClient));
        }
        else
        {
            services.AddHttpClient<IUpdateDownloader, HttpRangeDownloader>();
        }
        services.AddSingleton<IHashValidator, Sha256Validator>();
        services.AddSingleton<IApkInstaller, AndroidApkInstaller>();
        services.AddSingleton<IUpdateStorageProvider, UpdateFileStore>();
        services.AddSingleton<IUpdateLogger>(NullUpdateLogger.Instance);
        services.AddSingleton<IAndroidBootstrap, AndroidBootstrap>();

        return services;
    }

    /// <summary>
    /// Creates a default <see cref="IAndroidBootstrap"/> with built-in service implementations.
    /// </summary>
    /// <param name="httpClient">Optional external HttpClient. Ignored when <paramref name="httpOptions"/> is provided.</param>
    /// <param name="logger">Optional logger.</param>
    /// <param name="httpOptions">
    /// Optional HTTP configuration (SSL, proxy, auth, timeouts).
    /// When set, an internal HttpClient is constructed from these options
    /// and <paramref name="httpClient"/> is not used.
    /// </param>
    public static IAndroidBootstrap CreateDefault(
        HttpClient? httpClient = null,
        IUpdateLogger? logger = null,
        HttpDownloadOptions? httpOptions = null)
    {
        if (httpOptions != null)
        {
            // Build HttpClient from HttpDownloadOptions (SSL, proxy, auth, timeouts)
            // Note: when httpOptions is provided, the httpClient parameter is NOT used.
            var handler = httpOptions.BuildHandler();
            var client = new HttpClient(handler, disposeHandler: true)
            {
                Timeout = System.Threading.Timeout.InfiniteTimeSpan
            };
            return new AndroidBootstrap(
                new HttpRangeDownloader(client, httpOptions),
                new Sha256Validator(),
                new AndroidApkInstaller(),
                new UpdateFileStore(),
                logger);
        }

        // Legacy path: bare HttpClient
        var usedClient = httpClient ?? new HttpClient();
        return new AndroidBootstrap(
            new HttpRangeDownloader(usedClient),
            new Sha256Validator(),
            new AndroidApkInstaller(),
            new UpdateFileStore(),
            logger);
    }
}
