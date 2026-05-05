using Cyrena.Contracts;
using Microsoft.AspNetCore.Components;

namespace Cyrena.ToDoList.Services
{
    internal class TaskItemsShortcut : IShortcut
    {
        private readonly NavigationManager _nav;
        public TaskItemsShortcut(NavigationManager nav)
        {
            _nav = nav;
        }

        public string Title => "To Do";

        public string Description => "View your To-Do list. You can also ask Cyréna to help manage these tasks.";

        public string Icon => "bi bi-check-square";

        public string Color => "primary";

        public string Category => "Productivity";

        public string[] Tags => ["To Do", "Productivity"];

        public Task OnClick()
        {
            _nav.NavigateTo("samples/to-do-list");
            return Task.CompletedTask;
        }
    }
}
