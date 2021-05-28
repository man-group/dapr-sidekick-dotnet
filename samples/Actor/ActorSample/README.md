# Actor Sample

The Actor sample shows how to create a virtual actor (`DemoActor`) and invoke its methods on the client application. The sample is a copy of the [Dapr .NET SDK Actor Sample](https://github.com/dapr/dotnet-sdk/tree/master/examples/Actor), but modified to include Dapr Sidekick integration for launching a Dapr sidecar.

> See the [original source](https://github.com/dapr/dotnet-sdk/tree/master/examples/Actor) for more information on the purpose of the sample.

## How Dapr Sidekick was added

The main change to the template code to add Dapr support can be found in the `ConfigureServices` method in `Startup.cs` in the `ActorSample.DemoActor` project:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Add Dapr Sidekick
    services.AddDaprSidekick(Configuration);
}
```

## Running the example

This sample requires a full initialized Dapr installation with both the `dapr_placement` and `dapr_redis` containers up and running. If this is not the case please follow the instructions to [Init Dapr Locally](https://docs.dapr.io/getting-started/install-dapr-selfhost/).

Build the solution and set the `ActorSample.DemoActor` project as the startup, then run it in Visual Studio. The Dapr Sidecar will launch and the hosted actor will use it to registered with the placement service.

> By default Dapr will search for the Placement service on port 6050 on Windows, or 50005 on other platforms. This is the port exposed by the `dapr_placement` container.

Navigate to the ActorClient build output directory at `<RepoRoot>/bin/<Debug|Release>/samples/ActorSample.ActorClient/netcoreapp3.1` and run `ActorSample.ActorClient.exe`. This will connect to the same Dapr sidecar on port 3500 and submit calls to the hosted Actor.
