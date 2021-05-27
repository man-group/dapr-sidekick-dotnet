using System.Collections.Generic;
using System.Threading.Tasks;
using Man.Dapr.Sidekick;
using Man.Dapr.Sidekick.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace ConsulSample.Sender.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SenderController : ControllerBase
    {
        private readonly IDaprSidecarHost _daprSidecarHost;
        private readonly IDaprSidecarHttpClientFactory _httpClientFactory;
        private readonly ILogger<SenderController> _logger;

        public SenderController(
            IDaprSidecarHost daprSidecarHost,
            IDaprSidecarHttpClientFactory httpClientFactory,
            ILogger<SenderController> logger)
        {
            _daprSidecarHost = daprSidecarHost;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        [HttpGet("DaprStatus")]
        public ActionResult GetDaprStatus() => Ok(new
        {
            process = _daprSidecarHost.GetProcessInfo(),   // Information about the sidecar process such as if it is running
            options = _daprSidecarHost.GetProcessOptions() // The sidecar options if running, including ports and locations
        });

        [HttpGet("DaprWeatherForecast")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetDaprWeatherForecast()
        {
            if (!_daprSidecarHost.GetProcessInfo().IsRunning)
            {
                // Dapr not running
                return NotFound();
            }

            // Create an HttpClient for ther receiver AppId
            var httpClient = _httpClientFactory.CreateInvokeHttpClient("consulsample-receiver");

            // Invoke the relative endpoint on target service
            // In this case it will invoke the default Get method on this controller
            var result = await httpClient.GetStringAsync("/weatherforecast");

            // Deserialize and return the result
            return Ok(JsonConvert.DeserializeObject<IEnumerable<WeatherForecast>>(result));
        }
    }
}
