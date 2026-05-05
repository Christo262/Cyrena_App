using Cyrena.Extensa.Models;
using Cyrena.Extensions;
using Cyrena.Options;
using Cyrena.Persistence.Options;
using Cyrena.ToDoList.Contracts;
using Cyrena.ToDoList.Models;
using Cyrena.ToDoList.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Cyrena.ToDoList
{
    public class ToDoListExtension : Extension
    {
        public override void BuildExtension(CyrenaBuilder builder)
        {
            var persistence = builder.GetFeatureOption<ICyrenaPersistenceBuilder>();
            persistence.AddSingletonStore<TaskItem>("todo_list");

            builder.Services.AddSingleton<ITaskItemService, TaskItemService>();

            builder.AddFeatureAssembly<ToDoListExtension>("blazor"); //Use to map the @page pages in routing
            builder.AddShortcut<TaskItemsShortcut>();
            builder.AddAssistantPlugin<ToDoAssistantPlugin>();
        }
    }
}
