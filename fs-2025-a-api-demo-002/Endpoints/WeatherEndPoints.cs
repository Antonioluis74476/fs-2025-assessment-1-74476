using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Endpoints
{
    public static class WeatherEndPoints
    {
        static string[] summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};
        public static void AddWeatherEndPoints(this WebApplication app)
        {
            app.MapGet("/weatherforecast", () =>
            {
                var forecast = Enumerable.Range(1, 5).Select(index =>
                    new WeatherForecast
                    (
                        DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                        Random.Shared.Next(-20, 55),
                        summaries[Random.Shared.Next(summaries.Length)]
                    ))
                    .ToArray();
                return forecast;
            })
.WithName("GetWeatherForecast")
.WithOpenApi();
        }
    }
}
