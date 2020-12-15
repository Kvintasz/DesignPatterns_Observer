using Newspaper.Observable;
using Newspaper.Observer;
using System;
using System.Threading.Tasks;

namespace Newspaper
{
    class Program
    {
        static void Main(string[] args)
        {
            Newsstand stand1 = new Newsstand("Favourite newsstand");
            Newsstand stand2 = new Newsstand("Another newsstand");

            Reader reader1 = new Reader("Maria");
            Reader reader2 = new Reader("Jesus");
            Reader reader3 = new Reader("Agatha");
            stand1.Subscribe(reader1);
            stand1.Subscribe(reader2);
            stand1.Subscribe(reader3);
            stand2.Subscribe(reader3);

            Task.WaitAll(new Task[]{ stand1.WaitingForNewspaperTask, stand2.WaitingForNewspaperTask});
            Console.WriteLine("Main program: The newspapers are arrived.");
        }
    }
}
