using Cyrena.Models;
using Cyrena.Net.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.Blazor.Models
{
    public class BlazorProject : DotnetProject
    {
        public BlazorProject() : base("blazor-app")
        {
        }

        public BlazorProject(Project project) : base(project, "blazor-app")
        {
        }
    }
}
