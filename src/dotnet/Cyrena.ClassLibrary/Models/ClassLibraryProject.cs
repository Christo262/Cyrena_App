using Cyrena.Models;
using Cyrena.Net.Models;

namespace Cyrena.ClassLibrary.Models
{
    public class ClassLibraryProject : DotnetProject
    {
        public ClassLibraryProject() : base("dotnet-class-lib")
        {
        }

        public ClassLibraryProject(Project project) : base(project, "dotnet-class-lib")
        {
        }
    }
}
