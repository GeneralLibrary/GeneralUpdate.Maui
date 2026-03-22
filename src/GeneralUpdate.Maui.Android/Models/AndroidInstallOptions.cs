namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Provides Android installation options.
/// </summary>
public sealed class AndroidInstallOptions
{
    /// <summary>
    /// FileProvider authority configured in AndroidManifest.
    /// Example: "{applicationId}.fileprovider".
    /// </summary>
    public string FileProviderAuthority { get; init; } = string.Empty;

    /// <summary>
    /// Optional custom mime type used to open apk files.
    /// </summary>
    public string MimeType { get; init; } = "application/vnd.android.package-archive";
}
