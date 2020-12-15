using System;

namespace MonitoringMetroStations.Entities
{
    public enum ConditionsOfTraffic
    {
        HEAVY,
        LIGHT,
        MODERATE
    }

    public class TrafficReport
    {
        public string StationName { get; }
        public int DroneId { get; }
        public DateTime Time { get; }
        public double Speed { get; }
        public ConditionsOfTraffic ConditionOfTraffic { get; }

        public TrafficReport(string station, int droneId, DateTime time, double speed)
        {
            StationName = station;
            DroneId = droneId;
            Time = time;
            Speed = speed;
            ConditionOfTraffic = EstimateTraffic();
        }

        private ConditionsOfTraffic EstimateTraffic()
        {
            return (ConditionsOfTraffic)(new Random()).Next(0, 3);
        }
    }
}
