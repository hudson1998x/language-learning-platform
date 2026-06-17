namespace LLE.Dependencies.Providers
{
    /// <summary>
    /// Marks a class as a provider type that can be discovered and registered by the dependency/Provider system.
    /// </summary>
    /// <remarks>
    /// This attribute is typically used as a marker only (no runtime behavior by itself),
    /// allowing reflection-based discovery of provider implementations during application startup
    /// or registration phases.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class)]
    public class ProviderAttribute : Attribute
    {
    }
}