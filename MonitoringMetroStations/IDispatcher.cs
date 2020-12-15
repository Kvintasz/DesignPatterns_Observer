using MonitoringMetroStations.Entities;
using System.Collections.Generic;
using System.Threading;

namespace MonitoringMetroStations
{
    public interface IDispatcher
    {
        void Subscribe(IDrone drone);
        void Unsubscribe(IDrone drone);
        void Start(CancellationTokenSource tokenSource);
        void Stop();
        void SetRuntime(int runtime);
        void AddTargets(KeyValuePair<int, Queue<Target>> newTargets);
        Target GetNextTarget(IDrone drone);
    }
}
