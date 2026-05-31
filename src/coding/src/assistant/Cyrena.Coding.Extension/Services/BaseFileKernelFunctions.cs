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
        public ToolResult WriteFile(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("The new full content of the file.")]
            string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                _context.LogInfo($"Overwriting entire file {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                if (!_plan.Plan.TryWriteFileOverwrite(file, content, out var updated))
                    return new ToolResult(false, $"Unable to write to file '{file.RelativePath}'.");

                _plan.InvokeFileUpdated(updated!);

                return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) written. Call read_lines before making further edits.");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("replace")]
        [Description(
            "Replaces an exact block of text in a file with new content. " +
            "oldText must match the file content exactly — including whitespace and indentation — and must appear exactly once. " +
            "If oldText appears more than once, the operation fails; include more surrounding context to make it unique. " +
            "If oldText is not found, the operation fails; call read first to verify the exact text. " +
            "Prefer this over edit for modifying existing code, especially in markup files (.razor, .html, .xml) where line numbers are unreliable. " +
            "To delete a block without inserting anything, pass null or empty newText. " +
            "If this function fails repeatedly, fall back to Code_write to overwrite the whole file.")]
        public ToolResult ReplaceText(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("The exact text to find and replace. Must match the file content exactly, including whitespace and indentation, and must be unique within the file.")]
            string oldText,

            [Description("The new text to insert in place of oldText. Pass null or empty to delete the block without inserting anything.")]
            string? newText)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                if (string.IsNullOrEmpty(oldText))
                    return new ToolResult(false, "oldText cannot be null or empty.");

                if (!_plan.Plan.TryReadFileContent(file, out var fileContent) || fileContent?.Content == null)
                    return new ToolResult(false, $"Unable to read file '{file.RelativePath}'.");

                var content = fileContent.Content;
                var matchCount = CountOccurrences(content, oldText);

                if (matchCount == 0)
                    return new ToolResult(false, $"oldText not found in '{file.RelativePath}'. Call read to verify the exact text including whitespace and indentation.");

                if (matchCount > 1)
                    return new ToolResult(false, $"oldText found {matchCount} times in '{file.RelativePath}'. Include more surrounding context in oldText to make it unique.");

                _version.Backup(fileContent);

                var updatedContent = content.Replace(oldText, newText ?? string.Empty);

                if (!_plan.Plan.TryWriteFileOverwrite(file, updatedContent, out var updated))
                    return new ToolResult(false, $"Unable to write to file '{file.RelativePath}'.");

                _context.LogInfo($"Replaced text in {file.RelativePath}");
                _plan.InvokeFileUpdated(updated!);

                return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) updated. Call read before making further replacements.");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, $"Error: {ex.Message}");
            }
        }

        private static int CountOccurrences(string source, string value)
        {
            int count = 0;
            int index = 0;
            while ((index = source.IndexOf(value, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += value.Length;
            }
            return count;
        }

        [KernelFunction("edit")]
        [Description(
            "Edits a file by inserting or replacing lines. Always call read_lines first to get exact line numbers. " +
            "startLine is 1-based (line 1 is the first line of the file). " +
            "If lineCount is 0, content is inserted before startLine without removing anything. " +
            "If lineCount is greater than 0, that many lines are removed starting at startLine, then content is inserted at the same position. " +
            "To append to the end of the file, use startLine = totalLines + 1 with lineCount 0. " +
            "To delete lines without inserting, pass null or empty content with lineCount > 0. " +
            "CAUTION: Avoid using this function on markup files (.razor, .html, .xml etc) — tag nesting makes line-based edits unreliable. Use Code_replace or Code_write instead. " +
            "If repeated edits are not producing the correct result, fall back to Code_write to overwrite the whole file.")]
        public ToolResult EditLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("1-based line number where the edit begins. Use totalLines + 1 to append at the end.")]
            int startLine,

            [Description("Number of existing lines to remove starting at startLine. Use 0 to insert without removing anything.")]
            int lineCount,

            [Description("The content to insert at startLine. Pass null or empty to only delete lines.")]
            string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                if (startLine < 1)
                    return new ToolResult(false, $"Invalid startLine ({startLine}): must be >= 1.");

                if (lineCount < 0)
                    return new ToolResult(false, $"Invalid lineCount ({lineCount}): must be >= 0.");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                // Convert to 0-based for internal use
                var zeroBasedStart = startLine - 1;

                if (lineCount == 0)
                {
                    _context.LogInfo($"Inserting content at line {startLine} in {file.RelativePath}");

                    if (!_plan.Plan.TryWriteFileInsert(file, content, zeroBasedStart, out var inserted, out var insertTotal))
                    {
                        var rangeHint = insertTotal.HasValue
                            ? $" File has {insertTotal} line(s) (valid startLine range: 1–{insertTotal + 1})."
                            : string.Empty;
                        return new ToolResult(false, $"Unable to insert into '{file.RelativePath}'. Ensure startLine is within range.{rangeHint}");
                    }

                    _plan.InvokeFileUpdated(inserted!);
                    return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) edited. Call read_lines before making further edits.");
                }
                else
                {
                    _context.LogInfo($"Replacing {lineCount} line(s) at line {startLine} in {file.RelativePath}");

                    if (!_plan.Plan.TryWriteFileReplace(file, content, zeroBasedStart, lineCount, out var replaced, out var replaceTotal))
                    {
                        var rangeHint = replaceTotal.HasValue
                            ? $" File has {replaceTotal} line(s) (valid range: startLine 1–{replaceTotal}, lineCount 1–{replaceTotal})."
                            : string.Empty;
                        return new ToolResult(false, $"Unable to edit '{file.RelativePath}'. Ensure startLine and lineCount are within the file.{rangeHint}");
                    }

                    _plan.InvokeFileUpdated(replaced!);
                    return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) edited. Call read_lines before making further edits.");
                }
            }
            catch (Exception ex)
            {
                return new ToolResult(false, $"Error: {ex.Message}");
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