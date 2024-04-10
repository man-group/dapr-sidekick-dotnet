using Man.Dapr.Sidekick;
using Serilog;

// Add Serilog for enhanced console logging.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add Dapr Sidekick with Placement
builder.Services.AddDaprSidekick(builder.Configuration)
    .AddPlacement();

builder.Host.UseSerilog();

var app = builder.Build();

app.MapGet("/status", (IDaprSidecarHost sidecarHost, IDaprPlacementHost placementHost) => Results.Ok(new
{
    sidecar = new
    {
        process = sidecarHost.GetProcessInfo(),   // Information about the sidecar process such as if it is running
        options = sidecarHost.GetProcessOptions() // The sidecar options if running, including ports and locations
    },
    placement = new
    {
        process = placementHost.GetProcessInfo(),   // Information about the sentry process such as if it is running
        options = placementHost.GetProcessOptions() // The sentry options if running, including ports and locations
    },
}));

// For Dapr
app.MapHealthChecks("/health");

app.Run();
