using System.Collections.Generic;

namespace fs_2025_assessment_2_client.Models
{
    public class PagedStationsResponse
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }

        // This must match the JSON property "items"
        public List<StationDto> Items { get; set; } = new();
    }
}
