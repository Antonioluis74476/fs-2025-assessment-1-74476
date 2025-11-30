using System;
using System.Collections.Generic;
using System.Linq;
using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Models;

namespace fs_2025_a_api_demo_002.Services
{
    public class BikeQueryService
    {
        private readonly BikeData _data;

        public BikeQueryService(BikeData data)
        {
            _data = data;
        }

        /// <summary>
        /// Main query method used by the endpoints.
        /// Supports filtering, searching, sorting and paging.
        /// </summary>
        public (IEnumerable<Bike> items, int totalCount) GetStations(
            string? status,
            int? minBikes,
            string? q,
            string? sort,
            string? dir,
            int page,
            int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 200) pageSize = 50;

            var query = _data.Stations.AsQueryable();

            // ---------- Filters ----------

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(s =>
                    s.status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            if (minBikes is not null)
            {
                query = query.Where(s => s.available_bikes >= minBikes.Value);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                query = query.Where(s =>
                    s.name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    s.address.Contains(q, StringComparison.OrdinalIgnoreCase));
            }

            // ---------- Sorting ----------

            bool desc = string.Equals(dir, "desc", StringComparison.OrdinalIgnoreCase);

            query = sort?.ToLower() switch
            {
                "availablebikes" => desc
                    ? query.OrderByDescending(s => s.available_bikes)
                    : query.OrderBy(s => s.available_bikes),

                "occupancy" => desc
                    ? query.OrderByDescending(s => s.occupancy)
                    : query.OrderBy(s => s.occupancy),

                // default: sort by name
                _ => desc
                    ? query.OrderByDescending(s => s.name)
                    : query.OrderBy(s => s.name)
            };

            // ---------- Paging ----------

            var totalCount = query.Count();

            var items = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return (items, totalCount);
        }

        public Bike? GetByNumber(int number) =>
            _data.Stations.FirstOrDefault(s => s.number == number);

        public StationSummary GetSummary()
        {
            var stations = _data.Stations;

            return new StationSummary
            {
                totalStations = stations.Count,
                totalBikeStands = stations.Sum(s => s.bike_stands),
                totalAvailableBikes = stations.Sum(s => s.available_bikes),
                openStations = stations.Count(s =>
                    s.status.Equals("OPEN", StringComparison.OrdinalIgnoreCase)),
                closedStations = stations.Count(s =>
                    s.status.Equals("CLOSED", StringComparison.OrdinalIgnoreCase))
            };
        }

        public Bike Create(Bike station)
        {
            if (station is null) throw new ArgumentNullException(nameof(station));

            if (_data.Stations.Any(s => s.number == station.number))
            {
                throw new InvalidOperationException(
                    "Station with that number already exists.");
            }

            _data.Stations.Add(station);
            return station;
        }

        public Bike? Update(int number, Bike updated)
        {
            var existing = _data.Stations.FirstOrDefault(s => s.number == number);
            if (existing is null) return null;

            existing.contract_name = updated.contract_name;
            existing.name = updated.name;
            existing.address = updated.address;
            existing.position = updated.position;
            existing.banking = updated.banking;
            existing.bonus = updated.bonus;
            existing.bike_stands = updated.bike_stands;
            existing.available_bike_stands = updated.available_bike_stands;
            existing.available_bikes = updated.available_bikes;
            existing.status = updated.status;
            existing.last_update = updated.last_update;

            return existing;
        }
    }
}
