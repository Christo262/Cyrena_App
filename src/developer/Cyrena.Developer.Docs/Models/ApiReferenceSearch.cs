using Cyrena.Models;
using Newtonsoft.Json;

namespace Cyrena.Developer.Docs.Models
{
    public class ApiReferenceSearch : IJsonSerializable
    {
        public ApiReferenceSearch(string id, string? title, string? description)
        {
            Id = id;
            Title = title;
            Description = description;
        }

        public string Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public int Score { get; set; }

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
