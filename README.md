# Dapr Sidekick for .NET

[![Build Status](https://github.com/man-group/dapr-sidekick-dotnet/workflows/dapr-sidekick-dotnet/badge.svg?event=push&branch=master)](https://github.com/man-group/dapr-sidekick-dotnet/actions?workflow=dapr-sidekick-dotnet)
[![Unit Tests](https://github.com/man-group/dapr-sidekick-dotnet/workflows/dapr-sidekick-dotnet-test/badge.svg?event=schedule)](https://github.com/man-group/dapr-sidekick-dotnet/actions?workflow=dapr-sidekick-dotnet-test)
[![codecov](https://codecov.io/gh/man-group/dapr-sidekick-dotnet/branch/master/graph/badge.svg)](https://codecov.io/gh/man-group/dapr-sidekick-dotnet)
[![License: MIT](https://img.shields.io/badge/License-Apache2.0-yellow.svg)](https://opensource.org/licenses/Apache-2.0)
[![Follow on Twitter](https://img.shields.io/twitter/follow/ManGroup.svg?style=social&logo=twitter)](https://twitter.com/intent/follow?screen_name=ManGroup)

Dapr Sidekick for .NET is a control plane component that makes adding Dapr to your solutions frictionless. It simplifies the development and operations of distributed .NET applications that use [Dapr](https://dapr.io/) by providing lifetime management, core service invocation and improved debugging experiences across a wide range of .NET platforms and development tools. While it does not require the official [Dapr SDK for .NET](https://github.com/dapr/dotnet-sdk) it is complementary and designed to work alongside it.

Key features:
* Discovers, configures and launches the Dapr Sidecar process (daprd) with automatic port assignment
* Monitors and relaunches the Dapr Sidecar on unexpected exit
* Dapr command-line arguments exposed via the Microsoft Extensions Configuration Framework
* Routes Dapr stdout log messages to the Microsoft Extensions Logging Framework
* Consolidates Dapr health check and metrics endpoints into ASP.NET Core endpoints
* Seamless debugging experience within Visual Studio and other .NET development tools
* Compatible with all versions of .NET from .NET Framework 3.5 to .NET 5.0
* Supports Sidecar, Placement and Sentry processes

## Samples
Visit the [samples folder](./samples) for examples of how you can get up and running with Dapr Sidekick.

## Getting Started

This repository builds the following packages:

| Package                           | Description                                   | Compatibility                                           |
| --------------------------------- | --------------------------------------------- | ------------------------------------------------------- |
| Dapr.Sidekick                     | Core features                                 | .NET Framework 3.5+, .NET Core 3.1, .NET Standard 2.0   |
| Dapr.Sidekick.AspNetCore          | ASP.NET Core extensions                       | .NET Framework 4.6.2+, .NET Core 3.1, .NET Standard 2.0 |
| Dapr.Sidekick.Extensions.Logging  | Integrations for Microsoft Extensions Logging | .NET Framework 4.5+, .NET Core 3.1, .NET Standard 2.0   |


### Prerequisites

Dapr Sidekick requires a local installation of Dapr, the recommended approach is to follow the [Install Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [Init Dapr Locally](https://docs.dapr.io/getting-started/install-dapr-selfhost/) steps in the official [Dapr Docs](https://docs.dapr.io/).

> By default the `dapr init` command will install additional development components such as a Redis docker container. If you do not need these you can instead initialize Dapr using the [slim init mode](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-no-docker/) command `dapr init --slim`.

Each project in this repository is a normal C# project. At minimum, you need [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet/3.1) to build, test, and generate NuGet packages.

On Windows, we recommend installing [the latest Visual Studio 2019](https://www.visualstudio.com/vs/) which will set you up with all the .NET build tools and allow you to open the solution files. Community Edition is free and can be used to build everything here.

Make sure you [update Visual Studio to the most recent release](https://docs.microsoft.com/visualstudio/install/update-visual-studio).


### Build

To build everything and generate NuGet packages, run `dotnet` cli commands. Binaries and NuGet packages will be dropped in a *bin* directory at the repo root.

```bash
# Build Dapr Sidekick and tests
dotnet build -c Debug  # for release, -c Release

# Run unit tests
dotnet test

# Generate nuget packages in /bin/Debug
dotnet pack
```

Each project can also be built individually directly through the CLI or your editor/IDE. You can open the solution file `all.sln` in `<RepoRoot>` to load all projects at once.

> `<RepoRoot>` is the path where you cloned this repository.

Nuget packages are dropped under `<RepoRoot>/bin/<Debug|Release>/nugets` when you build locally.

### ASP.NET Core

Dapr Sidekick is typically used to manage the [Dapr Sidecar](https://docs.dapr.io/concepts/overview/) runtime process `daprd` in an ASP.NET Core application. First add the `Dapr.Sidekick.AspNetCore` NuGet package to the project file:

```xml
<ItemGroup>
  <PackageReference Include="Dapr.Sidekick.AspNetCore" Version="1.0.0" />
</ItemGroup>
```  

Next modify the `ConfigureServices` method in `Startup.cs` as follows:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    // Add the Dapr Sidecar
    services.AddDaprSidecar(Configuration);
}
```

When you run the application Dapr Sidekick will attempt to discover the Dapr sidecar in the default initialization folder, dynamically assign all required ports then launch and manage the lifetime of the runtime process. Detailed diagnostic log messages from both Dapr Sidekick and the Dapr sidecar are pumped through the standard Microsoft Extensions Logging framework. When the application is terminated Dapr Sidekick will shut down the Dapr sidecar.

Dapr Sidekick includes extensive configuration options for the Dapr Sidecar, Placement and Sentry processes - see the [Options](./src/Dapr.Sidekick/Options) code for for more details.