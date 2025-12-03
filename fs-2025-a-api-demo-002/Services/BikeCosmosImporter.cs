using System.Text.Json;
using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Services
{
    /// <summary>
    /// DEV helper: reads Data/dublinbike.json and pushes all stations into Cosmos DB.
    /// </summary>
    public class BikeCosmosImporter
    {
        private readonly CosmosBikeService _cosmos;

        public BikeCosmosImporter(CosmosBikeService cosmos)
        {
            _cosmos = cosmos;
        }

        /// <summary>
        /// Import all stations from Data/dublinbike.json into Cosmos.
        /// If a station already exists (same number), it is updated; otherwise it is created.
        /// </summary>
        public async Task<(int imported, int total)> ImportFromFileAsync()
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var filePath = Path.Combine(AppContext.BaseDirectory, "Data", "dublinbike.json");

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Could not find dublinbike.json at: {filePath}");
            }

            var json = await File.ReadAllTextAsync(filePath);
            var stations = JsonSerializer.Deserialize<List<Bike>>(json, options) ?? new List<Bike>();

            int imported = 0;

            foreach (var station in stations)
            {
                // Make sure id + partition key are set
                if (string.IsNullOrWhiteSpace(station.id))
                    station.id = station.number.ToString();

                if (string.IsNullOrWhiteSpace(station.contract_name))
                    station.contract_name = "dublin";

                // Try update first; if not found, create
                var updated = await _cosmos.UpdateAsync(station.number, station);
                if (updated == null)
                {
                    await _cosmos.CreateAsync(station);
                }

                imported++;
            }

            return (imported, stations.Count);
        }
    }
}
