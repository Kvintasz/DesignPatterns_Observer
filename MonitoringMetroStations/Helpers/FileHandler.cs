using MonitoringMetroStations.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringMetroStations.Helpers
{
    public class FileHandler
    {
        public static FileHandler GetInstance 
        { 
            get
            {
                return instance;
            }
        }

        private FileHandler()
        {
            currentPath = Directory.GetParent(typeof(Program).Assembly.Location).FullName;
            folderPathToWriteReports = currentPath + @"..\..\..\..\..\Logs";
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

        /// <summary>
        /// This method add the given string to the queue
        /// If the printing task doesn't run it will start it otherwise doesn't do anything else.
        /// </summary>
        /// <param name="messageIn">The text that the caller wants to log to the log file</param>
        public void Print(string messageIn)
        {
            messages.Enqueue(messageIn);
            lock (lockObject)
            {
                if (isPrinting) return;

                isPrinting = true;
                printingTask = Task.Run(() => PrintMessage());
            }
        }

        private static readonly FileHandler instance = new FileHandler();
        private string currentPath;
        private string folderPathToWriteReports;
        private bool isPrinting = false;
        private object lockObject = new object();
        private ConcurrentQueue<string> messages = new ConcurrentQueue<string>();
        private Task printingTask;
        private ReaderWriterLockSlim readWriteLock = new ReaderWriterLockSlim();

        /// <summary>
        /// Writes the text from the queue
        /// </summary>
        private void PrintMessage()
        {
            while (messages.Count > 0)
            {
                string filePath = folderPathToWriteReports + @"\log.txt";

                string message;
                while (!messages.TryDequeue(out message)) ;

                WriteToFileThreadSafe(message + Environment.NewLine,
                    filePath);
            }

            lock (lockObject)
            {
                isPrinting = false;
            }
        }

        /// <summary>
        /// Writes the given text into the file which is at the given path.
        /// Does it in a threadsafe way.
        /// </summary>
        /// <param name="text">The text that the caller wants to write into the file</param>
        /// <param name="path">the path where the file is</param>
        private void WriteToFileThreadSafe(string text, string path)
        {
            // Set Status to Locked
            readWriteLock.EnterWriteLock();
            try
            {
                // Append text to the file
                using (StreamWriter sw = File.AppendText(path))
                {
                    sw.WriteLine(text);
                    sw.Close();
                }
            }
            finally
            {
                // Release lock
                readWriteLock.ExitWriteLock();
            }
        }
    }
}
