using System.Collections.Concurrent;

namespace LLE.Kernel.Registry;

/// <summary>
/// Provides a thread-safe, process-wide cache of lazily-instantiated configuration objects,
/// keyed by their <see cref="Type"/>. Each configuration type is instantiated at most once,
/// using its parameterless constructor, and the resulting instance is reused for all
/// subsequent requests.
/// </summary>
public static class ConfigurationCatalog
{
    /// <summary>
    /// Backing store mapping a configuration <see cref="Type"/> to its singleton instance.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> ConfigMap = [];

    /// <summary>
    /// Retrieves the singleton configuration instance for the specified <paramref name="type"/>,
    /// creating and caching it via <see cref="Activator.CreateInstance(Type)"/> if it does not
    /// already exist.
    /// </summary>
    /// <param name="type">The configuration type to retrieve or instantiate. Must have an accessible parameterless constructor.</param>
    /// <returns>The cached or newly created configuration instance associated with <paramref name="type"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="type"/> cannot be instantiated (e.g. <see cref="Activator.CreateInstance(Type)"/> returns <see langword="null"/>).
    /// </exception>
    /// <remarks>
    /// If two threads race to create an instance for the same <paramref name="type"/>, both may
    /// construct an instance, but only one will be stored via <see cref="ConcurrentDictionary{TKey,TValue}.TryAdd"/>;
    /// the "losing" instance is discarded and the caller still receives the instance now present in the cache
    /// is not guaranteed — the caller receives whichever instance it constructed, even if it wasn't the one stored.
    /// Consider this if construction has side effects.
    /// </remarks>
    public static object GetConfiguration(Type type)
    {
        if (ConfigMap.TryGetValue(type, out var cfg))
        {
            return cfg;
        }
        
        var instance = Activator.CreateInstance(type) ?? throw new InvalidOperationException("Unable to instantiate a null configuration.");
        
        ConfigMap.TryAdd(type, instance);
        return instance;
    }
    
    /// <summary>
    /// Strongly-typed convenience wrapper over <see cref="GetConfiguration(Type)"/>.
    /// </summary>
    /// <typeparam name="T">The configuration type to retrieve or instantiate. Must have an accessible parameterless constructor.</typeparam>
    /// <returns>The cached or newly created configuration instance of type <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="T"/> cannot be instantiated.
    /// </exception>
    public static T GetConfiguration<T>() where T : class => (T)GetConfiguration(typeof(T));
}