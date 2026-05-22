using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Coding.Services
{
    internal class BaseFileKernelFunctions
    {
        private readonly IDevelopPlanService _plan;
        private readonly IChatMessageService _context;
        private readonly IVersionControl _version;

        public BaseFileKernelFunctions(
            IDevelopPlanService plan,
            IChatMessageService context,
            IVersionControl version)
        {
            _plan = plan;
            _context = context;
            _version = version;
        }

        [KernelFunction("read")]
        [Description("Returns the full text content of the specified file.")]
        public ToolResult<DevelopFileContent> ReadFileContent(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileContent>(false, $"File with id {fileId} not found.");

                _context.LogInfo($"Reading file {file!.RelativePath}");

                if (!_plan.Plan.TryReadFileContent(file, out var fileContent))
                    return new ToolResult<DevelopFileContent>(false, $"Unable to read file with id {fileId}.");

                return new ToolResult<DevelopFileContent>(fileContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("read_lines")]
        [Description(
            "Returns a structured list of the file's lines, each paired with its zero-based index. " +
            "Use this before calling write when you need to know exact line numbers.")]
        public ToolResult<DevelopFileLines> ReadFileLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                _context.LogInfo($"Reading file lines {file!.RelativePath}");

                if (!_plan.Plan.TryReadFileLines(file, out var fileLines))
                    return new ToolResult<DevelopFileLines>(false, $"Unable to read file with id {fileId}.");

                return new ToolResult<DevelopFileLines>(fileLines!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("write")]
        [Description(
    "Modifies the specified file. All line endings are normalized to \\n. " +
    "mode controls the operation: " +
    "Insert — inserts content at startLine, existing lines are shifted down, lineCount is ignored. " +
    "Replace — removes exactly lineCount lines starting at startLine, then inserts content at that position. lineCount must be > 0. " +
    "Overwrite — replaces the entire file with content, startLine and lineCount are ignored. " +
    "To append to end of file, use Insert with startLine equal to the current total line count. " +
    "Use read_lines first when editing by line number.")]
        public ToolResult<DevelopFileLines> WriteFile(
    [Description("The unique identifier of the target file within the current develop plan.")]
    string fileId,

    [Description("Insert: inserts content at startLine. Replace: removes lineCount lines at startLine then inserts content. Overwrite: replaces entire file.")]
    CodeWriteMode mode,

    [Description("The text content to insert or use as replacement. Null or empty to delete lines with Replace mode.")]
    string? content,

    [Description("The zero-based line number where the operation begins. Ignored when mode is Overwrite.")]
    int startLine = 0,

    [Description("The number of lines to remove before inserting. Only used when mode is Replace. Must be > 0.")]
    int lineCount = 0)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

                switch (mode)
                {
                    case CodeWriteMode.Overwrite:
                        _context.LogInfo($"Overwriting entire file {file.RelativePath}");
                        break;
                    case CodeWriteMode.Insert:
                        _context.LogInfo($"Inserting content at line {startLine} in {file.RelativePath}");
                        break;
                    case CodeWriteMode.Replace:
                        _context.LogInfo($"Replacing {lineCount} line(s) starting at line {startLine} in {file.RelativePath}");
                        break;
                }

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                if (!_plan.Plan.TryWriteFileLines(file, content, startLine, lineCount, mode, out var updated))
                    return new ToolResult<DevelopFileLines>(
                        false,
                        $"Unable to write to file '{file.RelativePath}'. Ensure startLine and lineCount are valid for the chosen mode.");

                _plan.InvokeFileUpdated(updated!);

                return new ToolResult<DevelopFileLines>(updated!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("delete")]
        [Description("Removes the specified file from the project.")]
        public ToolResult DeleteFile(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(true, "File does not exist.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                _context.LogInfo($"Deleting file {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                _plan.Plan.RemoveFile(file);
                _plan.InvokeFileDeleted(file);

                return new ToolResult(true, "File deleted.");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, $"Error: {ex.Message}");
            }
        }
    }
}