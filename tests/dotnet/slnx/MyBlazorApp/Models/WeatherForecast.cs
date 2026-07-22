namespace MyBlazorApp.Models
{
    /// <summary>
    /// Represents a weather forecast for a specific date with temperature and summary.
    /// </summary>
    public class WeatherForecast
    {
        /// <summary>
        /// The date of the forecast.
        /// </summary>
        public DateOnly Date { get; set; }

        /// <summary>
        /// The temperature in Celsius.
        /// </summary>
        public int TemperatureC { get; set; }

        /// <summary>
        /// A brief textual summary of the weather (e.g., "Sunny", "Rainy").
        /// </summary>
        public string? Summary { get; set; }

        /// <summary>
        /// The temperature in Fahrenheit, calculated from <see cref="TemperatureC"/>.
        /// </summary>
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
