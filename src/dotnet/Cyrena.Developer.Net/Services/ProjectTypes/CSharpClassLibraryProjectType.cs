using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;

namespace Cyrena.Developer.Services
{
    internal class CSharpClassLibraryProjectType : IDotnetProjectType
    {
        public string Id => DotnetOptions.CsClassLibrary;
        public string ProjectTypeName => ".NET C# Class Library";

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            model[DotnetOptions.CSharp.Namespace] = csproj.RootNamespace;
            model[DotnetOptions.CSharp.TargetFrameworks] = csproj.TargetFrameworks;
            return plan;
        }

        public bool IsOfType(ProjectInfo info)
        {
            if(Path.GetExtension(info.AbsolutePath) != ".csproj")
                return false;
            try
            {
                ProjectFileInfo csproj = ProjectParser.ParseProject(info.AbsolutePath);
                return csproj.SdkType == "Microsoft.NET.Sdk";
            }
            catch { return false; }
        }
    }
}
