namespace LLE.Configuration.Attributes;

/// <summary>
/// Marks a class as a configuration type that can be managed by <see cref="LLE.Configuration.Providers.ConfigurationProvider"/>.
/// </summary>
/// <remarks>
/// Configuration types are expected to be:
/// <list type="bullet">
/// <item><description>Parameterless (no constructors allowed)</description></item>
/// <item><description>Instantiated and cached as singletons by the configuration system</description></item>
/// <item><description>Resolved via <see cref="LLE.Configuration.Providers.ConfigurationProvider.Get(Type)"/></description></item>
/// </list>
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationAttribute : Attribute
{
}