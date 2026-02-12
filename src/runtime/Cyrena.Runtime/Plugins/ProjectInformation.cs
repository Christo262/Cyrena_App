using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class ProjectInformation
    {
        private readonly IDeveloperContext _context;
        private readonly IStore<Note> _notes;
        public ProjectInformation(IDeveloperContext context, IStore<Note> notes)
        {
            _context = context;
            _notes = notes;
        }

        [KernelFunction("get_plan")]
        [Description("Gets all folders and files in the project and any notes about the project.")]
        public async Task<ProjectPlanWithNotes> GetProjectPlan()
        {
            _context.LogInfo("Reading project plan");
            var plan = new ProjectPlanWithNotes(_context.ProjectPlan);
            var notes = await _notes.FindManyAsync(x => x.ProjectId == _context.Project.Id);
            plan.Notes = notes.Select(x => new NoteRecord(x.Id, x.Subject, x.Content));
            return plan;
        }
    }
}
