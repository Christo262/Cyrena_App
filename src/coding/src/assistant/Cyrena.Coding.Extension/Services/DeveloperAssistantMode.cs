using Cyrena.Contracts;
using Cyrena.Coding.Components.Shared;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.Coding.Services
{
    internal class DeveloperAssistantMode : IAssistantMode
    {
        private readonly IServiceProvider _services;
        public DeveloperAssistantMode(IServiceProvider services)
        {
            _services = services;
        }

        public string Id => DevelopOptions.AssistantModeId;

        public async Task ConfigureAsync(CyrenaKernelBuilder builder)
        {
            var config = builder.ChatConfiguration;
            //Migrate previous projects
            if (config.Properties.ContainsKey("dev.root-dir"))
            {
                if (string.IsNullOrEmpty(config.WorkingDirectory))
                    config.WorkingDirectory = config["dev.root-dir"];
                config.Properties.Remove("dev.root-dir");
            }

            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                throw new InvalidOperationException($"{DevelopOptions.BuilderId} not set, unable to configure");
            if (string.IsNullOrEmpty(config.WorkingDirectory) || !Directory.Exists(config.WorkingDirectory))
                throw new InvalidOperationException($"RootDirectory not set, unable to configure");

            var code_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if (code_builder == null)
                throw new NullReferenceException($"Unable to find code builder with id {config[DevelopOptions.BuilderId]}");

            var persistence = builder.AddFilePersistence(Path.Combine(config.WorkingDirectory, ".cyrena"));
            persistence.AddSingletonStore<DynamicDevelopPlan>("develop-plans");
            builder.Services.Configure<ChatOptions>(o =>
            {
                o.IncludeLogsInDisplay = true;
            });
            persistence.AddSingletonStore<StickyNote>("sticky_notes");
            var plan = await code_builder.ConfigureAsync(builder);
            var plan_service = new DevelopPlanService(plan);
            builder.Services.AddSingleton<IDevelopPlanService>(plan_service);
            builder.Services.AddSingleton<IVersionControl, VersionControl>();
            builder.Plugins.AddFromType<BaseFileKernelFunctions>("Code");
            builder.Plugins.AddFromType<ProjectInformation>("Project");
            builder.AddToolbarComponent<VersionControlViewer>(ToolbarAlignment.Start);
            builder.AddToolbarComponent<DevelopPlanEditorButton>(ToolbarAlignment.Start);
            builder.KernelBuilder.AddStartupTask<DevelopPlanWatcher>();
        }

        public Task DeleteAsync(ChatConfiguration config)
        {
            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                return Task.CompletedTask;
            var code_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if(code_builder == null)
                return Task.CompletedTask;
            return code_builder.DeleteAsync(config);
        }

        public Task EditAsync(ChatConfiguration config, IServiceProvider services)
        {
            if (string.IsNullOrWhiteSpace(config[DevelopOptions.BuilderId]))
                return Task.CompletedTask;
            var code_builder = _services.GetServices<ICodeBuilder>().FirstOrDefault(x => x.Id == config[DevelopOptions.BuilderId]);
            if (code_builder == null)
                throw new InvalidOperationException("Unable to find handler for this project type.");
            return code_builder.EditAsync(config, services);
        }
    }
}
