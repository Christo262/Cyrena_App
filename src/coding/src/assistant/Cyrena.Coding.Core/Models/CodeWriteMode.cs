using System.Text.Json.Serialization;

namespace Cyrena.Coding.Models
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CodeWriteMode
    {
        /// <summary>Inserts content at startLine without removing any existing lines.</summary>
        Insert,
        /// <summary>Removes lineCount lines at startLine, then inserts content at that position.</summary>
        Replace,
        /// <summary>Replaces the entire file with content. startLine and lineCount are ignored.</summary>
        Overwrite
    }
}
