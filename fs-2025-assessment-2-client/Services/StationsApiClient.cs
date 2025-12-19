using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using fs_2025_assessment_2_client.Models;

namespace fs_2025_assessment_2_client.Services
{
    public class StationsApiClient
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public StationsApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            };
        }

        // -----------------------------------------------------------
        //  MAIN METHOD USED BY THE UI (no args)
        // -----------------------------------------------------------
        public Task<List<StationDto>> GetStationsAsync()
        {
            // Use same paging as Swagger, but a big pageSize to get all
            return GetStationsAsync(page: 1, pageSize: 500);
        }

        // -----------------------------------------------------------
        //  PAGED GET (matches Swagger wrapper: { page, pageSize, totalCount, items })
        // -----------------------------------------------------------
        public async Task<List<StationDto>> GetStationsAsync(int page, int pageSize)
        {
            var url = $"api/v2/stations?page={page}&pageSize={pageSize}";

            var response = await _httpClient.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"API call to '{url}' failed with status {(int)response.StatusCode} {response.ReasonPhrase}. " +
                    $"Body: {json}");
            }

            try
            {
                // ✅ Deserialize the wrapper object
                var wrapper = JsonSerializer.Deserialize<PagedStationsResponse>(json, _jsonOptions);

                return wrapper?.Items ?? new List<StationDto>();
            }
            catch (Exception ex)
            {
                var preview = json.Length > 500 ? json[..500] + "..." : json;

                throw new InvalidOperationException(
                    $"Failed to deserialize stations JSON. Preview: {preview}", ex);
            }
        }

        // -----------------------------------------------------------
        //  GET SINGLE STATION
        // -----------------------------------------------------------
        public async Task<StationDto?> GetStationAsync(int number)
        {
            return await _httpClient.GetFromJsonAsync<StationDto>(
                $"api/v2/stations/{number}",
                _jsonOptions);
        }

        // -----------------------------------------------------------
        //  CREATE
        // -----------------------------------------------------------
        public async Task<StationDto?> CreateStationAsync(StationDto station)
        {
            var response = await _httpClient.PostAsJsonAsync("api/v2/stations", station, _jsonOptions);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<StationDto>(_jsonOptions);
        }

        // -----------------------------------------------------------
        //  UPDATE
        // -----------------------------------------------------------
        public async Task UpdateStationAsync(int number, StationDto station)
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v2/stations/{number}",
                station,
                _jsonOptions);

            response.EnsureSuccessStatusCode();
        }

        // -----------------------------------------------------------
        //  DELETE
        // -----------------------------------------------------------
        public async Task DeleteStationAsync(int number)
        {
            var response = await _httpClient.DeleteAsync($"api/v2/stations/{number}");
            response.EnsureSuccessStatusCode();
        }
    }
}
