using fs_2025_a_api_demo_002.Data;

namespace fs_2025_a_api_demo_002.Startup
{
    public static class DependenciesConfig
    {
        public static void AddDependencies(this WebApplicationBuilder builder)
        {
            builder.Services.AddTransient<CourseData>();
            builder.Services.AddMemoryCache();
        }
    }
}
