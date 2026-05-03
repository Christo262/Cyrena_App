using Cyrena.Coding.Contracts;
using Cyrena.Coding.Models;
using Cyrena.PlatformIO.Contracts;
using Cyrena.PlatformIO.Extensions;

namespace Cyrena.PlatformIO.Services
{
    internal class DevelopPlanIndexer : IDevelopPlanIndexer
    {
        private readonly IEnvironmentController _env;
        public DevelopPlanIndexer(IEnvironmentController env)
        {
            _env = env;
        }

        public DevelopPlan? RefreshPlan(DevelopPlan current)
        {
            current.IndexPlatformIODefaultPlan();
            if (_env.Current!.Framework?
                .Split(',', StringSplitOptions.TrimEntries)
                .Any(f => f.Equals("espidf", StringComparison.OrdinalIgnoreCase)) == true)
            {
                current.IndexPlatformIOEspIdf();
            }
            return current;
        }
    }
}
