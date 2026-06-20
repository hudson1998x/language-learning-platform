using LLE.Eventing;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Events;
using LLE.SharedUtils.Threading;

namespace LLE.Kernel.Builders;

public static class RepositoryProxyHelper
{
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

    public static async Task<T> ExecuteAsync<T>(IDatabaseAdapter adapter, AstNode node)
    {
        return (T)(await adapter.ExecuteQuery(node).ConfigureAwait(false));
    }
}