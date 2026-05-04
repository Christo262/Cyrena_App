# Angular Development

## Prerequisites

- **Node.js** – installed and on your PATH.
- **Angular CLI** – installed globally (`npm install -g @angular/cli`).
- **Cyréna Angular extension** (`cyrena.angular`) – installed and enabled in Cyréna.

## Supported Version

`cyrena.angular` targets **Angular v17 and above**. Earlier versions are not supported.

## Enforced Folder Structure

The extension enforces a strict global/feature-scoped layout. Cyréna will not deviate from this structure — it is a hard constraint, not a guideline.

### Global (src/app/)

| Folder | Purpose |
|--------|---------|
| **components/** | Global reusable components. |
| **services/** | Global shared services. |
| **guards/** | Global route guards. |
| **pipes/** | Global custom pipes. |
| **directives/** | Global custom directives. |
| **models/** | Global shared models. |
| **interceptors/** | Global HTTP interceptors. |
| **resolvers/** | Global route resolvers. |
| **features/** | Feature modules (see below). |
| `app.component.ts` | Root application component. |
| `app.config.ts` | Application configuration. |
| `app.routes.ts` | Root route definitions. |

### Feature Modules (src/app/features/feature-name/)

Each feature module mirrors the global structure:

| Folder | Purpose |
|--------|---------|
| **components/** | Feature-scoped components. |
| **services/** | Feature-scoped services. |
| **guards/** | Feature-scoped route guards. |
| **pipes/** | Feature-scoped pipes. |
| **directives/** | Feature-scoped directives. |
| **models/** | Feature-scoped models. |
| **interceptors/** | Feature-scoped HTTP interceptors. |
| **resolvers/** | Feature-scoped route resolvers. |

### Other Top-Level Folders

| Folder | Purpose |
|--------|---------|
| **src/assets/** | Static assets. |
| **src/styles/** | Global stylesheets. |
| **src/environments/** | Environment configuration files. |
| **public/** | Angular v17+ static assets. |
| **e2e/** | End-to-end tests. |

> **Note** – Global concerns belong under `src/app/`. Feature-specific code belongs under `src/app/features/feature-name/`. Keeping to this layout guarantees stable and predictable AI behaviour; Cyréna will not restructure the hierarchy.

## Recommended Workflow

### Step-by-Step Guide

1. **Create the project**
   - Use the Angular CLI: `ng new my-app`
2. **Open Cyréna** and start a **New Chat**.
3. Expand the **"Web Development"** shortcuts.
4. Select **Angular**.
5. In the dialog that appears:
   - Browse to your project's **`angular.json`** file to establish the project root.
   - Enter a **Title** to identify this project in the sidebar.
   - Select the **AI Connection** to use.
   - Toggle any **Activated Features** you need.
6. Click **Submit** and begin chatting with the AI to generate, modify, or reason about your code.

---

*Global and feature-scoped folders mirror each other intentionally. This symmetry is what allows the AI to reason consistently about both shared and feature-specific code without ambiguity.*