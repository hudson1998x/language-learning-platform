# Builders — Responsibilities

The `Builders` module provides runtime generation of concrete repository proxy classes using `System.Reflection.Emit`. It bridges user-defined repository interfaces and the database adapter layer without requiring manual implementation classes.

## `RepositoryProxyBuilder`

The core IL-emission engine. Given a repository interface (one that inherits `IEntityRepository<T>` and thus `IDatabaseAdapter`), it emits a dynamic type at runtime that:

- Implements every method across the full inherited interface chain.
- Holds a lazily resolved `IDatabaseAdapter` field (resolved via `GetOrInitializeAdapter` on first use, not at construction).
- Routes each method call to one of three emission strategies:
  - **CRUD methods** (`CreateAsync`, `UpdateAsync`, `DeleteAsync`, `FindByIdAsync`, `FindAllAsync`, `TotalRecords`) — emit IL that constructs the appropriate `AstNode` (`WriteQueryNode`, `ReadQueryNode`, `DeleteQueryNode`) and forwards to the execution pipeline.
  - **Custom query methods** (decorated with `[Query]`) — emit IL that parses the query string via `AstParser.Parse`, binds `:paramName` tokens to method arguments, and executes the resulting AST.
  - **ExecuteQuery** — direct pass-through to `IDatabaseAdapter.ExecuteQuery`.

Caching support (enabled via `[Repository(IsCached = true, CacheSize = N)]`) is also emitted: cached CRUD operations maintain an in-memory object array with automatic fallback to the adapter when sorting or pagination is specified.

## `RepositoryProxyHelper`

Runtime support library consumed by the emitted proxy code:

- **Lazy adapter resolution** — `GetOrInitializeAdapter` uses double-checked locking to resolve the adapter exactly once via the `RepositoryConstructionEvents.Constructed` event, decoupling proxy construction from database module initialization order.
- **Execution pipeline** — `ExecuteAsync<T>` enforces security policies (`PolicyEnforcer.Enforce`), dispatches before/after entity lifecycle events (`BeforeCreate`, `AfterCreate`, etc.), and executes the query against the adapter.
- **Caching** — `GetOrInitializeCache` lazily loads the first `cacheSize` entities; `FindInCache`, `FindAllFromCache`, `InsertIntoCache`, `UpdateInCache`, `RemoveFromCache` provide linear-scan cache maintenance.
- **`CacheOp` enum** — `Insert`, `Update`, `Remove` signals which cache operation to perform post-execution.
