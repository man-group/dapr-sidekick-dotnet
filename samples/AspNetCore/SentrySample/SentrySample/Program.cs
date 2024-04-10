using Man.Dapr.Sidekick;
using Serilog;

// Add Serilog for enhanced console logging.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Dapr Sidekick with Sentry
builder.Services.AddDaprSidekick(builder.Configuration)
    .AddSentry();

// Inject Sentry certificates into the Dapr sidecar to verify mTLS operation
builder.Services.PostConfigure<DaprOptions>(options =>
{
    var certsFolder = "sentry/certs/";

    // Assign security defaults (trust chain, certificates and keys) from embedded resources.
    // This needs to be replaced with proper certificate distribution in the future.
    options.TrustAnchorsCertificate ??= File.ReadAllText(certsFolder + DaprConstants.TrustAnchorsCertificateFilename);
    options.IssuerCertificate ??= File.ReadAllText(certsFolder + DaprConstants.IssuerCertificateFilename);
    options.IssuerKey ??= File.ReadAllText(certsFolder + DaprConstants.IssuerKeyFilename);
});

builder.Host.UseSerilog();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGet("/status", (IDaprSidecarHost sidecarHost, IDaprSentryHost sentryHost) => Results.Ok(new
{
    sidecar = new
    {
        process = sidecarHost.GetProcessInfo(),   // Information about the sidecar process such as if it is running
        options = sidecarHost.GetProcessOptions() // The sidecar options if running, including ports and locations
    },
    sentry = new
    {
        process = sentryHost.GetProcessInfo(),   // Information about the sentry process such as if it is running
        options = sentryHost.GetProcessOptions() // The sentry options if running, including ports and locations
    },
}));

// For Dapr
app.MapHealthChecks("/health");

app.Run();
