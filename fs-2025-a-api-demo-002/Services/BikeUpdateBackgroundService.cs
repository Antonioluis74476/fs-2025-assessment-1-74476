using System;
using System.Threading;
using System.Threading.Tasks;
using fs_2025_a_api_demo_002.Data;
using Microsoft.Extensions.Hosting;

namespace fs_2025_a_api_demo_002.Services
{
    public class BikeUpdateBackgroundService : BackgroundService
    {
        private readonly BikeData _bikeData;
        private readonly Random _random = new();

        public BikeUpdateBackgroundService(BikeData bikeData)
        {
            _bikeData = bikeData;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Runs the loop until the app is shutting down
            while (!stoppingToken.IsCancellationRequested)
            {
                UpdateStations();

                // Wait between updates (adjust if your brief says 10/20 seconds)
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }

        private void UpdateStations()
        {
            foreach (var station in _bikeData.Stations)
            {
                // Random total stands between 10 and 40
                int totalStands = _random.Next(10, 41);

                // Random available bikes between 0 and totalStands
                int availableBikes = _random.Next(0, totalStands + 1);

                station.bike_stands = totalStands;
                station.available_bikes = availableBikes;
                station.available_bike_stands = totalStands - availableBikes;

                // Update last_update to "now" in UTC, in ms since epoch
                station.last_update = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }
    }
}
