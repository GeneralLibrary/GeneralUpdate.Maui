using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;

#if ANDROID
using Android.Content;
using Android.OS;
using AndroidX.Core.Content;
using Java.IO;
#endif

namespace GeneralUpdate.Maui.Android.Platform.Android;

/// <summary>
/// Default Android APK installer implementation using FileProvider.
/// </summary>
public sealed class AndroidApkInstaller : IApkInstaller
{
    public bool CanRequestPackageInstalls()
    {
#if ANDROID
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
        {
            return true;
        }

        var context = Android.App.Application.Context;
        return context.PackageManager?.CanRequestPackageInstalls() ?? false;
#else
        return false;
#endif
    }

    public Task TriggerInstallAsync(string apkFilePath, AndroidInstallOptions options, CancellationToken cancellationToken)
    {
#if !ANDROID
        throw new PlatformNotSupportedException("APK installation is supported on Android only.");
#else
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(apkFilePath))
        {
            throw new ArgumentException("APK file path cannot be null or empty.", nameof(apkFilePath));
        }

        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        if (string.IsNullOrWhiteSpace(options.FileProviderAuthority))
        {
            throw new InvalidOperationException("AndroidInstallOptions.FileProviderAuthority must be configured.");
        }

        if (!System.IO.File.Exists(apkFilePath))
        {
            throw new FileNotFoundException("APK file was not found.", apkFilePath);
        }

        if (!CanRequestPackageInstalls())
        {
            throw new InvalidOperationException("App is not allowed to request package installs. Grant 'install unknown apps' permission in system settings.");
        }

        var context = Android.App.Application.Context;
        var apkFile = new File(apkFilePath);

        using var intent = new Intent(Intent.ActionView);
        intent.AddFlags(ActivityFlags.NewTask);
        intent.AddFlags(ActivityFlags.GrantReadUriPermission);

        Android.Net.Uri apkUri;
        if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
        {
            apkUri = FileProvider.GetUriForFile(context, options.FileProviderAuthority, apkFile);
        }
        else
        {
            apkUri = Uri.FromFile(apkFile);
        }

        intent.SetDataAndType(apkUri, options.MimeType);
        context.StartActivity(intent);

        return Task.CompletedTask;
#endif
    }
}
