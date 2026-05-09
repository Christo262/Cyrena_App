using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.Website.Extensions;

namespace Cyrena.Website.Services
{
    internal class DevelopPlanIndexer : IDevelopPlanIndexer
    {
        public DevelopPlan? RefreshPlan(DevelopPlan current)
        {
            current.IndexStaticWebsiteDefaultPlan();
            return current;
        }
    }
}
