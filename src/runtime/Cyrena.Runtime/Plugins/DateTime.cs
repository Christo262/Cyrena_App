using Cyrena.Models;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class DateTime
    {
        internal class AiDateTime : IJsonSerializable
        {
            public AiDateTime(int year,int month, int day, int hour, int minute, int second, int timeZone)
            {
                Year = year;
                Month = month;
                Day = day;
                Hour = hour;
                Minute = minute;
                Second = second;
                TimeZone = timeZone;
            }
            public int Year;
            public int Month;
            public int Day;

            public int Hour;
            public int Minute;
            public int Second;

            public int TimeZone;

            public string ToJson()
            {
                return JsonConvert.SerializeObject(this, Formatting.Indented);
            }

            public override string ToString()
            {
                return ToJson();
            }
        }

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