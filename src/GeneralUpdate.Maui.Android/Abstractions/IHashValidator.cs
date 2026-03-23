using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Abstractions;

/// <summary>
/// Validates downloaded files by comparing SHA256 hashes.
/// </summary>
public interface IHashValidator
{
    Task<HashValidationResult> ValidateSha256Async(
        string filePath,
        string expectedSha256,
        IProgress<double>? progress,
        CancellationToken cancellationToken);
}
