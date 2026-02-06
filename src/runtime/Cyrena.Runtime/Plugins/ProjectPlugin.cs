using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class ProjectPlugin
    {
        private readonly IDeveloperContext _context;
        private readonly IStore<Note> _notes;
        public ProjectPlugin(IDeveloperContext context, IStore<Note> notes)
        {
            _context = context;
            _notes = notes;
        }

        [KernelFunction]
        [Description("Gets all folders and files in the project and any notes about the project.")]
        public async Task<ProjectPlanWithNotes> GetProjectPlan()
        {
            _context.LogInfo("Reading project plan");
            var plan = new ProjectPlanWithNotes(_context.ProjectPlan);
            var notes = await _notes.FindManyAsync(x => true);
            plan.Notes = notes.Select(x => new NoteRecord(x.Id, x.Subject, x.Content));
            return plan;
        }
    }
}
