using fs_2025_a_api_demo_002.Data;

namespace fs_2025_a_api_demo_002.Endpoints
{
    public static class BikeEndPoints
    {
        public static void AddBikeEndPoints(this WebApplication app)
        {
            // GET /bikes
            app.MapGet("/bikes", LoadAllBikeStationsAsync);

            // GET /bikes/{number}
            app.MapGet("/bikes/{number:int}", LoadBikeStationByNumber);
        }

        private static async Task<IResult> LoadBikeStationByNumber(BikeData bikeData, int number)
        {
            var output = bikeData.Stations.FirstOrDefault(s => s.number == number);

            if (output is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(output);
        }

        private static async Task<IResult> LoadAllBikeStationsAsync(
            BikeData bikeData,
            string? contractName,
            string? status)
        {
            var output = bikeData.Stations.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(contractName))
            {
                output = output.Where(s =>
                    s.contract_name.Equals(contractName, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                output = output.Where(s =>
                    s.status.Equals(status, StringComparison.OrdinalIgnoreCase));
            }

            return Results.Ok(output);
        }
    }
}
