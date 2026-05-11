# Cyréna

Cyréna is an AI-native engineering workspace that adapts to your workflow through extensions.

It runs alongside your IDE — not inside it.

Cyréna helps developers build, maintain, and evolve real software projects across multiple engineering domains including .NET, embedded systems, firmware, web platforms, and static websites.

You choose the model.
Cyréna orchestrates the workflow.

---

## Why Cyréna Exists

Cyréna started from a practical problem.

I needed internal business tooling — invoicing, billing, supplier management, operational systems — and wanted to move faster using AI.

AI app builders helped generate software quickly, but the infrastructure, hosting, and execution environment remained locked behind proprietary platforms. The software was not truly mine. I could not fully control deployment, infrastructure, or long-term ownership.

Cyréna was built to solve that problem.

Instead of replacing developers, Cyréna operates as an engineering workspace that works alongside them. It helps developers build software faster while keeping ownership, architecture, infrastructure, and deployment fully under their control.

The goal is not magic.

The goal is disciplined AI-assisted engineering inside real projects.

---

## What Cyréna Actually Is

Cyréna is not:

* a chatbot
* a code autocomplete plugin
* a cloud-dependent SaaS
* a “one-click app builder”
* a replacement for engineering discipline

Cyréna is an engineering workspace built around structured workflows, project awareness, persistent technical memory, and extension-driven domain support.

It operates directly inside real projects and helps with:

* feature implementation
* architecture enforcement
* compile-error repair loops
* technical documentation
* project memory persistence
* iterative engineering workflows
* multi-domain development

The platform is designed to keep developers in control while dramatically increasing engineering throughput.

---

## Core Platform Concepts

### Extension-Driven Architecture

Cyréna is built around extensions.

Every engineering domain is implemented as an extension, including:

* .NET
* Arduino IDE
* PlatformIO
* Angular
* Static Website Development

Extensions define:

* project structures
* prompts
* workflows
* tooling constraints
* repair strategies
* domain-specific behavior

This allows Cyréna to adapt to different workflows without becoming a generic “AI assistant.”

---

### Dynamic System Prompts

Cyréna dynamically updates system prompts based on:

* active extensions
* project type
* enabled capabilities
* engineering domain
* current workflow context

The model only receives the instructions relevant to the active task.

This keeps prompts focused, smaller, and domain-aware.

---

### Feature Activation

Features can be enabled or disabled per chat.

Disabled functionality is removed from the model’s available toolset entirely.

This prevents:

* irrelevant tool usage
* accidental actions
* prompt bloat
* workflow confusion

The AI only knows what it needs to know.

---

### Prompt Queuing

Cyréna supports prompt queuing.

Developers can queue multiple engineering tasks and allow the platform to work through them sequentially.

Each iteration:

1. Loads relevant context
2. Inspects project files
3. Performs the requested work
4. Builds and validates
5. Repairs issues if necessary
6. Persists technical knowledge
7. Continues to the next queued task

This allows longer engineering workflows to execute in a controlled and repeatable way.

---

## Multi-Domain Engineering Support

Cyréna currently supports multiple engineering domains through extensions.

### .NET

* C# Class Libraries
* Blazor Applications
* MVC Applications
* MVC Libraries

### Embedded & Firmware

* Arduino IDE
* PlatformIO

  * Arduino Framework
  * ESP-IDF

### Web Development

* Angular
* Static HTML/CSS/JavaScript websites

Each domain has its own:

* project structures
* prompts
* engineering rules
* architecture constraints
* repair workflows

Different domains. Same engineering discipline.

---

## API References

API References are structured technical documents generated and maintained by the platform.

They provide persistent technical memory describing:

* APIs
* architecture rules
* service contracts
* module behavior
* integration patterns
* implementation details

API References are grounded in real project code rather than hallucinated summaries.

This allows Cyréna to maintain long-term project understanding beyond normal chat context limitations.

