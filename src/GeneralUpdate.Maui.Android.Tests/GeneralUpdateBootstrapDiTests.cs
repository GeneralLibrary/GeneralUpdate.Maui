using GeneralUpdate.Maui.Android.Abstractions;
using GeneralUpdate.Maui.Android.Services;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GeneralUpdate.Maui.Android.Tests;

public sealed class GeneralUpdateBootstrapDiTests
{
    [Fact]
    public void AddGeneralUpdateMauiAndroid_RegistersBootstrapAndCoreServices()
    {
        var services = new ServiceCollection();

        services.AddGeneralUpdateMauiAndroid();

        using var provider = services.BuildServiceProvider();
        var bootstrap = provider.GetRequiredService<IAndroidBootstrap>();

        Assert.NotNull(bootstrap);
        Assert.IsType<AndroidBootstrap>(bootstrap);
    }
}
