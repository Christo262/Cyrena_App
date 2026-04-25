using Cyrena.Coding.Models;
using Cyrena.Dotnet.Contracts;
using Cyrena.Dotnet.Extensions;
using Cyrena.Dotnet.Models;
using Cyrena.Dotnet.Options;

namespace Cyrena.Dotnet.Services
{
    internal class ConsoleAppProjectType : IDotnetProjectType
    {
        public string Id => ConsoleApplication.Id;
        public string ProjectTypeName => ConsoleApplication.Name;

        public bool IsOfType(ProjectInfo info)
        {
            if (Path.GetExtension(info.AbsolutePath) != ".csproj")
                return false;
            try
            {
                ProjectFileInfo csproj = ProjectParser.ParseProject(info.AbsolutePath);
                return csproj.SdkType == "Microsoft.NET.Sdk"
                    && (csproj.OutputType == "Exe" || csproj.OutputType == "WinExe");
            }
            catch { return false; }
        }

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            model[DotnetOptions.CSharp.Namespace] = csproj.RootNamespace;
            model[DotnetOptions.CSharp.TargetFrameworks] = csproj.TargetFrameworks;
            model.Plan = plan;
            model.ProjectTypeId = Id;
            model.ProjectTypeName = ProjectTypeName;
            return plan;
        }
    }
}
