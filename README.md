# Dapr Sidekick for .NET

[![Build Status](https://github.com/man-group/dapr-sidekick-dotnet/workflows/build/badge.svg?event=push&branch=main)](https://github.com/man-group/dapr-sidekick-dotnet/actions?workflow=build)
[![codecov](https://codecov.io/gh/man-group/dapr-sidekick-dotnet/branch/main/graph/badge.svg?token=y7Uq2TIAuI)](https://codecov.io/gh/man-group/dapr-sidekick-dotnet)
[![Maintainability](https://api.codeclimate.com/v1/badges/0c79d186a9b733579ae8/maintainability)](https://codeclimate.com/github/man-group/dapr-sidekick-dotnet/maintainability)
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

## Dapr Community Call

A presentation of Dapr Sidekick for .NET was given to the Dapr community as part of [Community Call 39](https://youtu.be/Cv-8e5wV44g), demonstrating its ability to easily integrate the Dapr sidecar into an ASP.NET Core application and enable seamless debugging sessions with Visual Studio 2019. Watch the [20-minute segment on YouTube](https://youtu.be/Cv-8e5wV44g?t=1758) to get started as quickly as possible.

## Samples

Visit the [samples folder](./samples) for examples of how you can get up and running with Dapr Sidekick.

## Getting Started

Dapr Sidekick requires a local installation of Dapr, the recommended approach is to follow the [Install Dapr CLI](https://docs.dapr.io/getting-started/install-dapr-cli/) and [Init Dapr Locally](https://docs.dapr.io/getting-started/install-dapr-selfhost/) steps in the official [Dapr Docs](https://docs.dapr.io/).

> By default the `dapr init` command will install additional development components such as a Redis docker container. If you do not need these you can instead initialize Dapr using the [slim init mode](https://docs.dapr.io/operations/hosting/self-hosted/self-hosted-no-docker/) command `dapr init --slim`.

Once Dapr is installed, Dapr Sidekick can be used to manage the [Dapr Sidecar](https://docs.dapr.io/concepts/overview/) runtime process `daprd`. For example, in an ASP.NET Core application add the `Man.Dapr.Sidekick.AspNetCore` NuGet package to the project file:

```xml
<ItemGroup>
  <PackageReference Include="Man.Dapr.Sidekick.AspNetCore" Version="1.0.0" />
</ItemGroup>
```  

Next modify the `ConfigureServices` method in `Startup.cs` as follows:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    // Add Dapr Sidekick
    services.AddDaprSidekick(Configuration);
}
```

That's it! When you run the application Dapr Sidekick will discover the Dapr sidecar in the default installation folder, dynamically assign all required ports then launch and manage the lifetime of the runtime process. Detailed diagnostic log messages from both Dapr Sidekick and the Dapr sidecar are pumped through the standard Microsoft Extensions Logging framework. When the application is terminated Dapr Sidekick will shut down the Dapr sidecar.

Dapr Sidekick includes extensive configuration options for the Dapr Sidecar, Placement and Sentry processes - see the [Options](./src/Man.Dapr.Sidekick/Options) code for for more details.

## Building the repository

This repository builds the following packages:

| Package                                | Description                                   | Compatibility                                                     |
| -------------------------------------- | --------------------------------------------- | ----------------------------------------------------------------- |
| `Man.Dapr.Sidekick`                    | Core features                                 | .NET Framework 3.5+, .NET Standard 2.0, .NET Core 3.1, .NET 5.0   |
| `Man.Dapr.Sidekick.AspNetCore`         | ASP.NET Core extensions                       | .NET Framework 4.6.2+, .NET Standard 2.0, .NET Core 3.1, .NET 5.0 |
| `Man.Dapr.Sidekick.Extensions.Logging` | Integrations for Microsoft Extensions Logging | .NET Framework 4.5+, .NET Standard 2.0, .NET Core 3.1, .NET 5.0   |

### Prerequisites

Each project in this repository is a normal C# project. We recommend building on Windows, where at a minimum you need the [.NET 5.0 SDK](https://dotnet.microsoft.com/download/dotnet/5.0) to build, test, and generate NuGet packages.

We also recommend installing [the latest Visual Studio 2019](https://www.visualstudio.com/vs/) which will set you up with all the .NET build tools and allow you to open the solution files. Community Edition is free and can be used to build everything here.

Make sure you [update Visual Studio to the most recent release](https://docs.microsoft.com/visualstudio/install/update-visual-studio).

### Build Process

To build everything and generate NuGet packages, run `dotnet` cli commands from `<RepoRoot>`. Binaries and NuGet packages will be dropped in `<RepoRoot>/bin`.

```powershell
# Build Dapr Sidekick and tests
dotnet build -c Debug  # for release, -c Release

# Run unit tests
dotnet test

# Generate nuget packages in /bin/Debug
dotnet pack
```

> `<RepoRoot>` is the path where you cloned this repository.

Each project can also be built individually directly through the CLI or your editor/IDE. You can open the solution file `all.sln` in `<RepoRoot>` to load all projects at once.

Nuget packages are dropped under `<RepoRoot>/bin/<Debug|Release>/nugets` when you build locally.

## External dependencies

In order to target as many possible .NET platforms as possible with minimal external dependencies, Dapr Sidekick gratefully includes source code from a number of external open-source projects. Where appropriate code files are annotated with the original source link and LICENSE file included. The following are the main sources of external code:

| Name                                                                         | License                                                                         | Local Source                                          |
| ---------------------------------------------------------------------------- | ------------------------------------------------------------------------------- | ----------------------------------------------------- |
| [Dapr SDK for .NET](https://github.com/dapr/dotnet-sdk)                      | [MIT](https://github.com/dapr/dotnet-sdk/blob/master/LICENSE)                   | [DaprClient](./src/Man.Dapr.Sidekick/DaprClient)      |
| [.NET Extensions 2.1](https://github.com/dotnet/extensions/tree/release/2.1) | [Apache 2.0](https://github.com/dotnet/extensions/blob/release/2.1/LICENSE.txt) | [Logging](./src/Man.Dapr.Sidekick/Logging)            |
| [Prometheus .NET](https://github.com/prometheus-net/prometheus-net)          | [MIT](https://github.com/prometheus-net/prometheus-net/blob/master/LICENSE)     | [Metrics](./src/Man.Dapr.Sidekick.AspNetCore/Metrics) |

Dapr Sidekick also includes a package reference to [Newtonsoft Json.NET](https://github.com/JamesNK/Newtonsoft.Json) for parsing JSON log messages from Dapr.

## Non-Windows Platforms

On platforms other than Windows (such as Linux and Mac OS) some features of Dapr Sidekick may not be available due to the required native API calls not being available. These include:

| Feature                     | Platforms    | Notes                                                             |
| --------------------------- | ------------ | ----------------------------------------------------------------- |
| Attach to existing instance | Linux/Mac OS | Will not detect existing `daprd` instance for same AppId and Port |

## Acknowledgements

Dapr Sidekick has been under active development at [Man Group](http://www.man.com/) since 2020.

Original concept and implementation: [Simon Jones](https://github.com/badgeratu)

Contributors:

* [Abdulaziz Elsheikh](https://github.com/a-elsheikh)

Contributions welcome! Please review the [Contribution Guidelines](CONTRIBUTING.md).

## License

Dapr Sidekick is licensed under Apache 2.0, a copy of which is included in [LICENSE](LICENSE).
