using Cyrena.Models;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Cyrena.ToDoList.Models
{
    public class TaskItem : Entity, IJsonSerializable
    {
        public DateTime Date { get; set; }
        public bool IsComplete { get; set; }
        [Required]
        public string? Title { get; set; }
        public string? Description { get; set; }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public override string ToString()
        {
            return ToJson();
        }
    }
}
