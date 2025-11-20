using System.Text.Json;
using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Data
{
    public class BikeData
    {
        public List<Bike> Stations { get; private set; } = new();

        public BikeData()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "BikeData.json");
            var jsonData = File.ReadAllText(filePath);

            //  List<Bikes> – must match the Bike model
            Stations = JsonSerializer.Deserialize<List<Bike>>(jsonData, options) ?? new();
        }
    }
}
