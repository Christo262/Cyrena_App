using Cyrena.Contracts;
using Cyrena.Extensions;
using Cyrena.Models;
using Cyrena.ToDoList.Components.Shared;
using Cyrena.ToDoList.Contracts;
using Cyrena.ToDoList.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.ToDoList.Services
{
    internal class TaskItemsKernelFunctions
    {
        private readonly ITaskItemService _service;
        private readonly IChatMessageService _chat;
        public TaskItemsKernelFunctions(ITaskItemService service, IChatMessageService chat)
        {
            _service = service;
            _chat = chat;
        }

        [KernelFunction("list")]
        [Description("Lists the user's to-do tasks for a specific date. Returns tasks sorted with incomplete items first, then by title. Use this to see what the user needs to do today or on any chosen day.")]
        public async Task<ToolResult<IEnumerable<TaskItemViewModel>>> ListTaskItemsAsync(
            [Description("The date to filter tasks by. Leave empty to retrieve tasks for today. Provide in yyyy/MM/dd format, e.g. 2026/05/02.")]string? date = null, 
            [Description("Filter tasks by completion status. Leave empty to retrieve both complete and incomplete tasks. Set to 'true' for only completed tasks, or 'false' for only incomplete tasks.")]string? isComplete = null)
        {
            try
            {
                DateTime dt;
                if (string.IsNullOrEmpty(date))
                    dt = DateTime.Today;
                else
                    dt = Convert.ToDateTime(date);
                if(isComplete == null)
                {
                    var items = await _service.GetByDateAsync(dt);
                    return new ToolResult<IEnumerable<TaskItemViewModel>>(TaskItemViewModel.Convert(items));
                }
                var itms = await _service.GetByDateAsync(dt, Convert.ToBoolean(isComplete));
                return new ToolResult<IEnumerable<TaskItemViewModel>>(TaskItemViewModel.Convert(itms));
            }
            catch (Exception ex)
            {
                return new ToolResult<IEnumerable<TaskItemViewModel>>(false, ex.Message);
            }
        }

        [KernelFunction("create")]
        [Description("Creates a new to-do task with a title, optional description, and a target date. Use this when the user wants to add something to their to-do list.")]
        public async Task<ToolResult<TaskItemViewModel>> CreateTaskItemAsync(
            [Description("The title or name of the task. Required.")]string title,
            [Description("An optional longer description or notes for the task.")]string? description = null,
            [Description("The date the task is scheduled for. Leave empty to schedule for today. Provide in yyyy/MM/dd format, e.g. 2026/05/02.")]string? date = null)
        {
            try
            {
                DateTime dt = string.IsNullOrEmpty(date) ? DateTime.Today : Convert.ToDateTime(date);
                await _chat.LogInfo($"Creating to-do task for {dt.Date}");
                var item = await _service.CreateAsync(title, description, dt);
                return new ToolResult<TaskItemViewModel>(TaskItemViewModel.Convert(item));
            }
            catch (Exception ex)
            {
                return new ToolResult<TaskItemViewModel>(false, ex.Message);
            }
        }

        [KernelFunction("update")]
        [Description("Updates an existing to-do task by its ID. You can change the title, description, date, or completion status. Use this when the user wants to edit or reschedule a task.")]
        public async Task<ToolResult<TaskItemViewModel>> UpdateTaskItemAsync(
            [Description("The unique ID of the task to update. Required.")]string id,
            [Description("The new title for the task. Leave empty to keep the current title.")]string? title = null,
            [Description("The new description for the task. Leave empty to keep the current description.")]string? description = null,
            [Description("The new date for the task. Leave empty to keep the current date. Provide in yyyy/MM/dd format, e.g. 2026/05/02.")]string? date = null,
            [Description("Set to 'true' to mark the task as complete, or 'false' to mark it as incomplete. Leave empty to keep the current completion status.")]string? isComplete = null)
        {
            try
            {
                var item = await _service.GetAsync(id);
                if (item == null)
                    return new ToolResult<TaskItemViewModel>(false, $"Task with ID '{id}' was not found.");

                if (!string.IsNullOrEmpty(title))
                    item.Title = title;
                if (description != null)
                    item.Description = description;
                if (!string.IsNullOrEmpty(date))
                    item.Date = Convert.ToDateTime(date);
                if (!string.IsNullOrEmpty(isComplete))
                {
                    try
                    {
                        item.IsComplete = Convert.ToBoolean(isComplete);
                    }
                    catch { }
                }

                await _chat.LogInfo($"Updating to-do task: {title}");

                var updated = await _service.UpdateAsync(item);
                return new ToolResult<TaskItemViewModel>(TaskItemViewModel.Convert(updated));
            }
            catch (Exception ex)
            {
                return new ToolResult<TaskItemViewModel>(false, ex.Message);
            }
        }

        [KernelFunction("delete")]
        [Description("Deletes a to-do task by its ID. Use this when the user wants to remove a task from their list permanently.")]
        public async Task<ToolResult> DeleteTaskItemAsync(
            [Description("The unique ID of the task to delete. Required.")]string id)
        {
            try
            {
                await _chat.LogInfo("Deleting to-do task");
                await _service.DeleteAsync(id);
                return new ToolResult(true, "Task deleted");
            }
            catch (Exception ex)
            {
                return new ToolResult(false, ex.Message);
            }
        }

        //[KernelFunction("show")]
        //[Description("Shows a dialog with the user's To Do list for a specific date.")]
        //public async Task<ToolResult> ShowAsync(
        //    [Description("The date to show. Leave empty to show today's date. Provide in yyyy/MM/dd format, e.g. 2026/05/02.")] string? date = null)
        //{
        //    try
        //    {
        //        DateTime dt;
        //        if (!string.IsNullOrEmpty(date))
        //            dt = Convert.ToDateTime(date);
        //        else
        //            dt = DateTime.Today;
        //        _ = _display.ShowModal<TaskItemsModal>(new BootstrapBlazor.Components.ResultDialogOption()
        //        {
        //            Title = "To-Do List",
        //            Size = BootstrapBlazor.Components.Size.Medium,
        //            ShowYesButton = false,
        //            ButtonNoText = "Close"
        //        });
        //        return new ToolResult(true, "Success, user can see their to-do list");
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ToolResult(false, ex.Message);
        //    }
        //}
    }
}
