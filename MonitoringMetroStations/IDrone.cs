using System;

namespace MonitoringMetroStations
{
    public interface IDrone
    {
        int GetId();

        void UpdateShutdown();

        void UpdateStart();
    }
}
