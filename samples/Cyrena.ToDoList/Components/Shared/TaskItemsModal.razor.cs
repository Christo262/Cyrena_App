using BootstrapBlazor.Components;
using Cyrena.ToDoList.Contracts;
using Cyrena.ToDoList.Models;
using Microsoft.AspNetCore.Components;

namespace Cyrena.ToDoList.Components.Shared
{
    public partial class TaskItemsModal : IResultDialog
    {
        [Inject] private ITaskItemService _tasks { get; set; } = default!;
        [Parameter]
        public DateTime? Date { get; set; }
        public Task OnClose(DialogResult result)
        {
            return Task.CompletedTask;
        }

        private IEnumerable<TaskItem> _models = Enumerable.Empty<TaskItem>();

        protected override async Task OnInitializedAsync()
        {
            if (Date == null)
                Date = DateTime.Today;
            _models = await _tasks.GetByDateAsync(Date.Value);
        }

        private async Task ToggleComplete(TaskItem task)
        {
            await _tasks.ToggleCompleteAsync(task.Id);
            await LoadTasks();
        }

        private async Task DeleteTask(TaskItem task)
        {
            await _tasks.DeleteAsync(task.Id);
            await LoadTasks();
        }

        private async Task LoadTasks()
        {
            _models = await _tasks.GetByDateAsync(Date!.Value);
        }
    }
}
