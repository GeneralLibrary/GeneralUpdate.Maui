namespace GeneralUpdate.Maui.Android.Enums;

/// <summary>
/// Represents the lifecycle state of the update workflow.
/// </summary>
public enum UpdateState
{
    None,
    Checking,
    UpdateAvailable,
    Downloading,
    Verifying,
    ReadyToInstall,
    Installing,
    Completed,
    Failed,
    Canceled
}
