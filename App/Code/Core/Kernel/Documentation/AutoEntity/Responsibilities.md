# AutoEntity — Responsibilities

`AutoEntityFeature` is a convention-based auto-routing system that automatically generates standard CRUD API endpoints for any entity type at startup, eliminating manual controller code.

## Core Responsibility

Given an entity type `T` and its corresponding repository interface `T1` (constrained to `IEntityRepository<T>`), `AutoFeature<T, T1>()` registers **5 feature endpoints** into the central `FeatureRegistry`:

| Endpoint | HTTP Method | Route |
|---|---|---|
| `create{TypeName}` | PUT | `/api/{typeName}/create` |
| `update{TypeName}` | PATCH | `/api/{typeName}/update` |
| `delete{TypeName}` | DELETE | `/api/{typeName}/delete` |
| `listAll{TypeName}` | GET | `/api/{typeName}/list` |
| `load{TypeName}` | GET | `/api/{typeName}/{id}` |

Route paths and feature names are derived **conventionally** from `typeof(T).Name`, keeping the API surface consistent across entities.

## Key Design Points

- **No boilerplate** — Adding a new entity with CRUD endpoints requires only one line of startup registration.
- **Reuses Kernel infrastructure** — Each handler resolves the repository from `RepositoryCatalog`, extracts `UserContext` from `HttpContext`, and delegates to standard `IEntityRepository<T>` methods.
- **Centralized registration** — All features are registered in the `FeatureRegistry`, making routing discoverable and auditable.
- **Standardized responses** — All endpoints return `ApiResponse<T>` wrapping `Success` and `Data`.
- **Error handling** — The `load` endpoint validates the `id` route parameter and throws `MalformedUrlException` if missing or invalid.

## Dependencies

- `FeatureRegistry` — Central store for routable features.
- `RepositoryCatalog` — Resolves concrete repository instances.
- `IEntityRepository<T>` — Contract for data access operations.
- `UserContext` — Security/user context extracted from the HTTP request.
- `DataOptions` — Options passed through to every repository call.
- `ApiResponse<T>` — Standardized response envelope.
