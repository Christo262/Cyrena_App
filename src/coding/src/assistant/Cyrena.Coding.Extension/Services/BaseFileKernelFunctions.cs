using Cyrena.Contracts;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Coding.Services
{
    internal class BaseFileKernelFunctions
    {
        private readonly IDevelopPlanService _plan;
        private readonly IChatMessageService _context;
        private readonly IVersionControl _version;
        public BaseFileKernelFunctions(IDevelopPlanService plan, IChatMessageService context, IVersionControl version)
        {
            _plan = plan;
            _context = context;
            _version = version;
        }

        [KernelFunction("read")]
        [Description("Returns the full text content of the specified file.")]
        public string ReadFileContent(
            [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return "[ERROR]File not found[/ERROR]";
                _context.LogInfo($"Reading file {file!.RelativePath}");
                if (!_plan.Plan.TryReadFileContent(file!, out var fileContent))
                    return "[ERROR]Unable to read file[/ERROR]";
                var sb = new StringBuilder();
                sb.AppendLine($"FILE: {fileContent!.RelativePath}");
                sb.AppendLine("----------------------------------------");
                sb.AppendLine(fileContent.Content);
                sb.AppendLine("----------------------------------------");
                return sb.ToString();
            }
            catch (Exception ex)
            {
                return $"[ERROR]{ex.Message}[/ERROR]";
            }
        }

        [KernelFunction("read_lines")]
        [Description(
            "Returns a structured list of the file’s lines, each paired with its zero‑based index (0 = first line).")]
        public ToolResult<DevelopFileLines> ReadFileLines(
            [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");
                _context.LogInfo($"Reading file lines {file!.RelativePath}");
                if (!_plan.Plan.TryReadFileLines(file!, out var fileContent))
                    return new ToolResult<DevelopFileLines>(false, $"Unable to read file with id {fileId}.");
                
                return new ToolResult<DevelopFileLines>(fileContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("write")]
        [Description(
            "Writes the supplied text to the specified file, overwriting any existing content.")]
        public ToolResult<DevelopFileContent> WriteFileContent(
            [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId,

            [Description(
        "The complete file content to write (null will clear the file).")]
    string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileContent>(false, $"File with id {fileId} not found.");
                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileContent>(false, "File is READ ONLY");
                _context.LogInfo($"Writing file {file!.RelativePath}");
                _plan.Plan.TryReadFileContent(file!, out var fileContent);
                _version.Backup(fileContent);
                if (!_plan.Plan.TryWriteFileContent(file!, content, out var newContent))
                    return new ToolResult<DevelopFileContent>(false, $"Unable to write to file");
                _plan.InvokeFileUpdated(newContent!);
                return new ToolResult<DevelopFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("replace_lines")]
        [Description(
            "Replace lines in a file. " +
            "startIndex is zero-based. " +
            "endIndex is the last line to remove (inclusive). " +
            "newLines is an array of replacement lines (can be empty to just delete). " +
            "Use FileActions_read_lines to get lines & index numbers of each line.")]
        public ToolResult<DevelopFileLines> ReplaceFileLines(
            [Description("The unique identifier of the target file.")]
    string fileId,

            [Description("Zero-based index of the first line to remove.")]
    int startIndex,

            [Description("Zero-based index of the last line to remove (inclusive). Use the same value as startIndex to replace a single line.")]
    int endIndex,

            [Description("The replacement lines. Pass an empty array to delete without inserting anything.")]
    string[] newLines)
        {
            if (!_plan.Plan.TryFindFile(fileId, out var file))
                return new ToolResult<DevelopFileLines>(false, $"File '{fileId}' not found.");

            if (file!.ReadOnly)
                return new ToolResult<DevelopFileLines>(false, "File is read-only.");

            int count = endIndex - startIndex + 1;
            if (count < 1)
                return new ToolResult<DevelopFileLines>(false, $"endIndex ({endIndex}) must be >= startIndex ({startIndex}).");

            _context.LogInfo($"Replacing lines {startIndex}–{endIndex} in {file.RelativePath}");

            _plan.Plan.TryReadFileContent(file, out var fileContent);
            _version.Backup(fileContent);

            if (!_plan.Plan.TryReplaceLines(file, startIndex, count, newLines, out var updated))
                return new ToolResult<DevelopFileLines>(false, $"Failed to replace lines {startIndex}–{endIndex}.");

            _plan.InvokeFileUpdated(updated!);
            return new ToolResult<DevelopFileLines>(updated!);
        }

        [KernelFunction("append_line")]
        [Description(
            "Appends a new line of text to the **end** of the specified file.")]
        public ToolResult<DevelopFileContent> AppendFileLine(
            [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId,

            [Description(
        "The line content to append (do not include line‑break characters).")]
    string text)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileContent>(false, $"File with id {fileId} not found.");
                if (!_plan.Plan.TryReadFileContent(file!, out var fileContent))
                    return new ToolResult<DevelopFileContent>(false, $"Unable to read file with id {fileId}.");
                _context.LogInfo($"Writing file {file!.RelativePath} append");
                _plan.Plan.TryReadFileContent(file!, out var ext_content);
                _version.Backup(ext_content);
                var sb = new StringBuilder();
                sb.Append(fileContent!.Content);
                sb.AppendLine(text);
                if (!_plan.Plan.TryWriteFileContent(file!, sb.ToString(), out var newContent))
                    return new ToolResult<DevelopFileContent>(false, $"Unable to write to file");

                _plan.InvokeFileUpdated(newContent!);
                return new ToolResult<DevelopFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("insert_lines")]
        [Description(
            "Inserts one or more lines after the line at the given zero-based index. " +
            "Use FileActions_read_lines first to get current line numbers. " +
            "To append at the end, pass the current line count as the index.")]
        public ToolResult<DevelopFileLines> InsertFileLines(
            [Description("The unique identifier of the target file.")]
    string fileId,

            [Description("Zero-based line number to insert after. Pass the total line count to append at the end.")]
    int afterIndex,

            [Description("The lines to insert. Each array entry becomes one new line — do not include line-break characters.")]
    string[] lines)
        {
            if (!_plan.Plan.TryFindFile(fileId, out var file))
                return new ToolResult<DevelopFileLines>(false, $"File '{fileId}' not found.");

            if (file!.ReadOnly)
                return new ToolResult<DevelopFileLines>(false, $"File '{file.RelativePath}' is read-only.");

            _context.LogInfo($"Inserting {lines.Length} line(s) after index {afterIndex} in {file.RelativePath}");
            _plan.Plan.TryReadFileContent(file, out var fileContent);
            _version.Backup(fileContent);
            if (!_plan.Plan.TryInsertLines(file, afterIndex, lines, out var updated))
                return new ToolResult<DevelopFileLines>(false, $"Failed to insert after line {afterIndex} in '{file.RelativePath}'.");

            _plan.InvokeFileUpdated(updated!);
            return new ToolResult<DevelopFileLines>(updated!);
        }

        [KernelFunction("delete")]
        [Description("Removes the specified file from the project.")]
        public ToolResult DeleteFile(
            [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(true, "File does not exist.");
                if (file!.ReadOnly)
                    return new ToolResult(false, "File is READ ONLY");
                _context.LogInfo($"Deleting file {file!.RelativePath}");
                _plan.Plan.TryReadFileContent(file!, out var ext_content);
                _version.Backup(ext_content);
                _plan.Plan.RemoveFile(file!);
                _plan.InvokeFileDeleted(file);
                return new ToolResult(true, "File deleted.");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, ex.Message);
            }
        }
    }
}
