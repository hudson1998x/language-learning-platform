using System.Collections.Concurrent;
using System.Reflection;
using LLE.Dependencies.Events;
using LLE.Dependencies.Utils;

namespace LLE.Dependencies.Repositories
{
    /// <summary>
    /// Provides access to dynamically generated repository implementations.
    ///
    /// Repository instances are created on first access from interfaces marked
    /// with <see cref="RepositoryAttribute"/>, cached for the lifetime of the
    /// application, and exposed as singletons.
    ///
    /// When a repository is created, a discovery event is published to allow
    /// external systems (such as database providers) to attach concrete query
    /// execution behaviour without introducing direct dependencies.
    /// </summary>
    public static partial class Repository
    {
        private static readonly ConcurrentDictionary<Type, object> RepositoryTable = new();

        /// <summary>
        /// Retrieves a repository implementation for the specified repository interface.
        ///
        /// If an implementation has not yet been created, a dynamic type is generated
        /// that satisfies the repository contract, registered internally, and announced
        /// through the repository discovery event pipeline.
        /// </summary>
        /// <param name="repositoryType">
        /// The repository interface type to resolve.
        /// </param>
        /// <returns>
        /// A singleton instance implementing the requested repository interface.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="repositoryType"/> is not an interface.
        /// </exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the interface is not marked with
        /// <see cref="RepositoryAttribute"/>.
        /// </exception>
        public static object Get(Type repositoryType)
        {
            if (RepositoryTable.TryGetValue(repositoryType, out var repository))
            {
                return repository;
            }

            // first, let's make sure it's an interface
            if (!repositoryType.IsInterface)
            {
                throw new ArgumentException($"{repositoryType.Name} is not an interface");
            }
        
            // next, let's make sure the repository attribute exists.
            if (repositoryType.GetCustomAttribute<RepositoryAttribute>() is null)
            {
                throw new InvalidOperationException($"Could not instantiate repository {repositoryType.Name}");
            }
        
            // now begins the fun. 
            // we're going to use typebuilder, to build a class
            // dynamically that satisfies the contract. 
            repository = CreateRepositoryFromInterface(repositoryType);

            // make the repository known.
            //
            // why? because database implementations can tap into this and
            // provide concrete query execution behaviour for newly created
            // repositories. this keeps repository contracts completely
            // database-agnostic, avoiding hard dependencies between the
            // repository layer and any persistence implementation.
            //
            // instead, adapters can discover repositories through the
            // event system and bind themselves using only .NET's type
            // system and reflection.
            AsyncUtils.Await(
                Eventing.Eventing.Of<DiscoveryEvents>().Repository.DispatchAsync((repositoryType, repository))
            );
        
            RepositoryTable.TryAdd(repositoryType, repository);
            return repository;
        }
    
        /// <summary>
        /// Retrieves a repository implementation for the specified repository interface.
        ///
        /// This is a type-safe wrapper around <see cref="Get(Type)"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The repository interface type to resolve.
        /// </typeparam>
        /// <returns>
        /// A singleton instance implementing <typeparamref name="T"/>.
        /// </returns>
        public static T Get<T>() => (T)Get(typeof(T)); 
    }
}