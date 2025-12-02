using System.Text.Json;
using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Data
{
    public class BikeData
    {
        public List<Bike> Stations { get; private set; } = new();

        // ==========================================================
        // 1) DEFAULT CONSTRUCTOR — USED BY THE REAL APPLICATION
        // ==========================================================
        public BikeData()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "BikeData.json");
            var jsonData = File.ReadAllText(filePath);

            Stations = JsonSerializer.Deserialize<List<Bike>>(jsonData, options) ?? new();
        }

        // ==========================================================
        // 2) TEST-ONLY CONSTRUCTOR — USED BY UNIT TESTS
        // ==========================================================
        public BikeData(List<Bike> seedStations)
        {
            Stations = seedStations ?? new List<Bike>();
        }
    }
}
