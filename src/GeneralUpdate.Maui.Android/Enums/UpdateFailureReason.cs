namespace GeneralUpdate.Maui.Android.Enums;

/// <summary>
/// Represents high-level categories for update failures.
/// </summary>
public enum UpdateFailureReason
{
    Unknown,
    InvalidInput,
    AlreadyInProgress,
    Network,
    Download,
    FileAccess,
    IntegrityCheckFailed,
    InstallPermissionDenied,
    Installation,
    Canceled,
    NoUpdateAvailable
}
