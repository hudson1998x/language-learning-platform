# Eventing Module Usage

## Overview

The Eventing module provides a lightweight, strongly-typed event system for communication between modules.

Events are grouped into `EventTable` classes. Each property on an `EventTable` represents a named event, while the associated `EventCollection<T>` stores the handlers subscribed to that event.

```csharp
public sealed class UserEvents : EventTable
{
    public EventCollection<User> Created { get; } = new();
    public EventCollection<User> Deleted { get; } = new();
}
```

In this example:

* `UserEvents` groups user-related events.
* `Created` represents a "User Created" event.
* `Deleted` represents a "User Deleted" event.
* `User` is the payload dispatched to subscribers.

---

# Retrieving Event Tables

Event tables are accessed through the global registry.

```csharp
var users = Eventing.Of<UserEvents>();
```

Each `EventTable` type is created once and reused for the lifetime of the application.

```csharp
var a = Eventing.Of<UserEvents>();
var b = Eventing.Of<UserEvents>();

Console.WriteLine(ReferenceEquals(a, b));
// True
```

---

# Subscribing to Events

Handlers can be registered using either `Pipeline()` or `Concurrent()`.

## Pipeline Handlers

Pipeline handlers execute sequentially.

Each handler receives the output of the previous handler.

```csharp
Eventing.Of<UserEvents>()
    .Created
    .Pipeline(user =>
    {
        user.Username = user.Username.Trim();
        return user;
    })
    .Pipeline(user =>
    {
        user.Username = user.Username.ToUpperInvariant();
        return user;
    });
```

Execution:

```text
User
 ↓
Pipeline #1
 ↓
Pipeline #2
 ↓
Final User
```

---

## Concurrent Handlers

Concurrent handlers execute in parallel after all pipeline handlers have completed.

Their return values are ignored.

```csharp
Eventing.Of<UserEvents>()
    .Created
    .Concurrent(user =>
    {
        Logger.Log("AUDIT", $"Created {user.Username}");
    })
    .Concurrent(async user =>
    {
        await Metrics.RecordAsync("user.created");
        return user;
    });
```

Execution:

```text
Final User
    │
 ┌──┼──┐
 ▼  ▼  ▼
 A  B  C
```

Concurrent handlers should be used for independent side effects such as:

* Logging
* Auditing
* Metrics
* Notifications
* Cache invalidation

---

# Dispatching Events

Events are dispatched using `DispatchAsync`.

```csharp
var user = new User
{
    Username = "john"
};

await Eventing.Of<UserEvents>()
    .Created
    .DispatchAsync(user);
```

All registered handlers will execute.

---

# Receiving Pipeline Results

Pipeline handlers may modify or replace the payload.

The final payload is returned to the caller.

```csharp
var result =
    await Eventing.Of<UserEvents>()
        .Created
        .DispatchAsync(user);

Console.WriteLine(result.Username);
```

Example:

```csharp
Eventing.Of<UserEvents>()
    .Created
    .Pipeline(user =>
    {
        user.Username = user.Username.Trim();
        return user;
    })
    .Pipeline(user =>
    {
        user.Username = user.Username.ToUpperInvariant();
        return user;
    });
```

Input:

```text
"  john  "
```

Output:

```text
"JOHN"
```

---

# Asynchronous Handlers

Both pipeline and concurrent handlers may be asynchronous.

```csharp
Eventing.Of<UserEvents>()
    .Created
    .Pipeline(async user =>
    {
        await repository.SaveAsync(user);
        return user;
    });
```

```csharp
Eventing.Of<UserEvents>()
    .Created
    .Concurrent(async user =>
    {
        await emailService.SendWelcomeEmailAsync(user);
        return user;
    });
```

---

# Error Handling

All handlers execute regardless of failures.

Exceptions are collected and re-thrown as a single `AggregateException`.

```csharp
try
{
    await Eventing.Of<UserEvents>()
        .Created
        .DispatchAsync(user);
}
catch (AggregateException ex)
{
    foreach (var error in ex.InnerExceptions)
    {
        Console.WriteLine(error.Message);
    }
}
```

This allows multiple handler failures to be observed from a single dispatch operation.

---

# Recommended Usage

Use pipeline handlers when:

* Validation is required
* Payload mutation is required
* Payload enrichment is required
* Execution order matters

Use concurrent handlers when:

* Logging
* Metrics collection
* Auditing
* Notifications
* Independent background work

---

# Complete Example

```csharp
public sealed class UserEvents : EventTable
{
    public EventCollection<User> Created { get; } = new();
}

Eventing.Of<UserEvents>()
    .Created
    .Pipeline(user =>
    {
        user.Username = user.Username.Trim();
        return user;
    })
    .Pipeline(user =>
    {
        Console.WriteLine("Persisting user");
        return user;
    })
    .Concurrent(user =>
    {
        Console.WriteLine("Audit log");
    })
    .Concurrent(user =>
    {
        Console.WriteLine("Metrics");
    });

var user = new User
{
    Username = " john "
};

await Eventing.Of<UserEvents>()
    .Created
    .DispatchAsync(user);
```

Execution order:

```text
Pipeline #1
Pipeline #2
Concurrent #1
Concurrent #2
```
