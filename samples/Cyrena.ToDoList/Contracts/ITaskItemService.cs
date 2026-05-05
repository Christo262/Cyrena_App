using Cyrena.ToDoList.Models;

namespace Cyrena.ToDoList.Contracts
{
    internal interface ITaskItemService
    {
        Task<TaskItem> CreateAsync(string title, string? description, DateTime date, CancellationToken ct = default);
        Task<TaskItem> UpdateAsync(TaskItem item, CancellationToken ct = default);
        Task DeleteAsync(string id, CancellationToken ct = default);
        Task<TaskItem?> GetAsync(string id, CancellationToken ct = default);
        Task<IEnumerable<TaskItem>> GetByDateAsync(DateTime date, CancellationToken ct = default);
        Task<IEnumerable<TaskItem>> GetByDateAsync(DateTime date, bool isComplete, CancellationToken ct = default);
        Task<TaskItem> ToggleCompleteAsync(string id, CancellationToken ct = default);
    }
}
