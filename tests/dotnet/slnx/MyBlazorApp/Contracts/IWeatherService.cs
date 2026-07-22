using MyBlazorApp.Models;

namespace MyBlazorApp.Contracts
{
    /// <summary>
    /// Provides weather forecast data.
    /// </summary>
    public interface IWeatherService
    {
        /// <summary>
        /// Retrieves a collection of weather forecasts asynchronously.
        /// </summary>
        /// <param name="days">The number of forecast days to generate.</param>
        /// <param name="delayMilliseconds">Optional simulated network delay in milliseconds.</param>
        /// <returns>An array of <see cref="WeatherForecast"/> instances.</returns>
        Task<WeatherForecast[]> GetForecastsAsync(int days = 5, int delayMilliseconds = 500);
    }
}
