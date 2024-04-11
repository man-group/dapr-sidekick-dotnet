# Placement example

This sample shows how Dapr Sidekick can be used to host the Dapr Placement service alongside a Dapr Sidecar instance. Sidekick will launch the Placement service and wait for it
to enter a healthy running state, then will launch the Sidecar alongside. The Sidecar will be configured to use the launched Placement service by default.
As with the Sidecar, the Placement service will be continually monitored and maintained for the lifetime of the application.

## How Dapr Sidekick was added

This is an ASP.NET Core minimal Web API project. Dapr Sidekick was added to the [Program.cs](PlacementSample\Program.cs) file as follows:

```csharp
// Add Dapr Sidekick with Placement
builder.Services.AddDaprSidekick(builder.Configuration)
    .AddPlacement();
```

Typically when installing Dapr in self-hosted mode, a Placement service container is added to Docker exposing the default port 6500. If this samle is run
while that container is up it will be unable to start due to a port conflict. Instead a different port 6501 is assigned to Placement in configuration:

```json5
"Placement": {
  "RuntimeDirectory": "placement",
  "Id": "dapr-placement-0",         // Optional unique identifier when used in a cluster
  "Port": 6051                      // To avoid conflicts with local Dapr Placement container. Sidecar will use this automatically as well.
}
```

By default the Sidecar that is launched alongside the Placement service will look for the Placement service locally on this custom port,
unless a specific remote address is defined. For example the following specifies a three-host remote Placement cluster:

```json5
"Sidecar": {
  "PlacementHostAddress": "remote-host-1:6050,remote-host-2:6050,remote-host-3:6050"
}
```

## Running the sample

 To run the sample simply set `PlacementSample` as the startup project and run it in Visual Studio,
 it will launch first the Placement service then the Dapr sidecar, then open a browser and display
 the configured launch options for both.

