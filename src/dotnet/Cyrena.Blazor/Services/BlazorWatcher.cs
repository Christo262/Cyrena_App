using Cyrena.Contracts;

namespace Cyrena.Blazor.Services
{
    internal class BlazorWatcher
    {
        private readonly IDeveloperContext _context;
        public BlazorWatcher(IDeveloperContext context)
        {
            _context = context;
        }
    }
}
