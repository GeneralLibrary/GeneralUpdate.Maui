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

    public static IAndroidBootstrap CreateDefault(
        HttpClient? httpClient = null,
        IUpdateLogger? logger = null,
        HttpDownloadOptions? httpOptions = null)
    {
        if (httpOptions != null)
        {
            // Build HttpClient from HttpDownloadOptions (SSL, proxy, auth, timeouts)
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
