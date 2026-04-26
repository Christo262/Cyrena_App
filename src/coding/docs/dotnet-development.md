# .NET Development

Requirements:
- .NET SDK Installed
- IDE Installed
- .NET Development extension (cyrena.dotnet) installed

The following project types are supported in ``cyrena.dotnet``:
- C# Class Library
- C# Console Application
- C# Model-View-Controller Web App
- C# Model-View-Controller Library
- C# Blazor Component library
- C# Blazor Web Application

To ensure stable and predictable AI behaviour, the structure of these projects are opinionated and Cyréna cannot break this structure. 
The default folder structure for all project types include the following:

- Attributes: Custom attributes for metadata and decoration.
- Contracts: Dependency injection interfaces.
- Extensions: Static helper/extension classes.
- Models: Data classes and DTOs.
- Services: Implementations of Contracts.
- Options: Classes required for configuration.
- *.cs: Any .cs files in the project root directory
	- *Important*: Cyrena can see and edit these files, but cannot create new ones
- *.csproj (read only) or *.sln/slnx (read only) if working in a solution

The following additional folders are available in *Blazor*:
- Components
- Components/Layout
- Components/Pages
- Components/Shared
- wwwroot/css
	- Only allows .css stylesheet creation
- wwwroot/js
	- Only allows .js script creation
- *.json (read only)

The following additional folders are available in *MVC*:
- Controllers
- Views
	- *All Subfolders*
	- all *.cshtml* files
- wwwroot/css
	- Only allows .css stylesheet creation
- wwwroot/js
	- Only allows .js script creation
- *.json (read only)

For the simplest workflow, its best to use the structure Cyrena understands and enforces. 
Custom structures can be added by developing a custom extension for the application.

1. Create your project first
	- Use your IDE or command prompt
2. Open Cyrena > New Chat
3. Expand the '.NET Development' shortcuts
4. Select the project type or select solution
5. A dialog will appear prompting you to provide a path to the csproj or the sln/slnx
    - You can configure the prefered AI connection to use as well as what additional features you would like the model to have
6. Click Submit and start chatting 