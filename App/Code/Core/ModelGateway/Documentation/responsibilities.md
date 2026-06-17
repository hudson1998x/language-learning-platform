# ModelGateway — Responsibilities

## Purpose

The `ModelGateway` module is a **transport and abstraction layer** between the application and external or local LLM providers.

Its sole responsibility is to provide a consistent interface for sending and receiving chat-based model interactions, regardless of backend implementation (remote API, local process, or local server runtime).

It does not contain or enforce domain logic, application rules, or decision-making about how model output is used.

---

## Core Responsibilities

### 1. Unified LLM Communication Interface

Provide a single abstraction (`IModelGateway`) for interacting with any supported model backend.

- Accepts a `ChatSession` as the request payload
- Returns a `ChatSession` containing model-generated output
- Exposes model metadata via `GetModelInfoAsync`

All provider-specific differences are hidden behind the interface.

---

### 2. Message Transport Only

The gateway is responsible for transporting structured chat data to and from model providers.

It must:

- Preserve message ordering
- Maintain message identity (`Id`) where possible
- Maintain timestamps where applicable
- Pass through `ChatContextRule` data as contextual input

It must NOT:

- Interpret message meaning or intent
- Apply domain-specific rules or business logic
- Modify conversation content based on application state
- Perform prompt engineering tied to business features

---

### 3. Backend Agnostic Execution

The gateway supports multiple execution strategies via `ModelBackendType`:

- `RemoteApi` → cloud-hosted LLM providers
- `LocalProcess` → spawned local model runtimes
- `LocalServer` → locally hosted HTTP inference services

Responsibilities include:

- Routing requests to the correct backend implementation
- Normalizing request/response formats across providers
- Abstracting differences in token limits, streaming, and tool support

---

### 4. Model Capability Reporting

Through `ChatModelInfo`, the gateway exposes:

- Model identity (name, provider, version)
- Context window and output limits
- Latency characteristics
- Cost metadata (if applicable)
- Feature support (streaming, tool calling)

This is purely descriptive and must not influence application behavior directly.

---

## Data Model Responsibilities

### ChatSession

Represents a single conversational context.

The gateway treats this as an **opaque container of messages and rules**.

- Messages are ordered and immutable in meaning once sent
- Context rules are passed through as guidance, not interpreted logic
- Session identity (`Id`) is preserved for traceability

---

### ChatMessage

Represents a single message in a conversation.

The gateway ensures:

- Role (`System`, `User`, `Assistant`, `Tool`) is preserved
- Content is transmitted without alteration
- Timestamp and identity are retained when supported by backend

---

### ChatContextRule

Represents contextual constraints or instructions.

The gateway:

- Passes these through to the model provider when supported
- Does not resolve conflicts between rules
- Does not prioritise or interpret rule semantics beyond ordering/priority metadata

---

### ChatModelInfo

Represents metadata about a model.

The gateway:

- Populates this from provider capabilities
- Does not derive or infer missing capabilities
- Does not enforce constraints based on this data internally

---

## Non-Goals (Explicitly Out of Scope)

The ModelGateway does NOT:

- Implement domain logic (e.g. language learning rules, agent behavior)
- Perform prompt construction beyond structural translation
- Maintain conversation state beyond a single session object
- Decide what messages “mean” or how they should be used
- Implement retry policies tied to business outcomes
- Store long-term conversation history

---

## Design Philosophy

The gateway should remain:

- **Stateless where possible**
- **Deterministic in transformation**
- **Provider-agnostic**
- **Domain-blind**

It is a translation layer, not an intelligence layer.

---

## Summary

If it answers:

> “How do I talk to this model?”

It belongs here.

If it answers:

> “What should the model do, and why?”

It does NOT belong here.