using System;

namespace fs_2025_a_api_demo_002.Models
{
    public class Bike
    {
        public int number { get; set; }
        public string contract_name { get; set; } = "";
        public string name { get; set; } = "";
        public string address { get; set; } = "";
        public Position position { get; set; } = new Position();
        public bool banking { get; set; }
        public bool bonus { get; set; }
        public int bike_stands { get; set; }
        public int available_bike_stands { get; set; }
        public int available_bikes { get; set; }
        public string status { get; set; } = "";
        public long last_update { get; set; }

        // ============================================
        //               CALCULATED FIELDS
        // ============================================

        // Occupancy = available_bikes / bike_stands
        public double occupancy =>
            bike_stands == 0 ? 0 : (double)available_bikes / bike_stands;

        // Convert UNIX milliseconds → UTC
        public DateTimeOffset lastUpdateUtc =>
            DateTimeOffset.FromUnixTimeMilliseconds(last_update);

        // Convert UTC → Ireland local time
        public DateTimeOffset lastUpdateLocal
        {
            get
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById("Europe/Dublin");
                return TimeZoneInfo.ConvertTime(lastUpdateUtc, tz);
            }
        }
    }

    public class Position
    {
        public float lat { get; set; }
        public float lng { get; set; }
    }
}
