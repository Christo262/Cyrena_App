using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Net.Models
{
    public class DotnetProject : Project
    {
        public DotnetProject(string type)
        {
            Type = type;
        }

        public DotnetProject(Project project, string type)
        {
            Id = project.Id;
            Name = project.Name;
            Description = project.Description;
            RootDirectory = project.RootDirectory;
            Created = project.Created;
            LastModified = project.LastModified;
            Type = type;
            ConnectionId = project.ConnectionId;
            Properties = project.Properties;
        }

        [Required]
        public string? RootNamespace
        {
            get
            {
                return Properties["namespace"];
            }
            set
            {
                Properties["namespace"] = value;
            }
        }

        [Required]
        public string? TargetFramework
        {
            get
            {
                return Properties["targetFramework"];
            }
            set
            {
                Properties["targetFramework"] = value;
            }
        }
    }
}
