using Microsoft.AspNetCore.Mvc;
using SantanderCT.Interfaces;

namespace SantanderCT.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StoriesController : ControllerBase
    {
        private readonly IHackerNewsService _service;

        public StoriesController(IHackerNewsService service)
        {
            _service = service;
        }

        [HttpGet("best")]
        public async Task<IActionResult> GetBestStories([FromQuery] int n = 10)
        {
            var stories = await _service.GetBestStoriesAsync(n);
            return Ok(stories);
        }
    }
}
