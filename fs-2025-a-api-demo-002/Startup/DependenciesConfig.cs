using fs_2025_a_api_demo_002.Data;

namespace fs_2025_a_api_demo_002.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            // Use singletons for data loaders
            builder.Services.AddSingleton<CourseData>();

            builder.Services.AddSingleton<BikeData>();  // 👈 register your bikes here
        }
    }
}
