using Man.Dapr.Sidekick;
using Serilog;

// Add Serilog for enhanced console logging.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// Add Dapr Sidekick with Scheduler
builder.Services.AddDaprSidekick(builder.Configuration)
    .AddScheduler();

builder.Host.UseSerilog();

var app = builder.Build();

app.MapGet("/status", (IDaprSidecarHost sidecarHost, IDaprSchedulerHost schedulerHost) => Results.Ok(new
{
    sidecar = new
    {
        process = sidecarHost.GetProcessInfo(),   // Information about the sidecar process such as if it is running
        options = sidecarHost.GetProcessOptions() // The sidecar options if running, including ports and locations
    },
    scheduler = new
    {
        process = schedulerHost.GetProcessInfo(),   // Information about the sentry process such as if it is running
        options = schedulerHost.GetProcessOptions() // The sentry options if running, including ports and locations
    },
}));

// For Dapr
app.MapHealthChecks("/health");

app.Run();
