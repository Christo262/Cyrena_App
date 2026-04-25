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
    internal class FileActions
    {
        private readonly IDevelopPlanService _plan;
        private readonly IChatMessageService _context;
        private readonly IVersionControl _version;
        public FileActions(IDevelopPlanService plan, IChatMessageService context, IVersionControl version)
        {
            _plan = plan;
            _context = context;
            _version = version;
        }

        [KernelFunction("read_file")]
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

        [KernelFunction("write_content")]
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
                return new ToolResult<DevelopFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("replace_line")]
        [Description(
    "Replaces a block of lines in a file starting at a given index.\n" +
    "Provide the zero‑based start line, the number of lines to remove, " +
    "and the new line(s) that should take their place (one line per entry).")]
        public ToolResult<DevelopFileLines> ReplaceFileLine(
    [Description(
        "The unique identifier of the target file within the current develop plan.")]
    string fileId,

    [Description(
        "Zero‑based line number where the replacement starts (0 = first line).")]
    int startIndex,

    [Description(
        "How many existing lines should be removed starting at *startIndex*.")]
    int count,

    [Description(
        "The new line(s) that will replace the removed block. " +
        "Separate multiple lines with the literal '\\n' (the method will split on it).")]
    string newLines)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false,
                        $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, "File is READ ONLY");

                _context.LogInfo(
                    $"Replacing {count} line(s) at index {startIndex} in {file.RelativePath}");

                // Backup the current content for version‑control
                _plan.Plan.TryReadFileContent(file, out var fileContent);
                _version.Backup(fileContent);

                // Split the incoming string into separate lines (the AI can send a
                // single string with '\n' as delimiter)
                var replacement = string.IsNullOrEmpty(newLines)
                    ? Enumerable.Empty<string>()
                    : newLines.Split('\n');

                if (!_plan.Plan.TryReplaceLines(
                        file, startIndex, count, replacement, out var updated))
                {
                    return new ToolResult<DevelopFileLines>(false,
                        $"Unable to replace lines at index {startIndex} (count {count}).");
                }

                return new ToolResult<DevelopFileLines>(updated!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false,
                    $"Error: {ex.Message}");
            }
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
                return new ToolResult<DevelopFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileContent>(false, $"Error: {ex.Message}");
            }
        }
        [KernelFunction("insert_line")]
        [Description("Inserts a new line of text after the line at the given zero‑based index.")]
        public ToolResult<DevelopFileLines> InsertFileLine(
            [Description("The unique identifier of the target file within the current plan.")]
    string fileId,
            [Description("Zero‑based line number after which the new text will be inserted. Use the current line count to append at the end of the file.")]
    int index,
            [Description("The line content to insert (do not include line‑break characters).")]
    string text)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false,
                        $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false,
                        $"File '{file.RelativePath}' is read‑only.");

                _context.LogInfo($"Writing file {file.RelativePath} line number {index} (insert)");
                _plan.Plan.TryReadFileContent(file, out var fileContent);
                _version.Backup(fileContent);

                if (!_plan.Plan.TryInsertLine(file, index, text, out var newContent))
                    return new ToolResult<DevelopFileLines>(false,
                        $"Failed to insert line at index {index} in file '{file.RelativePath}'.");

                return new ToolResult<DevelopFileLines>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false,
                    $"Unexpected error while inserting line: {ex.Message}");
            }
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
                return new ToolResult(true, "File deleted.");

            }
            catch (Exception ex)
            {
                return new ToolResult(false, ex.Message);
            }
        }
    }
}
