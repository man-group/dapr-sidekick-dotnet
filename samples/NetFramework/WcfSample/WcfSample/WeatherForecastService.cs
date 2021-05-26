using System;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Web;
using System.Threading;
using Man.Dapr.Sidekick;
using Newtonsoft.Json;

namespace WcfSample
{
    // This service makes a callback to itself via Dapr so it must be per-call else we'll get blocking.
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerCall)]
    public class WeatherForecastService : IWeatherForecastService
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private DaprSidekick Sidekick => Program.Sidekick;

        public Message Health() => ToJsonMessage(Sidekick.Sidecar.GetHealthAsync(CancellationToken.None).Result);

        public Message Metadata() => ToTextMessage(
            s => Sidekick.Sidecar.WriteMetadataAsync(s, CancellationToken.None).Wait(),
            DaprConstants.ContentTypeApplicationJson);

        public Message Metrics() => ToTextMessage(
            s => Sidekick.Sidecar.WriteMetricsAsync(s, CancellationToken.None).Wait());

        public Message Status() => ToJsonMessage(new
        {
            process = Sidekick.Sidecar.GetProcessInfo(),   // Information about the sidecar process such as if it is running
            options = Sidekick.Sidecar.GetProcessOptions() // The sidecar options if running, including ports and locations
        });

        public Message Weather()
        {
            var rng = new Random();
            return ToJsonMessage(Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            }));
        }

        public Message WeatherSidecar()
        {
            // Get a Dapr Service Invocation http client for this service's AppId.
            // This will perform a local round-trip call to the sidecar and back
            // to this controller to demonstrate service invocation in action.
            var appId = Sidekick.Sidecar.GetProcessOptions()?.AppId;
            if (appId == null)
            {
                // AppId not available, sidecar probably not running
                return null;
            }

            // Create an HttpClient for this service appId
            var httpClient = Sidekick.HttpClientFactory.CreateInvokeHttpClient(appId);

            // Invoke the relative endpoint on target service
            // In this case it will invoke the default Get method on this controller
            var result = httpClient.GetStringAsync("/Weather").Result;

            // Return the result
            return WebOperationContext.Current.CreateTextResponse(result, DaprConstants.ContentTypeApplicationJson);
        }

        private Message ToJsonMessage(object value) => WebOperationContext.Current.CreateTextResponse(
            JsonConvert.SerializeObject(value, Formatting.Indented), DaprConstants.ContentTypeApplicationJson);

        private Message ToTextMessage(Action<Stream> writeValue, string contentType = DaprConstants.ContentTypeTextPlain)
        {
            var ms = new MemoryStream();
            writeValue(ms);
            ms.Position = 0;
            var text = System.Text.Encoding.UTF8.GetString(ms.ToArray());
            return WebOperationContext.Current.CreateTextResponse(text, contentType);
        }
    }
}
