using System.Collections.Concurrent;
using System.Collections;
using System.Reflection;
using LLE.Eventing;
using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Events;
using LLE.Kernel.Security;
using LLE.SharedUtils.Threading;

namespace LLE.Kernel.Builders;

public static class RepositoryProxyHelper
{
    private static readonly ConcurrentDictionary<Type, PropertyInfo[]> _uniqueProperties = new();
    public static IDatabaseAdapter InitializeAdapter(Type repositoryType, Type entityType)
    {
        var ctx = new RepositoryConstructionContext
        {
            RepositoryType = repositoryType,
            EntityType = entityType
        };

        var result = AsyncUtils.Await(
            Eventing.Eventing.Of<RepositoryConstructionEvents>().Constructed.DispatchAsync(ctx));

        return result.Adapter
            ?? throw new InvalidOperationException(
                $"No database adapter was registered for repository '{repositoryType.Name}' " +
                $"(entity: '{entityType.Name}'). Ensure a database module has subscribed to " +
                $"{nameof(RepositoryConstructionEvents)}.Constructed.");
    }

    /// <summary>
    /// Lazily resolves and caches the <see cref="IDatabaseAdapter"/> for a repository proxy instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Proxy methods call this on every invocation instead of resolving the adapter in the proxy's
    /// constructor. This decouples proxy construction from database-module registration order: a proxy
    /// can be built before its owning database module has subscribed to
    /// <see cref="RepositoryConstructionEvents"/>, as long as the module has registered by the time the
    /// first call is actually made.
    /// </para>
    /// <para>
    /// Uses double-checked locking, keyed on <paramref name="repositoryType"/>, to avoid two threads
    /// racing into <see cref="InitializeAdapter"/> concurrently for the same proxy instance — that call
    /// dispatches an event and blocks synchronously on it via <see cref="AsyncUtils.Await"/>, which is
    /// expensive enough (and risky enough re: deadlocks) to be worth avoiding doing twice. The lock is
    /// keyed on <paramref name="repositoryType"/> rather than a dedicated per-instance lock object, since
    /// proxies don't currently carry one — this means concurrent first-calls to *different proxy
    /// instances of the same repository type* will serialize against each other too, which is a
    /// negligible cost given this only happens once per instance.
    /// </para>
    /// </remarks>
    /// <param name="adapter">
    /// Reference to the proxy instance's cached adapter field. Read under lock; written under lock if
    /// still <see langword="null"/>.
    /// </param>
    /// <param name="repositoryType">The repository interface type (e.g. IUserRepository).</param>
    /// <param name="entityType">The entity type T resolved from IEntityRepository&lt;T&gt;.</param>
    /// <returns>The resolved (now guaranteed non-null) adapter.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown by <see cref="InitializeAdapter"/> if no adapter has been registered for this
    /// repository/entity pair.
    /// </exception>
    public static IDatabaseAdapter GetOrInitializeAdapter(
        ref IDatabaseAdapter adapter, Type repositoryType, Type entityType)
    {
        // Fast path: already resolved and cached by a previous call — no lock needed to read it, since
        // once non-null it's never reassigned.
        if (adapter is not null)
        {
            return adapter;
        }

        lock (repositoryType)
        {
            // Re-check inside the lock: another thread may have resolved (and assigned) the adapter
            // between the fast-path check above and acquiring the lock.
            if (adapter is null)
            {
                adapter = InitializeAdapter(repositoryType, entityType);
            }
        }

        return adapter;
    }

    public static async Task<T> ExecuteAsync<T>(IDatabaseAdapter adapter, AstNode node, UserContext context, DataOptions options)
    {
        PolicyEnforcer.Enforce(node, context, options);

        var events = Eventing.Eventing.Of<EntityEvents<T>>();

        switch (node)
        {
            case WriteQueryNode write when write.Where is null:
                write.Payload = (await events.BeforeCreate.DispatchAsync((T)write.Payload!))!;
                break;
            case WriteQueryNode write:
                write.Payload = (await events.BeforeUpdate.DispatchAsync((T)write.Payload!))!;
                break;
            case DeleteQueryNode delete:
                delete.Payload = (await events.BeforeDelete.DispatchAsync((T)delete.Payload!))!;
                break;
        }

        var raw = await adapter.ExecuteQuery(node).ConfigureAwait(false);
        T result;
        if (raw is System.Collections.IList list
            && typeof(T).IsGenericType
            && typeof(T).GetGenericTypeDefinition() == typeof(List<>))
        {
            var elementType = typeof(T).GetGenericArguments()[0];
            var typedList = (System.Collections.IList)Activator
                .CreateInstance(typeof(List<>).MakeGenericType(elementType))!;
            for (var i = 0; i < list.Count; i++)
                typedList.Add(list[i]);
            result = (T)typedList;
        }
        else if (raw is System.Collections.IList singleList)
        {
            result = singleList.Count > 0 ? (T)singleList[0]! : default!;
        }
        else
        {
            result = (T)raw!;
        }

        switch (node)
        {
            case WriteQueryNode write when write.Where is null:
                await events.AfterCreate.DispatchAsync(result);
                break;
            case WriteQueryNode:
                await events.AfterUpdate.DispatchAsync(result);
                break;
            case DeleteQueryNode:
                await events.AfterDelete.DispatchAsync(result);
                break;
        }

        return result;
    }

