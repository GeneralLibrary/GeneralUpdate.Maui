using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// Default file storage provider for update package artifacts.
/// </summary>
public sealed class UpdateFileStore : IUpdateStorageProvider
{
    public (string targetFilePath, string temporaryFilePath) GetPackagePaths(UpdatePackageInfo packageInfo, UpdateOptions options)
    {
        var directory = string.IsNullOrWhiteSpace(options.DownloadDirectory)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "GeneralUpdate")
            : options.DownloadDirectory;

        EnsureDirectory(directory);

        var fileName = ResolveFileName(packageInfo);
        var target = Path.Combine(directory!, fileName);
        var temp = target + options.TemporaryFileExtension;
        return (target, temp);
    }

    public void EnsureDirectory(string? directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentException("Directory path cannot be null or empty.", nameof(directoryPath));
        }

        Directory.CreateDirectory(directoryPath);
    }

    public void ReplaceTemporaryWithFinal(string temporaryFilePath, string targetFilePath)
    {
        if (!File.Exists(temporaryFilePath))
        {
            throw new FileNotFoundException("Temporary package file was not found.", temporaryFilePath);
        }

        var targetDirectory = Path.GetDirectoryName(targetFilePath);
        EnsureDirectory(targetDirectory);
        File.Move(temporaryFilePath, targetFilePath, overwrite: true);
    }

    public void DeleteFileIfExists(string filePath)
    {
        if (!string.IsNullOrWhiteSpace(filePath) && File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private static string ResolveFileName(UpdatePackageInfo packageInfo)
    {
        if (!string.IsNullOrWhiteSpace(packageInfo.ApkFileName))
        {
            return Path.GetFileName(packageInfo.ApkFileName);
        }

        if (Uri.TryCreate(packageInfo.DownloadUrl, UriKind.Absolute, out var uri))
        {
            var urlName = Path.GetFileName(uri.LocalPath);
            if (!string.IsNullOrWhiteSpace(urlName))
            {
                return urlName;
            }
        }

        return $"update-{packageInfo.Version}.apk";
    }
}
