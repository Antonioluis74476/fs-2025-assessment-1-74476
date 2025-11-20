using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace fs_2025_a_api_demo_002.Endpoints
{
    public static class CourseEndPoints
    {
        public static void AddCourseEndPoints(this WebApplication app)
        {

            app.MapGet("/courses", LoadAllCoursesAsync);
            app.MapGet("/courses/{id:int}", LoadCourseById);

            var v1 = app.MapGroup("/api/v1/");
            v1.MapGet("courses", LoadAllCoursesAsync);
            v1.MapGet("/courses/{id:int}", LoadCourseById);

            var v2 = app.MapGroup("/api/v2/");
            v2.MapGet("courses", LoadAllCachedCoursesAsync);
            v2.MapGet("/courses/{id:int}", LoadCachedCourseById);
        }

     

        private static async Task<IResult> LoadCourseById(CourseData courseData, int id)
        {
            var output = courseData.Courses.FirstOrDefault(c => c.id == id);
            if (output is null)
            {
                return Results.NotFound();
            }   
            return Results.Ok(output);

        }

        //private static async Task<IResult> LoadCachedCourseById([FromServices] IMemoryCache _cache, [FromServices] CourseData courseData, int id)
        //{
        //    var output = courseData.Courses.FirstOrDefault(c => c.id == id);
        //    if (output is null)
        //    {
        //        return Results.NotFound();
        //    }
        //    return Results.Ok(output);

        //}

        private static async Task<IResult> LoadCachedCourseById(
    [FromServices] IMemoryCache cache,
    [FromServices] CourseData courseData,
    int id)
        {
            string cacheKey = $"course_{id}";

            // Try to get from cache
            if (!cache.TryGetValue(cacheKey, out CourseModel? course))
            {
                // Load from source
                course = courseData.Courses.FirstOrDefault(c => c.id == id);

                if (course is null)
                    return Results.NotFound();

                // Store in cache (e.g., 30 minutes)
                cache.Set(
                    cacheKey,
                    course,
                    new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                    });
            }

            return Results.Ok(course);
        }


        private static async Task<IResult> LoadAllCoursesAsync(
           CourseData courseData,
           string? courseType,
           string? search
           )
        {
            var output = courseData.Courses;

            if (!string.IsNullOrWhiteSpace(courseType))
            {
                output = output.Where(c => c.courseType.Equals(courseType, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                output = output.Where(c => c.courseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                           c.shortDescription.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return Results.Ok(output);
        }

        public static async Task<IResult> LoadAllCachedCoursesAsync(
    [FromServices] IMemoryCache cache,
      [FromServices]  CourseData courseData,
     string? courseType,
     string? search)
        {
            const string cacheKey = "allCourses";

            // 1. Try to get from cache
            if (!cache.TryGetValue(cacheKey, out List<CourseModel>? courses))
            {
                // Simulate loading from DB / external data
                courses =  courseData.Courses;

                // Cache for 30 minutes
                cache.Set(cacheKey, courses, TimeSpan.FromMinutes(30));
            }

            // 2. Apply filters
            IEnumerable<CourseModel> output = courses;

            if (!string.IsNullOrWhiteSpace(courseType))
            {
                output = output.Where(c =>
                    c.courseType.Equals(courseType, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                output = output.Where(c =>
                    c.courseName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.shortDescription.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return Results.Ok(output);
        }

    }
}
