using Newspaper.Observer;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Newspaper.Observable
{
    public class Newsstand : INewsstand
    {
        public IList<IReader> Subscribers { get; } = new List<IReader>();
        public string Name { get; }

        public Task WaitingForNewspaperTask { get; }

        public Newsstand(string nameIn)
        {
            Name = nameIn;
            WaitingForNewspaperTask = Task.Run(() => WaitingForTheNewspaper());
        }

        public void Subscribe(IReader reader)
        {
            if (Subscribers.Contains(reader)) return;

            Subscribers.Add(reader);
            Console.WriteLine($"{reader.Name} subscribed for the {Name}'s event");
        }

        public void Unsubscribe(IReader reader)
        {
            if (Subscribers.Contains(reader))
            {
                Subscribers.Remove(reader);
            }
        }

        private void NotifySubscribers()
        {
            foreach (IReader reader in Subscribers)
            {
                reader.UpdateNewspaperStatus(this);
            }
        }

        private void WaitingForTheNewspaper()
        {
            Random r = new Random();
            Thread.Sleep(r.Next(100, 5000));
            NotifySubscribers();
        }
    }
}
