using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Man.Dapr.Sidekick;
using Man.Dapr.Sidekick.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace ServiceInvocationSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IDaprSidecarHost _daprSidecarHost;
        private readonly IDaprSidecarHttpClientFactory _httpClientFactory;

        public WeatherForecastController(
            IDaprSidecarHost daprSidecarHost,
            IDaprSidecarHttpClientFactory httpClientFactory)
        {
            _daprSidecarHost = daprSidecarHost;
            _httpClientFactory = httpClientFactory;
        }

        [HttpGet]
        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }

        [HttpGet("status")]
        public ActionResult GetStatus() => Ok(new
        {
            process = _daprSidecarHost.GetProcessInfo(),   // Information about the sidecar process such as if it is running
            options = _daprSidecarHost.GetProcessOptions() // The sidecar options if running, including ports and locations
        });

        [HttpGet("sidecar")]
        public async Task<IEnumerable<WeatherForecast>> GetViaSidecar()
        {
            // Get a Dapr Service Invocation http client for this service's AppId.
            // This will perform a local round-trip call to the sidecar and back
            // to this controller to demonstrate service invocation in action.
            var appId = _daprSidecarHost.GetProcessOptions()?.AppId;
            if (appId == null)
            {
                // AppId not available, sidecar probably not running
                return null;
            }

            // Create an HttpClient for this service appId
            var httpClient = _httpClientFactory.CreateInvokeHttpClient(appId);

            // Invoke the relative endpoint on target service
            // In this case it will invoke the default Get method on this controller
            var result = await httpClient.GetStringAsync("/weatherforecast");

            // Deserialize and return the result
            return JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(result);
        }
    }
}
