using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.PlatformIO.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.PlatformIO.Services
{
    internal class Platform
    {
        private readonly IChatMessageService _context;
        private readonly IEnvironmentController _env;
        private readonly IDevelopPlanService _plan;

        private static readonly HashSet<string> ValidSubDirectories = new(StringComparer.OrdinalIgnoreCase)
        {
            "definitions", "actions", "internals"
        };

        public Platform(IChatMessageService context, IEnvironmentController env, IDevelopPlanService plan)
        {
            _context = context;
            _env = env;
            _plan = plan;
        }

        [KernelFunction("get_environment_info")]
        [Description("Gets information about the current PlatformIO environment you are working on.")]
        public Dictionary<string, string?> GetPlatformIOEnvironment()
        {
            if (_env.Current == null)
                return new Dictionary<string, string?>() { { "ERROR", "No active environments" } };

            var model = new Dictionary<string, string?>();
            model["environment"] = _env.Current.Name;
            foreach (var item in _env.Current.Properties)
                model[item.Key] = item.Value;
            return model;
        }

        [KernelFunction("create_feature")]
        [Description("Creates a new feature folder in both include/ and src/ with the structured sub-folders. In include/{feature}/: definitions/, actions/, internals/. In src/{feature}/: actions/, internals/. Returns the include feature folder ID.")]
        public ToolResult<DevelopFolder> CreateFeature(
            [Description("Name of the feature, e.g. 'motor_control' or 'wifi_manager'.")] string name)
        {
            name = name.ToLowerInvariant().Replace(" ", "_");

            // Create in include/
            var include = _plan.Plan.GetOrCreateFolder("include", "include");
            var includeFeature = _plan.Plan.GetOrCreateFolder(include, $"include_{name}", name);
            _plan.Plan.GetOrCreateFolder(includeFeature, $"include_{name}_definitions", "definitions");
            _plan.Plan.GetOrCreateFolder(includeFeature, $"include_{name}_actions", "actions");
            _plan.Plan.GetOrCreateFolder(includeFeature, $"include_{name}_internals", "internals");

            // Create in src/
            var src = _plan.Plan.GetOrCreateFolder("src", "src");
            var srcFeature = _plan.Plan.GetOrCreateFolder(src, $"src_{name}", name);
            _plan.Plan.GetOrCreateFolder(srcFeature, $"src_{name}_actions", "actions");
            _plan.Plan.GetOrCreateFolder(srcFeature, $"src_{name}_internals", "internals");

            _context.LogInfo($"Created feature '{name}' in include/ and src/ with structured sub-folders");
            return new ToolResult<DevelopFolder>(includeFeature);
        }

        [KernelFunction("create_h")]
        [Description("Creates a new header file (*.h). If featureFolderId is null, creates in the include/ folder root. If featureFolderId is provided (must start with 'include_'), creates inside that feature folder. If subDirectory is also provided, places it in that subfolder — valid values: definitions, actions, internals.")]
        public ToolResult<DevelopFile> CreateH(
            [Description("Name of the file without extension, e.g. 'my_header'.")] string name,
            [Description("Optional: ID of the feature folder in include/ (e.g. 'include_motor_control'). Pass null to create in include/ root.")] string? featureFolderId = null,
            [Description("Optional: Sub-folder within the feature. Valid values: definitions, actions, internals. Pass null to create directly in the feature folder.")] string? subDirectory = null)
        {
            return CreateSourceFile("include", "h", name, featureFolderId, subDirectory);
        }

        [KernelFunction("create_c")]
        [Description("Creates a new C file (*.c). If featureFolderId is null, creates in the src/ folder root. If featureFolderId is provided (must start with 'src_'), creates inside that feature folder. If subDirectory is also provided, places it in that subfolder — valid values: actions, internals.")]
        public ToolResult<DevelopFile> CreateC(
            [Description("Name of the file without extension, e.g. 'my_module'.")] string name,
            [Description("Optional: ID of the feature folder in src/ (e.g. 'src_motor_control'). Pass null to create in src/ root.")] string? featureFolderId = null,
            [Description("Optional: Sub-folder within the feature. Valid values: actions, internals. Pass null to create directly in the feature folder.")] string? subDirectory = null)
        {
            return CreateSourceFile("src", "c", name, featureFolderId, subDirectory);
        }

        [KernelFunction("create_cpp")]
        [Description("Creates a new C++ file (*.cpp). If featureFolderId is null, creates in the src/ folder root. If featureFolderId is provided (must start with 'src_'), creates inside that feature folder. If subDirectory is also provided, places it in that subfolder — valid values: actions, internals.")]
        public ToolResult<DevelopFile> CreateCpp(
            [Description("Name of the file without extension, e.g. 'my_module'.")] string name,
            [Description("Optional: ID of the feature folder in src/ (e.g. 'src_motor_control'). Pass null to create in src/ root.")] string? featureFolderId = null,
            [Description("Optional: Sub-folder within the feature. Valid values: actions, internals. Pass null to create directly in the feature folder.")] string? subDirectory = null)
        {
            return CreateSourceFile("src", "cpp", name, featureFolderId, subDirectory);
        }

        // ===== Private helpers =====

        private ToolResult<DevelopFile> CreateSourceFile(string parentDir, string ext, string name, string? featureFolderId, string? subDirectory)
        {
            name = Path.GetFileNameWithoutExtension(name);

            // Validate subDirectory if provided
            if (subDirectory is not null && !ValidSubDirectories.Contains(subDirectory))
            {
                return new ToolResult<DevelopFile>(false,
                    $"Invalid subDirectory '{subDirectory}'. Valid values: definitions, actions, internals.");
            }

            // src/ features do not have a definitions sub-folder
            if (parentDir == "src" && string.Equals(subDirectory, "definitions", StringComparison.OrdinalIgnoreCase))
            {
                return new ToolResult<DevelopFile>(false,
                    "The 'definitions' sub-directory is only valid in include/. src/ features have 'actions' and 'internals' only.");
            }

            // Root-level creation
            if (featureFolderId is null)
            {
                var id = $"{parentDir}_{ext}_{name}";
                if (_plan.Plan.TryFindFile(id, out var existingFile))
                    return new ToolResult<DevelopFile>(existingFile!, true, "File already exists");

                _context.LogInfo($"Creating file {parentDir}/{name}.{ext}");
                var target = _plan.Plan.GetOrCreateFolder(parentDir, parentDir);
                var nf = _plan.Plan.CreateFile(target, id, $"{name}.{ext}", null);
                _plan.InvokeFileCreated(nf);
                return new ToolResult<DevelopFile>(nf);
            }

            // Feature-level creation
            var expectedPrefix = parentDir == "include" ? "include_" : "src_";
            if (!featureFolderId.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return new ToolResult<DevelopFile>(false,
                    $"Feature folder ID '{featureFolderId}' does not belong to '{parentDir}/'. Expected prefix: '{expectedPrefix}'.");
            }

            var parent = _plan.Plan.GetOrCreateFolder(parentDir, parentDir);
            if (!parent.TryFindFolder(featureFolderId, out var featureFolder, false) || featureFolder is null)
                return new ToolResult<DevelopFile>(false, $"Feature folder '{featureFolderId}' not found in '{parentDir}'");

            DevelopFolder targetFolder;
            string fileId;

            if (subDirectory is null)
            {
                targetFolder = featureFolder;
                fileId = $"{featureFolderId}_{ext}_{name}";
            }
            else
            {
                var subFolderId = $"{featureFolderId}_{subDirectory}";
                if (!featureFolder.TryFindFolder(subFolderId, out var subFolder, false) || subFolder is null)
                    return new ToolResult<DevelopFile>(false, $"Sub-folder '{subDirectory}' not found in feature '{featureFolderId}'");

                targetFolder = subFolder;
                fileId = $"{featureFolderId}_{subDirectory}_{ext}_{name}";
            }

            if (_plan.Plan.TryFindFile(fileId, out var file))
                return new ToolResult<DevelopFile>(file!);

            var path = subDirectory is null
                ? $"{parentDir}/{featureFolder.Name}/{name}.{ext}"
                : $"{parentDir}/{featureFolder.Name}/{subDirectory}/{name}.{ext}";

            _context.LogInfo($"Creating file {path}");
            var newFile = _plan.Plan.CreateFile(targetFolder, fileId, $"{name}.{ext}", null);
            _plan.InvokeFileCreated(newFile);
            return new ToolResult<DevelopFile>(newFile);
        }
    }
}
