# AutoEntity — Usage

## Registration

Call `AutoEntityFeature.AutoFeature<T, T1>()` during application startup (e.g., in a composition root or service configurator):

```csharp
using LLE.Kernel.AutoEntity;

// Registers all 5 CRUD endpoints for Product with IProductRepository
AutoEntityFeature.AutoFeature<Product, IProductRepository>();

// Registers endpoints for Order with IOrderRepository
AutoEntityFeature.AutoFeature<Order, IOrderRepository>();
```

## Generated Routes

For `AutoFeature<Product, IProductRepository>()`, the following routes become active:

| Method | Route | Handler |
|---|---|---|
| `PUT` | `/api/product/create` | `repository.CreateAsync(entity, userContext, DataOptions.Default)` |
| `PATCH` | `/api/product/update` | `repository.UpdateAsync(entity, userContext, DataOptions.Default)` |
| `DELETE` | `/api/product/delete` | `repository.DeleteAsync(entity, userContext, DataOptions.Default)` |
| `GET` | `/api/product/list` | `repository.FindAllAsync(userContext, DataOptions.Default)` |
| `GET` | `/api/product/{id}` | `repository.FindByIdAsync(id, userContext, DataOptions.Default)` |

The `load` endpoint expects a `Guid` as the `id` route parameter. An invalid or missing `id` results in a `MalformedUrlException`.

## Prerequisites

Each entity must have:

1. A repository interface that inherits `IEntityRepository<T>`.
2. A concrete repository implementation registered in `RepositoryCatalog`.
3. The entity class must be a reference type (`class` constraint).

## Example

```csharp
// Domain model
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}

// Repository interface
public interface IProductRepository : IEntityRepository<Product> { }

// Startup registration
AutoEntityFeature.AutoFeature<Product, IProductRepository>();
```

No additional controller or handler code is needed for basic CRUD operations.
