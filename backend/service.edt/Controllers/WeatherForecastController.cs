using System.Diagnostics;
using edt.service.Infrastructure.Telemetry;
using edt.service.Kafka.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Prometheus;

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
        private readonly OtelMetrics _metrics;
        private static readonly Counter WeatherRequests = Metrics.CreateCounter("weather_requests", "Total number of weather requests.");

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IKafkaProducer<string, WeatherForecast> kafkaProducer, OtelMetrics metrics)
        {
            _logger = logger;
            _kafkaProducer = kafkaProducer;
            _metrics = metrics;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<IEnumerable<WeatherForecast>> Get()
        {

            using (var activity = new Activity("Weather Request").Start())
            {
                WeatherRequests.Inc();
                Activity.Current?.AddTag("weather.test", "test");

                var f = Enumerable.Range(1, 5).Select(index => new WeatherForecast
                {
                    Date = DateTime.Now.AddDays(index),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                })
                .ToArray();
                this._metrics.GetWeather();
                await this._kafkaProducer.ProduceAsync("beer-events", Guid.NewGuid().ToString(), f.FirstOrDefault());

                return f;
            }
        }
    }
}
