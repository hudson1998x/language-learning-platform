# TypeScript Module — Responsibilities

## Overview

The TypeScript module is responsible for generating TypeScript project configuration and API client source code from .NET backend definitions. It runs during application startup and provides tools for other modules to contribute settings and generate typed fetch wrappers.

---

## 1. TypeScript Project Configuration

Generates `tsconfig.json` and `package.json` at application startup via `TypeScriptModule.AppStart()`.

### tsconfig.json (`TsConfigBuilder`)

- Default path mapping `@api/* → ./App/Api/*` so feature-generated API code resolves cleanly.
- Configurable `compilerOptions`: target, module, moduleResolution, JSX mode, strict flags, path mappings, and more.
- Extensible via event dispatch — other modules hook into `TypeScriptEvents.TsConfig` to add their own compiler settings before the file is written.
- Serializes to pretty-printed JSON.

### package.json (`PackageJsonBuilder`)

- Fluent API for setting name, version, description, entry point, scripts, and dependencies.
- Distinguishes between runtime (`dependencies`) and dev-only (`devDependencies`) dependencies via the `Dependencies` flags enum.
- Extensible via event dispatch — other modules hook into `NodeEvents.PackageJson` to register their own npm dependencies before the file is written.
- Always includes `"type": "module"` for ESM support.
- Serializes to pretty-printed JSON.

### npm Install

After writing both config files, the module runs `npm install` automatically, streaming stdout/stderr to the console. Failure to install throws `InvalidOperationException`.

---

## 2. C# → TypeScript Type Mapping (`TypeScriptTypeMapper`)

A reflection-based mapper that converts C# DTOs, entities, and enums into TypeScript declarations.

| C# Type | TypeScript Equivalent |
|---|---|
| `string`, `char`, `Guid`, `DateTime`, `DateTimeOffset`, `DateOnly`, `TimeOnly`, `TimeSpan` | `string` |
| `bool` | `boolean` |
| All numeric types | `number` |
| `object` | `unknown` |
| `Dictionary<K,V>` / `IDictionary<K,V>` | `Record<K, V>` |
| `List<T>`, arrays, `IEnumerable<T>` | `T[]` |
| Enums | `export enum Name { A = "A", ... }` |
| Classes / structs | `export interface Name { ... }` (camelCase properties) |
| Generic types | `Name<T, U>` with proper generic parameter names |
| Nullable value types | Underlying type (e.g. `int?` → `number`) |
| Nullable reference types | `prop?: type \| null` |

Key behaviors:
- Types are deduplicated per feature group to avoid redundant declarations.
- Circular references are handled via an emitted-set tracker.
- Properties are flattened across the inheritance chain.
- Public instance readable properties only; indexers are excluded.

---

## 3. API Client Code Generation (`ApiBuilder`)

Generates typed TypeScript fetch wrappers from `FeatureDefinition` instances registered in the Kernel.

- Accepts `FeatureDefinition` records from the Kernel's `FeatureRegistry`.
- Groups generated code by `FeatureGroup` — each group produces one `.ts` file (e.g. `users.ts`, `items.ts`).
- For **POST/PUT/PATCH** features: emits the input DTO interface, output DTO interface, and a typed `export const` function that calls `fetch()` with `JSON.stringify(payload)`, `Content-Type: application/json`, and error handling.
- For **GET** features: emits only the output DTO interface and a function with no payload parameter, no headers/body.
- Generated functions return `Promise<T>` and throw on non-OK responses.

---

## 4. Event-Driven Extensibility

Two event tables allow other modules to participate in configuration without coupling:

| Event | Payload | Purpose |
|---|---|---|
| `TypeScriptEvents.TsConfig` | `TsConfigBuilder` | Add compiler options, path mappings, include/exclude patterns |
| `NodeEvents.PackageJson` | `PackageJsonBuilder` | Add npm scripts, runtime/dev dependencies |

Subscribers can chain synchronously or concurrently via `EventCollection<T>` pipeline/concurrent handlers.

---

## 5. Directory Layout

```
TypeScript/
├── Source/
│   ├── TypeScript.csproj          # .NET 10.0 project (references Eventing, Kernel)
│   ├── Module.cs                  # IModuleLoader entry point
│   ├── Builders/
│   │   ├── TsConfigBuilder.cs     # tsconfig.json builder + compiler options + enums
│   │   ├── PackageJsonBuilder.cs  # package.json fluent builder
│   │   ├── TypescriptTypeMapper.cs# C# → TS type reflection mapper
│   │   └── ApiBuilder.cs          # API client code generator
│   └── Events/
│       ├── TypeScriptEvents.cs    # TsConfig event table
│       └── NodeEvents.cs          # PackageJson event table
├── Tests/
│   ├── TypeScriptTests.csproj     # xUnit test project
│   ├── TsConfigBuilderTests.cs
│   ├── PackageJsonBuilderTests.cs
│   ├── TypescriptTypeMapperTests.cs
│   └── ApiBuilderTests.cs
└── Documentation/
    ├── Responsibilities.md
    └── Usage.md
```
