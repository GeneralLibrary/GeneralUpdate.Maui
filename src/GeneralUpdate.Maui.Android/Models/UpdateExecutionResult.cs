using GeneralUpdate.Maui.Android.Enums;

namespace GeneralUpdate.Maui.Android.Models;

/// <summary>
/// Represents the final result of an update workflow execution.
/// </summary>
public sealed class UpdateExecutionResult
{
    public bool IsSuccess { get; init; }

    public UpdateFailureReason FailureReason { get; init; }

    public string? Message { get; init; }

    public string? PackagePath { get; init; }

    public static UpdateExecutionResult Success(string packagePath) => new()
    {
        IsSuccess = true,
        FailureReason = UpdateFailureReason.Unknown,
        PackagePath = packagePath,
        Message = "Update workflow completed."
    };

    public static UpdateExecutionResult Failure(UpdateFailureReason reason, string message) => new()
    {
        IsSuccess = false,
        FailureReason = reason,
        Message = message
    };
}