    public static async Task<T> ExecuteWithCacheOp<T>(
        IDatabaseAdapter adapter, AstNode node, UserContext context, DataOptions options,
        object[] cache, Type entityType, CacheOp op) where T : class
    {
        var result = await ExecuteAsync<T>(adapter, node, context, options).ConfigureAwait(false);

        switch (op)
        {
            case CacheOp.Insert:
                InsertIntoCache(cache, result!, entityType);
                break;
            case CacheOp.Update:
                UpdateInCache(cache, result!, entityType);
                break;
            case CacheOp.Remove:
                RemoveFromCache(cache, result!, entityType);
                break;
        }

        return result;
    }

    // --- Cache helpers ---

    public static object[] GetOrInitializeCache(ref object[]? cache, IDatabaseAdapter adapter, Type entityType, int cacheSize)
    {
        if (cache is not null)
            return cache;

        lock (entityType)
        {
            if (cache is not null)
                return cache;

            var node = new ReadQueryNode
            {
                TableName = entityType.Name,
                EntityType = entityType
            };

            var result = AsyncUtils.Await(adapter.ExecuteQuery(node));
            cache = new object[cacheSize];

            if (result is IList list)
            {
                var count = Math.Min(list.Count, cacheSize);
                for (var i = 0; i < count; i++)
                    cache[i] = list[i]!;
            }
        }

        return cache;
    }

    public static object? FindInCache(object[] cache, Type entityType, Guid id)
    {
        var idProp = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp is null)
            return null;

        var comparer = EqualityComparer<Guid>.Default;
        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is not null && cache[i].GetType() == entityType)
            {
                var candidateId = idProp.GetValue(cache[i]);
                if (candidateId is Guid g && comparer.Equals(g, id))
                    return cache[i];
            }
        }

        return null;
    }

    public static List<T> FindAllFromCache<T>(object[] cache) where T : class
    {
        var result = new List<T>(cache.Length);
        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is T entity)
                result.Add(entity);
        }

        return result;
    }

    private static PropertyInfo[] GetUniqueProperties(Type entityType)
    {
        return _uniqueProperties.GetOrAdd(entityType, static t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<UniqueAttribute>() is not null)
                .ToArray());
    }

    private static bool IsDuplicateInCache(object[] cache, object item)
    {
        var entityType = item.GetType();
        var uniqueProps = GetUniqueProperties(entityType);
        if (uniqueProps.Length == 0)
            return false;

        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is null || cache[i].GetType() != entityType)
                continue;

            var match = true;
            foreach (var prop in uniqueProps)
            {
                var existingVal = prop.GetValue(cache[i]);
                var newVal = prop.GetValue(item);

                if (existingVal is null || newVal is null)
                {
                    if (existingVal != newVal)
                    {
                        match = false;
                        break;
                    }
                }
                else if (!existingVal.Equals(newVal))
                {
                    match = false;
                    break;
                }
            }

            if (match)
                return true;
        }

        return false;
    }

    public static void InsertIntoCache(object[] cache, object item, Type entityType)
    {
        if (IsDuplicateInCache(cache, item))
            return;

        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is null)
            {
                cache[i] = item;
                return;
            }
        }
    }

    public static void UpdateInCache(object[] cache, object item, Type entityType)
    {
        var idProp = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp is null)
            return;

        var id = idProp.GetValue(item);
        if (id is null)
            return;

        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is not null && cache[i].GetType() == entityType)
            {
                if (idProp.GetValue(cache[i])?.Equals(id) == true)
                {
                    cache[i] = item;
                    return;
                }
            }
        }

        InsertIntoCache(cache, item, entityType);
    }

    public static void RemoveFromCache(object[] cache, object item, Type entityType)
    {
        var idProp = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
        if (idProp is null)
            return;

        var id = idProp.GetValue(item);
        if (id is null)
            return;

        for (var i = 0; i < cache.Length; i++)
        {
            if (cache[i] is not null && cache[i].GetType() == entityType)
            {
                if (idProp.GetValue(cache[i])?.Equals(id) == true)
                {
                    cache[i] = null!;
                    return;
                }
            }
        }
    }
}

public enum CacheOp
{
    Insert,
    Update,
    Remove
}