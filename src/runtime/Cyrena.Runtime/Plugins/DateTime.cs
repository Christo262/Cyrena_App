using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class DateTime
    {
        [KernelFunction("now")]
        [Description("Gets the current date, time & time zone offset.")]
        public TextContent DateTimeNow()
        {
            string formatted = System.DateTime.Now.ToString("dddd, dd MMMM yyyy, HH:mm:ss");
            var tz = TimeZoneInfo.Local;
            return $"{formatted} {tz.BaseUtcOffset}";
        }
    }
}