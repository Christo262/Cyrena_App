using Cyrena.Contracts;
using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Developer.Plugins
{
    internal class ProjectInformation
    {
        private readonly IDevelopPlanService _plan;
        private readonly IStore<StickyNote> _notes;
        private readonly IChatMessageService _context;
        public ProjectInformation(IDevelopPlanService plan, IStore<StickyNote> notes, IChatMessageService context)
        {
            _plan = plan;
            _notes = notes;
            _context = context;
        }

        [KernelFunction("get_plan")]
        [Description("Gets all folders and files in the project and any sticky notes about the project.")]
        public async Task<DevelopPlanAIModel> GetProjectPlan()
        {
            await _context.LogInfo("Reading project plan");
            var plan = new DevelopPlanAIModel(_plan.Plan);
            var notes = await _notes.FindManyAsync(x => true);
            plan.Notes = notes.ToList();
            return plan;
        }

        [KernelFunction("create_note")]
        [Description("Creates a new sticky note for the project")]
        public async Task<ToolResult<string>> CreateNote(
            [Description("Title of the note.")]string title,
            [Description("The content of the note.")]string content)
        {
            var note = new StickyNote(title, content);
            await _notes.AddAsync(note);
            return new ToolResult<string>($"Note Create with id: {note.Id}");
        }

        [KernelFunction("update_note")]
        [Description("Updates an existing sticky note for the project.")]
        public async Task<ToolResult<string>> UpdateNote(
            [Description("The id of note to update, required.")]string id, 
            [Description("To set a new title for the note. Leave null or empty if unchanged.")]string? title,
            [Description("To set the new content for the note. Leave null or empty if unchanged.")] string? content)
        {
            var note = await _notes.FindAsync(x => x.Id == id);
            if (note == null)
                return new ToolResult<string>(false, $"Unable to find note with id {id}.");
            if(!string.IsNullOrEmpty(title))
                note.Title = title;
            if(!string.IsNullOrEmpty(content))
                note.Content = content;
            await _notes.UpdateAsync(note);
            return new ToolResult<string>($"Updated note with id {id}");
        }

        [KernelFunction("delete_note")]
        [Description("Deletes a sticky note that is obsolete or invalid.")]
        public async Task<ToolResult> DeleteNote(
            [Description("The id of note to delete, required.")] string id)
        {
            var note = await _notes.FindAsync(x => x.Id == id);
            if (note == null)
                return new ToolResult(true, $"Note with id {id} not found");
            await _notes.DeleteAsync(note);
            return new ToolResult(true, "Note deleted");
        }
    }
}
