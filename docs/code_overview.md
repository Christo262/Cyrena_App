# Developing Cyréna

## Overview

Cyréna is designed to run effectively offline using smaller local models, while still supporting cloud providers such as OpenAI. The architecture prioritizes compatibility with models in the ~20B parameter class and enforces structured interaction patterns so they can reliably perform function calls.

The core philosophy is:

> Make it easy for the model to do the right thing.

Loose or ambiguous prompts produce unpredictable behavior. Cyréna enforces strict structure so that even smaller models can behave consistently, safely, and predictably.

## Enforced Project Structure

Rather than exposing a general-purpose "create file" function, Cyréna provides dedicated functions for creating specific file types in specific folders. The model cannot deviate from the defined project structure because the capability to do so simply does not exist.

Each domain defines a fixed folder structure and a set of scoped creation functions. The model only decides *what* to create — the application decides *where* it goes and *what type* it must be.

### Example: .NET Class Library

A .NET class library domain defines the following structure:

| Folder | Purpose |
|---|---|
| `Attributes` | Custom attribute classes, e.g. `MyAttribute : Attribute` |
| `Contracts` | Interfaces |
| `Extensions` | Static extension methods and classes |
| `Models` | Data classes and view models |
| `Options` | Configuration classes |
| `Services` | Implementations of Contracts |

When the model calls `create_model`, the application ensures the file is created in the `Models` folder as a `.cs` file. The model has no control over the path or file type.

Some project subtypes extend the base structure with additional folders and constraints. For example, a Blazor project adds:

| Folder | Allowed File Types |
|---|---|
| `Components` | `.razor` |
| `Components/Pages` | `.razor` |
| `Components/Shared` | `.razor` |

The model cannot place `.razor` files anywhere else, and cannot create files in these folders with any other extension.

This same pattern applies to every domain Cyréna supports. Arduino enforces a flat sketch structure. A future Angular domain would enforce feature module conventions. The domain defines the rules — the model simply works within them.

## Why This Works

This approach shifts the burden of structural correctness away from the model and onto the application. The model only needs to decide *what* to create, not *where* or *how* to place it. This makes behavior predictable across different model sizes and providers, and prevents the gradual structural entropy that typically occurs when an AI agent has unrestricted file system access.

---

## Project Types

Cyréna is intended to run alongside an IDE — not replace it.

The IDE remains the primary development environment. Cyréna acts as an assistant layer that understands and augments an existing project.

For .NET projects:

1. Create a standard .NET project in your IDE
2. Ensure the .NET SDK is installed
3. Create a Cyréna project
4. Link it to the existing `.csproj`

This pattern keeps Cyréna IDE-agnostic and avoids coupling it to any specific editor.

Additional project types can be added beyond .NET. Cyréna is designed to support multiple languages and ecosystems.

See:

👉 [Setup Project Type](./code_new_project_type.md)

---

## UI

Cyréna uses [Photino.NET](https://github.com/tryphotino/photino.NET) as a lightweight cross-platform web view shell to support Windows, macOS, and Linux.

It is implemented as a Blazor hybrid application to simplify development and maintain a consistent cross-platform UI stack.

The UI layer is built with:

- [BootstrapBlazor](https://blazor.zone)
- [Bootstrap CSS](https://getbootstrap.com/)

The interface prioritizes clarity, responsiveness, and low visual overhead so it can function as a companion tool without competing with the IDE.

