using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;

namespace Cyrena.Developer.Services
{
    internal class DevelopPlanService : IDevelopPlanService
    {
        public DevelopPlanService(DevelopPlan plan)
        {
            _plan = plan;
        }

        private DevelopPlan _plan { get; set; }

        public DevelopPlan Plan => _plan;

        public void SetPlan(DevelopPlan newPlan)
        {
            _plan = newPlan;
        }
    }
}
