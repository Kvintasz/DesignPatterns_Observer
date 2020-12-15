using System;

namespace MonitoringMetroStations.Entities
{
    public class Target
    {
        public int DroneId { get; set; }
        public Coordinate Coordinate { get; set; }
        public DateTime Time { get; set; }
    }
}
