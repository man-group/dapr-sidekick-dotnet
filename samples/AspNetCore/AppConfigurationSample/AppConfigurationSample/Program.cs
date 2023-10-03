using Dapr.Client;
using Dapr.Extensions.Configuration;
using Man.Dapr.Sidekick;
using Man.Dapr.Sidekick.Threading;
using Serilog;

// Add Serilog for enhanced console logging.
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();
    var env = builder.Environment;

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

    // Add services to the container.

    builder.Services
        .AddControllers()
        .AddDapr();
    builder.Services.AddDaprClient();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (env.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // Wait for the Dapr sidecar to report healthy before attempting to use any Dapr components.
    var client = new DaprClientBuilder().Build();
    using (var tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
    {
        // use the await keyword in your service instead
        await client.WaitForSidecarAsync(tokenSource.Token);
    }

    // Get secrets from store during startup
    var secret = await client.GetSecretAsync("localsecretstore", "secret");

    // Use secrets to setup your services
    Log.Logger.Information("----- Secret from DaprClient: " + string.Join(",", secret.Select(d => d.Value)));

    // or forward them in the ConfigurationManager

    builder.Configuration.AddDaprSecretStore("localsecretstore", client);

    Log.Logger.Information("----- Secret from ConfigurationManager: " + builder.Configuration.GetSection("secret").Value);

    app.UseHttpsRedirection();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
