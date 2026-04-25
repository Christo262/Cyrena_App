using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Extensions;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;

namespace Cyrena.Dotnet.Services
{
    internal class CSharpClassLibraryProjectType : IDotnetProjectType
    {
        public string Id => DotnetOptions.CsClassLibrary;
        public string ProjectTypeName => "Class Library";

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            plan.IndexFiles("cs", "lib_cs_");
            model[DotnetOptions.CSharp.Namespace] = csproj.RootNamespace;
            model[DotnetOptions.CSharp.TargetFrameworks] = csproj.TargetFrameworks;
            model.Plan = plan;
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
