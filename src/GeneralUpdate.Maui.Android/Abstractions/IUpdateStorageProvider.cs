using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Manages update package file paths and storage operations.
/// </summary>
public interface IUpdateStorageProvider
{
    (string targetFilePath, string temporaryFilePath) GetPackagePaths(UpdatePackageInfo packageInfo, UpdateOptions options);

    void EnsureDirectory(string? directoryPath);

    void ReplaceTemporaryWithFinal(string temporaryFilePath, string targetFilePath);

    void DeleteFileIfExists(string filePath);
}
