using GeneralUpdate.Maui.Android.Abstractions;
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

    public static IAndroidBootstrap CreateDefault(HttpClient? httpClient = null, IUpdateLogger? logger = null)
    {
        var client = httpClient ?? new HttpClient();
        return new AndroidBootstrap(
            new HttpRangeDownloader(client),
            new Sha256Validator(),
            new AndroidApkInstaller(),
            new UpdateFileStore(),
            logger);
    }
}
