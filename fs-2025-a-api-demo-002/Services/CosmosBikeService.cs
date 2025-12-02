using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Services
{
    public class CosmosBikeService
    {
        private readonly CosmosClient _client;
        private readonly Container _container;
        private readonly string _databaseId;
        private readonly string _containerId;

        public CosmosBikeService(IConfiguration configuration)
        {
            var endpoint = configuration["CosmosDb:AccountEndpoint"];
            var key = configuration["CosmosDb:AccountKey"];

            _databaseId = configuration["CosmosDb:DatabaseName"]
                              ?? throw new InvalidOperationException("CosmosDb:DatabaseName missing");
            _containerId = configuration["CosmosDb:ContainerName"]
                              ?? throw new InvalidOperationException("CosmosDb:ContainerName missing");

            var options = new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Gateway,

                // ✅ Fail fast instead of retrying forever
                RequestTimeout = TimeSpan.FromSeconds(5),
                MaxRetryAttemptsOnRateLimitedRequests = 0,
                MaxRetryWaitTimeOnRateLimitedRequests = TimeSpan.Zero,

                HttpClientFactory = () =>
                {
                    var handler = new HttpClientHandler
                    {
                        // ⚠ DEV ONLY: accept self-signed certificate from the emulator
                        ServerCertificateCustomValidationCallback =
                            (request, cert, chain, errors) => true
                    };

                    return new HttpClient(handler);
                }
            };

            _client = new CosmosClient(endpoint, key, options);
            _container = _client.GetContainer(_databaseId, _containerId);
        }

        // ============================================
        //  GET MANY  (with filters + sort + paging)
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
            // Normalise paging
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 50;
            if (pageSize > 100) pageSize = 100;

            // Hard 5-second timeout for the whole query
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            // load everything once, then apply the same
            // filter/sort logic as V1 in memory.
            var queryDef = new QueryDefinition("SELECT * FROM c");

            var all = new List<Bike>();
            using var iterator = _container.GetItemQueryIterator<Bike>(queryDef);

            while (iterator.HasMoreResults && !cts.IsCancellationRequested)
            {
                var response = await iterator.ReadNextAsync(cts.Token);
                all.AddRange(response);
            }

            if (cts.IsCancellationRequested)
            {
                throw new TimeoutException("Timed out reading stations from Cosmos DB.");
            }

            // ---------- FILTERING ----------
            IEnumerable<Bike> filtered = all;

            if (!string.IsNullOrWhiteSpace(status))
            {
                var wantedStatus = status.Trim().ToUpperInvariant();
                filtered = filtered.Where(b =>
                    string.Equals(b.status, wantedStatus, StringComparison.OrdinalIgnoreCase));
            }

            if (minBikes.HasValue)
            {
                filtered = filtered.Where(b => b.available_bikes >= minBikes.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var term = q.Trim().ToLowerInvariant();
                filtered = filtered.Where(b =>
                    (!string.IsNullOrWhiteSpace(b.name) &&
                     b.name.ToLowerInvariant().Contains(term))
                    ||
                    (!string.IsNullOrWhiteSpace(b.address) &&
                     b.address.ToLowerInvariant().Contains(term)));
            }

            // ---------- SORTING ----------
            var sortKey = string.IsNullOrWhiteSpace(sort)
                ? "name"
                : sort.Trim().ToLowerInvariant();

            var descending = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);

            Func<Bike, object> keySelector = sortKey switch
            {
                "available_bikes" => b => b.available_bikes,
                "available_bike_stands" => b => b.available_bike_stands,
                "occupancy" => b => b.occupancy,
                "number" => b => b.number,
                _ => b => b.name ?? string.Empty
            };

            filtered = descending
                ? filtered.OrderByDescending(keySelector)
                : filtered.OrderBy(keySelector);

            // ---------- PAGING ----------
            var totalCount = filtered.Count();
            var skip = (page - 1) * pageSize;

            var items = filtered
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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var query = new QueryDefinition(
                "SELECT * FROM c WHERE c.number = @number")
                .WithParameter("@number", number);

            using var iterator = _container.GetItemQueryIterator<Bike>(query);

            while (iterator.HasMoreResults && !cts.IsCancellationRequested)
            {
                var response = await iterator.ReadNextAsync(cts.Token);
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
            // Load all once (no paging) and compute.
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
                // Good practice: stable id per station
                newStation.id = newStation.number.ToString();
            }

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var response = await _container.CreateItemAsync(
                newStation,
                new PartitionKey(newStation.contract_name),
                cancellationToken: cts.Token);

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

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var response = await _container.ReplaceItemAsync(
                updated,
                updated.id,
                new PartitionKey(updated.contract_name),
                cancellationToken: cts.Token);

            return response.Resource;
        }
    }
}
