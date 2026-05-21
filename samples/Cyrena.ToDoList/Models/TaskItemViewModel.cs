using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.ToDoList.Models
{
    public class TaskItemViewModel : Entity
    {
        public DateTime Date { get; set; }
        public string? IsComplete { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }

        public static TaskItemViewModel Convert(TaskItem item)
        {
            return new TaskItemViewModel()
            {
                Date = item.Date,
                Id = item.Id,
                IsComplete = item.IsComplete.ToString(),
                Title = item.Title,
                Description = item.Description,
            };
        }

        public static IEnumerable<TaskItemViewModel> Convert(IEnumerable<TaskItem> items)
        {
            return items.Select(Convert);
        }
    }
}
