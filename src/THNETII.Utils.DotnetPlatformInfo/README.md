# dotnet-platforminfo

.NET Platform Information Command-line Utility

| Deployment | Latest Status |
| - | - |
| **Build Status** | [![Build Status](https://dev.azure.com/thnetii/ci-cd/_apis/build/status/thnetii.utils?branchName=rel/dotnet-platforminfo/latest)](https://dev.azure.com/thnetii/ci-cd/_build/latest?definitionId=83&branchName=rel/dotnet-platforminfo/latest) |
| **Nightlies** | [![THNETII.Utils.DotnetPlatformInfo package in nightly feed in Azure Artifacts](https://feeds.dev.azure.com/thnetii/2c1e277e-4a44-4255-bb5a-0e12a2e181eb/_apis/public/Packaging/Feeds/3f3a46b4-4d40-4031-9137-77e7901c7626/Packages/5e034465-50b3-4a60-b899-04b508156364/Badge)](https://dev.azure.com/thnetii/ci-cd/_packaging?_a=package&feed=3f3a46b4-4d40-4031-9137-77e7901c7626&package=5e034465-50b3-4a60-b899-04b508156364&preferRelease=true) |
| **Azure Artifacts** | [![THNETII.Utils.DotnetPlatformInfo package in public feed in Azure Artifacts](https://feeds.dev.azure.com/thnetii/f1165ef2-8f9b-46e1-87a8-be4ce26ce217/_apis/public/Packaging/Feeds/4046ec89-3396-4ce5-914e-40429cd037c2/Packages/104d7c22-ed49-4502-baa2-99087e7f6ee9/Badge)](https://dev.azure.com/thnetii/artifacts/_packaging?_a=package&feed=4046ec89-3396-4ce5-914e-40429cd037c2&package=104d7c22-ed49-4502-baa2-99087e7f6ee9&preferRelease=true) |
| **NuGet.org** | *N/A* |


## Installation

The platform information utility is published as a .NET Core Tool.

### Nuget

*Work in progress*

### Azure Artifacts

*The tool will be published to the TH-NETII artifacts feed and can be downloaded from there, once the package is published.*

### Form source

To build and install the tool globally from source, run the following command in PowerShell:

``` ps1
$UtilsDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "thnetii-utils"
& git clone --depth 1 --branch rel/dotnet-platforminfo/latest "https://github.com/thnetii/utils.git" -- $UtilsDirectory
& dotnet pack -c Release "`"$(Join-Path $UtilsDirectory "src/THNETII.Utils.DotnetPlatformInfo")`""
& dotnet tool install --global --add-source "`"$(Join-Path $UtilsDirectory "bin/Release")`"" THNETII.Utils.DotnetPlatformInfo
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
