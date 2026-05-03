using Cyrena.Coding.Contracts;
using Cyrena.Coding.Extensions;
using Cyrena.Coding.Models;

namespace Cyrena.ArduinoIDE.Services
{
    internal class DevelopPlanIndexer : IDevelopPlanIndexer
    {
        public DevelopPlan? RefreshPlan(DevelopPlan current)
        {
            current.IndexFiles("ino", "ino_");
            current.IndexFiles("h", "h_");
            current.IndexFiles("cpp", "cpp_");
            return current;
        }
    }
}
