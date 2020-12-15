using Newspaper.Observable;
using System;

namespace Newspaper.Observer
{
    public class Reader : IReader
    {
        public string Name { get; }

        public Reader(string nameIn)
        {
            Name = nameIn;
        }

        public void UpdateNewspaperStatus(INewsstand stand)
        {
            Console.WriteLine($"{Name} is going to the newstand {stand.Name}");
        }
    }
}
