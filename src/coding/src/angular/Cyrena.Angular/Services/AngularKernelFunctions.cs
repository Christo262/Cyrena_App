using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Cyrena.Angular.Services
{
    internal class AngularKernelFunctions
    {
        private readonly IChatMessageService _context;
        private readonly IDevelopPlanService _plan;
        public AngularKernelFunctions(IChatMessageService context, IDevelopPlanService plan)
        {
            _context = context;
            _plan = plan;
        }

        // ------------------------------------------------------------------
        // Feature creation
        // ------------------------------------------------------------------

        [KernelFunction("create_feature")]
        [Description("Creates a new feature module under src/app/features/ with standard subfolders (components, services, guards, pipes, directives, models).")]
        public ToolResult<DevelopFolder> CreateFeature(
            [Description("Name of the feature in camelCase or kebab-case, e.g. 'users' or 'user-management'.")] string name)
        {
            name = Path.GetFileNameWithoutExtension(name).ToLowerInvariant();
            var kebab = ToKebabCase(name);

            var app = GetAppFolder();
            var features = _plan.Plan.GetOrCreateFolder(app, "features", "features");
            var feature = _plan.Plan.GetOrCreateFolder(features, $"features_{kebab}", kebab);

            // Create standard subfolders in plan AND on disk
            var rootDir = _plan.Plan.RootDirectory;
            var featurePath = Path.Combine(rootDir, feature.RelativePath);
            Directory.CreateDirectory(featurePath);

            var subfolders = new[] { "components", "services", "guards", "pipes", "directives", "models", "interceptors", "resolvers" };
            foreach (var sub in subfolders)
            {
                var subFolder = _plan.Plan.GetOrCreateFolder(feature, $"features_{kebab}_{sub}", sub);
                var subPath = Path.Combine(rootDir, subFolder.RelativePath);
                Directory.CreateDirectory(subPath);
            }

            _context.LogInfo($"Created feature '{kebab}' with standard subfolders");
            return new ToolResult<DevelopFolder>(feature);
        }

        // ------------------------------------------------------------------
        // Component creation — ALWAYS in src/app/components/ or src/app/features/<feature>/components/
        // ------------------------------------------------------------------

        [KernelFunction("create_component")]
        [Description("Creates a new Angular standalone component using 'ng generate component'. If inFeature is provided, runs in src/app/features/<feature>/components/; otherwise in src/app/components/.")]
        public ToolResult<DevelopFile> CreateComponent(
            [Description("Name of the component in PascalCase, e.g. 'UserProfile'.")] string name,
            [Description("Optional feature name. If provided, component goes in src/app/features/<feature>/components/. If null, goes in src/app/components/.")] string? inFeature = null)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var kebab = ToKebabCase(name);

            var folder = GetTypedFolder("components", inFeature);
            var rootDir = _plan.Plan.RootDirectory;
            var workingDir = Path.Combine(rootDir, folder.RelativePath);

            // Ensure the directory exists on disk
            Directory.CreateDirectory(workingDir);

            _context.LogInfo($"Running 'ng generate component {kebab}' in {workingDir}");

            var result = RunNgCommand($"generate component {kebab}", workingDir);
            if (!result.Success)
                return new ToolResult<DevelopFile>(false, $"ng generate component failed: {result.Message}");

            // Index the newly created component folder into the plan
            var componentFolder = _plan.Plan.GetOrCreateFolder(folder, $"{folder.Id}_{kebab}", kebab);
            var componentPath = Path.Combine(workingDir, kebab);

            if (Directory.Exists(componentPath))
            {
                // Index all files in the component folder
                foreach (var filePath in Directory.GetFiles(componentPath))
                {
                    var fileName = Path.GetFileName(filePath);
                    var ext = Path.GetExtension(fileName).TrimStart('.');
                    var baseName = Path.GetFileNameWithoutExtension(fileName);
                    var id = $"{folder.Id}_{kebab}_{ext}_{baseName}";
                    var file = _plan.Plan.CreateFile(componentFolder, id, fileName, null);
                    _plan.InvokeFileCreated(file);
                }
            }

            // Return the .ts file as the primary file
            var tsId = $"{folder.Id}_{kebab}_ts_{kebab}.component";
            if (_plan.Plan.TryFindFile(tsId, out var tsFile))
                return new ToolResult<DevelopFile>(tsFile!);

            // Fallback: return first file found
            if (componentFolder.Files.Count > 0)
                return new ToolResult<DevelopFile>(componentFolder.Files[0]);

            return new ToolResult<DevelopFile>(false, "Component created but no files were indexed.");
        }

        // ------------------------------------------------------------------
        // Service creation — ALWAYS in src/app/services/ or src/app/features/<feature>/services/
        // ------------------------------------------------------------------

        [KernelFunction("create_service")]
        [Description("Creates a new Angular injectable service using 'ng generate service'. If inFeature is provided, creates in src/app/features/<feature>/services/; otherwise in src/app/services/.")]
        public ToolResult<DevelopFile> CreateService(
            [Description("Name of the service in PascalCase, e.g. 'UserService'.")] string name,
            [Description("Optional feature name. If provided, service goes in src/app/features/<feature>/services/. If null, goes in src/app/services/.")] string? inFeature = null)
        {
            return CreateNgArtifact("services", "service", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Guard creation — ALWAYS in src/app/guards/ or src/app/features/<feature>/guards/
        // ------------------------------------------------------------------

        [KernelFunction("create_guard")]
        [Description("Creates a new Angular route guard using 'ng generate guard'. If inFeature is provided, creates in src/app/features/<feature>/guards/; otherwise in src/app/guards/.")]
        public ToolResult<DevelopFile> CreateGuard(
            [Description("Name of the guard in PascalCase, e.g. 'AuthGuard'.")] string name,
            [Description("Optional feature name. If provided, guard goes in src/app/features/<feature>/guards/. If null, goes in src/app/guards/.")] string? inFeature = null)
        {
            return CreateNgArtifact("guards", "guard", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Pipe creation — ALWAYS in src/app/pipes/ or src/app/features/<feature>/pipes/
        // ------------------------------------------------------------------

        [KernelFunction("create_pipe")]
        [Description("Creates a new Angular pipe using 'ng generate pipe'. If inFeature is provided, creates in src/app/features/<feature>/pipes/; otherwise in src/app/pipes/.")]
        public ToolResult<DevelopFile> CreatePipe(
            [Description("Name of the pipe in PascalCase, e.g. 'CurrencyPipe'.")] string name,
            [Description("Optional feature name. If provided, pipe goes in src/app/features/<feature>/pipes/. If null, goes in src/app/pipes/.")] string? inFeature = null)
        {
            return CreateNgArtifact("pipes", "pipe", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Directive creation — ALWAYS in src/app/directives/ or src/app/features/<feature>/directives/
        // ------------------------------------------------------------------

        [KernelFunction("create_directive")]
        [Description("Creates a new Angular directive using 'ng generate directive'. If inFeature is provided, creates in src/app/features/<feature>/directives/; otherwise in src/app/directives/.")]
        public ToolResult<DevelopFile> CreateDirective(
            [Description("Name of the directive in PascalCase, e.g. 'HighlightDirective'.")] string name,
            [Description("Optional feature name. If provided, directive goes in src/app/features/<feature>/directives/. If null, goes in src/app/directives/.")] string? inFeature = null)
        {
            return CreateNgArtifact("directives", "directive", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Model creation — ALWAYS in src/app/models/ or src/app/features/<feature>/models/
        // ------------------------------------------------------------------

        [KernelFunction("create_model")]
        [Description("Creates a new TypeScript model/interface file. If inFeature is provided, creates in src/app/features/<feature>/models/; otherwise in src/app/models/.")]
        public ToolResult<DevelopFile> CreateModel(
            [Description("Name of the model in PascalCase, e.g. 'User'.")] string name,
            [Description("Optional feature name. If provided, model goes in src/app/features/<feature>/models/. If null, goes in src/app/models/.")] string? inFeature = null)
        {
            var folder = GetTypedFolder("models", inFeature);
            return CreateArtifactInFolder(folder, name, typeSuffix: "model", fileSuffix: "model");
        }

        // ------------------------------------------------------------------
        // Interceptor creation — ALWAYS in src/app/interceptors/ or src/app/features/<feature>/interceptors/
        // ------------------------------------------------------------------

        [KernelFunction("create_interceptor")]
        [Description("Creates a new Angular HTTP interceptor using 'ng generate interceptor'. If inFeature is provided, creates in src/app/features/<feature>/interceptors/; otherwise in src/app/interceptors/.")]
        public ToolResult<DevelopFile> CreateInterceptor(
            [Description("Name of the interceptor in PascalCase, e.g. 'AuthInterceptor'.")] string name,
            [Description("Optional feature name. If provided, interceptor goes in src/app/features/<feature>/interceptors/. If null, goes in src/app/interceptors/.")] string? inFeature = null)
        {
            return CreateNgArtifact("interceptors", "interceptor", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Resolver creation — ALWAYS in src/app/resolvers/ or src/app/features/<feature>/resolvers/
        // ------------------------------------------------------------------

        [KernelFunction("create_resolver")]
        [Description("Creates a new Angular route resolver using 'ng generate resolver'. If inFeature is provided, creates in src/app/features/<feature>/resolvers/; otherwise in src/app/resolvers/.")]
        public ToolResult<DevelopFile> CreateResolver(
            [Description("Name of the resolver in PascalCase, e.g. 'UserResolver'.")] string name,
            [Description("Optional feature name. If provided, resolver goes in src/app/features/<feature>/resolvers/. If null, goes in src/app/resolvers/.")] string? inFeature = null)
        {
            return CreateNgArtifact("resolvers", "resolver", name, inFeature);
        }

        // ------------------------------------------------------------------
        // Generic TypeScript file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_ts")]
        [Description("Creates a new TypeScript file (*.ts) in the src folder.")]
        public ToolResult<DevelopFile> CreateTs(
            [Description("Name of the file without extension, e.g. 'main' or 'polyfills'.")] string name)
        {
            return CreateFileInRoot("src", "ts", name);
        }

        // ------------------------------------------------------------------
        // Generic HTML file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_html")]
        [Description("Creates a new HTML file (*.html) in the src folder.")]
        public ToolResult<DevelopFile> CreateHtml(
            [Description("Name of the file without extension, e.g. 'index'.")] string name)
        {
            return CreateFileInRoot("src", "html", name);
        }

        // ------------------------------------------------------------------
        // Generic CSS file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_css")]
        [Description("Creates a new CSS file (*.css) in the src folder.")]
        public ToolResult<DevelopFile> CreateCss(
            [Description("Name of the file without extension, e.g. 'styles'.")] string name)
        {
            return CreateFileInRoot("src", "css", name);
        }

        // ------------------------------------------------------------------
        // Generic SCSS file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_scss")]
        [Description("Creates a new SCSS file (*.scss) in the src folder.")]
        public ToolResult<DevelopFile> CreateScss(
            [Description("Name of the file without extension, e.g. 'variables'.")] string name)
        {
            return CreateFileInRoot("src", "scss", name);
        }

        // ------------------------------------------------------------------
        // Generic LESS file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_less")]
        [Description("Creates a new LESS file (*.less) in the src folder.")]
        public ToolResult<DevelopFile> CreateLess(
            [Description("Name of the file without extension, e.g. 'theme'.")] string name)
        {
            return CreateFileInRoot("src", "less", name);
        }

        // ------------------------------------------------------------------
        // Generic JSON file creation (src/)
        // ------------------------------------------------------------------

        [KernelFunction("create_json")]
        [Description("Creates a new JSON file (*.json) in the src folder.")]
        public ToolResult<DevelopFile> CreateJson(
            [Description("Name of the file without extension, e.g. 'config'.")] string name)
        {
            return CreateFileInRoot("src", "json", name);
        }

        // ------------------------------------------------------------------
        // Stylesheet creation (src/styles/)
        // ------------------------------------------------------------------

        [KernelFunction("create_stylesheet")]
        [Description("Creates a new global stylesheet in src/styles/.")]
        public ToolResult<DevelopFile> CreateStylesheet(
            [Description("Name of the stylesheet, e.g. 'variables' or 'theme'.")] string name,
            [Description("Extension: css, scss, or less. Default is scss.")] string ext = "scss")
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"styles_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Stylesheet already exists");

            _context.LogInfo($"Creating stylesheet {name}.{ext} in src/styles");
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var styles = _plan.Plan.GetOrCreateFolder(src, "styles", "styles");
            var file = _plan.Plan.CreateFile(styles, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Environment file creation (src/environments/)
        // ------------------------------------------------------------------

        [KernelFunction("create_environment")]
        [Description("Creates a new environment TypeScript file in src/environments/.")]
        public ToolResult<DevelopFile> CreateEnvironment(
            [Description("Name of the environment file without extension, e.g. 'environment.prod'.")] string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"environments_ts_{name}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Environment file already exists");

            _context.LogInfo($"Creating environment {name}.ts in src/environments");
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var env = _plan.Plan.GetOrCreateFolder(src, "environments", "environments");
            var file = _plan.Plan.CreateFile(env, id, $"{name}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Asset file creation (src/assets/)
        // ------------------------------------------------------------------

        [KernelFunction("create_asset")]
        [Description("Creates a new asset file in src/assets/.")]
        public ToolResult<DevelopFile> CreateAsset(
            [Description("Name of the asset file with extension, e.g. 'data.json' or 'logo.svg'.")] string name)
        {
            var ext = Path.GetExtension(name).TrimStart('.');
            var baseName = Path.GetFileNameWithoutExtension(name);
            var id = $"assets_{ext}_{baseName}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Asset already exists");

            _context.LogInfo($"Creating asset {name} in src/assets");
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var assets = _plan.Plan.GetOrCreateFolder(src, "assets", "assets");
            var file = _plan.Plan.CreateFile(assets, id, name, null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // e2e file creation (e2e/)
        // ------------------------------------------------------------------

        [KernelFunction("create_e2e")]
        [Description("Creates a new end-to-end test file in the e2e/ folder.")]
        public ToolResult<DevelopFile> CreateE2E(
            [Description("Name of the file with extension, e.g. 'app.spec.ts' or 'login.test.js'.")] string name)
        {
            var ext = Path.GetExtension(name).TrimStart('.');
            var baseName = Path.GetFileNameWithoutExtension(name);
            var id = $"e2e_{ext}_{baseName}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "e2e file already exists");

            _context.LogInfo($"Creating e2e file {name} in e2e");
            var e2e = _plan.Plan.GetOrCreateFolder("e2e", "e2e");
            var file = _plan.Plan.CreateFile(e2e, id, name, null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Public file creation (public/)
        // ------------------------------------------------------------------

        [KernelFunction("create_public_file")]
        [Description("Creates a new file in the public/ folder (Angular v17+ static assets).")]
        public ToolResult<DevelopFile> CreatePublicFile(
            [Description("Name of the file with extension, e.g. 'robots.txt' or 'favicon.ico'.")] string name)
        {
            var ext = Path.GetExtension(name).TrimStart('.');
            var baseName = Path.GetFileNameWithoutExtension(name);
            var id = $"public_{ext}_{baseName}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Public file already exists");

            _context.LogInfo($"Creating public file {name} in public");
            var pub = _plan.Plan.GetOrCreateFolder("public", "public");
            var file = _plan.Plan.CreateFile(pub, id, name, null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Folder creation
        // ------------------------------------------------------------------

        [KernelFunction("create_folder_in_assets")]
        [Description("Creates a new folder within src/assets/.")]
        public ToolResult<DevelopFolder> CreateFolderInAssets(
            [Description("Name of the folder, e.g. 'icons' or 'images'.")] string name)
        {
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var assets = _plan.Plan.GetOrCreateFolder(src, "assets", "assets");
            var folder = _plan.Plan.GetOrCreateFolder(assets, $"assets_{name.ToLowerInvariant()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_styles")]
        [Description("Creates a new folder within src/styles/.")]
        public ToolResult<DevelopFolder> CreateFolderInStyles(
            [Description("Name of the folder, e.g. 'themes'.")] string name)
        {
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var styles = _plan.Plan.GetOrCreateFolder(src, "styles", "styles");
            var folder = _plan.Plan.GetOrCreateFolder(styles, $"styles_{name.ToLowerInvariant()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_e2e")]
        [Description("Creates a new folder within e2e/.")]
        public ToolResult<DevelopFolder> CreateFolderInE2E(
            [Description("Name of the folder, e.g. 'specs'.")] string name)
        {
            var e2e = _plan.Plan.GetOrCreateFolder("e2e", "e2e");
            var folder = _plan.Plan.GetOrCreateFolder(e2e, $"e2e_{name.ToLowerInvariant()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_public")]
        [Description("Creates a new folder within public/ (Angular v17+ static assets).")]
        public ToolResult<DevelopFolder> CreateFolderInPublic(
            [Description("Name of the folder, e.g. 'images'.")] string name)
        {
            var pub = _plan.Plan.GetOrCreateFolder("public", "public");
            var folder = _plan.Plan.GetOrCreateFolder(pub, $"public_{name.ToLowerInvariant()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        // ------------------------------------------------------------------
        // Build
        // ------------------------------------------------------------------

        [KernelFunction("build")]
        [Description("Runs 'ng build' in the project root directory using the Angular CLI. Returns the build output and exit code so the AI can verify if the code compiles correctly.")]
        public ToolResult<ConsoleOutput> Build(
            [Description("Optional build configuration, e.g. 'production' or 'development'. Defaults to 'production'.")] string configuration = "production")
        {
            var rootDir = _plan.Plan.RootDirectory;
            _context.LogInfo($"Running ng build --configuration={configuration} in {rootDir}");
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
            var psi = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "ng",
                Arguments = isWindows ? $"/c ng build --configuration={configuration}" : "build --configuration={configuration}",
                WorkingDirectory = rootDir,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            var output = new ConsoleOutput()
            {
                Command = $"ng build --configuration={configuration}"
            };
            using var process = Process.Start(psi);
            if (process == null)
                return new ToolResult<ConsoleOutput>(false, "Failed to start ng build process. Ensure Angular CLI is installed and available in PATH.");

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    output.WriteLine("info", e.Data);
                    _context.LogInfo($"\t{e.Data}");
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    output.WriteLine("error", e.Data);
                    _context.LogError($"\t{e.Data}");
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.WaitForExit(); // flush buffers

            if (process.ExitCode != 0)
                return new ToolResult<ConsoleOutput>(output, false, $"ng build exit code {process.ExitCode}");

            return new ToolResult<ConsoleOutput>(output, true, $"ng build exit code {process.ExitCode}");
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        /// <summary>
        /// Gets or creates the src/app folder.
        /// </summary>
        private DevelopFolder GetAppFolder()
        {
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            return _plan.Plan.GetOrCreateFolder(src, "app", "app");
        }

        /// <summary>
        /// Gets or creates a typed folder (e.g., components, services) under src/app/ or src/app/features/&lt;feature>/.
        /// </summary>
        private DevelopFolder GetTypedFolder(string type, string? inFeature)
        {
            var app = GetAppFolder();

            if (string.IsNullOrWhiteSpace(inFeature))
            {
                // Global: src/app/{type}/
                return _plan.Plan.GetOrCreateFolder(app, type, type);
            }

            // Feature: src/app/features/{feature}/{type}/
            var featureName = inFeature.Trim().ToLowerInvariant();
            var features = _plan.Plan.GetOrCreateFolder(app, "features", "features");
            var feature = _plan.Plan.GetOrCreateFolder(features, $"features_{featureName}", featureName);
            return _plan.Plan.GetOrCreateFolder(feature, $"features_{featureName}_{type}", type);
        }

        /// <summary>
        /// Creates an Angular artifact using 'ng generate <schematic>' in the appropriate folder.
        /// After the CLI creates files on disk, indexes them into the DevelopPlan.
        /// </summary>
        private ToolResult<DevelopFile> CreateNgArtifact(string folderType, string schematic, string name, string? inFeature)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var kebab = ToKebabCase(name);

            var folder = GetTypedFolder(folderType, inFeature);
            var rootDir = _plan.Plan.RootDirectory;
            var workingDir = Path.Combine(rootDir, folder.RelativePath);

            // Ensure the directory exists on disk
            Directory.CreateDirectory(workingDir);

            _context.LogInfo($"Running 'ng generate {schematic} {kebab}' in {workingDir}");

            var result = RunNgCommand($"generate {schematic} {kebab}", workingDir);
            if (!result.Success)
                return new ToolResult<DevelopFile>(false, $"ng generate {schematic} failed: {result.Message}");

            // ng generate creates files directly in the workingDir (not a subfolder like component)
            // Index all .ts files created in the folder
            var tsFiles = new List<string>();
            if (Directory.Exists(workingDir))
            {
                foreach (var filePath in Directory.GetFiles(workingDir, $"{kebab}*.ts"))
                {
                    var fileName = Path.GetFileName(filePath);
                    var ext = Path.GetExtension(fileName).TrimStart('.');
                    var baseName = Path.GetFileNameWithoutExtension(fileName);
                    var id = $"{folder.Id}_{baseName}_{ext}_{baseName}";
                    var file = _plan.Plan.CreateFile(folder, id, fileName, null);
                    _plan.InvokeFileCreated(file);
                    tsFiles.Add(fileName);
                }
            }

            // Return the first .ts file found
            if (tsFiles.Count > 0)
            {
                var firstFile = tsFiles[0];
                var firstId = $"{folder.Id}_{Path.GetFileNameWithoutExtension(firstFile)}_ts_{Path.GetFileNameWithoutExtension(firstFile)}";
                if (_plan.Plan.TryFindFile(firstId, out var tsFile))
                    return new ToolResult<DevelopFile>(tsFile!);
            }

            return new ToolResult<DevelopFile>(false, $"{schematic} created but no .ts files were indexed.");
        }

        /// <summary>
        /// Creates a single-file artifact (model) in a folder.
        /// Automatically appends the type suffix if missing.
        /// </summary>
        private ToolResult<DevelopFile> CreateArtifactInFolder(DevelopFolder folder, string name, string? typeSuffix = null, string? fileSuffix = null)
        {
            name = Path.GetFileNameWithoutExtension(name);

            if (!string.IsNullOrEmpty(typeSuffix) && !name.EndsWith(typeSuffix))
                name += typeSuffix;

            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var suffix = !string.IsNullOrEmpty(fileSuffix) ? $".{fileSuffix}" : "";
            var id = $"{prefix}ts_{kebab}{suffix}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, $"{typeSuffix ?? "File"} already exists");

            _context.LogInfo($"Creating {typeSuffix?.ToLowerInvariant() ?? "file"} {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}{suffix}.ts", null);
            _plan.InvokeFileCreated(file);
            return new ToolResult<DevelopFile>(file);
        }

        private ToolResult<DevelopFile> CreateFileInRoot(string rootId, string ext, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"{rootId}_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists");

            _context.LogInfo($"Creating file {rootId}/{name}.{ext}");
            var target = _plan.Plan.GetOrCreateFolder(rootId, rootId);
            var nf = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
            _plan.InvokeFileCreated(nf);
            return new ToolResult<DevelopFile>(nf);
        }

        private static string ToKebabCase(string pascalCase)
        {
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < pascalCase.Length; i++)
            {
                var c = pascalCase[i];
                if (char.IsUpper(c) && i > 0)
                    result.Append('-');
                result.Append(char.ToLower(c));
            }
            return result.ToString();
        }

        /// <summary>
        /// Runs an ng CLI command in the specified working directory.
        /// </summary>
        private ToolResult<string[]> RunNgCommand(string arguments, string workingDirectory)
        {
            bool isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

            var psi = new ProcessStartInfo
            {
                FileName = isWindows ? "cmd.exe" : "ng",
                Arguments = isWindows ? $"/c ng {arguments}" : arguments,
                WorkingDirectory = workingDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process == null)
                return new ToolResult<string[]>(false, "Failed to start ng process. Ensure Angular CLI is installed and available in PATH.");

            var logs = new List<string>();
            var errors = new List<string>();
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    logs.Add(e.Data);
                    _context.LogInfo($"\t{e.Data}");
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    errors.Add(e.Data);
                    _context.LogError($"\t{e.Data}");
                }
            };

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            process.WaitForExit(); // flush buffers

            if (process.ExitCode != 0)
                return new ToolResult<string[]>(errors.Concat(logs).ToArray(), false, $"ng exit code {process.ExitCode}");

            return new ToolResult<string[]>(logs.ToArray(), true, $"ng exit code {process.ExitCode}");
        }
    }
}
