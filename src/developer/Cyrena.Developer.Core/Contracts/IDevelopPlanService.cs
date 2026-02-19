using Cyrena.Developer.Models;

namespace Cyrena.Developer.Contracts
{
    /// <summary>
    /// Used to access the current <see cref="DevelopPlan"/>. Allows changing in case of project referencing
    /// </summary>
    public interface IDevelopPlanService
    {
        DevelopPlan Plan { get; }
        void SetPlan(DevelopPlan newPlan);
    }
}
