using System.Text.Json.Serialization;

namespace fs_2025_assessment_2_client.Models
{
    public class PositionDto
    {
        [JsonPropertyName("lat")]
        public double Lat { get; set; }

        [JsonPropertyName("lng")]
        public double Lng { get; set; }
    }

    public class StationDto
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("number")]
        public int Number { get; set; }

        [JsonPropertyName("contract_name")]
        public string? ContractName { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("position")]
        public PositionDto? Position { get; set; }

        [JsonPropertyName("banking")]
        public bool Banking { get; set; }

        [JsonPropertyName("bonus")]
        public bool Bonus { get; set; }

        [JsonPropertyName("bike_stands")]
        public int BikeStands { get; set; }

        [JsonPropertyName("available_bike_stands")]
        public int AvailableBikeStands { get; set; }

        [JsonPropertyName("available_bikes")]
        public int AvailableBikes { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        // In Cosmos JSON this is a long (milliseconds since epoch)
        [JsonPropertyName("last_update")]
        public long LastUpdateRaw { get; set; }

        // Convenience properties for UI
        public double Latitude => Position?.Lat ?? 0;
        public double Longitude => Position?.Lng ?? 0;
    }
}
