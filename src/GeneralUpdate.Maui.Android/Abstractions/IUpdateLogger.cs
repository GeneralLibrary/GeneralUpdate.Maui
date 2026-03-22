namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Provides pluggable logging hooks for update workflows.
/// </summary>
public interface IUpdateLogger
{
    void LogInfo(string message);

    void LogError(string message, Exception? exception = null);
}
