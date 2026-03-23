using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Platform.Android;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// Creates default <see cref="IAndroidBootstrap"/> instances with built-in service implementations.
/// </summary>
public static class GeneralUpdateBootstrap
{
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
