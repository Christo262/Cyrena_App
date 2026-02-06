using Cyrena.Models;
using Cyrena.Net.Models;

namespace Cyrena.ClassLibrary.Models
{
    public class ClassLibraryProject : DotnetProject
    {
        public ClassLibraryProject() : base("blazor-app")
        {
        }

        public ClassLibraryProject(Project project) : base(project, "blazor-app")
        {
        }
    }
}
