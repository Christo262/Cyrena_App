using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.ArduinoIDE.Plugins
{
    internal class Arduino
    {
        private readonly IDeveloperContext _context;
        public Arduino(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction("create_cpp")]
        [Description(@"Creates a new C++ source file (.cpp) in the project.

Use this when the project needs a new implementation file.

Input should be a base name only (for example: ""display"").
Do not include file extensions or paths.

If the file already exists, the existing file is returned and no new file is created.")]
        public ToolResult<ProjectFile> CreateCppFile(
            [Description("The name of the file, for example, 'display'.")] string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"cpp_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating CPP file {name}");
            var nf = _context.ProjectPlan.CreateFile(id, $"{name}.cpp", null);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }

        [KernelFunction("create_h")]
        [Description(@"Creates a new C++ header file (.h) in the project.

Use this when the project needs a new header or interface file.

Input should be a base name only (for example: ""display"").
Do not include file extensions or paths.

If the file already exists, the existing file is returned and no new file is created.")]
        public ToolResult<ProjectFile> CreateCppHeader(
            [Description("The name of the file, for example, 'display'.")] string name)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"h_{name}";
            if (_context.ProjectPlan.TryFindFile(id, out var file))
                return new ToolResult<ProjectFile>(file!, true, "File already exists.");
            _context.LogInfo($"Creating header file {name}");
            var nf = _context.ProjectPlan.CreateFile(id, $"{name}.h", null);
            ProjectPlan.Save(_context.ProjectPlan);
            return new ToolResult<ProjectFile>(nf);
        }
    }
}
