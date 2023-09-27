// ------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT License.
// ------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapr.Client;
using Man.Dapr.Sidekick;
using Man.Dapr.Sidekick.Threading;
using Serilog;

namespace ControllerSample
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;

    /// <summary>
    /// Startup class.
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">Configuration.</param>
        /// <param name="environment">environment.</param>
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        public IConfiguration Configuration { get; }

        public IHostEnvironment Environment { get; }

        /// <summary>
        /// Configures Services.
        /// </summary>
        /// <param name="services">Service Collection.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Manage local dapr sidecar
            if (Environment.IsDevelopment())
            {
                // Set options in code
                var sidecarOptions = new DaprSidecarOptions
                {
                    AppId = "controllersample",
                    AppPort = 5000,
                    ResourcesDirectory = Path.Combine(Environment.ContentRootPath, "Dapr/Components"),

                    // Set the working directory to our project to allow relative paths in component yaml files
                    WorkingDirectory = Environment.ContentRootPath
                };

                // Build the default Sidekick controller
                var sidekick = new DaprSidekickBuilder().Build();

                // Start the Dapr Sidecar early in the pipeline, this will come up in the background
                sidekick.Sidecar.Start(() => new DaprOptions { Sidecar = sidecarOptions }, DaprCancellationToken.None);

                // Add Dapr Sidekick
                services.AddDaprSidekick(Configuration, o =>
                {
                    o.Sidecar = sidecarOptions;
                });
            }

            // Wait for the Dapr sidecar to report healthy before attempting to use any Dapr components.
            var client = new DaprClientBuilder().Build();
            using (var tokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(1)))
            {
                // use the await keyword in your service instead
                client.WaitForSidecarAsync(tokenSource.Token).ConfigureAwait(false);
            }

            // Get secrets from store during startup
            var secret = client.GetSecretAsync("localsecretstore", "secret").Result;

            Log.Logger.Information("----- Secret: " + string.Join(",", secret.Select(d => d.Value)));

            services.AddControllers().AddDapr();
        }

        /// <summary>
        /// Configures Application Builder and WebHost environment.
        /// </summary>
        /// <param name="app">Application builder.</param>
        /// <param name="env">Webhost environment.</param>
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseCloudEvents();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapSubscribeHandler();
                endpoints.MapControllers();
            });
        }
    }
}