---

## Sticky Notes

Sticky Notes act as lightweight persistent project memory.

They capture:

* architectural decisions
* engineering constraints
* developer reminders
* project-specific rules
* workflow guidance

Sticky Notes survive chat resets and model switches.

---

## Project-Aware Engineering

Cyréna works directly inside real project structures.

It:

* reads existing files
* inspects architecture
* follows project conventions
* performs minimal edits
* validates builds
* repairs failures iteratively

The platform is designed to avoid uncontrolled code generation and reduce architectural entropy over time.

---

## Iterative Repair Loops

Engineering tasks operate through structured loops:

1. Inspect project state
2. Load API References
3. Read Sticky Notes
4. Review relevant files
5. Implement minimal changes
6. Build and validate
7. Repair failures
8. Persist technical knowledge
9. Summarise work completed

This approach keeps workflows controlled and repeatable.

---

## Offline-First & Model Agnostic

Cyréna is designed to work with:

* Ollama
* OpenAI
* compatible AI providers

Developers can:

* run models locally
* switch providers mid-project
* work offline
* keep code on their own infrastructure

The platform is model agnostic.

You bring the model.
Cyréna provides the engineering workflow.

---

## Static Website Development

Cyréna now includes a static website engineering extension.

The Website extension supports:

* semantic HTML5
* responsive CSS
* vanilla JavaScript
* structured project layouts
* SEO-friendly static content
* accessibility-aware markup
* asset management
* multi-page websites

This extension was used to rebuild the Cyréna marketing website from Angular-rendered landing pages into static crawlable content.

---

## Screenshots & Documentation

* 📸 [Screenshots](./docs/screenshots.md)
* 🧠 [Architecture Overview](./docs/code_overview.md)
* 📸 [UI Overview](./docs/ui_overview.md)
* 🤝 [Contributing](./contributing.md)
* 👉 [Getting Started](https://cyrena.dev)

---

## Demo

Watch Cyréna perform a real prompt → build → repair workflow:

👉 https://cyrena.dev

This is not autocomplete.

This is structured AI-assisted engineering inside real projects.

---

## Requirements

* .NET 10
* Windows / Linux / macOS
* Ollama or OpenAI
* Recommended: ≥16k context models

Hardware requirements scale with model size.

---

## Getting Started

### Setup Ollama

1. Run Cyréna
2. Open Settings
3. Add an Ollama connection
4. Configure model + tokens + context
5. Save

### Setup OpenAI

1. Open Settings
2. Add API key + model
3. Save

### Create a Project

1. Click **New Chat**
2. Select a project type
3. Configure the project
4. Submit

---

## Hardware Notes

Baseline development system:

* RTX 3060 12GB
* 48GB RAM
* Ryzen 7 8700F

Typical local test configuration:

* `gpt-oss:20b`
* 8k tokens
* 16k context

Larger models require additional VRAM and RAM.

---

## Philosophy

Cyréna is being built around:

* disciplined AI behavior
* reversible engineering workflows
* project-aware reasoning
* persistent technical memory
* predictable code generation
* developer supervision
* architecture safety
* long-term maintainability

The goal is not replacing developers.

The goal is building better engineering workflows.

---

## Thanks to Open Source

Cyréna stands on the shoulders of excellent open-source projects:

* [.NET](https://github.com/dotnet)
* [Ollama](https://ollama.com)
* [Arduino](https://www.arduino.cc/)
* [BootstrapBlazor](https://github.com/dotnetcore/BootstrapBlazor)
* [BlazorMonaco](https://github.com/serdarciplak/BlazorMonaco)
* [Photino.NET](https://github.com/tryphotino/photino.NET)
* [Bootstrap](https://getbootstrap.com/)

---

## Disclaimer

Cyréna modifies real files inside real projects.

Always use version control.
Always review AI-generated changes.

You are responsible for your codebase.

Provided as-is. Use at your own risk.
