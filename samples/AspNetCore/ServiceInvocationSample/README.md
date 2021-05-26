# Service Invocation Sample

This example is based on the standard ASP.NET Core 3.1 Web API WeatherForecast template and demonstrates the Service Invocation feature of Dapr. 

The `WeatherForecastController` constructor has been extended with an `IDaprSidecarHost` which returns information about the running Dapr Sidecar, and an `IDaprSidecarHttpClientFactory` for creating `HttpClient` instances that can invoke other Dapr services by their `AppId`. The following methods are exposed on the controller:

| Method                     | Description                                                                  |
| -------------------------- | ---------------------------------------------------------------------------- |
| `/weatherforecast`         | Unchanged from the template, returns a list of weather forecast results      |
| `/weatherforecast/status`  | Returns information about the Dapr sidecar status and its startup parameters |
| `/weatherforecast/sidecar` | Invokes the `/weatherforecast` method via the Dapr sidecar                   |

## Running the sample

Set this project as the Startup Project in Visual Studio 2019 and run it. A console window appears with diagnostic startup messages and the browser opens on the `status` page which should indicate that the Dapr sidecar is starting up. Note that it looks for Dapr in the default `dapr init` installation location `%USERPROFILE%/.dapr` (`$HOME/.dapr` on Linux). Monitor the logs in the console window until Dapr enters the `Started` state then refresh the page to view the updated Dapr process status and all the startup details
including dynamic port assignments.

In the browser navigate to the `/weatherforecast` method which will return random weather forecast results. Now navigate to `/weatherforecast/sidecar` which will return similar results but this time the call is looped back via the Dapr sidecar to illustrate Dapr service invocation.

## Stopping the application and Dapr

To stop the application and terminate Dapr you can enter `CTRL-C` in the console window which will invoke the Dapr Shutdown API to perform a clean shutdown. Alternative close the browser window or stop the debug session directly
and the Dapr process will be immediately terminated.

## How Dapr was added

The main change to the template code to add Dapr support can be found in the `ConfigureServices` method in `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Add Dapr Sidekick
    services.AddDaprSidekick(Configuration);
}
```

Additional code has also been included to add enhanced logging via Serilog for improved diagnostics.
