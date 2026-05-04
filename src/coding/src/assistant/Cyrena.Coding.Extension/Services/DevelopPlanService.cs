using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Models;

namespace Cyrena.Coding.Services
{
    internal class DevelopPlanService : IDevelopPlanService
    {
        private readonly DevelopPlanEventPipeline _pipe;
        public DevelopPlanService(DevelopPlan plan)
        {
            _plan = plan;
            _pipe = new DevelopPlanEventPipeline();
        }

        private DevelopPlan _plan { get; set; }

        public DevelopPlan Plan => _plan;

        public void SetPlan(DevelopPlan newPlan)
        {
            _plan = newPlan;
            _pipe.InvokeDevelopPlanChanged(newPlan);
        }

        public void InvokeFileCreated(DevelopFile file) => _pipe.InvokeFileCreated(file);
        public void InvokeFileUpdated(DevelopFile file) => _pipe.InvokeFileUpdated(file);
        public void InvokeFileDeleted(DevelopFile file) => _pipe.InvokeFileDeleted(file);

        public IDisposable OnDevelopPlanChanged(Action<DevelopPlan> plan) => _pipe.OnDevelopPlanChanged(plan);
        public IDisposable OnFileCreated(Action<DevelopFile> cb) => _pipe.OnFileCreated(cb);
        public IDisposable OnFileUpdated(Action<DevelopFile> cb) => _pipe.OnFileUpdated(cb);
        public IDisposable OnFileDeleted(Action<DevelopFile> cb) => _pipe.OnFileDeleted(cb);
    }

    internal class DevelopPlanEventPipeline : EventPipeline
    {
        public IDisposable OnDevelopPlanChanged(Action<DevelopPlan> cb) => this.ConfigurePipe("plan_change", cb);
        public IDisposable OnFileCreated(Action<DevelopFile> cb) => this.ConfigurePipe("file_created", cb);
        public IDisposable OnFileUpdated(Action<DevelopFile> cb) => this.ConfigurePipe("file_updated", cb);
        public IDisposable OnFileDeleted(Action<DevelopFile> cb) => this.ConfigurePipe("file_deleted", cb);

        public void InvokeDevelopPlanChanged(DevelopPlan plan) => this.InvokePipeline("plan_change", plan);
        public void InvokeFileCreated(DevelopFile file) => this.InvokePipeline("file_created", file);
        public void InvokeFileUpdated(DevelopFile file) => this.InvokePipeline("file_updated", file);
        public void InvokeFileDeleted(DevelopFile file) => this.InvokePipeline("file_deleted", file);
    }
}
