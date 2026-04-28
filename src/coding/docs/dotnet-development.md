# .NET Development  

## Prerequisites  

- **.NET SDK** – installed and on your PATH.  
- **IDE** – Visual Studio, VS Code, Rider, or any editor that supports .NET projects.  
- **Cyrena .NET Development extension** (`cyrena.dotnet`) – installed and enabled in Cyrena.  

## Supported Project Types  

`cyrena.dotnet` works with the following C# templates:

| Project Type | Description |
|--------------|-------------|
| **Class Library** | Standard reusable library (`.csproj`). |
| **Console Application** | Simple command‑line program. |
| **MVC Web App** | ASP.NET Core Model‑View‑Controller web application. |
| **MVC Library** | Library that contains MVC‑related components (controllers, views, etc.). |
| **Blazor Component Library** | Reusable UI components for Blazor. |
| **Blazor Web Application** | Full‑stack Blazor client‑side or server‑side app. |

> **Note** – The folder layout for each project type is opinionated. Keeping to the default structure guarantees stable and predictable AI behaviour; Cyrena will not restructure the hierarchy.

## Default Folder Structure  

All supported project types share the following top‑level items.  Files and folders are **writable** unless explicitly marked as read‑only.

| Folder / File | Purpose | Read‑Only? |
|---------------|--------|------------|
| **Attributes** | Custom attributes for metadata and decoration. | No |
| **Contracts** | Dependency‑injection interfaces. | No |
| **Extensions** | Static helper / extension classes. | No |
| **Models** | Data classes and DTOs. | No |
| **Services** | Implementations of the contracts. | No |
| **Options** | Configuration‑related POCOs. | No |
| `*.cs` (project root) | Miscellaneous C# source files. | No (Cyrena can read & edit `.cs` files here but not create new ones) |
| `*.csproj` / `*.sln` / `*.slnx` | Project and solution descriptors. | **Yes** – read‑only for the AI (they must not be edited directly). |

### Additional Folders for **Blazor** Projects  

| Folder | Allowed Content | Read‑Only? |
|-------|-----------------|------------|
| **Components** | `.razor` component files. | No |
| **Components/Layout** | Layout components. | No |
| **Components/Pages** | Page components. | No |
| **Components/Shared** | Shared UI pieces. | No |
| **wwwroot/css** | `.css` style‑sheet files. | No (fully readable & writable). |
| **wwwroot/js** | `.js` script files. | No (fully readable & writable). |
| `*.json` | Configuration files. | **Yes** – read‑only. |

### Additional Folders for **MVC** Projects  

| Folder | Allowed Content | Read‑Only? |
|-------|-----------------|------------|
| **Controllers** | C# controller classes. | No |
| **Views** (including all sub‑folders) | `.cshtml` Razor view files. | No |
| **wwwroot/css** | `.css` style‑sheet files. | No (fully readable & writable). |
| **wwwroot/js** | `.js` script files. | No (fully readable & writable). |
| `*.json` | Configuration files. | **Yes** – read‑only. |

## Recommended Workflow  

Using the folder layout that Cyrena understands and enforces yields the most reliable results.  If you need a different organization, you can create a **custom extension** that defines the rules you require.

### Step‑by‑Step Guide  

1. **Create the project**  
   - Use your IDE’s UI or the `dotnet` CLI (`dotnet new <template> …`).  
2. **Open Cyrena** and start a **New Chat**.  
3. Expand the **“.NET Development”** shortcuts.  
4. Choose the appropriate **project type** or select an existing **solution**.  
5. In the dialog that appears:  
   - Provide the full path to the `.csproj` or `.sln/.slnx` file.  
   - Select the **preferred AI connection**.  
   - Enable any additional model features you need.  
6. Click **Submit** and begin chatting with the AI to generate, modify, or reason about your code.  

---  

*Cyrena respects the permissions indicated above: files marked “read‑only” cannot be altered by the AI, while all other folders and files—including `wwwroot/css` and `wwwroot/js`—are fully readable and writable.*