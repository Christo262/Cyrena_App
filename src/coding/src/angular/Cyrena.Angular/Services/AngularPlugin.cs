using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Angular.Services
{
    internal class Angular
    {
        private readonly IChatMessageService _context;
        private readonly IDevelopPlanService _plan;
        public Angular(IChatMessageService context, IDevelopPlanService plan)
        {
            _context = context;
            _plan = plan;
        }

        // ------------------------------------------------------------------
        // Project structure
        // ------------------------------------------------------------------

        [KernelFunction("get_project_structure")]
        [Description("Gets the Angular project structure. Lists all folders and files in the DevelopPlan, including src/app subdirectories, src/styles, src/assets, src/environments, e2e, and public folders.")]
        public Dictionary<string, object> GetProjectStructure()
        {
            var result = new Dictionary<string, object>();

            foreach (var folder in _plan.Plan.Folders)
                result[folder.Id] = ListFolderContents(folder);

            return result;
        }

        private static Dictionary<string, object> ListFolderContents(DevelopFolder folder)
        {
            var dict = new Dictionary<string, object>
            {
                ["name"] = folder.Name,
                ["relativePath"] = folder.RelativePath,
                ["files"] = folder.Files.Select(f => new { f.Id, f.Name, f.RelativePath, f.ReadOnly }).ToList(),
                ["subfolders"] = folder.Folders.Select(f => f.Id).ToList()
            };
            return dict;
        }

        // ------------------------------------------------------------------
        // Component creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_component")]
        [Description("Creates a new Angular standalone component with .ts, .html, .css, and .spec.ts files in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateComponent(
            [Description("Name of the component in PascalCase, e.g. 'UserProfile'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'features/users'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateComponentInFolder(folder, name);
        }

        [KernelFunction("create_component_in_folder")]
        [Description("Creates a new Angular standalone component with .ts, .html, .css, and .spec.ts files in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateComponentInFolder(
            [Description("Id of the folder where the component will be created.")] string folderId,
            [Description("Name of the component in PascalCase, e.g. 'UserProfile'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateComponentInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateComponentInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";

            var tsId = $"{prefix}ts_{kebab}.component";
            if (_plan.Plan.TryFindFile(tsId, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Component already exists");

            _context.LogInfo($"Creating component {name} in {folder.RelativePath}");

            var tsFile = _plan.Plan.CreateFile(folder, tsId, $"{kebab}.component.ts", null);
            _plan.Plan.CreateFile(folder, $"{prefix}html_{kebab}.component", $"{kebab}.component.html", null);
            _plan.Plan.CreateFile(folder, $"{prefix}css_{kebab}.component", $"{kebab}.component.css", null);
            _plan.Plan.CreateFile(folder, $"{prefix}ts_{kebab}.component.spec", $"{kebab}.component.spec.ts", null);

            return new ToolResult<DevelopFile>(tsFile);
        }

        // ------------------------------------------------------------------
        // Service creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_service")]
        [Description("Creates a new Angular injectable service in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateService(
            [Description("Name of the service in PascalCase, e.g. 'UserService'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'services'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateServiceInFolder(folder, name);
        }

        [KernelFunction("create_service_in_folder")]
        [Description("Creates a new Angular injectable service in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateServiceInFolder(
            [Description("Id of the folder where the service will be created.")] string folderId,
            [Description("Name of the service in PascalCase, e.g. 'UserService'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateServiceInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateServiceInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Service"))
                name += "Service";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Service already exists");

            _context.LogInfo($"Creating service {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Guard creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_guard")]
        [Description("Creates a new Angular route guard in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateGuard(
            [Description("Name of the guard in PascalCase, e.g. 'AuthGuard'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'guards'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateGuardInFolder(folder, name);
        }

        [KernelFunction("create_guard_in_folder")]
        [Description("Creates a new Angular route guard in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateGuardInFolder(
            [Description("Id of the folder where the guard will be created.")] string folderId,
            [Description("Name of the guard in PascalCase, e.g. 'AuthGuard'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateGuardInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateGuardInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Guard"))
                name += "Guard";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Guard already exists");

            _context.LogInfo($"Creating guard {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Pipe creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_pipe")]
        [Description("Creates a new Angular pipe in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreatePipe(
            [Description("Name of the pipe in PascalCase, e.g. 'CurrencyPipe'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'pipes'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreatePipeInFolder(folder, name);
        }

        [KernelFunction("create_pipe_in_folder")]
        [Description("Creates a new Angular pipe in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreatePipeInFolder(
            [Description("Id of the folder where the pipe will be created.")] string folderId,
            [Description("Name of the pipe in PascalCase, e.g. 'CurrencyPipe'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreatePipeInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreatePipeInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Pipe"))
                name += "Pipe";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Pipe already exists");

            _context.LogInfo($"Creating pipe {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Directive creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_directive")]
        [Description("Creates a new Angular directive in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateDirective(
            [Description("Name of the directive in PascalCase, e.g. 'HighlightDirective'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'directives'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateDirectiveInFolder(folder, name);
        }

        [KernelFunction("create_directive_in_folder")]
        [Description("Creates a new Angular directive in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateDirectiveInFolder(
            [Description("Id of the folder where the directive will be created.")] string folderId,
            [Description("Name of the directive in PascalCase, e.g. 'HighlightDirective'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateDirectiveInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateDirectiveInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Directive"))
                name += "Directive";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Directive already exists");

            _context.LogInfo($"Creating directive {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Model creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_model")]
        [Description("Creates a new TypeScript model/interface file in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateModel(
            [Description("Name of the model in PascalCase, e.g. 'User'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'models'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateModelInFolder(folder, name);
        }

        [KernelFunction("create_model_in_folder")]
        [Description("Creates a new TypeScript model/interface file in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateModelInFolder(
            [Description("Id of the folder where the model will be created.")] string folderId,
            [Description("Name of the model in PascalCase, e.g. 'User'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateModelInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateModelInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}.model";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Model already exists");

            _context.LogInfo($"Creating model {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.model.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Interceptor creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_interceptor")]
        [Description("Creates a new Angular HTTP interceptor in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateInterceptor(
            [Description("Name of the interceptor in PascalCase, e.g. 'AuthInterceptor'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'core/interceptors'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateInterceptorInFolder(folder, name);
        }

        [KernelFunction("create_interceptor_in_folder")]
        [Description("Creates a new Angular HTTP interceptor in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateInterceptorInFolder(
            [Description("Id of the folder where the interceptor will be created.")] string folderId,
            [Description("Name of the interceptor in PascalCase, e.g. 'AuthInterceptor'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateInterceptorInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateInterceptorInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Interceptor"))
                name += "Interceptor";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Interceptor already exists");

            _context.LogInfo($"Creating interceptor {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Resolver creation (src/app/ or subpath)
        // ------------------------------------------------------------------

        [KernelFunction("create_resolver")]
        [Description("Creates a new Angular route resolver in src/app or a subfolder.")]
        public ToolResult<DevelopFile> CreateResolver(
            [Description("Name of the resolver in PascalCase, e.g. 'UserResolver'.")] string name,
            [Description("Optional subfolder path within src/app, e.g. 'resolvers'. Use forward slashes.")] string? path = null)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return CreateResolverInFolder(folder, name);
        }

        [KernelFunction("create_resolver_in_folder")]
        [Description("Creates a new Angular route resolver in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateResolverInFolder(
            [Description("Id of the folder where the resolver will be created.")] string folderId,
            [Description("Name of the resolver in PascalCase, e.g. 'UserResolver'.")] string name)
        {
            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");
            return CreateResolverInFolder(folder!, name);
        }

        private ToolResult<DevelopFile> CreateResolverInFolder(DevelopFolder folder, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            if (!name.EndsWith("Resolver"))
                name += "Resolver";
            var kebab = ToKebabCase(name);
            var prefix = folder.Id == "app" ? "" : $"{folder.Id}_";
            var id = $"{prefix}ts_{kebab}";

            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Resolver already exists");

            _context.LogInfo($"Creating resolver {name} in {folder.RelativePath}");
            var file = _plan.Plan.CreateFile(folder, id, $"{kebab}.ts", null);
            return new ToolResult<DevelopFile>(file);
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

        [KernelFunction("create_ts_in_folder")]
        [Description("Creates a new TypeScript file (*.ts) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateTsInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'utils'.")] string name)
        {
            return CreateFileInFolder(folderId, "ts", name);
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

        [KernelFunction("create_html_in_folder")]
        [Description("Creates a new HTML file (*.html) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateHtmlInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'template'.")] string name)
        {
            return CreateFileInFolder(folderId, "html", name);
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

        [KernelFunction("create_css_in_folder")]
        [Description("Creates a new CSS file (*.css) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateCssInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'styles'.")] string name)
        {
            return CreateFileInFolder(folderId, "css", name);
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

        [KernelFunction("create_scss_in_folder")]
        [Description("Creates a new SCSS file (*.scss) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateScssInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'variables'.")] string name)
        {
            return CreateFileInFolder(folderId, "scss", name);
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

        [KernelFunction("create_less_in_folder")]
        [Description("Creates a new LESS file (*.less) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateLessInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'theme'.")] string name)
        {
            return CreateFileInFolder(folderId, "less", name);
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

        [KernelFunction("create_json_in_folder")]
        [Description("Creates a new JSON file (*.json) in a specific folder by its folderId.")]
        public ToolResult<DevelopFile> CreateJsonInFolder(
            [Description("Id of the folder where the file will be created.")] string folderId,
            [Description("Name of the file without extension, e.g. 'config'.")] string name)
        {
            return CreateFileInFolder(folderId, "json", name);
        }

        // ------------------------------------------------------------------
        // Stylesheet creation (src/styles/)
        // ------------------------------------------------------------------

        [KernelFunction("create_stylesheet")]
        [Description("Creates a new global stylesheet in src/styles or a subfolder.")]
        public ToolResult<DevelopFile> CreateStylesheet(
            [Description("Name of the stylesheet, e.g. 'variables' or 'theme'.")] string name,
            [Description("Extension: css, scss, or less. Default is scss.")] string ext = "scss")
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"styles_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Stylesheet already exists");

            _context.LogInfo($"Creating stylesheet {name}.{ext} in src/styles");
            var styles = _plan.Plan.GetOrCreateFolder("styles", "styles");
            var file = _plan.Plan.CreateFile(styles, id, $"{name}.{ext}", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Environment file creation (src/environments/)
        // ------------------------------------------------------------------

        [KernelFunction("create_environment")]
        [Description("Creates a new environment TypeScript file in src/environments.")]
        public ToolResult<DevelopFile> CreateEnvironment(
            [Description("Name of the environment file without extension, e.g. 'environment.prod'.")] string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"environments_ts_{name}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Environment file already exists");

            _context.LogInfo($"Creating environment {name}.ts in src/environments");
            var env = _plan.Plan.GetOrCreateFolder("environments", "environments");
            var file = _plan.Plan.CreateFile(env, id, $"{name}.ts", null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // Asset file creation (src/assets/)
        // ------------------------------------------------------------------

        [KernelFunction("create_asset")]
        [Description("Creates a new asset file in src/assets or a subfolder.")]
        public ToolResult<DevelopFile> CreateAsset(
            [Description("Name of the asset file with extension, e.g. 'data.json' or 'logo.svg'.")] string name)
        {
            var ext = Path.GetExtension(name).TrimStart('.');
            var baseName = Path.GetFileNameWithoutExtension(name);
            var id = $"assets_{ext}_{baseName}";
            if (_plan.Plan.TryFindFile(id, out var existing))
                return new ToolResult<DevelopFile>(existing!, true, "Asset already exists");

            _context.LogInfo($"Creating asset {name} in src/assets");
            var assets = _plan.Plan.GetOrCreateFolder("assets", "assets");
            var file = _plan.Plan.CreateFile(assets, id, name, null);
            return new ToolResult<DevelopFile>(file);
        }

        // ------------------------------------------------------------------
        // e2e file creation (e2e/)
        // ------------------------------------------------------------------

        [KernelFunction("create_e2e")]
        [Description("Creates a new end-to-end test file in the e2e folder or a subfolder.")]
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
        [Description("Creates a new file in the public folder (Angular v17+ static assets) or a subfolder.")]
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

        [KernelFunction("create_folder_in_app")]
        [Description("Creates a new folder within src/app.")]
        public ToolResult<DevelopFolder> CreateFolderInApp(
            [Description("Path of the folder relative to src/app, e.g. 'features/users'. Use forward slashes.")] string path)
        {
            var folder = GetOrCreateAppSubfolder(path);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_styles")]
        [Description("Creates a new folder within src/styles.")]
        public ToolResult<DevelopFolder> CreateFolderInStyles(
            [Description("Name of the folder, e.g. 'themes'.")] string name)
        {
            var styles = _plan.Plan.GetOrCreateFolder("styles", "styles");
            var folder = _plan.Plan.GetOrCreateFolder(styles, $"styles_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_assets")]
        [Description("Creates a new folder within src/assets.")]
        public ToolResult<DevelopFolder> CreateFolderInAssets(
            [Description("Name of the folder, e.g. 'icons' or 'images'.")] string name)
        {
            var assets = _plan.Plan.GetOrCreateFolder("assets", "assets");
            var folder = _plan.Plan.GetOrCreateFolder(assets, $"assets_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_e2e")]
        [Description("Creates a new folder within e2e.")]
        public ToolResult<DevelopFolder> CreateFolderInE2E(
            [Description("Name of the folder, e.g. 'specs'.")] string name)
        {
            var e2e = _plan.Plan.GetOrCreateFolder("e2e", "e2e");
            var folder = _plan.Plan.GetOrCreateFolder(e2e, $"e2e_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        [KernelFunction("create_folder_in_public")]
        [Description("Creates a new folder within public (Angular v17+ static assets).")]
        public ToolResult<DevelopFolder> CreateFolderInPublic(
            [Description("Name of the folder, e.g. 'images'.")] string name)
        {
            var pub = _plan.Plan.GetOrCreateFolder("public", "public");
            var folder = _plan.Plan.GetOrCreateFolder(pub, $"public_{name.ToLower()}", name);
            return new ToolResult<DevelopFolder>(folder);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private DevelopFolder GetOrCreateAppSubfolder(string? path)
        {
            var app = _plan.Plan.GetOrCreateFolder("app", "app");
            if (string.IsNullOrEmpty(path))
                return app;

            var parts = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            var current = app;
            var currentId = "app";

            foreach (var part in parts)
            {
                currentId = $"{currentId}_{part.ToLower()}";
                current = _plan.Plan.GetOrCreateFolder(current, currentId, part);
            }

            return current;
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
            return new ToolResult<DevelopFile>(nf);
        }

        private ToolResult<DevelopFile> CreateFileInFolder(string folderId, string ext, string name)
        {
            name = Path.GetFileNameWithoutExtension(name);

            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult<DevelopFile>(false, $"Folder '{folderId}' not found in the project plan.");

            var id = $"{folderId}_{ext}_{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists");

            _context.LogInfo($"Creating file {folder!.RelativePath}/{name}.{ext}");
            var nf = _plan.Plan.CreateFile(folder, id, $"{name}.{ext}", null);
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
    }
}
