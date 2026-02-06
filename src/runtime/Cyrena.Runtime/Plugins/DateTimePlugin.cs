using Cyrena.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Runtime.Plugins
{
    internal class DateTimePlugin
    {
        public class AiDate : JsonStringObject
        {
            public AiDate() { }
            public AiDate(int year, int month, int day)
            {
                Year = year;
                Month = month;
                Day = day;
            }
            public int Year { get; set; }
            public int Month { get; set; }
            public int Day { get; set; }
        }

        public class AiTime : JsonStringObject
        {
            public AiTime() { }
            public AiTime(int hour, int minute, int second)
            {
                Hour = hour;
                Minute = minute;
                Second = second;
            }
            public int Hour { get; set; }
            public int Minute { get; set; }
            public int Second { get; set; }
        }

        public class AiDateTime : JsonStringObject
        {
            public AiDate Date { get; set; } = new AiDate();
            public AiTime Time { get; set; } = new AiTime();
        }

        [KernelFunction]
        [Description("Gets the current date.")]
        public AiDate GetCurrentDate()
        {
            var dt = DateTime.Now;
            return new AiDate(dt.Year, dt.Month, dt.Day);
        }

        [KernelFunction]
        [Description("Gets the current date and time.")]
        public AiDateTime GetCurrentDateAndTime()
        {
            var dt = DateTime.Now;
            var date = new AiDate(dt.Year, dt.Month, dt.Day);
            var time = new AiTime(dt.Hour, dt.Minute, dt.Second);
            return new AiDateTime() { Date = date, Time = time };
        }

        [KernelFunction]
        [Description("Gets the current time.")]
        public AiTime GetCurrentTime()
        {
            var dt = DateTime.Now;
            var time = new AiTime(dt.Hour, dt.Minute, dt.Second);
            return time;
        }
    }
}
