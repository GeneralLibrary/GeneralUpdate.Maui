using System.Security.Cryptography;
using GeneralUpdate.Maui.Android.Services;
using Xunit;

namespace GeneralUpdate.Maui.Android.Tests;

public class Sha256ValidatorTests
{
    [Fact]
    public async Task ValidateSha256Async_Should_ReturnSuccess_WhenHashMatches_IgnoringCase()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        await File.WriteAllBytesAsync(file, [10, 20, 30, 40]);

        using var sha = SHA256.Create();
        var expected = Convert.ToHexString(sha.ComputeHash(await File.ReadAllBytesAsync(file))).ToLowerInvariant();

        var sut = new Sha256Validator();
        var result = await sut.ValidateSha256Async(file, expected, progress: null, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.ExpectedHash);
        Assert.NotEmpty(result.ActualHash);
    }

    [Fact]
    public async Task ValidateSha256Async_Should_ReturnFailure_WhenHashDoesNotMatch()
    {
        var file = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".bin");
        await File.WriteAllBytesAsync(file, [1, 2, 3, 4]);

        var sut = new Sha256Validator();
        var result = await sut.ValidateSha256Async(file, "FFFFFFFF", progress: null, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal("SHA256 does not match expected value.", result.FailureReason);
    }
}
