using Cyrena.Extensions;
using Cyrena.Persistence.Contracts;
using Cyrena.ToDoList.Contracts;
using Cyrena.ToDoList.Models;

namespace Cyrena.ToDoList.Services
{
    internal class TaskItemService : ITaskItemService
    {
        private readonly IStore<TaskItem> _store;

        public TaskItemService(IStore<TaskItem> store)
        {
            _store = store;
        }

        public async Task<TaskItem> CreateAsync(string title, string? description, DateTime date, CancellationToken ct = default)
        {
            var item = new TaskItem
            {
                Title = title,
                Description = description,
                Date = date.Date,
                IsComplete = false
            };

            await _store.AddAsync(item, ct);
            return item;
        }

        public async Task<TaskItem> UpdateAsync(TaskItem item, CancellationToken ct = default)
        {
            item.Date = item.Date.Date;
            await _store.UpdateAsync(item, ct);
            return item;
        }

        public async Task DeleteAsync(string id, CancellationToken ct = default)
        {
            var item = await GetAsync(id, ct);
            if (item is not null)
            {
                await _store.DeleteAsync(item, ct);
            }
        }

        public Task<TaskItem?> GetAsync(string id, CancellationToken ct = default)
        {
            return _store.FindAsync(x => x.Id == id, ct);
        }

        public Task<IEnumerable<TaskItem>> GetByDateAsync(DateTime date, CancellationToken ct = default)
        {
            var targetDate = date.Date;
            return _store.FindManyAsync(x => x.Date.Date == targetDate, ct:ct);
        }

        public Task<IEnumerable<TaskItem>> GetByDateAsync(DateTime date, bool isComplete, CancellationToken ct = default)
        {
            var targetDate = date.Date;
            return _store.FindManyAsync(x => x.Date.Date == targetDate && x.IsComplete == isComplete, ct: ct);
        }

        public async Task<TaskItem> ToggleCompleteAsync(string id, CancellationToken ct = default)
        {
            var item = await GetAsync(id, ct)
                ?? throw new InvalidOperationException($"Task item with id '{id}' not found.");

            item.IsComplete = !item.IsComplete;
            await _store.UpdateAsync(item, ct);
            return item;
        }
    }
}
