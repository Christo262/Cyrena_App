using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Developer.Models
{
    public class ProjectModel : Entity, IJsonSerializable
    {
        public ProjectModel()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, string?>();
        }
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

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }

    public class ProjectViewModel : ProjectModel
    {
        public ProjectViewModel() { }
        public ProjectViewModel(ProjectModel project)
        {
            Id = project.Id;
            ConversationId = project.ConversationId;
            ProjectFilePath = project.ProjectFilePath;
            ProjectName = project.ProjectName;
            ProjectDirectory = project.ProjectDirectory;
            ProjectTypeId = project.ProjectTypeId;
            ProjectTypeName = project.ProjectTypeName;
            Properties = project.Properties;
        }
        public DevelopPlan? Plan { get; set; }

        public ProjectModel ToModel()
        {
            return new ProjectModel()
            {
                Id = Id,
                ConversationId = ConversationId,
                ProjectFilePath = ProjectFilePath,
                ProjectName = ProjectName,
                ProjectDirectory = ProjectDirectory,
                ProjectTypeId = ProjectTypeId,
                ProjectTypeName = ProjectTypeName,
                Properties = Properties
            };
        }
    }
}
