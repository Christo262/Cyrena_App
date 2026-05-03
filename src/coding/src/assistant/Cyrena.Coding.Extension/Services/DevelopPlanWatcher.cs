using Cyrena.Coding.Contracts;
using Cyrena.Contracts;
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

        public Task RunAsync(CancellationToken cancellationToken = default)
        {
            var refresher = _services.GetService<IDevelopPlanIndexer>();
            if(refresher != null)
            {
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
            return Task.CompletedTask;
        }
    }
}
