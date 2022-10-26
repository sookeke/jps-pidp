using edt.service.Kafka.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace edt.service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IKafkaProducer<string, WeatherForecast> _kafkaProducer;


        public WeatherForecastController(ILogger<WeatherForecastController> logger, IKafkaProducer<string, WeatherForecast> kafkaProducer)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {
            var f = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();

            await this._kafkaProducer.ProduceAsync("beer-events", Guid.NewGuid().ToString(), f.FirstOrDefault());

            return f;
        }
    }
}
