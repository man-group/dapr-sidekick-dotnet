using System;
using System.ServiceModel;
using System.ServiceModel.Web;
using Dapr.Sidekick;
using Dapr.Sidekick.Threading;

namespace WcfSample
{
    public class Program
    {
        public static DaprSidekick Sidekick { get; private set; }

#pragma warning disable RCS1163 // Unused parameter.
        public static void Main(string[] args)
#pragma warning restore RCS1163 // Unused parameter.
        {
            // If you get an access error when starting the WCF Host you
            // probably need to add a URL ACL for this port. Run the following as an Administrator:
            //
            //   netsh http add urlacl url=http://+:8500/ user=Everyone
            //
            // See https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/configuring-http-and-https?redirectedfrom=MSDN
            var appPort = 8500;

            // Set options in code
            var options = new DaprOptions
            {
                Sidecar = new DaprSidecarOptions
                {
                    AppPort = appPort
                }
            };

            // Build the default Sidekick controller
            Sidekick = new DaprSidekickBuilder().Build();

            // Start the Dapr Sidecar, this will come up in the background
            Sidekick.Sidecar.Start(() => options, DaprCancellationToken.None);

            // Open the WCF HTTP Service Host at the specified port
            var host = new WebServiceHost(typeof(WeatherForecastService), new Uri($"http://localhost:{options.Sidecar.AppPort.Value}/"));
            var ep = host.AddServiceEndpoint(typeof(IWeatherForecastService), new WebHttpBinding(), string.Empty);
            host.Open();

            // Wait for ENTER
            Console.WriteLine("WCF HTTP Service Host is running");
            Console.WriteLine("Press ENTER to quit...");
            Console.ReadLine();
            host.Close();

            // Shut down the Dapr Sidecar - this is a blocking call
            Sidekick.Sidecar.Stop(DaprCancellationToken.None);
        }
    }
}
