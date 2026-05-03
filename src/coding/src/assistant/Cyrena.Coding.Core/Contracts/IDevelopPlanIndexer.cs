using Cyrena.Coding.Models;

namespace Cyrena.Coding.Contracts
{
    /// <summary>
    /// Allows the <see cref="Cyrena.Coding.Models.DevelopPlan"/> to be refreshed in <see cref="IDevelopPlanService"/> when <see cref="Cyrena.Contracts.IIterationService.OnIterationStart(Action{bool})"/> is triggered.
    /// Kernel Locked.
    /// </summary>
    public interface IDevelopPlanIndexer
    {
        DevelopPlan? RefreshPlan(DevelopPlan current);
    }
}
