using System.Collections.Concurrent;
using System.Reflection;
using LLE.Eventing;
using LLE.Kernel.Registry;

namespace LLE.Kernel.Events;

public class DatabaseEvents : EventTable
{
    private readonly ConcurrentDictionary<Type, object> _seedCollections = new();

    public EventCollection<T> Seeding<T>() where T : class
    {
        return (EventCollection<T>)_seedCollections.GetOrAdd(
            typeof(T), _ => new EventCollection<T>());
    }

    public async Task SeedAllAsync()
    {
        foreach (var kvp in _seedCollections)
        {
            var repoType = kvp.Key;
            var collection = kvp.Value;
            var repository = RepositoryCatalog.GetRepository(repoType);

            var dispatch = typeof(EventCollection<>)
                .MakeGenericType(repoType)
                .GetMethod(nameof(EventCollection<object>.DispatchAsync),
                    BindingFlags.Public | BindingFlags.Instance)!;

            await (Task)dispatch.Invoke(collection, [repository])!;
        }
    }
}
