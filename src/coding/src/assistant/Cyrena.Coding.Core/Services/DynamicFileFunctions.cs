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
    internal class DynamicFileFunctions(
        IDevelopPlanService plan,
        IChatMessageService context,
        IVersionControl version,
        IServiceProvider services)
    {
        private readonly IVersionControl _version = version;
        private readonly IServiceProvider _services = services;

        [KernelFunction("create")]
        [Description("Creates a new file.")]
        public ToolResult CreateFile(
    [Description("The name of the file including the file type extension. i.e. MyModel.cs, main.py, styles.css")] string name,
    [Description("The content to insert into the file. Leave empty to create file with no content.")] string? content,
    [Description("Provide if the file needs to be created in a specific folder in the develop plan. Empty folderId will create the file in the root directory.")] string? folderId = null)
        {
            var extension = Path.GetExtension(name).Replace(".", string.Empty);
            if (string.IsNullOrEmpty(folderId))
            {
                if (!plan.Plan.AllowedFileTypes.Contains(extension))
                    return new ToolResult(false, $"Unable to create file. Root directory only supports the following file types: {string.Join(", *.", plan.Plan.AllowedFileTypes)}.");
                context.LogInfo($"Creating file {name} in {folderId ?? "root"}");
                var rootFile = plan.Plan.CreateFile($"root_{extension}_{Path.GetFileNameWithoutExtension(name)}", name, content);
                return new ToolResult(true, $"File '{rootFile.RelativePath}' (id: {rootFile.Id}) created. Call read_lines before making further edits.");
            }

            if (!plan.Plan.TryFindFolder(folderId, out var folder))
                return new ToolResult(false, $"Folder {folderId} not found.");
            context.LogInfo($"Creating file {name} in {folderId ?? "root"}");
            var file = plan.Plan.CreateFile(folder!, $"{folderId}_{extension}_{Path.GetFileNameWithoutExtension(name)}", name, content);
            return new ToolResult(true, $"File '{file.RelativePath}' (id: {file.Id}) created. Call read_lines before making further edits.");
        }

        [KernelFunction("create_folder")]
        [Description("Creates a new folder.")]
        public ToolResult<DevelopFolder> CreateFolder(
            [Description("The name of the new folder")] string name,
            [Description("The folder Id of the parent folder this folder must be created in. Leave empty to create at root/.")] string? parentFolderId)
        {
            if (string.IsNullOrEmpty(parentFolderId))
            {
                var folder = plan.Plan.GetOrCreateFolder(name.ToLower(), name);
                return new ToolResult<DevelopFolder>(folder);
            }

            if (!plan.Plan.TryFindFolder(parentFolderId, out var parent))
                return new ToolResult<DevelopFolder>(false, $"Unable to find parent folder with id {parentFolderId}");
            var model = plan.Plan.GetOrCreateFolder(parent!, name.ToLower(), name);
            return new ToolResult<DevelopFolder>(model);
        }

        [KernelFunction("delete_folder")]
        [Description("Deletes a folder.")]
        public ToolResult DeleteFolder(
            [Description("The id of the folder to delete.")] string folderId,
            [Description("If true, deletes sub-folders and files within the folder.")] bool recursive)
        {
            if (plan.Plan.TryFindFolder(folderId, out var folder))
            {
                var s = plan.Plan.RemoveFolder(folder!, recursive);
                if (s)
                    return new ToolResult(true, "Folder removed");
                else
                    return new ToolResult(false, "Unable to remove folder");
            }
            return new ToolResult(true, "Folder removed");
        }
    }
}
