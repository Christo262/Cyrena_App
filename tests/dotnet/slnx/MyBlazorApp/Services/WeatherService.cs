using MyBlazorApp.Contracts;
using MyBlazorApp.Models;

namespace MyBlazorApp.Services
{
    /// <summary>
    /// Generates simulated weather forecast data.
    /// </summary>
    public class WeatherService : IWeatherService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild",
            "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        /// <inheritdoc/>
        public async Task<WeatherForecast[]> GetForecastsAsync(int days = 5, int delayMilliseconds = 500)
        {
            // Simulate asynchronous loading to demonstrate a loading indicator
            await Task.Delay(delayMilliseconds);

            var startDate = DateOnly.FromDateTime(DateTime.Now);

            return Enumerable.Range(1, days).Select(index => new WeatherForecast
            {
                Date = startDate.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();
        }
    }
}
