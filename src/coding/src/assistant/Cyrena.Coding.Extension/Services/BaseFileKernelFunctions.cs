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
            "Returns a structured list of the file's lines, each paired with its 1-based line number (first line is 1). " +
            "Always call this before using write with Insert or Replace so you have exact line numbers.")]
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
        [Description("Overwrites the entire file with the provided content. Use when rewriting the whole file from scratch.")]
        public ToolResult<DevelopFileLines> WriteFile(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("The new full content of the file.")]
            string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

                _context.LogInfo($"Overwriting entire file {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                if (!_plan.Plan.TryWriteFileOverwrite(file, content, out var updated))
                    return new ToolResult<DevelopFileLines>(false, $"Unable to write to file '{file.RelativePath}'.");

                _plan.InvokeFileUpdated(updated!);

                return new ToolResult<DevelopFileLines>(updated!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("replace")]
        [Description(
            "Replaces a range of lines in the file with new content. Line numbers are 1-based. " +
            "Removes all lines from startLine to endLine (inclusive), then inserts content at that position. " +
            "Pass null or empty content to delete the lines without inserting anything. " +
            "Always call read_lines first to get exact line numbers. Do not keep replacing if it is not working and use Code_write.")]
        public ToolResult<DevelopFileLines> ReplaceLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("1-based line number of the first line to remove (line 1 is the first line of the file).")]
            int startLine,

            [Description("1-based line number of the last line to remove (inclusive).")]
            int endLine,

            [Description("The content to insert at startLine after removal. Null or empty to only delete lines.")]
            string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

                if (startLine < 1 || endLine < startLine)
                    return new ToolResult<DevelopFileLines>(false, $"Invalid range: startLine ({startLine}) must be >= 1 and endLine ({endLine}) must be >= startLine.");

                _context.LogInfo($"Replacing lines {startLine}–{endLine} in {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                // Convert to 0-based for internal use; lineCount derived from the inclusive range
                var zeroBasedStart = startLine - 1;
                var lineCount = endLine - startLine + 1;

                if (!_plan.Plan.TryWriteFileReplace(file, content, zeroBasedStart, lineCount, out var updated, out var totalLines))
                {
                    var rangeHint = totalLines.HasValue
                        ? $" File has {totalLines} line(s) (valid range: 1–{totalLines})."
                        : string.Empty;
                    return new ToolResult<DevelopFileLines>(
                        false,
                        $"Unable to replace lines in '{file.RelativePath}'. Ensure startLine and endLine are within the file.{rangeHint}");
                }

                _plan.InvokeFileUpdated(updated!);

                return new ToolResult<DevelopFileLines>(updated!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("insert")]
        [Description(
            "Inserts content before the specified line, shifting all subsequent lines down. Line numbers are 1-based. " +
            "To append to the end of the file, use startLine = totalLines + 1. " +
            "Always call read_lines first to get exact line numbers.")]
        public ToolResult<DevelopFileLines> InsertLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("1-based line number to insert before. Use totalLines + 1 to append at end of file.")]
            int startLine,

            [Description("The content to insert.")]
            string content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

                if (startLine < 1)
                    return new ToolResult<DevelopFileLines>(false, $"Invalid startLine ({startLine}): must be >= 1.");

                _context.LogInfo($"Inserting content at line {startLine} in {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                // Convert to 0-based for internal use
                var zeroBasedStart = startLine - 1;

                if (!_plan.Plan.TryWriteFileInsert(file, content, zeroBasedStart, out var updated, out var totalLines))
                {
                    var rangeHint = totalLines.HasValue
                        ? $" File has {totalLines} line(s) (valid startLine range: 1–{totalLines + 1})."
                        : string.Empty;
                    return new ToolResult<DevelopFileLines>(
                        false,
                        $"Unable to insert into '{file.RelativePath}'. Ensure startLine is within range.{rangeHint}");
                }

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