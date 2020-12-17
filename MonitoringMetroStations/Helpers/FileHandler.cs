using MonitoringMetroStations.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MonitoringMetroStations.Helpers
{
    public class FileHandler
    {
        public static FileHandler GetInstance { get; } = new FileHandler();

        private FileHandler()
        {

        }

        internal List<Tube> GetTubes(string pathOfTubes)
        {
            using (StreamReader reader = new StreamReader(pathOfTubes))
            {
                List<Tube> tubes = new List<Tube>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineElements = line.Split(',');
                    Tube newTube = new Tube();
                    newTube.Name = lineElements[0].Trim('\"');
                    Coordinate coordinate = new Coordinate();
                    coordinate.Latitude = Convert.ToDouble(lineElements[1]);
                    coordinate.Longitude = Convert.ToDouble(lineElements[2]);
                    newTube.Coordinate = coordinate;
                    tubes.Add(newTube);
                }
                return tubes;
            }
        }

        internal KeyValuePair<int, Queue<Target>> GetDroneTargets(string path)
        {
            using (StreamReader reader = new StreamReader(path))
            {
                Target firstTarget = new Target();
                Target secondTarget = new Target();
                int droneId = 0;
                Queue<Target> queue = new Queue<Target>();
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    string[] lineElements = line.Split(',');
                    var newTarget = new Target();
                    newTarget.DroneId = Convert.ToInt32(lineElements[0]);
                    if (droneId != newTarget.DroneId)
                    {
                        droneId = newTarget.DroneId;
                    }
                    Coordinate coordinate = new Coordinate();
                    coordinate.Latitude = Convert.ToDouble(lineElements[1].Trim('\"'));
                    coordinate.Longitude = Convert.ToDouble(lineElements[2].Trim('\"'));
                    newTarget.Coordinate = coordinate;
                    newTarget.Time = Convert.ToDateTime(lineElements[3].Trim('\"'));
                    queue.Enqueue(newTarget);
                    if (firstTarget is null)
                    {
                        firstTarget = newTarget;
                    }
                    else if (secondTarget is null)
                    {
                        secondTarget = newTarget;
                    }
                }

                return new KeyValuePair<int, Queue<Target>>(droneId, queue);
            }
        }

        internal void Print(string messageIn)
        {
            messages.Enqueue(messageIn);
            lock (lockObject)
            {
                if (isPrinting) return;
                
                isPrinting = true;
                Task.Run(() => PrintMessage());
            }
        }

        private static string currentPath = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
        private static string folderPathToWriteReports = currentPath + @"..\..\..\..\..\Logs"; 
        private static bool isPrinting = false;
        private static object lockObject = new object();
        private static ConcurrentQueue<string> messages = new ConcurrentQueue<string>();

        private static void PrintMessage()
        {
            while (messages.Count > 0)
            {
                if (!File.Exists(folderPathToWriteReports))
                {
                    Directory.CreateDirectory(folderPathToWriteReports);
                }

                string filePath = folderPathToWriteReports + @"\log.txt";

                string startMessage = "";
                if (!File.Exists(filePath))
                {
                    // Create a file to write to.
                    startMessage = "***********************************************" +
                        $"\nThis is the log file where the drones reports are" +
                        $"\n***********************************************\n" + Environment.NewLine;
                    File.Create(filePath);
                }

                string message;
                while (!messages.TryDequeue(out message)) ;

                File.AppendAllText(filePath, (startMessage + message + Environment.NewLine));
            }

            lock (lockObject)
            {
                isPrinting = false;
            }
        }
    }
}
