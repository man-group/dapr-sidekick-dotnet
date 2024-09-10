# Scheduler example

This sample shows how Dapr Sidekick can be used to host the Dapr Scheduler service alongside a Dapr Sidecar instance. Sidekick will launch the Scheduler service and wait for it
to enter a healthy running state, then will launch the Sidecar alongside. The Sidecar will be configured to use the launched Scheduler service by default.
As with the Sidecar, the Scheduler service will be continually monitored and maintained for the lifetime of the application.

## How Dapr Sidekick was added

This is an ASP.NET Core minimal Web API project. Dapr Sidekick was added to the [Program.cs](SchedulerSample\Program.cs) file as follows:

```csharp
// Add Dapr Sidekick with Scheduler
builder.Services.AddDaprSidekick(builder.Configuration)
    .AddScheduler();
```

Typically when installing Dapr in self-hosted mode, a Scheduler service container is added to Docker exposing the default port 6060. If this same is run
while that container is up it will be unable to start due to a port conflict. Instead a different port 6061 is assigned to Scheduler in configuration:

```json5
"Scheduler": {
  "RuntimeDirectory": "scheduler",
  "Id": "dapr-scheduler-server-0",         // Optional unique identifier when used in a cluster
  "Port": 6061                      // To avoid conflicts with local Dapr Scheduler container. Sidecar will use this automatically as well.
}
```

By default the Sidecar that is launched alongside the Scheduler service will look for the Scheduler service locally on this custom port,
unless a specific remote address is defined. For example the following specifies a three-host remote Scheduler cluster:

```json5
"Sidecar": {
  "SchedulerHostAddress": "remote-host-1:50006,remote-host-2:50006,remote-host-3:50006"
}
```

## Running the sample

 To run the sample simply set `SchedulerSample` as the startup project and run it in Visual Studio,
 it will launch first the Scheduler service then the Dapr sidecar, then open a browser and display
 the configured launch options for both.

