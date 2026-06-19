using System.Collections.Concurrent;
using System.Reflection;
using LLE.Kernel.Attributes;

namespace LLE.Kernel.Registry;

/// <summary>
/// Responsible for creating, caching, and resolving manager instances within the kernel.
/// 
/// Managers are treated as top-level orchestrators in the system and are constructed
/// using constructor injection from services only (not other managers).
/// 
/// This enforces a strict architectural rule:
/// managers define orchestration boundaries, services define dependencies.
/// </summary>
internal static class ManagerCatalog
{
    /// <summary>
    /// Cache of already constructed manager singletons.
    /// Keyed by manager type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> Managers = [];

    /// <summary>
    /// Retrieves an existing manager instance or constructs a new one if not cached.
    /// Construction uses the "greediest constructor" (most parameters) and resolves
    /// dependencies exclusively from the ServiceCatalog.
    /// </summary>
    /// <param name="managerType">The manager type to resolve.</param>
    /// <returns>An instantiated and cached manager instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if no valid constructor exists or instance creation fails.
    /// </exception>
    internal static object GetManager(Type managerType)
    {
        // =========================
        // 1. FAST PATH: CACHE HIT
        // =========================
        if (Managers.TryGetValue(managerType, out var existing))
        {
            return existing;
        }

        // =========================
        // 2. SELECT CONSTRUCTOR
        //    - Choose the constructor with the most parameters
        //    - This represents the "most complete dependency set"
        // =========================
        var constructor = managerType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"No public constructor found for manager '{managerType.FullName}'.");
        }

        var parameters = constructor.GetParameters();

        // =========================
        // 3. BUILD ARGUMENT LIST
        //    - Resolve each dependency from ServiceCatalog
        //    - Enforce rule: only services are allowed here
        // =========================
        var args = new object?[parameters.Length];

        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];

            // Enforce architectural boundary:
            // managers may NOT depend on other managers
            if (param.ParameterType.GetCustomAttribute<ServiceAttribute>() is null)
            {
                throw new InvalidOperationException(
                    $"Invalid dependency '{param.ParameterType.Name}' in manager '{managerType.Name}'. " +
                    "Only services marked with [Service] can be injected into managers.");
            }

            // Resolve service instance from service registry
            args[i] = ServiceCatalog.GetService(param.ParameterType);
        }

        // =========================
        // 4. INSTANTIATE MANAGER
        // =========================
        var instance = Activator.CreateInstance(managerType, args);

        if (instance is null)
        {
            throw new InvalidOperationException(
                $"Failed to create manager instance '{managerType.FullName}'.");
        }

        // =========================
        // 5. CACHE RESULT (THREAD-SAFE)
        //    - Only store if not already added by another thread
        // =========================
        Managers.TryAdd(managerType, instance);

        return instance;
    }
}