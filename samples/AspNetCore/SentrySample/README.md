# ASP.NET Core Controller example

This sample shows using Dapr with ASP.NET Core controllers. This application is a simple and not-so-secure banking application. The application uses the Dapr state-store for its data storage. The sample is a copy of the [Dapr .NET SDK Controller example](https://github.com/dapr/dotnet-sdk/tree/master/examples/AspNetCore/ControllerSample), but modified to include Dapr Sidekick integration for launching a Dapr sidecar.

> See the [original source](https://github.com/dapr/dotnet-sdk/tree/master/examples/AspNetCore/ControllerSample) for more information on how to use the sample.

## How Dapr Sidekick was added

The main change to the template code to add Dapr support can be found in the `ConfigureServices` method in `Startup.cs`:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    ...
    // Add Dapr Sidekick
    services.AddDaprSidekick(Configuration);
}
```

## Running the sample

 To run the sample simply set `ControllerSample` as the startup project and run it in Visual Studio, it will launch the Dapr sidecar and connect to it.

 For all further instructions, including prerequisites and testing, please refer to the [original source](https://github.com/dapr/dotnet-sdk/tree/master/examples/AspNetCore/ControllerSample).
