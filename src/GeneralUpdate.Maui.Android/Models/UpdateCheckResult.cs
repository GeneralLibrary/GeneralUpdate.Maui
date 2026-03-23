namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents the result of update discovery.
/// </summary>
public sealed class UpdateCheckResult
{
    public bool IsUpdateAvailable { get; init; }

    public UpdatePackageInfo? PackageInfo { get; init; }

    public string? Message { get; init; }

    public static UpdateCheckResult NoUpdate(string? message = null) => new()
    {
        IsUpdateAvailable = false,
        Message = message ?? "No update is available."
    };

    public static UpdateCheckResult UpdateAvailable(UpdatePackageInfo packageInfo, string? message = null) => new()
    {
        IsUpdateAvailable = true,
        PackageInfo = packageInfo,
        Message = message
    };
}
