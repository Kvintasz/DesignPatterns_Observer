using MonitoringMetroStations.Entities;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringMetroStations
{
    public class Dispatcher : IDispatcher
    {
        public void SetRuntime(int time)
        {
            this.runtime = time;
        }

        public void NotifyShutdown()
        {
            NotifyAllDrones(0, false);
        }

        public void NotifyStart()
        {
            NotifyAllDrones(0, true);
        }

        public void Subscribe(IDrone drone)
        {
            lock (lockObject)
            {
                if (!drones.Contains(drone))
                {
                    drones.Add(drone);
                }
            }            
        }

        public void Unsubscribe(IDrone drone)
        {
            lock (this)
            {
                if (drones.Contains(drone))
                {
                    drones.Remove(drone);
                }
            }            
        }

        public Target GetNextTarget(IDrone drone)
        {
            Target target;
            while (!targets[drone.GetId()].TryDequeue(out target));
            return target;
        }

        public void AddTargets(KeyValuePair<int, Queue<Target>> newTargets)
        {
            while(!targets.TryAdd(newTargets.Key, newTargets.Value));
        }

        public void Start(CancellationTokenSource tokenSource)
        {
            NotifyStart();
            Task.Run(() => StartTimer(tokenSource), tokenSource.Token).Wait();
        }

        public void Stop()
        {
            NotifyShutdown();
        }

        private List<IDrone> drones = new List<IDrone>();
        private object lockObject = new object();
        private int runtime = 0;
        private ConcurrentDictionary<int, Queue<Target>> targets = new ConcurrentDictionary<int, Queue<Target>>();
        
        private void NotifyAllDrones(int index, bool isStart)
        {
            if (isStart)
            {
                drones[index].UpdateStart();
            }
            else
            {
                drones[index].UpdateShutdown();
            }

            if (index + 1 != drones.Count)
            {
                NotifyAllDrones(index + 1, isStart);
            }
        }

        private void StartTimer(CancellationTokenSource tokenSource)
        {
            Task.Delay(this.runtime).Wait();
            Stop();
            tokenSource.Cancel();
        }
    }
}
