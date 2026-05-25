using System.Text.Json.Serialization;

namespace Cyrena.Models
{
    /// <summary>
    /// Controls how chat history is sent to the AI
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum HistoryInclusionMode
    {
        /// <summary>
        /// Includes entire history
        /// </summary>
        All,
        /// <summary>
        /// Includes only last 2 iterations
        /// </summary>
        LastTwo,
        /// <summary>
        /// Includes only last 10 iterations
        /// </summary>
        LastTen,
        /// <summary>
        /// Includes no history, instruct mode
        /// </summary>
        Instruct
    }
}
