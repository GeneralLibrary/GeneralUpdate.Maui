using System.Security.Cryptography;
using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Models;

namespace GeneralUpdate.Maui.Android.Services;

/// <summary>
/// Default SHA256 validation service based on stream hashing.
/// </summary>
public sealed class Sha256Validator : IHashValidator
{
    public async Task<HashValidationResult> ValidateSha256Async(
        string filePath,
        string expectedSha256,
        IProgress<double>? progress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(expectedSha256))
        {
            throw new ArgumentException("Expected SHA256 cannot be null or empty.", nameof(expectedSha256));
        }

        if (!File.Exists(filePath))
        {
            return new HashValidationResult
            {
                IsSuccess = false,
                ExpectedHash = expectedSha256,
                FailureReason = "Downloaded file does not exist."
            };
        }

        await using var stream = File.OpenRead(filePath);
        var totalBytes = stream.Length;
        var processed = 0L;
        var buffer = new byte[81920];

        using var sha256 = SHA256.Create();

        int read;
        while ((read = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false)) > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
            sha256.TransformBlock(buffer, 0, read, null, 0);
            processed += read;

            if (totalBytes > 0)
            {
                progress?.Report((double)processed / totalBytes * 100D);
            }
        }

        sha256.TransformFinalBlock([], 0, 0);
        var hashBytes = sha256.Hash ?? [];
        var actual = Convert.ToHexString(hashBytes);
        var isMatch = string.Equals(actual, expectedSha256, StringComparison.OrdinalIgnoreCase);

        return new HashValidationResult
        {
            IsSuccess = isMatch,
            ExpectedHash = expectedSha256,
            ActualHash = actual,
            FailureReason = isMatch ? null : "SHA256 does not match expected value."
        };
    }
}
