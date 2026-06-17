# ModelGateway — Usage Guide

## Overview

The `ModelGateway` module provides a unified way to interact with different LLM backends through a single abstraction.

All model interactions go through:

- `IModelGateway` (the contract)
- `Model providers` (concrete implementations)

The application never talks directly to external APIs or local runtimes.

---

## Core Flow

Using a model always follows the same pattern:

```text
Get model provider → Send chat session → Receive updated session
````

---

## Getting a Model Provider

A model provider represents a specific LLM backend implementation.

Example:

```csharp
var model = Provider.Get<OpenAiModelGateway>();
```

Or:

```csharp
var model = Provider.Get<OllamaModelGateway>();
```

You always retrieve a provider by its concrete type.

---

## Sending a Chat Request

Once you have a provider, you interact with it using a `ChatSession`.

### 1. Create a session

```csharp
var session = new ChatSession();

session.AddMessage("Hello!", ChatMessageRole.User);
```

---

### 2. Send to model

```csharp
var response = await model.ChatAsync(session);
```

---

### 3. Read response

The returned session contains the model’s output as additional messages:

```csharp
var lastMessage = response.Messages.Last();

Console.WriteLine(lastMessage.Message);
```

---

## Working with ChatSession

A `ChatSession` represents a full conversation history.

### Adding messages

```csharp
session.AddMessage("You are a helpful assistant", ChatMessageRole.System);
session.AddMessage("What is the capital of France?", ChatMessageRole.User);
```

---

### Removing messages

```csharp
session.RemoveMessage(messageId);
```

Or clear the session:

```csharp
session.Clear();
```

---

## Context Rules

You can attach behavioural constraints using `ContextRules`.

```csharp
session.ContextRules.Add(new ChatContextRule
{
    ContextMessage = "Respond in Spanish",
    Priority = 1.0f
});
```

These rules are included as part of the request context sent to the model.

---

## Model Information

Each provider can expose metadata about its capabilities:

```csharp
var info = await model.GetModelInfoAsync();
```

Example usage:

```csharp
Console.WriteLine(info.Name);
Console.WriteLine(info.ContextWindow);
Console.WriteLine(info.SupportsStreaming);
```

---

## Multiple Providers

You can work with multiple models in the same application:

```csharp
var openAi = Provider.Get<OpenAiModelGateway>();
var local = Provider.Get<OllamaModelGateway>();
```

Each provider is independent and stateless from the perspective of the caller.

---

## Typical Usage Pattern

```csharp
var model = Provider.Get<OllamaModelGateway>();

var session = new ChatSession();
session.AddMessage("Teach me Spanish greetings", ChatMessageRole.User);

var response = await model.ChatAsync(session);

foreach (var message in response.Messages)
{
    Console.WriteLine($"{message.Role}: {message.Message}");
}
```

---

## Key Rules

### 1. Sessions are the unit of communication

Everything is passed through `ChatSession`.

---

### 2. Providers are stateless from the caller’s perspective

You should treat every call as independent.

---

### 3. Responses always return a full session

The model does not return a single string — it returns an updated conversation state.

---

### 4. Role matters

Always use the correct `ChatMessageRole`:

* `System` → behavioural instructions
* `User` → user input
* `Assistant` → model output
* `Tool` → external results

---

## Summary

To use the system:

1. Get a model provider via `Provider.Get<T>()`
2. Build a `ChatSession`
3. Call `ChatAsync`
4. Read the returned session

That is the full interaction model.
