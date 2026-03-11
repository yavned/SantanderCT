using SantanderCT.Models;

namespace SantanderCT.Interfaces
{
    public interface IHackerNewsService
    {
        Task<IEnumerable<HackerNewsStory>> GetBestStoriesAsync(int n);
    }
}
