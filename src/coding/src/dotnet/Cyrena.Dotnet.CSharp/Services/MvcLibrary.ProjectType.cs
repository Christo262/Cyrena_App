using Cyrena.Coding.Models;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Extensions;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;

namespace Cyrena.Dotnet.CSharp.Services
{
    internal class MvcLibraryProjectType : IDotnetProjectType
    {
        public string Id => MvcLibrary.Id;
        public string ProjectTypeName => MvcLibrary.Name;

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            plan.IndexMvcProjectType();
            model[DotnetOptions.Namespace] = csproj.RootNamespace;
            model[DotnetOptions.TargetFrameworks] = csproj.TargetFrameworks;
            model.Plan = plan;
            return plan;
        }

        public bool IsOfType(ProjectInfo info)
        {
            if (Path.GetExtension(info.AbsolutePath) != ".csproj")
                return false;
            try
            {
                ProjectFileInfo csproj = ProjectParser.ParseProject(info.AbsolutePath);
                if (csproj.SdkType == "Microsoft.NET.Sdk.Razor" && csproj.FrameworkReferences.Any(x => x == "Microsoft.AspNetCore.App"))
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}
