using Cyrena.Developer.Contracts;
using Cyrena.Developer.Extensions;
using Cyrena.Developer.Models;
using Cyrena.Developer.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Developer.Services
{
    internal class BlazorAppProjectType : IDotnetProjectType
    {
        public string Id => DotnetOptions.CsBlazorApp;
        public string ProjectTypeName => "Blazor App";

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
                var dir = Path.GetDirectoryName(info.AbsolutePath);
                var imports = Path.Combine(dir!, "Components", "_Imports.razor");
                if (csproj.SdkType == "Microsoft.NET.Sdk.Web" && File.Exists(imports))
                    return true;
                return false;
            }
            catch { return false; }
        }
    }
}
