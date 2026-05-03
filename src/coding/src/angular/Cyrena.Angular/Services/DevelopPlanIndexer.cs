using Cyrena.Angular.Extensions;
using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;

namespace Cyrena.Angular.Services
{
    internal class DevelopPlanIndexer : IDevelopPlanIndexer
    {
        public DevelopPlan? RefreshPlan(DevelopPlan current)
        {
            current.IndexAngularDefaultPlan();
            return current;
        }
    }
}
