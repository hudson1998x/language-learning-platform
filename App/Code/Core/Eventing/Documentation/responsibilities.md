# Eventing Module Responsibilities

## Overview

The Eventing module provides a lightweight, in-process event system for communication between modules and services.

Events are organised into strongly-typed `EventTable` classes and exposed through `EventCollection<T>` properties.

The module enables:

* Loose coupling between components
* Event-driven workflows
* Payload transformation pipelines
* Independent side-effect execution
* Centralised event registration and dispatch

---

# Core Responsibilities

## Event Organisation

Provide a mechanism for grouping related events into logical domains.

Responsibilities:

* Group related events within a single `EventTable`
* Provide a discoverable event structure
* Create clear ownership boundaries between domains
* Reduce coupling between modules

Example:

```csharp
public sealed class UserEvents : EventTable
{
    public EventCollection<User> Created { get; } = new();
    public EventCollection<User> Deleted { get; } = new();
}
```

In this example:

* `UserEvents` represents a domain of related events.
* `Created` and `Deleted` are individual events.
* `EventCollection<User>` stores subscribers for those events.

---

## Event Subscription

Provide a thread-safe mechanism for registering event consumers.

Responsibilities:

* Register synchronous handlers
* Register asynchronous handlers
* Register pipeline consumers
* Register concurrent consumers
* Support runtime subscription

Subscribers may react to events without requiring direct references to the publisher.

---

## Event Dispatching

Provide a mechanism for publishing payloads to subscribers.

Responsibilities:

* Dispatch payloads to all registered consumers
* Execute pipeline handlers in registration order
* Execute concurrent handlers after pipeline completion
* Return the final pipeline result
* Ensure dispatch remains isolated from subscriber implementation details

---

## Payload Transformation

Provide support for sequential payload processing.

Responsibilities:

* Allow handlers to modify payloads
* Allow handlers to replace payloads
* Pass handler output to subsequent handlers
* Return the final transformed payload to the caller

Example flow:

```text
Payload
   ↓
Pipeline #1
   ↓
Pipeline #2
   ↓
Pipeline #3
   ↓
Final Payload
```

This enables validation, enrichment, normalisation, and other transformation workflows.

---

## Side-Effect Execution

Provide support for independent post-processing work.

Responsibilities:

* Execute handlers concurrently
* Allow multiple consumers to react independently
* Isolate side-effect workloads from payload transformation
* Ignore handler return values

Typical examples include:

* Logging
* Auditing
* Metrics collection
* Notifications
* Cache invalidation

---

## Error Aggregation

Provide consistent failure handling during dispatch.

Responsibilities:

* Continue executing remaining handlers after failures
* Collect exceptions from all handlers
* Expose failures through a single `AggregateException`
* Prevent a single subscriber from blocking the dispatch pipeline

---

## Event Table Lifetime Management

Provide singleton management for event tables.

Responsibilities:

* Lazily create event tables on first access
* Maintain a single instance per `EventTable` type
* Provide thread-safe retrieval
* Eliminate manual registration requirements

Example:

```csharp
var users = Eventing.Of<UserEvents>();
```

Every call returns the same instance.

---

## Thread Safety

Ensure event registration and dispatch can occur safely in multi-threaded environments.

Responsibilities:

* Protect handler collections from concurrent modification
* Create immutable dispatch snapshots
* Support concurrent event publication
* Provide safe singleton creation

---

# Intended Usage

The Eventing module should be used for:

* Domain events
* Module communication
* Lifecycle notifications
* Validation pipelines
* Audit trails
* Metrics collection
* Logging hooks
* Cache invalidation
* Application workflows

---

# Non-Responsibilities

The Eventing module is not responsible for:

* Network communication
* Message brokers
* Distributed event delivery
* Message persistence
* Event replay
* Event sourcing
* Retry policies
* Scheduled execution
* Dependency injection
* Service discovery

These concerns belong to dedicated infrastructure modules.

---

# Design Principles

## Loose Coupling

Publishers should not know which consumers are subscribed.

## Strong Typing

Payloads should be validated at compile time through generic event definitions.

## Predictable Execution

Pipeline handlers execute sequentially.

Concurrent handlers execute onlyj after pipeline completion.

## Failure Isolation

Subscriber failures should not prevent other subscribers from executing.

## Minimal Infrastructure

Event registration and dispatch should require little configuration and no external dependencies.

---

# Architectural Position

```text
Modules
    │
    ▼
Event Tables
    │
    ▼
Named Events
    │
    ▼
Event Collections
    │
    ├── Pipeline Handlers
    │
    └── Concurrent Handlers
```

The Eventing module acts as the application's internal communication layer, allowing independent modules to publish and consume events without direct knowledge of each other.
