using System.Collections.Concurrent;
using System.Reflection;

namespace LLE.Dependencies.Providers
{
    /// <summary>
    /// Provides a simple type-based singleton registry for provider instances.
    /// Instances are created lazily on first request and cached for the lifetime of the application.
    /// </summary>
    /// <remarks>
    /// This registry:
    /// <list type="bullet">
    /// <item>
    /// <description>Stores one instance per <see cref="Type"/>.</description>
    /// </item>
    /// <item>
    /// <description>Creates instances using a parameterless constructor via reflection.</description>
    /// </item>
    /// <item>
    /// <description>Is thread-safe via <see cref="ConcurrentDictionary{TKey, TValue}"/>.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static class Provider
    {
        /// <summary>
        /// Internal cache of provider instances keyed by their concrete <see cref="Type"/>.
        /// </summary>
        private static readonly ConcurrentDictionary<Type, object> Providers = [];

        /// <summary>
        /// Retrieves an existing provider instance for the specified type, or creates and caches a new one
        /// if it does not already exist.
        /// </summary>
        /// <param name="type">The concrete type of the provider to retrieve.</param>
        /// <returns>An instance of the requested provider type.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the specified type does not have a parameterless constructor.
        /// </exception>
        internal static object Get(Type type)
        {
            if (Providers.TryGetValue(type, out var provider))
            {
                return provider;
            }

            if (type.GetCustomAttribute<ProviderAttribute>() is null)
            {
                throw new InvalidOperationException(
                    $"Class {type.Name} doesn't have a {nameof(ProviderAttribute)} attribute, therefore it cannot be instanced by the provider class");
            }

            var constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor == null)
            {
                throw new InvalidOperationException("No empty constructor found");
            }

            var instanced = constructor.Invoke(Array.Empty<object>());
            Providers.TryAdd(type, instanced);
            return instanced;
        }

        /// <summary>
        /// Retrieves a singleton instance of the specified provider type.
        /// If the instance does not exist, it will be created using its parameterless constructor.
        /// </summary>
        /// <typeparam name="T">The provider type to retrieve.</typeparam>
        /// <returns>A cached or newly created instance of <typeparamref name="T"/>.</returns>
        public static T Get<T>() where T : new()
        {
            return (T)Get(typeof(T));
        }
    }
}