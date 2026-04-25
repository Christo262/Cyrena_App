using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Dotnet.Extensions;
using Cyrena.Dotnet.Options;
using Cyrena.Models;

namespace Cyrena.Dotnet.Services
{
    internal class ConsoleAppSolutionBuilder : ICodeBuilder
    {
        public string Id => ConsoleApplication.Id;

        public Task<DevelopPlan> ConfigureAsync(CyrenaKernelBuilder options)
        {
            var config = options.ChatConfiguration;
            var projectPath = config[DotnetOptions.ProjectFilePath];
            
            if (string.IsNullOrEmpty(projectPath) || !File.Exists(projectPath))
                throw new InvalidOperationException("Project file path is not configured.");

            var rootDir = new FileInfo(projectPath).DirectoryName!;
            var plan = new DevelopPlan(rootDir);
            plan.IndexDefaultCSharpProject();

            var projectInfo = ProjectParser.ParseProject(projectPath);
            config[DotnetOptions.CSharp.Namespace] = projectInfo.RootNamespace;
            config[DotnetOptions.CSharp.TargetFrameworks] = projectInfo.TargetFrameworks;

            return Task.FromResult(plan);
        }

        public Task DeleteAsync(ChatConfiguration config) => Task.CompletedTask;

        public Task EditAsync(ChatConfiguration config, IServiceProvider services) => Task.CompletedTask;
    }
}
