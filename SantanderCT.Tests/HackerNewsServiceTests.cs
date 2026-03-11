using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using SantanderCT.Models;
using SantanderCT.Services;
using System.Net;
using System.Text;
using System.Text.Json;

namespace SantanderCT.Tests
{
    [TestFixture]
    public class HackerNewsServiceTests
    {
        private IMemoryCache _cache;

        [SetUp]
        public void Setup()
        {
            _cache = new MemoryCache(new MemoryCacheOptions());
        }

        [TearDown]
        public void TearDown()
        {
            _cache.Dispose();
        }

        private HttpClient CreateHttpClientForIdsAndItems(List<int> ids, List<HackerNewsItem> items)
        {
            var handler = new Mock<HttpMessageHandler>();
            int callCount = 0;

            handler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(() =>
                {
                    if (callCount == 0)
                    {
                        callCount++;
                        return new HttpResponseMessage
                        {
                            StatusCode = HttpStatusCode.OK,
                            Content = new StringContent(JsonSerializer.Serialize(ids), Encoding.UTF8, "application/json")
                        };
                    }

                    var item = items[callCount - 1];
                    callCount++;

                    return new HttpResponseMessage
                    {
                        StatusCode = HttpStatusCode.OK,
                        Content = new StringContent(JsonSerializer.Serialize(item), Encoding.UTF8, "application/json")
                    };
                });

            return new HttpClient(handler.Object);
        }

        [Test]
        public async Task GetBestStoriesAsync_ReturnsEmptyList_WhenNoIds()
        {
            // Arrange
            var ids = new List<int>();
            var items = new List<HackerNewsItem>();
            var httpClient = CreateHttpClientForIdsAndItems(ids, items);
            var service = new HackerNewsService(httpClient, _cache);

            // Act
            var result = (await service.GetBestStoriesAsync(5)).ToList();

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetBestStoriesAsync_ReturnsStoriesSortedByScore()
        {
            // Arrange
            var ids = new List<int> { 1, 2 };
            var items = new List<HackerNewsItem>
            {
                new HackerNewsItem { id = 1, score = 100, title="Story A", by="user A", time=1, descendants=10, type="story" },
                new HackerNewsItem { id = 2, score = 200, title="Story B", by="user B", time=1, descendants=20, type="story" }
            };
            var httpClient = CreateHttpClientForIdsAndItems(ids, items);
            var service = new HackerNewsService(httpClient, _cache);

            // Act
            var result = (await service.GetBestStoriesAsync(2)).ToList();

            // Assert
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Score, Is.GreaterThan(result[1].Score));
            Assert.That(result[0].Title, Is.EqualTo("Story B"));
        }
    }
}
