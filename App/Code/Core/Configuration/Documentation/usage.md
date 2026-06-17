# Configuration System Usage Guide

## Defining a Configuration

To create a configuration type, define a class and mark it with `ConfigurationAttribute`.

The class must have no constructors.

```csharp
using LLE.Configuration.Attributes;

[Configuration]
public class AppConfig
{
    public string ApiUrl => "https://example.com";

    public int TimeoutMs => 5000;
}
````

## Retrieving a Configuration

Configurations are resolved through `ConfigurationProvider`.

### Strongly typed access

```csharp
var provider = new ConfigurationProvider();

var config = provider.Get<AppConfig>();

Console.WriteLine(config.ApiUrl);
```

### Untyped access

```csharp
var config = (AppConfig)provider.Get(typeof(AppConfig));
```

## Behaviour

* The first request creates the configuration instance
* All subsequent requests return the same instance
* Instances are cached per type

## Validation Rules

If a type violates configuration rules, resolution will fail:

### Missing attribute

```csharp
// Throws InvalidOperationException
var config = provider.Get<UnmarkedConfig>();
```

### Constructor present

```csharp
[Configuration]
public class BadConfig
{
    public BadConfig(int x) { }
}
```

This will throw during resolution because configuration types must be parameterless.

## Thread Safety

The provider is safe for concurrent use. Multiple threads may request the same configuration type without creating duplicate instances.

## Recommended Usage Pattern

* Use configurations for static or startup-loaded application settings
* Avoid runtime mutation where possible
* Treat configuration instances as shared read-only state
