using fs_2025_a_api_demo_002.Models;
using System.Text.Json;

namespace fs_2025_a_api_demo_002.Data
{
    public class CourseData
    {
        public List<CourseModel> Courses { get; private set; } = new List<CourseModel>();

        public CourseData()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "coursedata.json");
            var jsonData = File.ReadAllText(filePath);
            Courses = JsonSerializer.Deserialize<List<CourseModel>>(jsonData, options) ?? new List<CourseModel>();
        }

    }
}
