using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Coding.Services
{
    internal class BaseFileKernelFunctions
    {
        private readonly IDevelopPlanService _plan;
        private readonly IChatMessageService _context;
        private readonly IVersionControl _version;
        private readonly IServiceProvider _services;

        public BaseFileKernelFunctions(
            IDevelopPlanService plan,
            IChatMessageService context,
            IVersionControl version, IServiceProvider services)
        {
            _plan = plan;
            _context = context;
            _version = version;
            _services = services;
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

        [KernelFunction("insert")]
        [Description(
            "Inserts content before the specified line without removing anything. Always call read_lines first to get exact line numbers. " +
            "startLine is 1-based (line 1 is the first line of the file). " +
            "To append to the end of the file, use startLine = totalLines + 1. " +
            "To replace existing content, use Code_replace instead.")]
        public ToolResult InsertLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("1-based line number to insert before. Use totalLines + 1 to append at the end.")]
            int startLine,

            [Description("The content to insert.")]
            string content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                if (startLine < 1)
                    return new ToolResult(false, $"Invalid startLine ({startLine}): must be >= 1.");

                _context.LogInfo($"Inserting content at line {startLine} in {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                var zeroBasedStart = startLine - 1;

                if (!_plan.Plan.TryWriteFileInsert(file, content, zeroBasedStart, out var inserted, out var totalLines))
                {
                    var rangeHint = totalLines.HasValue
                        ? $" File has {totalLines} line(s) (valid startLine range: 1–{totalLines + 1})."
                        : string.Empty;
                    return new ToolResult(false, $"Unable to insert into '{file.RelativePath}'. Ensure startLine is within range.{rangeHint}");
                }

                _plan.InvokeFileUpdated(inserted!);
                return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) updated. Call read_lines before making further edits.");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("delete_lines")]
        [Description(
            "Deletes a range of lines from a file. Always call read_lines first to get exact line numbers. " +
            "startLine and endLine are 1-based and inclusive.")]
        public ToolResult DeleteLines(
            [Description("The unique identifier of the target file within the current develop plan.")]
            string fileId,

            [Description("1-based line number of the first line to delete.")]
            int startLine,

            [Description("1-based line number of the last line to delete (inclusive).")]
            int endLine)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(false, $"File with id {fileId} not found.");

                if (file!.ReadOnly)
                    return new ToolResult(false, $"File '{file.RelativePath}' is read-only.");

                if (startLine < 1 || endLine < startLine)
                    return new ToolResult(false, $"Invalid range: startLine ({startLine}) must be >= 1 and endLine ({endLine}) must be >= startLine.");

                _context.LogInfo($"Deleting lines {startLine}–{endLine} in {file.RelativePath}");

                _plan.Plan.TryReadFileContent(file, out var existingContent);
                _version.Backup(existingContent);

                var zeroBasedStart = startLine - 1;
                var lineCount = endLine - startLine + 1;

                if (!_plan.Plan.TryWriteFileReplace(file, null, zeroBasedStart, lineCount, out var updated, out var totalLines))
                {
                    var rangeHint = totalLines.HasValue
                        ? $" File has {totalLines} line(s) (valid range: 1–{totalLines})."
                        : string.Empty;
                    return new ToolResult(false, $"Unable to delete lines from '{file.RelativePath}'. Ensure startLine and endLine are within the file.{rangeHint}");
                }

                _plan.InvokeFileUpdated(updated!);
                return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) updated. Call read_lines before making further edits.");
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

        [KernelFunction("create")]
        [Description("Creates a new file.")]
        public ToolResult CreateFile(
            [Description("The name of the file including the file type extension. i.e. MyModel.cs, main.py, styles.css")] string name,
            [Description("The content to insert into the file. Leave empty to create file with no content.")] string? content,
            [Description("Provide if the file needs to be created in a specific folder in the develop plan. Empty folderId will create the file in the root directory.")] string? folderId = null)
        {
            var options = _services.GetService<DynamicDiscoveryOptions>();
            if (options == null)
                return new ToolResult(false, "This function is not currently accessible. Please use dedicated create file functions.");
            var extension = Path.GetExtension(name).Replace(".", string.Empty);
            if (string.IsNullOrEmpty(folderId))
            {
                if (!_plan.Plan.AllowedFileTypes.Contains(extension))
                    return new ToolResult(false, $"Unable to create file. Root directory only supports the following file types: {string.Join(", *.", _plan.Plan.AllowedFileTypes)}.");
                _context.LogInfo($"Creating file {name} in {folderId ?? "root"}");
                var rootFile = _plan.Plan.CreateFile($"root_{extension}_{Path.GetFileNameWithoutExtension(name)}", name, content);
                return new ToolResult(true, $"File '{rootFile.RelativePath}' (id: {rootFile.Id}) created. Call read_lines before making further edits.");
            }

            if (!_plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult(false, $"Folder {folderId} not found.");
            _context.LogInfo($"Creating file {name} in {folderId ?? "root"}");
            var file = _plan.Plan.CreateFile(folder!, $"{folderId}_{extension}_{Path.GetFileNameWithoutExtension(name)}", name, content);
            return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) created. Call read_lines before making further edits.");
        }

        [KernelFunction("create_folder")]
        [Description("Creates a new folder.")]
        public ToolResult<DevelopFolder> CreateFolder(
            [Description("The name of the new folder")]string name, 
            [Description("The folder Id of the parent folder this folder must be created in. Leave empty to create at root/.")]string? parentFolderId)
        {
            var options = _services.GetService<DynamicDiscoveryOptions>();
            if (options == null)
                return new ToolResult<DevelopFolder>(false, "This function is not currently accessible. Please use dedicated create file functions.");
            if (string.IsNullOrEmpty(parentFolderId))
            {
                var folder = _plan.Plan.GetOrCreateFolder(name.ToLower(), name);
                return new ToolResult<DevelopFolder>(folder);
            }

            if (!_plan.Plan.TryFindFolder(parentFolderId, out var parent))
                return new ToolResult<DevelopFolder>(false, $"Unable to find parent folder with id {parentFolderId}");
            var model = _plan.Plan.GetOrCreateFolder(parent!, name.ToLower(), name);
            return new ToolResult<DevelopFolder>(model);
        }

        [KernelFunction("delete_folder")]
        [Description("Deletes a folder.")]
        public ToolResult DeleteFolder(
            [Description("The id of the folder to delete.")]string folderId,
            [Description("If true, deletes sub-folders and files within the folder.")]bool recursive)
        {
            var options = _services.GetService<DynamicDiscoveryOptions>();
            if (options == null)
                return new ToolResult<DevelopFolder>(false, "This function is not currently accessible. Please use dedicated create/delete file functions.");
            if(_plan.Plan.TryFindFolder(folderId, out var folder))
            {
                var s = _plan.Plan.RemoveFolder(folder!, recursive);
                if (s)
                    return new ToolResult(true, "Folder removed");
                else
                    return new ToolResult(false, "Unable to remove folder");
            }
            return new ToolResult(true, "Folder removed");
        }
    }
}