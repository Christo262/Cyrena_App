# Cyrena.Synthesis

Cyrena.Synthesis is a runtime capability system for Cyréna that enables AI assistants to safely discover, execute, create, and maintain reusable Dynamic Capabilities.

Dynamic Capabilities are runtime-executed F# capabilities that extend what AI assistants can do through approved ABI services and constrained execution environments.

---

# Overview

Cyrena.Synthesis introduces a structured runtime architecture for AI-driven extensibility.

Instead of relying entirely on static built-in tools, assistants can:

* discover reusable capabilities
* execute approved capabilities
* request new capabilities
* build new capabilities through a dedicated builder assistant
* maintain and evolve capability ecosystems over time

Dynamic Capabilities operate inside a constrained runtime with:

* ABI-based service access
* structured arguments
* structured logging
* structured result output
* permission-scoped execution
* sandboxed filesystem access
* workspace-aware execution

---

# Core Concepts

## Dynamic Capabilities

Dynamic Capabilities are reusable runtime capabilities written in F#.

Each capability:

* is stored persistently
* can be reused across conversations
* exposes structured arguments
* executes through the Cyrena.Synthesis runtime
* accesses approved services through ABI contracts

Examples:

* markdown automation
* food tracking
* project utilities
* workspace automation
* file processing
* local tooling

---

## Builder vs Consumer Assistants

Cyrena.Synthesis separates capability usage from capability authoring.

### Consumer Assistants

Consumer assistants can:

* list capabilities
* search capabilities
* execute capabilities
* request new capabilities
	* requests are forwarded to a new chat with the builder assistant

Consumer assistants cannot:

* create capabilities
* delete capabilities
* access ABI authoring details

---

### Builder Assistants

Builder assistants are responsible for:

* discovering ABI services
* generating F# capabilities
* testing capabilities
* repairing broken capabilities
* deleting obsolete capabilities
* maintaining compatibility between related capabilities

Builder assistants operate using ABI descriptors and runtime validation.

---

# ABI System

Dynamic Capabilities never access the system directly.

Instead, all functionality is exposed through ABI services.

Examples:

* `IFileSystemAbi`
* `ICapabilityArgs`
* `ICapabilityLogger`
* `ICapabilityResultWriter`

ABI descriptors provide:

* usage instructions
* supported methods
* examples
* runtime guidance

This allows assistants to dynamically discover and learn available runtime services without requiring massive static prompts.

---

# Runtime Context

Dynamic Capabilities execute using:

```fsharp
let main (ctx: ICapabilityExecutionContext) =
```

The execution context exposes:

```fsharp
ctx.Args
ctx.Log
ctx.Result
ctx.GetService<T>()
ctx.GetRequiredService<T>()
```

Capabilities communicate through:

* structured arguments
* structured logs
* structured result objects

---

# Structured Results

Dynamic Capabilities return structured data through:

```fsharp
ctx.Result.Text("summary", "Operation completed")
ctx.Result.Boolean("success", true)
ctx.Result.Json("data", value)
```

This separates runtime diagnostics from execution results.

---

# Sandboxed Execution

Dynamic Capabilities execute inside controlled workspaces.

The user controls:

* working directory selection
* filesystem boundaries
* permission approval

AI assistants cannot self-grant permissions.

Permission requests always require explicit user approval.

---

# Feature Activation

Cyrena.Synthesis can be enabled or disabled per chat through Cyréna's Feature Activation dialog.

This allows users to:

* disable Dynamic Capabilities for specific conversations
* isolate assistants from runtime capability access
* control which chats may execute or build capabilities
* reduce unnecessary runtime/tool exposure when not needed

Dynamic Capability access is therefore conversation-scoped and user-controlled.

---

# Dynamic Prompt Awareness

Cyrena.Synthesis integrates with Cyréna's Dynamic Prompt system.

Available capabilities are injected into the active system prompt during iteration startup.

This gives assistants lightweight awareness of existing capabilities without bloating context windows.

Assistants can then:

* discover capabilities dynamically
* inspect capabilities on demand
* reuse capabilities instead of regenerating them

---

# Architecture Goals

Cyrena.Synthesis is designed around several core principles:

* reusable AI-created functionality
* constrained runtime execution
* human-controlled permissions
* runtime discoverability
* workspace-aware tooling
* capability ecosystems instead of isolated scripts
* maintainable long-lived AI tooling

---

# Current State

Cyrena.Synthesis is actively evolving.

Current functionality includes:

* dynamic capability storage
* runtime execution
* ABI descriptors
* structured execution context
* builder/consumer separation
* workspace-scoped file operations
* structured logging and results
* runtime validation
* dynamic prompt integration
* F# capability execution

---

# Vision

Cyrena.Synthesis explores a different direction for AI systems.

Instead of stateless function calling, assistants can:

* accumulate reusable functionality
* evolve runtime tooling
* maintain capability ecosystems
* dynamically learn runtime APIs
* operate across persistent workspaces

The long-term goal is a safe, inspectable, extensible runtime capability platform for AI assistants.
