using Microsoft.Azure.Cosmos;
using fs_2025_a_api_demo_002.Models;


namespace fs_2025_a_api_demo_002.Services
{
    public class CosmosBikeService
    {
        private readonly CosmosClient _client;
        private readonly Container _container;

        public CosmosBikeService(IConfiguration configuration)
        {
            var endpoint = configuration["Cosmos:Endpoint"];
            var key = configuration["Cosmos:Key"];
            var databaseId = configuration["Cosmos:DatabaseId"];
            var containerId = configuration["Cosmos:StationsContainerId"];

            _client = new CosmosClient(endpoint, key);
            _container = _client.GetContainer(databaseId, containerId);
        }

        // ============================================
        //  GET MANY (with simple paging, no filters)
        // ============================================
        public async Task<(IReadOnlyList<Bike> items, int totalCount)> GetStationsAsync(
            string? status,
            int? minBikes,
            string? q,
            string? sort,
            string? dir,
            int page,
            int pageSize)
        {
            // For the assignment we’ll keep it simple:
            // ignore filters and sorting, just return all docs and page in memory.

            var query = new QueryDefinition("SELECT * FROM c");

            var all = new List<Bike>();
            using var iterator = _container.GetItemQueryIterator<Bike>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                all.AddRange(response);
            }

            var totalCount = all.Count;

            var skip = (page - 1) * pageSize;
            var items = all
                .Skip(skip)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        // ============================================
        //  GET ONE BY NUMBER
        // ============================================
        public async Task<Bike?> GetByNumberAsync(int number)
        {
            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.number = @number")
                .WithParameter("@number", number);

            using var iterator = _container.GetItemQueryIterator<Bike>(query);

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                var match = response.FirstOrDefault();
                if (match != null)
                    return match;
            }

            return null;
        }

        // ============================================
        //  SUMMARY
        // ============================================
        public async Task<StationSummary> GetSummaryAsync()
        {
            // Get EVERYTHING once (no paging) and compute summary.
            var (items, totalCount) =
                await GetStationsAsync(null, null, null, null, null, 1, int.MaxValue);

            var totalBikeStands = items.Sum(b => b.bike_stands);
            var totalAvailableBikes = items.Sum(b => b.available_bikes);
            var totalAvailableStands = items.Sum(b => b.available_bike_stands);

            return new StationSummary
            {
                stationCount = totalCount,
                totalBikeStands = totalBikeStands,
                totalAvailableBikes = totalAvailableBikes,
                totalAvailableStands = totalAvailableStands
            };
        }

        // ============================================
        //  CREATE
        // ============================================
        public async Task<Bike> CreateAsync(Bike newStation)
        {
            if (string.IsNullOrWhiteSpace(newStation.id))
            {
                newStation.id = newStation.number.ToString();
            }

            var response = await _container.CreateItemAsync(
                newStation,
                new PartitionKey(newStation.contract_name));

            return response.Resource;
        }

        // ============================================
        //  UPDATE
        // ============================================
        public async Task<Bike?> UpdateAsync(int number, Bike updated)
        {
            var existing = await GetByNumberAsync(number);
            if (existing == null)
                return null;

            // Keep same id + partition key
            updated.id = existing.id;
            updated.contract_name = existing.contract_name;

            var response = await _container.ReplaceItemAsync(
                updated,
                updated.id,
                new PartitionKey(updated.contract_name));

            return response.Resource;
        }
    }
}
