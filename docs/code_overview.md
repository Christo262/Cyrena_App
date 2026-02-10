# Developing Cyréna

## Overview

Cyréna is designed to run effectively offline using smaller local models, while still supporting cloud providers such as OpenAI. The architecture prioritizes compatibility with ~20B-class local models and enforces structured interaction patterns so they can reliably perform function calls.

The core philosophy is:

> Make it easy for the model to do the right thing.

Loose or ambiguous prompts produce unpredictable behavior. Cyréna enforces strict structure so even smaller models can behave consistently, safely, and deterministically.

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

