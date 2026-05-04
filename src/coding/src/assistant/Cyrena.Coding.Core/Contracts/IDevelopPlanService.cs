using Cyrena.Coding.Models;

namespace Cyrena.Coding.Contracts
{
    /// <summary>
    /// Used to access the current <see cref="DevelopPlan"/>. Allows changing in case of project referencing
    /// </summary>
    public interface IDevelopPlanService
    {
        DevelopPlan Plan { get; }
        void SetPlan(DevelopPlan newPlan);

        IDisposable OnDevelopPlanChanged(Action<DevelopPlan> plan);
        IDisposable OnFileCreated(Action<DevelopFile> cb);
        IDisposable OnFileUpdated(Action<DevelopFile> cb);
        IDisposable OnFileDeleted(Action<DevelopFile> cb);

        void InvokeFileCreated(DevelopFile file);
        void InvokeFileUpdated(DevelopFile file);
        void InvokeFileDeleted(DevelopFile file);
    }
}
