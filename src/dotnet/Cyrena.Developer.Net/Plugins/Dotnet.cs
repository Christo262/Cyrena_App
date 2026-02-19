using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Reflection;

namespace Cyrena.Developer.Plugins
{
    public class Dotnet
    {
        private readonly ISolutionController _sln;
        private readonly IChatMessageService _chat;
        private readonly IDevelopPlanService _plan;
        public Dotnet(ISolutionController sln, IChatMessageService chat, IDevelopPlanService plan)
        {
            _sln = sln;
            _chat = chat;
            _plan = plan;
        }
        [KernelFunction("create_attribute")]
        [Description("Creates a new attribute class in the 'Attributes' directory with starter code")]
        public ToolResult<DevelopFile> CreateAttribute(
            [Description("The name of the attribute, i.e. 'CustomAttribute'.")] string name)
        {
            var folder_id = "attributes";
            var folder_name = "Attributes";
            var id_pre = "attributes_";
            var template_id = "attribute.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        [KernelFunction("create_contract")]
        [Description("Creates a new interface in the 'Contracts' directory with starter code")]
        public ToolResult<DevelopFile> CreateContract(
            [Description("The name of the contract, i.e. 'IUserService'.")] string name)
        {
            var folder_id = "contracts";
            var folder_name = "Contracts";
            var id_pre = "contracts_";
            var template_id = "contract.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        [KernelFunction("create_model")]
        [Description("Creates a new data class in the 'Models' directory with starter code")]
        public ToolResult<DevelopFile> CreateModel(
            [Description("The name of the model, i.e. 'UserModel'.")]string name)
        {
            var folder_id = "models";
            var folder_name = "Models";
            var id_pre = "models_";
            var template_id = "model.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        [KernelFunction("create_option")]
        [Description("Creates a new class in the 'Options' directory with starter code")]
        public ToolResult<DevelopFile> CreateOption(
            [Description("The name of the option, i.e. 'IdentityOptions'.")] string name)
        {
            var folder_id = "options";
            var folder_name = "Options";
            var id_pre = "options_";
            var template_id = "option.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        [KernelFunction("create_service")]
        [Description("Creates a new class in the 'Services' directory with starter code")]
        public ToolResult<DevelopFile> CreateService(
            [Description("The name of the service, i.e. 'UserService'.")] string name)
        {
            var folder_id = "services";
            var folder_name = "Services";
            var id_pre = "services_";
            var template_id = "service.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        [KernelFunction("create_extension")]
        [Description("Creates a new class in the 'Extensions' directory with starter code")]
        public ToolResult<DevelopFile> CreateExtension(
    [Description("The name of the extension, i.e. 'UserExtensions'.")] string name)
        {
            var folder_id = "extensions";
            var folder_name = "Extensions";
            var id_pre = "extensions_";
            var template_id = "extension.txt";
            return CreateFile(folder_id, folder_name, id_pre, template_id, name, "cs");
        }

        private ToolResult<DevelopFile> CreateFile(string folder_id, string folder_name, string id_pre, string template_id, string name, string ext)
        {
            name = Path.GetFileNameWithoutExtension(name);
            var id = $"{id_pre}{name}";
            if (_plan.Plan.TryFindFile(id, out var file))
                return new ToolResult<DevelopFile>(file!, true, "File already exists.");
            _chat.LogInfo($"Creating {name}.{ext} in {folder_name}");
            var template = ReadTemplate(template_id);
            foreach (var item in _sln.Current.Properties)
                template = template.Replace($"{{{item.Key}}}", item.Value);
            template = template.Replace("{name}", name);
            var folder = _plan.Plan.GetOrCreateFolder(folder_id, folder_name);
            file = _plan.Plan.CreateFile(folder, id, $"{name}.{ext}", template);
            return new ToolResult<DevelopFile>(file!);
        }

        public static string ReadTemplate(string name)
        {
            var assembly = typeof(DotnetSolution).Assembly;
            var resourceName = $"Cyrena.Developer.cs_templates.{name}";
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
                throw new NullReferenceException($"Unable to find {resourceName}");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
