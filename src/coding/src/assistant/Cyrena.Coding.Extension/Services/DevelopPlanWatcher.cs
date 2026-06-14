using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;
using Cyrena.Coding.Options;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Cyrena.Coding.Services
{
    internal class DevelopPlanWatcher : IStartupTask
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DevelopPlanWatcher> _logger;
        public DevelopPlanWatcher(IServiceProvider services, ILogger<DevelopPlanWatcher> logger)
        {
            _services = services;
            _logger = logger;
        }

        public int Order => 10;

        public async Task RunAsync(CancellationToken cancellationToken = default)
        {
            var refresher = _services.GetService<IDevelopPlanIndexer>();
            if(refresher != null)
            {
                _logger.LogInformation("Found develop plan refresher");
                var its = _services.GetRequiredService<IIterationService>();
                its.OnIterationStart(e =>
                {
                    try
                    {
                        var d_plan = _services.GetRequiredService<IDevelopPlanService>();
                        var plan = refresher.RefreshPlan(d_plan.Plan);
                        if (plan != null)
                            d_plan.SetPlan(plan);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                    }
                });
            }

            var options = _services.GetService<DynamicDiscoveryOptions>();
            if (options != null)
            {
                var its = _services.GetRequiredService<IIterationService>();
                its.OnIterationStart(async void (e) =>
                {
                    try
                    {
                        await BuildDynamic();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message, ex);
                    }
                });
                await BuildDynamic();
            }
        }

        private async Task BuildDynamic()
        {
            try
            {
                var options = _services.GetService<DynamicDiscoveryOptions>();
                if(options?.Initialization == null)return;
                var planStore = _services.GetRequiredService<IStore<DynamicDevelopPlan>>();
                var d_plan = _services.GetRequiredService<IDevelopPlanService>();
                var targetPlan = await planStore.FindAsync(x => x.Id == d_plan.Plan.Id);
                if (targetPlan == null || targetPlan.Folders.Count == 0)
                {
                    options.Initialization.Invoke(d_plan.Plan);
                    return;
                }
                d_plan.Plan.AllowedFileTypes = targetPlan.AllowedFileTypes;
                d_plan.Plan.ReadOnlyFileTypes = targetPlan.ReadOnlyFileTypes;
                foreach(var fsType in d_plan.Plan.AllowedFileTypes)
                    d_plan.Plan.IndexFiles(fsType, $"root_{fsType}_");
                foreach(var fsType in d_plan.Plan.ReadOnlyFileTypes)
                    d_plan.Plan.IndexFiles(fsType, $"root_{fsType}_",true);
                foreach (var item in targetPlan.Folders)
                {
                    TraverseDynamics(d_plan, item);
                }
            }catch(Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }
        }

        private void TraverseDynamics(IDevelopPlanService plan, DynamicDevelopFolder dynamicFolder)
        {
            var folder = plan.Plan.GetOrCreateFolder(dynamicFolder.Id, dynamicFolder.Name);
            folder.AllowedFileTypes = dynamicFolder.AllowedFileTypes;
            folder.ReadOnlyFileTypes = dynamicFolder.ReadOnlyFileTypes;
            foreach(var fsType in folder.AllowedFileTypes)
                plan.Plan.IndexFiles(folder,fsType, $"{folder.Id}_{fsType}_");
            foreach(var fsType in folder.ReadOnlyFileTypes)
                plan.Plan.IndexFiles(folder,fsType, $"{folder.Id}_{fsType}_", true);
            foreach (var item in dynamicFolder.Children)
                TraverseDynamics(plan, item, folder);
        }
        
        private void TraverseDynamics(IDevelopPlanService plan, DynamicDevelopFolder dynamicFolder, DevelopFolder parent)
        {
            var folder = plan.Plan.GetOrCreateFolder(parent, dynamicFolder.Id, dynamicFolder.Name);
            folder.AllowedFileTypes = dynamicFolder.AllowedFileTypes;
            folder.ReadOnlyFileTypes = dynamicFolder.ReadOnlyFileTypes;
            foreach(var fsType in folder.AllowedFileTypes)
                plan.Plan.IndexFiles(folder,fsType, $"{folder.Id}_{fsType}_");
            foreach(var fsType in folder.ReadOnlyFileTypes)
                plan.Plan.IndexFiles(folder,fsType, $"{folder.Id}_{fsType}_", true);
            foreach (var item in dynamicFolder.Children)
                TraverseDynamics(plan, item, folder);
        }
    }
}
