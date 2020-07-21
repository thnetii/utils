# dotnet-platforminfo

.NET Platform Information Command-line Utility

| Deployment | Latest Status |
| - | - |
| **Build Status** | [![Build Status](https://dev.azure.com/thnetii/ci-cd/_apis/build/status/thnetii.utils?branchName=rel/dotnet-platforminfo/latest)](https://dev.azure.com/thnetii/ci-cd/_build/latest?definitionId=83&branchName=rel/dotnet-platforminfo/latest) |
| **Nightlies** | [![THNETII.Utils.DotnetPlatformInfo package in nightly feed in Azure Artifacts](https://feeds.dev.azure.com/thnetii/ci-cd/_apis/public/packaging/Feeds/nightly/Packages/5e034465-50b3-4a60-b899-04b508156364/badge)](https://dev.azure.com/thnetii/ci-cd/_packaging?_a=package&feed=nightly&package=THNETII.Utils.DotnetPlatformInfo&protocolType=NuGet&preferRelease=true) |
| **Azure Artifacts** | [![THNETII.Utils.DotnetPlatformInfo package in public feed in Azure Artifacts](https://feeds.dev.azure.com/thnetii/artifacts/_apis/public/packaging/Feeds/public/Packages/104d7c22-ed49-4502-baa2-99087e7f6ee9/badge)](https://dev.azure.com/thnetii/artifacts/_packaging?_a=package&feed=public&package=THNETII.Utils.DotnetPlatformInfo&protocolType=NuGet&preferRelease=true) |
| **NuGet Gallery** | [![THNETII.Utils.DotnetPlatformInfo package on nuget.org](https://img.shields.io/nuget/vpre/THNETII.Utils.DotnetPlatformInfo?label=nuget.org)](https://www.nuget.org/packages/THNETII.Utils.DotnetPlatformInfo) |


## Installation

The platform information utility is published as a .NET Core Tool.

### Nuget

This tool is listed on the [NuGet Gallery](https://www.nuget.org/packages) with the package id [`THNETII.Utils.DotnetPlatformInfo`](https://www.nuget.org/packages/THNETII.Utils.DotnetPlatformInfo)

You can install the tool from nuget using the following command

``` sh
dotnet tool install --global THNETII.Utils.DotnetPlatformInfo
```

### Azure Artifacts

As an alternative to the NuGet Gallery, the tool can also be obtained from the [TH-NETII Public Artifacts feed](https://dev.azure.com/thnetii/artifacts/_packaging?_a=feed&feed=public) on Azure DevOps.

``` sh
dotnet tool install --global --add-source "https://pkgs.dev.azure.com/thnetii/artifacts/_packaging/public/nuget/v3/index.json" THNETII.Utils.DotnetPlatformInfo
```

### Nightlies

Whenever commits are pushed to the `master` or a release branch (`rel/*`) the CI/CD pipeline for this repository will publish the tool in the [nightly feed](https://dev.azure.com/thnetii/ci-cd/_packaging?_a=feed&feed=nightly).

The latest artifact can be installed by running the following command

``` sh
dotnet tool install --global --add-source "https://pkgs.dev.azure.com/thnetii/ci-cd/_packaging/nightly/nuget/v3/index.json" THNETII.Utils.DotnetPlatformInfo
```

### From source

To build and install the tool globally from source, run the following command at the root of the repository:

``` sh
dotnet pack -c Release "./src/THNETII.Utils.DotnetPlatformInfo"
dotnet tool install --global --add-source "./bin/Release" THNETII.Utils.DotnetPlatformInfo
```

## Usage

Run the following command to get the help information for the tool:

``` sh
dotnet tool run dotnet-platforminfo -- --help
```

### Subcommands

The tool offers several sub-commands that output different values of the static `RuntimeEnvironment` class located in the `Microsoft.DotNet.PlatformAbstractions` package.

The .NET CLI uses the same information to write the output of the `dotnet --info` command.

|Command|Description|
|-|-|
|`rid`|Reports the runtime identifier (RID)<br/>The value matches exactly the output reported by<br/>`dotnet --info`.|
|`os`|Reports the Operating System name as described by the .NET Runtime Environment<br/>For Linux, this outputs the name of the Distro.|
|`osversion`|Reports the OS version|
|`arch`|Reports the .NET process architecture string|

### Use-Cases

The `rid` subcommand can be used to obtain the RID of the current build agent during CI/CD pipelines. This can be useful for populating the Test Platform when executing Unit tests on a build agent. In .NET the RID is composed out of the output of all the other subcommands.
