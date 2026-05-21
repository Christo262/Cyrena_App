using Cyrena.Models;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.ToDoList.Models
{
    public class TaskItem : Entity
    {
        public DateTime Date { get; set; }
        public bool IsComplete { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }
    }
}
