using System.Collections.Concurrent;
using System.Reflection;
using LLE.Configuration.Attributes;
using LLE.Dependencies.Providers;

namespace LLE.Configuration.Providers
{
    [Provider]
    public class ConfigurationProvider
    {
        /// <summary>
        /// Thread-safe cache of configuration singletons.
        /// Each configuration type is created at most once.
        /// </summary>
        private readonly ConcurrentDictionary<Type, object> _configs = [];

        /// <summary>
        /// Resolves a configuration instance for the specified type.
        /// </summary>
        /// <param name="type">
        /// The configuration type to resolve. Must be marked with <see cref="ConfigurationAttribute"/>
        /// and expose a public parameterless constructor.
        /// </param>
        /// <returns>
        /// A singleton instance of the requested configuration type.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the type is not valid or cannot be instantiated.
        /// </exception>
        public object Get(Type type)
        {
            return _configs.GetOrAdd(type, CreateInstance);
        }

        /// <summary>
        /// Strongly-typed convenience method for resolving a configuration instance.
        /// </summary>
        public T Get<T>() => (T)Get(typeof(T));

        /// <summary>
        /// Validates and creates a configuration instance.
        /// Executed at most once per type under concurrency.
        /// </summary>
        private static object CreateInstance(Type type)
        {
            ValidateConfigurationType(type);

            var ctor = type.GetConstructor(Type.EmptyTypes);
            if (ctor is null)
            {
                throw new InvalidOperationException(
                    $"Configuration type {type.Name} must have a public parameterless constructor");
            }

            return ctor.Invoke(null)
                   ?? throw new InvalidOperationException(
                       $"Unable to instantiate {type.Name}");
        }

        /// <summary>
        /// Ensures the type is marked as a configuration.
        /// </summary>
        private static void ValidateConfigurationType(Type type)
        {
            if (type.GetCustomAttribute<ConfigurationAttribute>() is null)
            {
                throw new InvalidOperationException(
                    $"Missing configuration attribute on {type.Name}");
            }
        }
    }
}