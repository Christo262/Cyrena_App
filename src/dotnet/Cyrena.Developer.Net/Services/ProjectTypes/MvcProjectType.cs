using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;

namespace Cyrena.Developer.Services
{
    internal class MvcProjectType : IDotnetProjectType
    {
        public string Id => DotnetOptions.CsMvcApp;
        public string ProjectTypeName => ".NET MVC";

        public DevelopPlan IndexPlan(ProjectModel model)
        {
            ProjectFileInfo csproj = ProjectParser.ParseProject(model.ProjectFilePath);
            var plan = new DevelopPlan(model.ProjectDirectory);
            plan.IndexDefaultCSharpProject();
            plan.IndexMvcProjectType();
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
                var dir = Path.GetDirectoryName(info.AbsolutePath);
                var imports = Path.Combine(dir!, "Views", "_ViewImports.cshtml");
                if (csproj.SdkType == "Microsoft.NET.Sdk.Web" && File.Exists(imports))
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}
