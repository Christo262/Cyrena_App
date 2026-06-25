using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Dotnet.Contracts;
using Cyrena.VisualStudio.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.VisualStudio.Services;

internal class DynamicPlanInitializer(IEnumerable<IProjHandler> handlers, ISolutionController sln, IDevelopPlanService planService) : IDynamicPlanInitializer
{
    
    public void Initialize()
    {
        var plans = sln.GetValidProjects();
        foreach (var plan in plans)
        {
            var filter = Path.GetExtension(plan.Id).TrimStart(".").ToString();
            var handler = handlers.FirstOrDefault(x => string.Equals(x.Filter, filter, StringComparison.OrdinalIgnoreCase));
            if(handler == null)
                continue;
            plan.Plan ??= new DevelopPlan(plan.ProjectDirectory, plan.Id);
            handler.Initialize(plan.Plan);
        }

        planService.SetPlan(sln.Current.Plan!);
    }

    public void RunIndex()
    {
        var plan = sln.Current;
        var filter = Path.GetExtension(plan.Id).TrimStart(".").ToString();
        var handler = handlers.FirstOrDefault(x => string.Equals(x.Filter, filter, StringComparison.OrdinalIgnoreCase));
        if(handler == null)
            return;
        plan.Plan ??= new DevelopPlan(plan.ProjectDirectory, plan.Id);
        handler.Initialize(plan.Plan);
        planService.SetPlan(sln.Current.Plan!);
    }
}