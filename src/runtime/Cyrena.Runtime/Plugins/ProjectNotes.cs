using Microsoft.SemanticKernel;
using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.Persistence.Contracts;
using Cyrena.Runtime.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Runtime.Plugins
{
    internal class ProjectNotes
    {
        private readonly IStore<Note> _store;
        private readonly IDeveloperContext _context;
        public ProjectNotes(IDeveloperContext context, IStore<Note> store)
        {
            _context = context;
            _store = store;
        }

        [KernelFunction("list_subjects")]
        [Description("Lists all long-term project note subjects. Notes store durable rules, decisions, and conventions that persist across AI tasks.")]
        public async Task<ToolResult<NoteSubject[]>> ListAllSubjectsOfProjectNotes()
        {
            var items = await _store.FindManyAsync(x => x.ProjectId == _context.Project.Id);
            var subs = items.Select(x => new NoteSubject(x.Id, x.Subject));
            return new ToolResult<NoteSubject[]>(subs.ToArray());
        }

        [KernelFunction("read")]
        [Description("Reads a single project note by id. Notes contain persistent knowledge such as architecture rules or conventions.")]
        public async Task<ToolResult<Note>> ReadProjectNote([Description("The unique identifier of the note to read.")] string id)
        {
            var note = await _store.FindAsync(x => x.Id == id);
            if (note == null)
                return new ToolResult<Note>(false, $"Unable to find note with id {id}");
            return new ToolResult<Note>(note);
        }

        [KernelFunction("create")]
        [Description("Creates a new long-term project note. Only store durable facts, rules, or decisions that should persist across future tasks.")]
        public async Task<ToolResult<NoteSubject>> CreateProjectNote(
            [Description("A short stable subject that identifies the memory category. Subjects act as keys and should describe lasting project concepts.")] string subject,
            [Description("The long-term memory content. Write concise, timeless facts or rules. Avoid logs or temporary details.")] string? note)
        {
            _context.LogInfo($"Creating note: {subject}");
            var model = new Note()
            {
                ProjectId = _context.Project.Id,
                Subject = subject,
                Content = note,
                Id = Guid.NewGuid().ToString(),
            };
            await _store.AddAsync(model);
            return new ToolResult<NoteSubject>(new NoteSubject(model.Id, model.Subject));
        }

        [KernelFunction("delete")]
        [Description("Deletes a project note. Use when stored memory is outdated or no longer valid.")]
        public async Task<ToolResult> DeleteProjectNote([Description("The unique identifier of the note to delete.")] string id)
        {
            var note = await _store.FindAsync(x => x.Id == id);
            if(note != null)
            {
                _context.LogInfo($"Deleting note: {note.Subject}");
                await _store.DeleteAsync(note);
            }
            return new ToolResult(true, "Note deleted.");
        }

        [KernelFunction("update")]
        [Description("Updates an existing project note. Use this to refine or replace long-term memory rather than creating duplicate notes.")]
        public async Task<ToolResult<Note>> UpdateProjectNote(
            [Description("The unique identifier of the note to update.")] string id,
            [Description("Optional new subject. Provide only when renaming the memory category.")] string? subject,
            [Description("Replacement long-term memory content. Must remain concise and timeless.")] string? note)
        {
            var model = await _store.FindAsync(x => x.Id == id);
            if (model == null)
                return new ToolResult<Note>(false, $"Note {id} does not exist");
            if(!string.IsNullOrEmpty(subject))
                model.Subject = subject;
            _context.LogInfo($"Updating note: {model.Subject}");
            model.Content = note;
            await _store.UpdateAsync(model);
            return new ToolResult<Note>(model);
        }
    }
}
