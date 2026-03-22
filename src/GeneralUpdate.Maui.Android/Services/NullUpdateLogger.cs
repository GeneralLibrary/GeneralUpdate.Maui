using GeneralUpdate.Maui.Android.Abstractions;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// No-op logger used when no logger is provided.
/// </summary>
public sealed class NullUpdateLogger : IUpdateLogger
{
    public static readonly NullUpdateLogger Instance = new();

    private NullUpdateLogger()
    {
    }

    public void LogInfo(string message)
    {
    }

    public void LogError(string message, Exception? exception = null)
    {
    }
}
