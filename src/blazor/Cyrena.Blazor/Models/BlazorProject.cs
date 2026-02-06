using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Blazor.Models
{
    public class BlazorProject : Project
    {
        public BlazorProject() : base()
        {
            Type = "blazor-app";
        }

        public BlazorProject(Project project)
        {
            Id = project.Id;
            Name = project.Name;
            Description = project.Description;
            RootDirectory = project.RootDirectory;
            Created = project.Created;
            LastModified = project.LastModified;
            Type = "blazor-app";
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
