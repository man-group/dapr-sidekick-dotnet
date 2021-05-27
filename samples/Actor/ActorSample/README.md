# Actor Sample

The Actor sample shows how to create a virtual actor (`DemoActor`) and invoke its methods on the client application. The sample is a copy of the [Dapr .NET SDK Actor Sample](https://github.com/dapr/dotnet-sdk/tree/master/examples/Actor), but modified to include Dapr Sidekick integration for launching a Dapr sidecar and optionally the Dapr Placement service.

> See the [original source](https://github.com/dapr/dotnet-sdk/tree/master/examples/Actor) for more information on the purpose of the sample.

## How Dapr Sidekick was added

The main change to the template code to add Dapr support can be found in the `ConfigureServices` method in `Startup.cs` in the `ActorSample.DemoActor` project:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Add Dapr Sidekick with both Sidecar and Placement support
    services.AddDaprSidekick(Configuration)
        .AddPlacement();
}
```

The following section has also been added in `appsettings.json` in the same project:

```json
{
  "Dapr": {
    "Placement": {
      "Enabled":  false,
      "Port": 6060
    }
  }
}
```

> This configuration will initially disable the Placement service support in Dapr Sidekick, we'll come back to this shortly.

## Running the example

This sample requires a full initialized Dapr installation with both the `dapr_placement` and `dapr_redis` containers up and running. If this is not the case please follow the instructions to [Init Dapr Locally](https://docs.dapr.io/getting-started/install-dapr-selfhost/).

Build the solution and set the `ActorSample.DemoActor` project as the startup, then run it in Visual Studio. The Dapr Sidecar will launch and the hosted actor will use it to registered with the placement service.

> By default Dapr will search for the Placement service on port 6050 on Windows, or 50005 on other platforms. This is the port exposed by the `dapr_placement` container on Windows.

Navigate to the ActorClient build output directory at `<RepoRoot>/bin/<Debug|Release>/samples/ActorSample.ActorClient/netcoreapp3.1` and run `ActorSample.ActorClient.exe`. This will connect to the same Dapr sidecar on port 3500 and submit calls to the hosted Actor.

Stop the running solution in Visual Studio, then change the `Dapr:Placement:Enabled` property in `appsettings.json` from `false` to `true`:

```json
{
  "Dapr": {
    "Placement": {
      "Enabled":  true, // <-- Change this from false to true
      "Port": 6060
    }
  }
}
```

Run the solution in Visual Studio again - this time the Placement service will come up first (pay close attention to the log messages!) on port 6060, followed by the Sidecar which will automatically connect to that port instead of the default container instance. Run the `ActorSample.ActorClient.exe` application again and this time you will see additional logging occuring in the `ActorSample.DemoActor` console output window showing the locally-hosted Placement service handling the requests.
