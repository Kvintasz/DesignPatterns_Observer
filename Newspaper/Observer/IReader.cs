using Newspaper.Observable;

namespace Newspaper.Observer
{
    public interface IReader
    {
        string Name { get; }
        void UpdateNewspaperStatus(INewsstand stand);
    }
}
