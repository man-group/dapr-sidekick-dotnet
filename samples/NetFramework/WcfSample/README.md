# WCF Sample

This example provides a simple WCF HTTP service implementation to demonstrate the Service Invocation feature of Dapr under .NET Framework.

The WCF HTTP service is exposed on port 8500. The sample uses `DaprSidekickBuilder` to create an instance of `DaprSidekick` which is used to start/stop the Dapr Sidecar and provide access to the `IDaprSidecarHttpClientFactory` for creating `HttpClient` instances able to invoke other Dapr services by their `AppId`.

The WCF service is implemented in `WeatherForecastService` which exposes the following methods:

| Method            | Description                                                                  |
| ----------------- | ---------------------------------------------------------------------------- |
| `/Health`         | Returns the health check response from the Dapr Sidecar                      |
| `/Metadata`       | Returns the metadata response from the Dapr Sidecar                          |
| `/Metrics`        | Returns the metrics response from the Dapr Sidecar                           |
| `/Status`         | Returns information about the Dapr sidecar status and its startup parameters |
| `/Weather`        | Returns a list of weather forecast results                                   |
| `/WeatherSidecar` | Invokes the `/Weather` method via the Dapr sidecar                           |

## Running the sample

Set this project as the Startup Project in Visual Studio 2019 and run it. A console window appears with diagnostic startup messages. Dapr Sidekick looks for Dapr in the default `dapr init` installation location `%USERPROFILE%/.dapr` (`$HOME/.dapr` on Linux). Monitor the logs in the console window until Dapr enters the `Started` state.

In a browser navigate to the `/Status` method (default <http://127.0.0.1:8500/Status>) to view the Dapr sidecar process status and all the startup details including dynamic port assignments. Navigate in turn to the `/Health`, `/Metadata` and `/Metrics` methods to view additional information from the sidecar.

Navigate to the `/Weather` method which will return random weather forecast results. Now navigate to `/WeatherSidecar` which will return similar results but this time the call is looped back via the Dapr sidecar to illustrate Dapr service invocation.

## Stopping the application and Dapr

To stop the application and terminate Dapr you can press `ENTER` in the console window which will invoke the Dapr Shutdown API to perform a clean shutdown. Alternative close the browser window or stop the debug session directly
and the Dapr process will be immediately terminated.
