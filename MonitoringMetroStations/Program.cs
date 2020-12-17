using MonitoringMetroStations.Entities;
using MonitoringMetroStations.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringMetroStations
{
    class Program
    {
        static void Main(string[] args)
        {
            //Get the tube list
            string currentPath = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
            string pathOfTubes = currentPath + @"\Sources\tube.csv";
            List<Tube> tubes = FileHandler.GetInstance.GetTubes(pathOfTubes);

            //Get the targets of the drones
            Dictionary<int, Queue<Target>> dronesTargets = new Dictionary<int, Queue<Target>>();
            List<string> pathsOfDronesTargets = new List<string>()
            {
                currentPath+@"\Sources\5937.csv",
                currentPath+@"\Sources\6043.csv"
            };
            foreach (string path in pathsOfDronesTargets)
            {
                KeyValuePair<int, Queue<Target>> kvp = FileHandler.GetInstance.GetDroneTargets(path);
                dronesTargets.TryAdd(kvp.Key, kvp.Value);
            }

            //Create a dispatcher
            IDispatcher dispatcher = new Dispatcher();
            foreach (KeyValuePair<int, Queue<Target>> kvp in dronesTargets)
            {
                dispatcher.AddTargets(kvp);
            }

            //Look for the first datetime
            List<Drone> drones = new List<Drone>();
            DateTime earliestDate = DateTime.Now;
            foreach (KeyValuePair<int, Queue<Target>> kvp in dronesTargets)
            {
                DateTime actualTime = kvp.Value.Peek().Time;
                if (earliestDate > actualTime)
                {
                    earliestDate = actualTime;
                }
            }

            //Create drones
            foreach (KeyValuePair<int, Queue<Target>> kvp in dronesTargets)
            {
                //Calculate the longness of the delay to start reporting to the dispatcher
                double delay = (kvp.Value.Peek().Time - earliestDate).TotalMilliseconds;
                Drone newDrone = new Drone(kvp.Key, tubes, (int)delay);
                newDrone.AddDispatcher(dispatcher);
                drones.Add(newDrone);
            }

            //Subscribe the drones for the events of dispatcher
            drones.ForEach(d => dispatcher.Subscribe(d));

            //Set the shutdown time
            DateTime shutDownTime = DateTime.Parse("22/03/2011 08:00");
            int estimatedRunningTime = (int)(shutDownTime - earliestDate).TotalMilliseconds;
            dispatcher.SetRuntime(estimatedRunningTime);

            Console.WriteLine(GetWelcomeMessage(estimatedRunningTime));

            //Waiting for the user to start
            char startingChar = Console.ReadKey().KeyChar;
            while (startingChar != 's' && startingChar != 'S')
            {
                Console.Clear();
                Console.WriteLine(GetWelcomeMessage(estimatedRunningTime));
                startingChar = Console.ReadKey().KeyChar;
            }

            CancellationTokenSource tokenSource = new CancellationTokenSource();

            //Start the traffic assessment
            print("The simulation is started\n\tThe drones reports are following");
            Task simulationStartTask = Task.Run(() => dispatcher.Start(tokenSource), tokenSource.Token);

            //Start a task for the user "interaction". The user can cancel the simulation
            Task CacellationTask = Task.Run(() => CancelSimulation(dispatcher, tokenSource), tokenSource.Token);

            try
            {
                simulationStartTask.Wait(tokenSource.Token);
            }
            catch (Exception)
            {
                print("The simulation is cancelled");
            }
        }

        private static void CancelSimulation(IDispatcher dispatcher, CancellationTokenSource tokenSource)
        {
            char cancellationChar;
            do
            {
                cancellationChar = Console.ReadKey().KeyChar;
            } while (cancellationChar != 'c' && cancellationChar != 'C');

            dispatcher.Stop();
            tokenSource.Cancel();
        }

        private static string GetWelcomeMessage(int estimatedRunningTime)
        {
            return "Hello dear user!" +
                "\n\t* Please press \'s\' or \'S\' button to start the simulation." +
                $"\n\t* The simulation is {estimatedRunningTime / 1000} seconds long!!!" +
                "\n\t* If you want to stop it earlier please press \'c\' or \'C\' button.";
        }

        private static void print(string message)
        {
            Console.WriteLine("\n***********************************************" +
                    $"\n\t{message}" +
                    "\n***********************************************\n");
        }
    }
}
