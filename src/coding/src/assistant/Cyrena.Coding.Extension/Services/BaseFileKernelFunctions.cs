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
     "Modifies the specified file using a single splice operation. " +
     "All line endings are normalized to \\n. " +
     "By default, this inserts content at startLine without removing existing lines. " +
     "If replaceAll is true, the entire file is replaced with content and startLine/lineCount are ignored. " +
     "If replaceAll is false and lineCount is 0, content is inserted at startLine. " +
     "If replaceAll is false and lineCount is greater than 0, that many existing lines are removed starting at startLine, then content is inserted at the same position. " +
     "To delete lines, pass empty content with replaceAll false and lineCount greater than 0. " +
     "To append, use startLine equal to the current line count and lineCount 0. " +
     "Use read_lines first when editing by line number.")]
        public ToolResult<DevelopFileLines> WriteFile(
     [Description("The unique identifier of the target file within the current develop plan.")]
    string fileId,

     [Description("The text content to write, insert, or use as replacement content. Null is treated as empty content.")]
    string? content,

     [Description("The zero-based line number where the insert or replacement should begin. Required for line-based edits.")]
    int startLine,

     [Description("The number of existing lines to remove before inserting content. Use 0 to insert without removing anything.")]
    int lineCount = 0,

     [Description("Set true only when replacing the entire file. When true, startLine and lineCount are ignored.")]
    bool replaceAll = false)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

                if (replaceAll)
                    _context.LogInfo($"Writing entire file {file.RelativePath}");
                else if (lineCount == 0)
                    _context.LogInfo($"Inserting content at line {startLine} in {file.RelativePath}");
                else
                    _context.LogInfo($"Replacing {lineCount} line(s) starting at line {startLine} in {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                if (!_plan.Plan.TryWriteFileLines(file, content, startLine, lineCount, replaceAll, out var updated))
                    return new ToolResult<DevelopFileLines>(
                        false,
                        $"Unable to write to file '{file.RelativePath}'. Check startLine, lineCount, and replaceAll.");

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