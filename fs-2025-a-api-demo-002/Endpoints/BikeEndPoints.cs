using fs_2025_a_api_demo_002.Models;
using fs_2025_a_api_demo_002.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace fs_2025_a_api_demo_002.Endpoints
{
    public static class BikeEndPoints
    {
        public static void AddBikeEndPoints(this WebApplication app)
        {
            // ========== V1: JSON + Memory Cache ==========

            var v1 = app.MapGroup("/api/v1/stations");

            v1.MapGet("", GetStationsV1);
            v1.MapGet("/{number:int}", GetStationByNumberV1);
            v1.MapGet("/summary", GetSummaryV1);
            v1.MapPost("", CreateStationV1);
            v1.MapPut("/{number:int}", UpdateStationV1);

            // ========== V2: Cosmos DB (placeholder for now) ==========

            var v2 = app.MapGroup("/api/v2/stations");

            v2.MapGet("", GetStationsV2);
            v2.MapGet("/{number:int}", GetStationByNumberV2);
            v2.MapGet("/summary", GetSummaryV2);
            v2.MapPost("", CreateStationV2);
            v2.MapPut("/{number:int}", UpdateStationV2);
        }

        // ============================================
        //                V1 IMPLEMENTATION
        // ============================================

        private static async Task<IResult> GetStationsV1(
            [FromServices] IMemoryCache cache,
            [FromServices] BikeQueryService service,
            string? status,
            int? minBikes,
            string? q,
            string? sort,
            string? dir,
            int page = 1,
            int pageSize = 50)
        {
            string cacheKey = $"v1_stations_{status}_{minBikes}_{q}_{sort}_{dir}_{page}_{pageSize}";

            if (!cache.TryGetValue(cacheKey, out object? cached))
            {
                var (items, totalCount) =
                    service.GetStations(status, minBikes, q, sort, dir, page, pageSize);

                cached = new
                {
                    page,
                    pageSize,
                    totalCount,
                    items
                };

                cache.Set(cacheKey, cached, TimeSpan.FromMinutes(5));
            }

            return Results.Ok(cached);
        }

        private static async Task<IResult> GetStationByNumberV1(
            [FromServices] IMemoryCache cache,
            [FromServices] BikeQueryService service,
            int number)
        {
            string cacheKey = $"v1_station_{number}";

            if (!cache.TryGetValue(cacheKey, out Bike? station))
            {
                station = service.GetByNumber(number);
                if (station is null)
                    return Results.NotFound();

                cache.Set(cacheKey, station, TimeSpan.FromMinutes(5));
            }

            return Results.Ok(station);
        }

        private static async Task<IResult> GetSummaryV1(
            [FromServices] IMemoryCache cache,
            [FromServices] BikeQueryService service)
        {
            const string cacheKey = "v1_summary";

            if (!cache.TryGetValue(cacheKey, out StationSummary? summary))
            {
                summary = service.GetSummary();
                cache.Set(cacheKey, summary, TimeSpan.FromMinutes(5));
            }

            return Results.Ok(summary);
        }

        private static async Task<IResult> CreateStationV1(
            [FromServices] BikeQueryService service,
            [FromBody] Bike newStation)
        {
            try
            {
                var created = service.Create(newStation);
                return Results.Created($"/api/v1/stations/{created.number}", created);
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        }

        private static async Task<IResult> UpdateStationV1(
            [FromServices] BikeQueryService service,
            int number,
            [FromBody] Bike updated)
        {
            var result = service.Update(number, updated);
            if (result is null)
                return Results.NotFound();

            return Results.Ok(result);
        }

        // ============================================
        //                V2 PLACEHOLDERS
        // ============================================

        //  later connect these to Cosmos.
        // For now they just return 501 so Swagger and Postman show them.

        private static Task<IResult> GetStationsV2(
            string? status,
            int? minBikes,
            string? q,
            string? sort,
            string? dir,
            int page = 1,
            int pageSize = 50)
        {
            return Task.FromResult<IResult>(
                Results.StatusCode(StatusCodes.Status501NotImplemented));
        }

        private static Task<IResult> GetStationByNumberV2(int number) =>
            Task.FromResult<IResult>(
                Results.StatusCode(StatusCodes.Status501NotImplemented));

        private static Task<IResult> GetSummaryV2() =>
            Task.FromResult<IResult>(
                Results.StatusCode(StatusCodes.Status501NotImplemented));

        private static Task<IResult> CreateStationV2(Bike _) =>
            Task.FromResult<IResult>(
                Results.StatusCode(StatusCodes.Status501NotImplemented));

        private static Task<IResult> UpdateStationV2(int number, Bike _) =>
            Task.FromResult<IResult>(
                Results.StatusCode(StatusCodes.Status501NotImplemented));
    }
}
