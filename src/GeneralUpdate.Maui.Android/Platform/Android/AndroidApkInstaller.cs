using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;

#if ANDROID
using AndroidApp = Android.App;
using AndroidContent = Android.Content;
using AndroidNet = Android.Net;
using AndroidOS = Android.OS;
using AndroidX.Core.Content;
using JavaFile = Java.IO.File;
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
        if (AndroidOS.Build.VERSION.SdkInt < AndroidOS.BuildVersionCodes.O)
        {
            return true;
        }

        var context = AndroidApp.Application.Context;
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
            throw new System.IO.FileNotFoundException("APK file was not found.", apkFilePath);
        }

        if (!CanRequestPackageInstalls())
        {
            throw new InvalidOperationException("App is not allowed to request package installs. Grant 'install unknown apps' permission in system settings.");
        }

        var context = AndroidApp.Application.Context;
        var apkFile = new JavaFile(apkFilePath);

        using var intent = new AndroidContent.Intent(AndroidContent.Intent.ActionView);
        intent.AddFlags(AndroidContent.ActivityFlags.NewTask);
        intent.AddFlags(AndroidContent.ActivityFlags.GrantReadUriPermission);

        AndroidNet.Uri apkUri;
        if (AndroidOS.Build.VERSION.SdkInt >= AndroidOS.BuildVersionCodes.N)
        {
            apkUri = FileProvider.GetUriForFile(context, options.FileProviderAuthority, apkFile);
        }
        else
        {
            apkUri = AndroidNet.Uri.FromFile(apkFile);
        }

        intent.SetDataAndType(apkUri, options.MimeType);
        context.StartActivity(intent);

        return Task.CompletedTask;
#endif
    }
}
