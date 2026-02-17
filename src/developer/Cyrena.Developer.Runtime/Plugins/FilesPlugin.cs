using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Developer.Plugins
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
        [Description("Reads the text of a file.")]
        public string ReadFileContent(
            [Description("The id of the file.")] string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return "[ERROR]File not found[/ERROR]";
                _context.LogInfo($"Reading file {file!.Name}");
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
        [Description("Reads the text of a file and returns a structured list of lines in the file as well as the index number of each line.")]
        public ToolResult<DevelopFileLines> ReadFileLines(
            [Description("The id of the file.")] string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");
                _context.LogInfo($"Reading file {file!.Name}");
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
        [Description("Write text to a file. If the file already exists, the content is overwritten.")]
        public ToolResult<DevelopFileContent> WriteFileContent(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The content to write.")] string? content)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileContent>(false, $"File with id {fileId} not found.");
                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileContent>(false, "File is READ ONLY");
                _context.LogInfo($"Writing file {file!.Name}");
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

        [KernelFunction("write_line")]
        [Description("Replaces a line of text in a file.")]
        public ToolResult<DevelopFileLines> ReplaceFileLine(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The index number of the line to replace.")] int index,
            [Description("The text to replace the line with.")] string text)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileLines>(false, $"File with id {fileId} not found.");
                if (file!.ReadOnly)
                    return new ToolResult<DevelopFileLines>(false, "File is READ ONLY");
                _context.LogInfo($"Writing file {file!.Name}");
                _plan.Plan.TryReadFileContent(file!, out var fileContent);
                _version.Backup(fileContent);
                if (!_plan.Plan.TryWriteFileLine(file!, index, text, out var newContent))
                    return new ToolResult<DevelopFileLines>(false, $"Unable to replace file line");
                return new ToolResult<DevelopFileLines>(newContent!);
            }
            catch (Exception ex)
            {
                return new ToolResult<DevelopFileLines>(false, $"Error: {ex.Message}");
            }
        }

        [KernelFunction("append_line")]
        [Description("Appends a line of text to the end of a file.")]
        public ToolResult<DevelopFileContent> AppendFileLine(
            [Description("The id of the file to write to.")] string fileId,
            [Description("The text to append.")] string text)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult<DevelopFileContent>(false, $"File with id {fileId} not found.");
                if (!_plan.Plan.TryReadFileContent(file!, out var fileContent))
                    return new ToolResult<DevelopFileContent>(false, $"Unable to read file with id {fileId}.");
                _context.LogInfo($"Writing file {file!.Name}");
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

        [KernelFunction("delete")]
        [Description("Deletes a file from the project.")]
        public ToolResult DeleteFile([Description("The id of the file to delete.")] string fileId)
        {
            try
            {
                if (!_plan.Plan.TryFindFile(fileId, out var file))
                    return new ToolResult(true, "File does not exist.");
                if (file!.ReadOnly)
                    return new ToolResult(false, "File is READ ONLY");
                _context.LogInfo($"Deleting file {file!.Name}");
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
