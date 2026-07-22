using Cyrena.Coding.Models;
using Cyrena.Dotnet.Options;
using Cyrena.Models;
using System.Text.Json;

namespace Cyrena.Dotnet.Models
{
    public class ProjectModel 
    {
        public ProjectModel()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, string?>();
        }
        public string Id { get; set; } = default!;
        public string ConversationId { get; set; } = default!;
        public string ProjectFilePath { get; set; } = default!;
        public string? ProjectName { get; set; }
        public string ProjectDirectory { get; set; } = default!;
        public string? ProjectTypeId { get; set;  }
        public string? ProjectTypeName { get; set; }

        public Dictionary<string, string?> Properties { get; set; }
        public string? this[string key]
        {
            get
            {
                if (!Properties.ContainsKey(key))
                    return null;
                return Properties[key];
            }
            set
            {
                Properties[key] = value;
            }
        }

        public DevelopPlan? Plan { get; set; }
    }

    /// <summary>
    /// Only used to simplify for AI
    /// </summary>
    public class ProjectViewModel 
    {
        public ProjectViewModel(ProjectModel model)
        {
            Id = model.Id;
            ProjectName = model.ProjectName;
            ProjectTypeName = model.ProjectTypeName;
            TargetFrameworks = model[DotnetOptions.TargetFrameworks];
            RootNamespace = model[DotnetOptions.Namespace];
        }
        public string Id { get; set; } = default!;
        public string? ProjectName { get; set; }
        public string? ProjectTypeName { get; set; }
        public string? TargetFrameworks { get; set; }
        public string? RootNamespace { get; set; }
    }
}
