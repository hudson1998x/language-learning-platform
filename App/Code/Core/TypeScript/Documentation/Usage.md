# TypeScript Module — Usage

## Adding Compiler Options to tsconfig.json

Subscribe to `TypeScriptEvents.TsConfig` in your module's startup to inject compiler settings before the file is written:

```csharp
using LLE.Eventing;
using LLE.TypeScript.Events;

public class MyModule : IModuleLoader
{
    public Task Install()
    {
        Eventing.Of<TypeScriptEvents>().TsConfig.Pipeline(config =>
        {
            config.CompilerOptions.Declaration = true;
            config.CompilerOptions.SourceMap = true;
            config.CompilerOptions.Paths["@shared/*"] = ["./Shared/*"];
            config.Include.Add("src/**/*.ts");
            config.Exclude.Add("**/*.spec.ts");
            return config;
        });

        return Task.CompletedTask;
    }
}
```

For async handlers:

```csharp
Eventing.Of<TypeScriptEvents>().TsConfig.Pipeline(async config =>
{
    var settings = await LoadRemoteSettingsAsync();
    config.CompilerOptions.Strict = settings.StrictMode;
    return config;
});
```

---

## Adding npm Dependencies

Subscribe to `NodeEvents.PackageJson` to register packages:

```csharp
using LLE.Eventing;
using LLE.TypeScript.Events;
using LLE.TypeScript.Builders;

Eventing.Of<NodeEvents>().PackageJson.Pipeline(pkg =>
{
    pkg.AddScript("build", "tsc");
    pkg.AddScript("test", "jest");

    pkg.AddDependency("express", "^4.18", Dependencies.App);
    pkg.AddDependency("typescript", "^5.4", Dependencies.Dev);

    return pkg;
});
```

Use `Dependencies.App | Dependencies.Dev` to add the same package to both sections:

```csharp
pkg.AddDependency("lodash", "^4.17", Dependencies.App | Dependencies.Dev);
```

---

## Generating TypeScript API Clients

Use `ApiBuilder` in conjunction with the Kernel's `FeatureRegistry` to produce typed fetch wrappers:

```csharp
using LLE.TypeScript.Builders;
using LLE.Kernel.Registry;

var features = FeatureRegistry.GetAll(); // hypothetical — features are already registered

using var apiBuilder = new ApiBuilder();

foreach (var feature in features)
{
    apiBuilder.AddFeature(feature);
}

// Option A: get the source as strings, keyed by feature group
var source = apiBuilder.Build();
foreach (var (group, tsSource) in source)
{
    Console.WriteLine($"// {group}.ts\n{tsSource}");
}

// Option B: write directly to disk as {group}.ts files
apiBuilder.WriteToDisk("./App/Api");
```

**Generated output example** (POST feature):

```typescript
export interface CreateUserInput {
    name: string;
    email: string;
}

export interface User {
    id: number;
    name: string;
}

export const createUser = (payload: CreateUserInput): Promise<User> => {
    return fetch('/api/users', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};
```

**Generated output example** (GET feature):

```typescript
export interface User {
    id: number;
    name: string;
}

export const getUsers = (): Promise<User> => {
    return fetch('/api/users', {
        method: "GET"
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};
```

---

## Using TypeScriptTypeMapper Independently

The type mapper can be used standalone to emit TypeScript declarations from any C# type:

```csharp
using System.Text;
using LLE.TypeScript.Builders;

var mapper = new TypeScriptTypeMapper();
var sb = new StringBuilder();

// Emit an interface for a DTO (and all its transitive dependencies)
mapper.EmitTypeAndDependencies(sb, "my-group", typeof(MyDto));

// Get a type reference for inline use (e.g., in a function signature)
var refName = mapper.GetTypeReference(typeof(List<MyDto>));
// → "MyDto[]"

var source = sb.ToString();
```

The `featureGroup` parameter controls deduplication — the same type can be emitted independently for different groups:

```csharp
var admin = new StringBuilder();
var publicSb = new StringBuilder();

mapper.EmitTypeAndDependencies(admin, "admin", typeof(User));
mapper.EmitTypeAndDependencies(publicSb, "public", typeof(User));
// User interface appears in both outputs
```

---

## Module Lifecycle

The `TypeScriptModule` implements `IModuleLoader` and is managed by the Kernel:

| Phase | Action |
|---|---|
| `Install()` | No-op (event subscriptions should happen here) |
| `AppStart()` | 1. Creates `TsConfigBuilder` and `PackageJsonBuilder` |
| | 2. Dispatches both through events for modification |
| | 3. Adds `@api/*` path mapping |
| | 4. Writes `tsconfig.json` and `package.json` to disk |
| | 5. Runs `npm install` |
| `AppStop()` | No-op |
| `Uninstall()` | No-op |
