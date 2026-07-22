using Cyrena.Coding.Contracts;
using Cyrena.Contracts;

namespace Cyrena.Coding.Services;

internal class DynamicDevelopPlanWatcher : IStartupTask
{
    private readonly IDynamicPlanInitializer _initializer;
    private readonly IIterationService _its;
    public DynamicDevelopPlanWatcher(IDynamicPlanInitializer initializer, IIterationService its)
    {
        _initializer = initializer;
        _its = its;
    }


    public Task RunAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        _its.OnIterationStart((e) =>
        {
            _initializer.RunIndex();
        });
        _initializer.Initialize();
        return Task.CompletedTask;
    }

    public int Order => 10;
}