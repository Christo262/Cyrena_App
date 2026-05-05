using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.ToDoList.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;

namespace Cyrena.ToDoList.Services
{
    internal class ToDoAssistantPlugin : IAssistantPlugin
    {
        private readonly ITaskItemService _tasks;
        public ToDoAssistantPlugin(ITaskItemService tasks)
        {
            _tasks = tasks;
        }

        public string Id => "cyrena.samples.todo";
        public string[] Modes => [IAssistantMode.AssistantModeDefault];
        public int Priority => 10;
        public bool Required => false;
        public string Title => "To-Do List";

        public Task LoadAsync(CyrenaKernelBuilder builder)
        {
            builder.Services.AddSingleton(_tasks);
            builder.Plugins.AddFromType<TaskItemsKernelFunctions>("ToDo");
            builder.GetFeatureOption<IPromptManager>().AddPrompt(10, Resources.Read(typeof(ToDoAssistantPlugin).Assembly, "Cyrena.ToDoList.Resources.prompt.md"));
            return Task.CompletedTask;
        }
    }
}
