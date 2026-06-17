# Configuration System Responsibilities

## Overview

The configuration system provides a lightweight mechanism for registering and resolving strongly-typed configuration objects at runtime.

It is designed around a simple rule: configuration types are discovered, validated, and instantiated automatically, then cached as singletons for the lifetime of the provider.

## Core Components

### ConfigurationProvider

The `ConfigurationProvider` is responsible for:

- Resolving configuration instances by type
- Enforcing configuration type rules
- Ensuring each configuration type is instantiated only once
- Providing both untyped (`Get(Type)`) and strongly-typed (`Get<T>`) access

It acts as the central access point for all configuration retrieval.

### ConfigurationAttribute

The `ConfigurationAttribute` marks a class as a valid configuration type.

Only types decorated with this attribute are eligible for resolution by the provider.

## Responsibilities

### ConfigurationProvider is responsible for:

- Enforcing that only marked configuration types can be resolved
- Ensuring configuration types cannot define constructors
- Creating instances via parameterless activation
- Caching instances in a thread-safe manner
- Returning consistent singleton instances per type

### Configuration types are responsible for:

- Representing immutable or semi-immutable configuration state
- Containing no constructor logic or dependencies
- Remaining simple data carriers for application configuration

## Constraints

Configuration types must adhere to the following rules:

- Must be decorated with `ConfigurationAttribute`
- Must not declare any constructors
- Must be instantiable via `Activator.CreateInstance`
- Must not rely on external services or dependencies

## Lifecycle

1. A configuration type is requested via `ConfigurationProvider`
2. The provider validates the type
3. The provider instantiates the type (if not already cached)
4. The instance is stored in a concurrent cache
5. Subsequent requests return the cached instance

## Design Intent

This system is intentionally minimal:

- No dependency injection graph
- No configuration binding pipeline
- No lifecycle complexity beyond singleton caching

It is designed for fast, predictable access to configuration data with minimal overhead.