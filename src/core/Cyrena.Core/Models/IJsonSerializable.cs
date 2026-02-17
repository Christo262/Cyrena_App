using Newtonsoft.Json;

namespace Cyrena.Models
{
    /// <summary>
    /// Marks a object as serializable to JSON in a way its easier for a LLM to understand
    /// </summary>
    public interface IJsonSerializable
    {
        string ToJson();
    }

    [Obsolete]
    public abstract class JsonStringObject : IJsonSerializable
    {
        public override string ToString()
        {
            return ToJson();
        }
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }
}
