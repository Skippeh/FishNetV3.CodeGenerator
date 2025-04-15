# FishNetV3.CodeGenerator

Code generator for FishNet V3 that works outside of the unity editor. Not compatible with FishNet V4. Made specifically for modding Schedule I but probably works with anything that uses FishNet V3.

**NOT ASSOCIATED WITH OFFICIAL FISHNET PROJECT MADE BY FIRST GEAR GAMES.**

## Installation

Install the package using NuGet. This can be done in various ways.

### .NET CLI

```powershell
dotnet add package FishNetV3.CodeGenerator.MSBuild
```

### NuGet

```powershell
Install-Package FishNetV3.CodeGenerator.MSBuild
```

### Visual Studio

Add the package using the NuGet package manager.

### Rider

Right click the project, then click "Manage NuGet Packages..." and search for "FishNetV3.CodeGenerator.MSBuild".

## Usage

After adding the package any FishNet related networking code such as RPCs or SyncTypes in your project will be expanded on when building the project.

You can verify this by opening the dll in a decompiler such as dnSpy or dotPeek and look for the `FishNet.Serializing.Generated` namespace. Note: It may not exist if there's no code to expand on.

### Advanced usage

The code generator needs assembly search paths to find dependency dll's. By default it will look in the same place as the input dll, and every dependency's folder. This includes NuGet packages, but local references are prioritized.

If you need to manually specify more directories you can add the following to your csproj file:

```xml
<ItemGroup>
    <FishNetCodeGenAssemblySearchPaths Include="C:\Path\To\Dependencies" />
</ItemGroup>
```

Manually defined search paths have the highest priority (resolving assemblies will search through these directories first).
