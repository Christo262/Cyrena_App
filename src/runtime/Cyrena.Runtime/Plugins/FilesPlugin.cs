using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Runtime.Plugins
{
    internal class FilesPlugin
    {
        private readonly IDeveloperContext _context;
        public FilesPlugin(IDeveloperContext context)
        {
            _context = context;
        }

        [KernelFunction]
        [Description("Reads the text of a file.")]
        public string ReadFileContent(
            [Description("The id of the file.")] string fileId)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return "[ERROR]File not found[/ERROR]";
                _context.LogInfo($"Reading file {file!.Name}");
                if (!_context.ProjectPlan.TryReadFileContent(file!, out var fileContent))
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

        [KernelFunction]
        [Description("Reads the text of a file and returns a structured list of lines in the file as well as the index number of each line.")]
        public ToolResult<ProjectFileLines> ReadFileLines(
            [Description("The id of the file.")] string fileId)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return new ToolResult<ProjectFileLines>(false, $"File with id {fileId} not found.");
                _context.LogInfo($"Reading file {file!.Name}");
                if (!_context.ProjectPlan.TryReadFileLines(file!, out var fileContent))
                    return new ToolResult<ProjectFileLines>(false, $"Unable to read file with id {fileId}.");
                return new ToolResult<ProjectFileLines>(fileContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<ProjectFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction]
        [Description("Write text to a file. If the file already exists, the content is overwritten.")]
        public ToolResult<ProjectFileContent> WriteFileContent(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The content to write.")] string? content)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return new ToolResult<ProjectFileContent>(false, $"File with id {fileId} not found.");
                if (file!.ReadOnly)
                    return new ToolResult<ProjectFileContent>(false, "File is READ ONLY");
                _context.LogInfo($"Writing file {file!.Name}");
                if (!_context.ProjectPlan.TryWriteFileContent(file!, content, out var newContent))
                    return new ToolResult<ProjectFileContent>(false, $"Unable to write to file");
                return new ToolResult<ProjectFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<ProjectFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction]
        [Description("Replaces a line of text in a file.")]
        public ToolResult<ProjectFileLines> ReplaceFileLine(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The index number of the line to replace.")] int index,
            [Description("The text to replace the line with.")] string text)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return new ToolResult<ProjectFileLines>(false, $"File with id {fileId} not found.");
                if (file!.ReadOnly)
                    return new ToolResult<ProjectFileLines>(false, "File is READ ONLY");
                _context.LogInfo($"Writing file {file!.Name}");
                if (!_context.ProjectPlan.TryWriteFileLine(file!, index, text, out var newContent))
                    return new ToolResult<ProjectFileLines>(false, $"Unable to replace file line");
                return new ToolResult<ProjectFileLines>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<ProjectFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction]
        [Description("Appends a line of text to the end of a file.")]
        public ToolResult<ProjectFileContent> AppendFileLine(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The text to append.")] string text)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return new ToolResult<ProjectFileContent>(false, $"File with id {fileId} not found.");
                if (!_context.ProjectPlan.TryReadFileContent(file!, out var fileContent))
                    return new ToolResult<ProjectFileContent>(false, $"Unable to read file with id {fileId}.");
                _context.LogInfo($"Writing file {file!.Name}");
                var sb = new StringBuilder();
                sb.Append(fileContent!.Content);
                sb.AppendLine(text);
                if (!_context.ProjectPlan.TryWriteFileContent(file!, sb.ToString(), out var newContent))
                    return new ToolResult<ProjectFileContent>(false, $"Unable to write to file");
                return new ToolResult<ProjectFileContent>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<ProjectFileContent>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction]
        [Description("Deletes a file from the project.")]
        public ToolResult DeleteFile([Description("The id of the file to delete.")] string fileId)
        {
            try
            {
                if (!_context.ProjectPlan.TryFindFile(fileId, out var file))
                    return new ToolResult(true, "File does not exist.");
                if (file!.ReadOnly)
                    return new ToolResult(false, "File is READ ONLY");
                _context.LogInfo($"Deleting file {file!.Name}");
                _context.ProjectPlan.RemoveFile(file!);
                return new ToolResult(true, "File deleted.");

            }catch(Exception ex)
            {
                return new ToolResult(false, ex.Message);
            }
        }
    }
}
