using Microsoft.Extensions.Caching.Memory;
using SantanderCT.Interfaces;
using SantanderCT.Models;
using System.Buffers.Text;

namespace SantanderCT.Services
{
    public class HackerNewsService : IHackerNewsService
    {
        private readonly string BaseUrl = "https://hacker-news.firebaseio.com/v0/item";
        private readonly HttpClient _httpClient;
        private readonly IMemoryCache _cache;
        private const int CacheExpireTime = 5;
        private const int MaxConcurrency = 10;
        private const int FetchLimit = 30; // limit to 30 stories just for performance

        // In a production environment this goes to configuration like appsettings.json or environment variables or a secrets store.
        private const string BestStoriesUrl = "https://hacker-news.firebaseio.com/v0/beststories.json";

        public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
        {
            _httpClient = httpClient;
            _cache = cache;
        }

        public async Task<IEnumerable<HackerNewsStory>> GetBestStoriesAsync(int n)
        {
            var stories = await _cache.GetOrCreateAsync("bestStories", async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CacheExpireTime);

                var ids = await _httpClient.GetFromJsonAsync<List<int>>(BestStoriesUrl);

                var semaphore = new SemaphoreSlim(MaxConcurrency);

                var tasks = ids.Take(FetchLimit).Select(async id =>
                {
                    await semaphore.WaitAsync();

                    try
                    {
                        return await GetStoryAsync(id);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                var results = await Task.WhenAll(tasks);

                return results
                    .Where(s => s != null)
                    .OrderByDescending(s => s.Score)
                    .ToList();
            });

            return stories.Take(n);
        }

        private async Task<HackerNewsStory> GetStoryAsync(int id)
        {
            var item = await _httpClient.GetFromJsonAsync<HackerNewsItem>($"{BaseUrl}/{id}.json");

            if (item == null)
                return null;

            return new HackerNewsStory
            {
                Id = item.id,
                Title = item.title,
                Uri = item.url,
                PostedBy = item.by,
                Time = DateTimeOffset.FromUnixTimeSeconds(item.time ?? 0).UtcDateTime,
                Score = item.score ?? 0,
                CommentCount = item.descendants ?? 0,
                Type = item.type,
                Dead = item.dead,
                Deleted = item.deleted
            };
        }
    }
}
