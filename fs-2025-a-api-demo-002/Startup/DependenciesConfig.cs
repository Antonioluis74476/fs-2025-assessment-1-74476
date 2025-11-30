using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Services;

namespace fs_2025_a_api_demo_002.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            // Use singletons for data loaders
            builder.Services.AddSingleton<CourseData>();

            builder.Services.AddSingleton<BikeData>();

            // NEW: service that contains all the bike query logic
            builder.Services.AddSingleton<BikeQueryService>();

            // NEW: background updater
            builder.Services.AddHostedService<BikeUpdateBackgroundService>();
        }
    }
}
