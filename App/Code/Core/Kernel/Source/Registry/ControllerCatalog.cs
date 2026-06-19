using System.Collections.Concurrent;
using System.Reflection;
using LLE.Kernel.Attributes;

namespace LLE.Kernel.Registry;

/// <summary>
/// Responsible for constructing, caching, and exposing controller instances.
///
/// Controllers are the HTTP boundary layer and may ONLY depend on managers.
/// This enforces a strict architectural hierarchy:
///
/// Controller → Manager → Service → Repository
/// </summary>
internal static class ControllerCatalog
{
    /// <summary>
    /// Cached controller instances keyed by type.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, object> Controllers = [];

    /// <summary>
    /// Resolves a controller instance, constructing it if not already cached.
    /// </summary>
    /// <param name="controllerType">Controller type to resolve.</param>
    /// <returns>Instantiated controller.</returns>
    internal static object GetController(Type controllerType)
    {
        // =========================
        // 1. FAST PATH: CACHE HIT
        // =========================
        if (Controllers.TryGetValue(controllerType, out var existing))
        {
            return existing;
        }

        // =========================
        // 2. VALIDATE CONTROLLER ATTRIBUTE
        // =========================
        var controllerAttr = controllerType.GetCustomAttribute<ControllerAttribute>();

        if (controllerAttr is null)
        {
            throw new InvalidOperationException(
                $"Type '{controllerType.FullName}' is not marked with [Controller].");
        }

        // =========================
        // 3. SELECT CONSTRUCTOR (greediest)
        // =========================
        var constructor = controllerType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"No public constructor found for controller '{controllerType.FullName}'.");
        }

        var parameters = constructor.GetParameters();
        var args = new object?[parameters.Length];

        // =========================
        // 4. RESOLVE DEPENDENCIES
        // RULE: controllers may ONLY depend on managers
        // =========================
        for (var i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;

            // Enforce architectural rule via attribute
            var isManager = paramType.GetCustomAttribute<ManagerAttribute>() is not null;

            if (!isManager)
            {
                throw new InvalidOperationException(
                    $"Invalid dependency '{paramType.Name}' in controller '{controllerType.Name}'. " +
                    "Controllers may only depend on types marked with [Manager].");
            }

            // Resolve manager instance
            args[i] = ManagerCatalog.GetManager(paramType);
        }

        // =========================
        // 5. INSTANTIATE CONTROLLER
        // =========================
        var instance = Activator.CreateInstance(controllerType, args);

        if (instance is null)
        {
            throw new InvalidOperationException(
                $"Failed to create controller instance '{controllerType.FullName}'.");
        }

        // =========================
        // 6. CACHE
        // =========================
        Controllers.TryAdd(controllerType, instance);

        return instance;
    }

    internal static object[] GetInstances()
    {
        return Controllers.Values.ToArray();
    }
}