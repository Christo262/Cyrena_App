using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;

namespace Cyrena.Developer.Services
{
    internal class BlazorLibraryProjectType : IDotnetProjectType
    {
        public string Id => DotnetOptions.CsBlazorLibrary;
        public string ProjectTypeName => "Blazor Component Library";

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            plan.IndexBlazorProjectType();
            model[DotnetOptions.CSharp.Namespace] = csproj.RootNamespace;
            model[DotnetOptions.CSharp.TargetFrameworks] = csproj.TargetFrameworks;
            return plan;
        }

        public bool IsOfType(ProjectInfo info)
        {
            if (Path.GetExtension(info.AbsolutePath) != ".csproj")
                return false;
            try
            {
                ProjectFileInfo csproj = ProjectParser.ParseProject(info.AbsolutePath);
                if(csproj.SdkType == "Microsoft.NET.Sdk.Razor" && csproj.NuGetPackages.Any(x => x.Name == "Microsoft.AspNetCore.Components.Web"))
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}
