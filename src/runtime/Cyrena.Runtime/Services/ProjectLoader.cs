using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;

namespace Cyrena.Runtime.Services
{
    internal class ProjectLoader : IProjectLoader
    {
        private readonly IStore<Project> _projects;
        private readonly IServiceProvider _services;
        public ProjectLoader(IStore<Project> projects, IServiceProvider services)
        {
            _projects = projects;
            _services = services;
        }

        public async Task<IDeveloperContext> LoadProjectAsync(string projectId)
        {
            var project = await _projects.FindAsync(x => x.Id == projectId);
            if (project == null)
                throw new NullReferenceException($"Unable to find project with id {projectId}");
            var provider = _services.GetServices<IProjectConfigurator>().FirstOrDefault(x => x.ProjectType == project.Type);
            if (provider == null)
                throw new NullReferenceException($"Unable to find provider for {project.Type}");
            var kernelBuilder = Kernel.CreateBuilder();
            var connections = _services.GetServices<IConnectionProvider>();
            IConnection? connection = null;
            foreach(var connectionProvider in connections)
            {
                if (await connectionProvider.HasConnectionAsync(project.ConnectionId))
                {
                    connection = await connectionProvider.CreateAsync(kernelBuilder, project.ConnectionId);
                    break;
                }
            }
            if (connection == null)
                throw new NullReferenceException($"Unable to construct connection for project {project.Name}");
            var devBuilder = new DeveloperContextBuilder(kernelBuilder, project);
            var plan = await provider.InitializeAsync(devBuilder);
            var extensions = _services.GetServices<IDeveloperContextExtension>();
            foreach (var extension in extensions.OrderBy(x => x.Priority))
                await extension.ExtendAsync(devBuilder);
            var context = new DeveloperContext(project, plan, connection);
            kernelBuilder.Services.AddSingleton<IDeveloperContext>(context);
            context.KernelHistory = devBuilder.KernelHistory;
            context.DisplayHistory = devBuilder.DisplayHistory;
            var kernel = kernelBuilder.Build();
            context.Kernel = kernel;
            return context;
        }
    }
}
