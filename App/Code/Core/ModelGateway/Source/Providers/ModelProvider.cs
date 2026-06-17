using System.Collections.Concurrent;
using LLE.Dependencies.Providers;
using LLE.ModelGateway.Contracts;

namespace LLE.ModelGateway.Providers
{
    /// <summary>
    /// Central registry for managing and resolving registered <see cref="IModelGateway"/> implementations.
    /// </summary>
    /// <remarks>
    /// This provider acts as a lightweight in-memory container for model gateway implementations.
    /// It allows multiple gateways to be registered and retrieved by their concrete type.
    /// </remarks>
    [Provider]
    public class ModelProvider
    {
        /// <summary>
        /// Internal thread-safe dictionary storing registered model gateway instances,
        /// keyed by their concrete runtime <see cref="Type"/>.
        /// </summary>
        private readonly ConcurrentDictionary<Type, IModelGateway> _providers = [];

        /// <summary>
        /// Registers a new <see cref="IModelGateway"/> implementation in the provider registry.
        /// </summary>
        /// <param name="provider">The model gateway instance to register.</param>
        public void AddProvider(IModelGateway provider)
        {
            _providers[provider.GetType()] = provider;
        }

        /// <summary>
        /// Retrieves a registered <see cref="IModelGateway"/> implementation by its concrete type.
        /// </summary>
        /// <typeparam name="T">The concrete type of the model gateway to retrieve.</typeparam>
        /// <returns>The registered instance of <typeparamref name="T"/>.</returns>
        /// <exception cref="TypeAccessException">
        /// Thrown when no provider of the requested type has been registered.
        /// </exception>
        public IModelGateway GetProvider<T>() where T : IModelGateway
        {
            return _providers.TryGetValue(typeof(T), out var provider)
                ? (T)provider
                : throw new TypeAccessException("No provider found for type " + typeof(T));
        }

        /// <summary>
        /// Retrieves the first registered <see cref="IModelGateway"/> instance, if any exist.
        /// </summary>
        /// <returns>
        /// The first registered provider, or <see langword="null"/> if no providers are registered.
        /// </returns>
        public IModelGateway? GetDefaultProvider()
        {
            return _providers.Values.FirstOrDefault() ?? null;
        }
    }
}