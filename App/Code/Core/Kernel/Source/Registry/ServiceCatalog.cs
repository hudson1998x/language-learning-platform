using System.Collections.Concurrent;
using System.Reflection;
using LLE.Kernel.Attributes;

namespace LLE.Kernel.Registry;

/// <summary>
/// Responsible for constructing and caching service instances.
/// 
/// Services represent business logic units and may depend on repositories only.
/// They must NOT depend on other services.
/// </summary>
public static class ServiceCatalog
{
    private static readonly ConcurrentDictionary<Type, object> Services = [];

    internal static object GetService(Type serviceType)
    {
        // =========================
        // 1. CACHE FAST PATH
        // =========================
        if (Services.TryGetValue(serviceType, out var existing))
        {
            return existing;
        }

        // =========================
        // 2. SELECT CONSTRUCTOR
        // =========================
        var constructor = serviceType
            .GetConstructors(BindingFlags.Public | BindingFlags.Instance)
            .OrderByDescending(c => c.GetParameters().Length)
            .FirstOrDefault();

        if (constructor is null)
        {
            throw new InvalidOperationException(
                $"No public constructor found for service '{serviceType.FullName}'");
        }

        var parameters = constructor.GetParameters();
        var args = new object?[parameters.Length];

        // =========================
        // 3. RESOLVE DEPENDENCIES
        //    RULES:
        //    - only repositories allowed
        //    - no service-to-service calls
        // =========================
        for (var i = 0; i < parameters.Length; i++)
        {
            var param = parameters[i];

            var paramType = param.ParameterType;

            // ---- Repository enforcement ----
            var isRepository = paramType.GetCustomAttribute<RepositoryAttribute>() is not null;

            if (!isRepository)
            {
                throw new InvalidOperationException(
                    $"Invalid dependency '{paramType.Name}' in service '{serviceType.Name}'. " +
                    "Services may only depend on repositories.");
            }

            // Resolve repository (you likely already have this somewhere)
            args[i] = RepositoryCatalog.GetRepository(paramType);
        }

        // =========================
        // 4. CREATE INSTANCE
        // =========================
        var instance = Activator.CreateInstance(serviceType, args);

        if (instance is null)
        {
            throw new InvalidOperationException(
                $"Failed to create service '{serviceType.FullName}'.");
        }

        // =========================
        // 5. CACHE
        // =========================
        Services.TryAdd(serviceType, instance);

        return instance;
    }
    
    public static T GetService<T>() => (T)GetService(typeof(T));
}