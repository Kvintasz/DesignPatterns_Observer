using Newspaper.Observer;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Newspaper.Observable
{
    public interface INewsstand
    {
        IList<IReader> Subscribers { get; }
        string Name { get; }
        Task WaitingForNewspaperTask { get; }
        void Subscribe(IReader reader);
        void Unsubscribe(IReader reader);
    }
}
