namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents SHA256 verification output.
/// </summary>
public sealed class HashValidationResult
{
    public bool IsSuccess { get; init; }

    public string ExpectedHash { get; init; } = string.Empty;

    public string ActualHash { get; init; } = string.Empty;

    public string? FailureReason { get; init; }
}
