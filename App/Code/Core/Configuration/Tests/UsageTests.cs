using LLE.Configuration.Attributes;
using LLE.Configuration.Providers;
using LLE.Dependencies.Providers;

namespace LLE.ConfigurationTests;

public class ConfigurationProviderTests
{
    // -------------------------
    // Test types
    // -------------------------

    [Configuration]
    private class ValidConfig
    {
        public int Value => 42;
    }

    private class MissingAttributeConfig
    {
        public string Name => "bad";
    }

    // -------------------------
    // Helpers
    // -------------------------

    private static ConfigurationProvider GetProvider()
        => Provider.Get<ConfigurationProvider>();

    // -------------------------
    // Tests
    // -------------------------

    [Fact]
    public void Get_WithValidConfig_ReturnsInstance()
    {
        var provider = GetProvider();

        var config = provider.Get<ValidConfig>();

        Assert.NotNull(config);
        Assert.Equal(42, config.Value);
    }

    [Fact]
    public void Get_WithSameType_ReturnsSameInstance()
    {
        var provider = GetProvider();

        var first = provider.Get<ValidConfig>();
        var second = provider.Get<ValidConfig>();

        Assert.Same(first, second);
    }

    [Fact]
    public void Get_UntypedAndTyped_ReturnSameInstance()
    {
        var provider = GetProvider();

        var typed = provider.Get<ValidConfig>();
        var untyped = (ValidConfig)provider.Get(typeof(ValidConfig));

        Assert.Same(typed, untyped);
    }

    [Fact]
    public void Get_TypeWithoutAttribute_Throws()
    {
        var provider = GetProvider();

        Assert.Throws<InvalidOperationException>(
            (Func<object>)(() => provider.Get(typeof(MissingAttributeConfig)))
        );
    }

    [Fact]
    public void Get_MultipleThreads_DoNotCreateMultipleInstances()
    {
        var provider = GetProvider();

        const int iterations = 50;
        var results = new ValidConfig[iterations];

        Parallel.For(0, iterations, i =>
        {
            results[i] = provider.Get<ValidConfig>();
        });

        Assert.True(results.All(r => ReferenceEquals(r, results[0])));
    }
}