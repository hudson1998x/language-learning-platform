# Builders — Usage

## Defining a Repository Interface

Create an interface that inherits `IEntityRepository<T>`. Optionally, add custom query methods decorated with `[Query]`:

```csharp
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;

[Repository(typeof(User))]
public interface IUserRepository : IEntityRepository<User>
{
    [Query("Email = :email")]
    Task<User?> GetByEmailAsync(string email, UserContext context, DataOptions options);

    [Query("Status = :status and Age > :minAge")]
    Task<List<User>> GetActiveAdultsAsync(int status, int minAge, UserContext context, DataOptions options);
}
```

## Building a Proxy

Call `BuildProxyRepository<T>()` where you would normally instantiate a concrete repository:

```csharp
using LLE.Kernel.Builders;

var userRepo = RepositoryProxyBuilder.BuildProxyRepository<IUserRepository>();
// userRepo implements IUserRepository (and thus IEntityRepository<User>)
```

The proxy is a fully functional implementation. Use it anywhere the repository interface is expected.

## Enabling Caching

Apply the `[Repository]` attribute to the interface:

```csharp
using LLE.Kernel.Attributes;

[Repository(typeof(Product), IsCached = true, CacheSize = 100)]
public interface IProductRepository : IEntityRepository<Product>
{
}
```

With caching enabled:
- `FindAllAsync` returns results from the in-memory cache (unless `SortOption` or `Pagination` parameters are specified, which fall through to the adapter).
- `FindByIdAsync` searches the cache linearly by entity ID.
- CRUD operations (`Create`, `Update`, `Delete`) automatically maintain the cache.

## Execution Pipeline

Every method call flows through:

```
Proxy Method
  → GetOrInitializeAdapter (lazy, once)
  → PolicyEnforcer.Enforce (security)
  → Before* event (e.g., BeforeCreate)
  → adapter.ExecuteQuery (AstNode)
  → After* event (e.g., AfterCreate)
  → Cache maintenance (if enabled)
  → Result returned
```

## Notes

- The proxy type is generated with a fresh GUID suffix, so `BuildProxyRepository` can be called multiple times safely.
- Adapter resolution is deferred until the first method call, allowing you to build proxies before the database module registers itself.
- Generated types live only in memory; no assemblies are written to disk.
