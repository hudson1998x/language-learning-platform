using System.Collections.Concurrent;
using System.Reflection;
using LLE.Kernel.Attributes;

namespace LLE.Kernel.Registry;

/// <summary>
/// Central registry for repository contracts.
///
/// Repositories are interface-based abstractions that will later be backed by
/// runtime-generated proxy implementations.
/// </summary>
internal static class RepositoryCatalog
{
    /// <summary>
    /// Cached repository interface types mapped to their resolved proxy instances.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> Repositories = [];

    /// <summary>
    /// Resolves a repository instance for the given interface type.
    /// </summary>
    /// <param name="repositoryType">Interface type representing the repository.</param>
    /// <returns>Proxy-backed repository instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the type is not a valid repository interface or cannot be resolved.
    /// </exception>
    internal static object GetRepository(Type repositoryType)
    {
        // =========================
        // 1. FAST PATH: CACHE HIT
        // =========================
        if (Repositories.TryGetValue(repositoryType, out var existing))
        {
            return existing;
        }

        // =========================
        // 2. VALIDATION: MUST BE INTERFACE
        // =========================
        if (!repositoryType.IsInterface)
        {
            throw new InvalidOperationException(
                $"Repository type '{repositoryType.FullName}' must be an interface.");
        }

        // =========================
        // 3. VALIDATION: MUST BE MARKED AS [Repository]
        // =========================
        if (repositoryType.GetCustomAttribute<RepositoryAttribute>() is null)
        {
            throw new InvalidOperationException(
                $"Interface '{repositoryType.Name}' is not marked with [Repository].");
        }

        // =========================
        // 4. PROXY CREATION PLACEHOLDER
        // =========================
        // NOTE:
        // Actual implementation will be replaced with a dynamic proxy generator
        // (DispatchProxy, Reflection.Emit, or source-generated proxy).
        var proxy = CreateRepositoryProxy(repositoryType);

        // =========================
        // 5. CACHE RESULT
        // =========================
        Repositories.TryAdd(repositoryType, proxy);

        return proxy;
    }

    /// <summary>
    /// Placeholder for future proxy generation system.
    /// This will be replaced with a real dynamic proxy implementation.
    /// </summary>
    private static object CreateRepositoryProxy(Type repositoryType)
    {
        throw new NotImplementedException(
            $"Repository proxy generation not implemented for '{repositoryType.FullName}'.");
    }
}