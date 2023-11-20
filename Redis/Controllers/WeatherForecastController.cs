using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Redis.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IDistributedCache _distributedCache;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IDistributedCache distributedCache)
        {
            _logger = logger;
            _distributedCache = distributedCache;
        }

        [HttpGet]
        public async Task<List<WeatherForecast>> SetAsync()
        {
            var value = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }).ToArray();

            var serialize = JsonSerializer.Serialize(value);

            await _distributedCache.SetStringAsync("Key", serialize, new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60)
            });

            List<WeatherForecast> result = JsonSerializer.Deserialize<List<WeatherForecast>>(serialize).ToList();

            return result;
        }

        [HttpGet]
        public async ValueTask<IActionResult> GetAsync()
        {
            var result = await _distributedCache.GetStringAsync("Key");

            return Ok(result);
        }
    }
}