using System.Collections.Generic;
using System.Linq;
using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Models;
using fs_2025_a_api_demo_002.Services;
using Xunit;

namespace fs_2025_a_api_demo_002.Tests
{
    public class BikeQueryServiceTests
    {
        // Helper: create a service with a small in-memory dataset
        private BikeQueryService CreateService()
        {
            var bikes = new List<Bike>
            {
                new Bike
                {
                    number = 1,
                    contract_name = "dublin",
                    name = "ALPHA STATION",
                    address = "Alpha Street",
                    available_bikes = 5,
                    bike_stands = 10,
                    status = "OPEN"
                },
                new Bike
                {
                    number = 2,
                    contract_name = "dublin",
                    name = "BETA STATION",
                    address = "Beta Street",
                    available_bikes = 0,
                    bike_stands = 15,
                    status = "CLOSED"
                },
                new Bike
                {
                    number = 3,
                    contract_name = "dublin",
                    name = "GAMMA STATION",
                    address = "Gamma Road",
                    available_bikes = 8,
                    bike_stands = 20,
                    status = "OPEN"
                },
                new Bike
                {
                    number = 4,
                    contract_name = "dublin",
                    name = "DELTA STATION",
                    address = "Delta Avenue",
                    available_bikes = 3,
                    bike_stands = 5,
                    status = "OPEN"
                }
            };

            // ✅ Uses the test-friendly constructor you just added
            var data = new BikeData(bikes);
            return new BikeQueryService(data);
        }

        [Fact]
        public void GetStations_NoFilters_ReturnsAllSortedByNameAsc()
        {
            // Arrange
            var service = CreateService();

            // Act
            var (items, totalCount) = service.GetStations(
                status: null,
                minBikes: null,
                q: null,
                sort: null,
                dir: null,
                page: 1,
                pageSize: 50);

            // Assert
            Assert.Equal(4, totalCount);
            Assert.Equal(4, items.Count());

            var names = items.Select(s => s.name).ToList();
            var sorted = names.OrderBy(n => n).ToList();
            Assert.Equal(sorted, names); // default sort = name asc
        }

        [Fact]
        public void GetStations_FilterByStatus_OpenOnly()
        {
            var service = CreateService();

            var (items, totalCount) = service.GetStations(
                status: "OPEN",
                minBikes: null,
                q: null,
                sort: null,
                dir: null,
                page: 1,
                pageSize: 50);

            Assert.Equal(3, totalCount);
            Assert.All(items, s =>
                Assert.Equal("OPEN", s.status, ignoreCase: true));
        }

        [Fact]
        public void GetStations_FilterByMinBikes_Works()
        {
            var service = CreateService();

            var (items, totalCount) = service.GetStations(
                status: null,
                minBikes: 5,
                q: null,
                sort: null,
                dir: null,
                page: 1,
                pageSize: 50);

            // only stations 1 (5 bikes) and 3 (8 bikes) qualify
            var numbers = items.Select(s => s.number).OrderBy(n => n).ToList();
            Assert.Equal(new[] { 1, 3 }, numbers);
        }

        [Fact]
        public void GetStations_SearchByNameOrAddress_Works()
        {
            var service = CreateService();

            var (items, totalCount) = service.GetStations(
                status: null,
                minBikes: null,
                q: "gamma",   // should match GAMMA STATION / Gamma Road
                sort: null,
                dir: null,
                page: 1,
                pageSize: 50);

            Assert.Equal(1, totalCount);
            var station = Assert.Single(items);
            Assert.Equal(3, station.number);
        }

        [Fact]
        public void GetStations_SortByAvailableBikes_Descending()
        {
            var service = CreateService();

            var (items, totalCount) = service.GetStations(
                status: null,
                minBikes: null,
                q: null,
                sort: "availableBikes",
                dir: "desc",
                page: 1,
                pageSize: 50);

            var bikes = items.ToList();
            var avail = bikes.Select(b => b.available_bikes).ToList();

            // Our dataset: 8, 5, 3, 0 in that order if sorting worked
            Assert.Equal(new[] { 8, 5, 3, 0 }, avail);
        }

        [Fact]
        public void GetStations_Paging_ReturnsSecondPage()
        {
            var service = CreateService();

            var (items, totalCount) = service.GetStations(
                status: null,
                minBikes: null,
                q: null,
                sort: "availableBikes",
                dir: "desc",
                page: 2,
                pageSize: 2);

            Assert.Equal(4, totalCount);
            Assert.Equal(2, items.Count());

            // First page: [8, 5] → Second page: [3, 0]
            var avail = items.Select(b => b.available_bikes).ToList();
            Assert.Equal(new[] { 3, 0 }, avail);
        }

        [Fact]
        public void GetByNumber_ReturnsCorrectStation()
        {
            var service = CreateService();

            var station = service.GetByNumber(3);

            Assert.NotNull(station);
            Assert.Equal("GAMMA STATION", station!.name);
        }

        [Fact]
        public void GetSummary_ComputesCorrectTotals()
        {
            var service = CreateService();

            var summary = service.GetSummary();

            // From our dataset:
            // bike_stands: 10 + 15 + 20 + 5 = 50
            // available_bikes: 5 + 0 + 8 + 3 = 16
            // open stations: 1, 3, 4 (3 total)
            // closed stations: 2 (1 total)
            Assert.Equal(4, summary.totalStations);
            Assert.Equal(50, summary.totalBikeStands);
            Assert.Equal(16, summary.totalAvailableBikes);
            Assert.Equal(3, summary.openStations);
            Assert.Equal(1, summary.closedStations);
        }
    }
}
