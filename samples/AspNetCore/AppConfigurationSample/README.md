# ASP.NET Core Controller example

This sample shows using Dapr with ASP.NET Core controllers. This application is a simple Web API application.
The application uses a Dapr secret store to read secrets.
The sample is starting a Dapr Sidekick process very early in the execution of the `Program.cs`
and then waits for the sidecar components to be loaded so that we can read secrets from the secret store 
and use them when we setup our services.
The Sidekick hosted service will then reuse the same daprd process instance and manage it as usual.

## How Dapr Sidekick was added

The main change to the ASPCore template is the following block of code at the beginning of the `Program.cs`:

```csharp
    // Manage local dapr sidecar
    if (env.IsDevelopment())
    {
        // Set options in code
        var sidecarOptions = new DaprSidecarOptions
        {
            AppId = "AppConfigurationSample",
            AppPort = 5000,
            DaprGrpcPort = 50001,
            ResourcesDirectory = Path.Combine(env.ContentRootPath, "Dapr/Components"),

            // Set the working directory to our project to allow relative paths in component yaml files
            WorkingDirectory = env.ContentRootPath
        };

        // Build the default Sidekick controller
        var sidekick = new DaprSidekickBuilder().Build();

        // Start the Dapr Sidecar early in the pipeline, this will come up in the background
        sidekick.Sidecar.Start(() => new DaprOptions { Sidecar = sidecarOptions }, DaprCancellationToken.None);

        // Add Dapr Sidekick
        builder.Services.AddDaprSidekick(builder.Configuration, o =>
        {
            o.Sidecar = sidecarOptions;
        });
    }
```

## Running the sample

 To run the sample simply set `AppConfigurationSample` as the startup project and run it in Visual Studio, it will launch the Dapr sidecar and connect to it.
