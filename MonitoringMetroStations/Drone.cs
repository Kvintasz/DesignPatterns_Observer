using MonitoringMetroStations.Entities;
using MonitoringMetroStations.Helpers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringMetroStations
{
    public class Drone : IDrone
    {
        public Drone(int droneId, List<Tube> tubesIn, int delayIn)
        {
            this.id = droneId;
            this.tubes = tubesIn;
            this.delay = delayIn;
            this.tokenSource = new CancellationTokenSource();
        }

        public void AddDispatcher(IDispatcher dispatcherIn)
        {
            lock (lockObject)
            {
                if (!this.dispatchers.Contains(dispatcherIn))
                {
                    this.dispatchers.Add(dispatcherIn);
                }
            }

            while (targets.Count < 10)
            {
                AskNextTargetFromDispatcher();
            }
        }

        public void RemoveDispatcher(IDispatcher dispatcherIn)
        {
            lock (lockObject)
            {
                if (this.dispatchers.Contains(dispatcherIn))
                {
                    this.dispatchers.Remove(dispatcherIn);
                }
            }
        }

        public void UpdateShutdown()
        {
            this.tokenSource.Cancel();
        }

        public int GetId()
        {
            return id;
        }

        public void UpdateStart()
        {
            Task.Run(() => CheckTargets(), this.tokenSource.Token);
        }

        private int id;
        private List<IDispatcher> dispatchers = new List<IDispatcher>();
        private ConcurrentQueue<Target> targets = new ConcurrentQueue<Target>();
        private object lockObject = new object();
        private int delay;
        private List<Tube> tubes;
        private bool isNotAvailableTargets = false;
        private CancellationTokenSource tokenSource;
        private Target previousTarget = null;

        private void CheckTargets()
        {
            while(targets.Count > 0)
            {
                Target target = GetTargetFromQueue();
                if (!(this.previousTarget is null))
                {
                    this.delay = (int)(target.Time - previousTarget.Time).TotalMilliseconds;
                }

                if (previousTarget is null
                    || (target.Coordinate.Latitude != previousTarget.Coordinate.Latitude
                    && target.Coordinate.Longitude != previousTarget.Coordinate.Longitude))
                {
                    Task movingTask = Task.Run(() => Move(target.Coordinate));

                    double distance = 0;

                    Tube tube = tubes.Find(t =>
                    {
                        distance = GetDistance(t.Coordinate, target.Coordinate);
                        return distance < 351;
                    });

                    double speed = previousTarget is null
                        ? 30 
                        : speed = CalculateSpeed(previousTarget, target);

                    string reportMessage;
                    if (!(tube is null))
                    {
                        TrafficReport report = new TrafficReport(tube.Name, id, target.Time, speed);
                        reportMessage = CreateSuccessReport(report, distance);
                    }
                    else
                    {
                        reportMessage = $"{id} drone reports:" +
                            $"\n\tThere is no station within a radius of 350 meters of the the given position.";
                    }

                    this.previousTarget = target;
                    movingTask.Wait();

                    MakeReport(reportMessage);
                }
            }
        }

        private void MakeReport(string reportMessage)
        {
            //Write the report into file
            FileHandler.GetInstance.Print(reportMessage);

            //write the report to the console
            Console.WriteLine(reportMessage);
        }

        private Target GetTargetFromQueue()
        {
            Target target = new Target();
            while (targets.Count > 0 && targets.TryDequeue(out target)) ;
            AskNextTargetFromDispatcher();
            return target;
        }

        private string CreateSuccessReport(TrafficReport report, double distance)
        {
            return $"{id} drone reports:" +
                        $"\n\tStation: {report.StationName}" +
                        $"\n\tTime: {report.Time}" +
                        $"\n\tSpeed: { string.Format("{0:00.0}", report.Speed) } km/h" +
                        $"\n\tConditions of Traffic: {report.ConditionOfTraffic}" +
                        $"\n\tDistance between the drone and the station is : {string.Format("{0:00.0}", distance)} meters";
        }

        private void Move(Coordinate coordinate)
        {
            string message = $"{id} drone reports: " +
                $"\n\tApproaching the " +
                $"\'lat:{coordinate.Latitude} lon:{coordinate.Longitude}\' position.";
            MakeReport(message);
            Task.Delay((int)this.delay).Wait();
        }

        private double CalculateSpeed(Target target1, Target target2)
        {
            double deltaTime = (target2.Time - target1.Time).TotalHours;
            double distance = GetDistance(target1.Coordinate, target2.Coordinate) / 1000;
            return distance / deltaTime;
        }

        public double GetDistance(Coordinate coordinate1, Coordinate coordinate2)
        {
            double radiusOfEarth = 6371000;

            double dLat = (coordinate2.Latitude - coordinate1.Latitude) * Math.PI / 180;
            double dLon = (coordinate2.Longitude - coordinate1.Longitude) * Math.PI / 180;

            double haversin = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
                + Math.Cos(coordinate1.Latitude * Math.PI / 180)
                * Math.Cos(coordinate2.Latitude * Math.PI / 180)
                * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);

            double angularDistance = 2 * Math.Asin(Math.Min(1, Math.Sqrt(haversin)));
            double distance = radiusOfEarth * angularDistance;
            return distance;
        }

        private void AskNextTargetFromDispatcher()
        {
            if (isNotAvailableTargets) return;

            bool isThereNewTarget = false;
            foreach (IDispatcher dispatcher in dispatchers)
            {
                Target target = dispatcher.GetNextTarget(this);
                if (!(target is null))
                {
                    targets.Enqueue(target);
                    isThereNewTarget = true;
                    break;
                }
            }

            if (!isThereNewTarget)
            {
                this.isNotAvailableTargets = true;
            }
        }
    }
}
