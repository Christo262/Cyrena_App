# PlatformIO Development

## Requirements

- Visual Studio Code installed.
- PlatformIO extension added to VS Code.
- Cyréna PlatformIO extension (`cyrena.platformio`) installed and enabled.

## Enforced Project Structure

Cyréna enforces a strict feature-based layout inside `src/` and `include/`. This structure is not a suggestion — the agent cannot deviate from it, ensuring consistent and maintainable firmware across sessions.

All firmware is organised into **features**. A feature represents a self-contained hardware or software concern, for example: `display`, `sensors`, `networking`, `motor_control`.

### src/ Layout

```
src/
  main.c / main.cpp
  {feature}/
    {feature}.c / {feature}.cpp
    actions/
    internals/
```

### include/ Layout

```
include/
  {feature}/
    {feature}.h
    definitions/
    actions/
    internals/
```

### Folder Responsibilities

| Folder | Location | Purpose |
|--------|----------|---------|
| `definitions/` | `include/{feature}/` only | Types, structs, enums, and constants. Never in `src/`. |
| `actions/` | Both | Function declarations in `include/`, implementations in `src/`. |
| `internals/` | Both | Private headers in `include/`, private implementations in `src/`. Never exposed outside the feature. |
| `{feature}.h` | `include/{feature}/` | The single public entry point for the feature. Consumers include only this file. |
| `{feature}.c / {feature}.cpp` | `src/{feature}/` | Feature initialisation or coordinator. Optional. |

> **Note** — Consumers of a feature include only `{feature}.h`. Internal headers are never included from outside their own feature.

## What Cyréna Indexes

### Core Project Layout (Arduino & ESP-IDF)

| Folder / File | Content | Access |
|---------------|---------|--------|
| **src** | Feature folders, `main.c` / `main.cpp`. | Read / write |
| **include** | Feature folders and their sub-folders. | Read / write |
| **lib** | All sub-folders; `.c`, `.cpp`, `.h` library files. | Read-only |
| **platformio.ini** | Project configuration file. | Read-only |

### Additional Folders for ESP-IDF Projects

| Folder / File | Content | Access |
|---------------|---------|--------|
| **managed_components** | All sub-folders; `.c`, `.cpp`, `.h` files. | Read-only |
| **components** | All sub-folders; `.c`, `.cpp`, `.h` files. | Read-only |
| **sdkconfig*** | ESP-IDF configuration files. | Read-only |

## Getting Started

1. **Create a PlatformIO project** in Visual Studio Code.
2. **Open Cyréna** and start a **New Chat**.
3. Expand the **Embedded** shortcuts.
4. Click **PlatformIO**.
5. In the dialog that appears:
   - Enter a title for the chat.
   - Provide the full path to the `platformio.ini` file (or browse to select it).
   - Choose the AI connection you wish to use.
   - Optionally enable or disable specific Cyréna features.
6. Press **Submit**.
7. Begin chatting with the AI to:
   - Add or modify source files.
   - Implement new features using the enforced structure.
   - Resolve build issues.
   - Ask any other project-specific questions.

---

*All indexed folders and files are accessed according to the permissions shown above. Cyréna will never modify read-only items.*