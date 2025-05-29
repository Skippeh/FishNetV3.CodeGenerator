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

### Known issues

Compiling in Visual Studio might fail. If that happens try adding the following to your csproj file:

```xml
<PropertyGroup>
    <FishNetCodeGenDontIncludeReferencePaths>true</FishNetCodeGenDontIncludeReferencePaths>
</PropertyGroup>

<ItemGroup>
    <FishNetCodeGenAssemblySearchPaths Include="C:\Path\To\Dependencies" />
    <!-- repeat for other directories if necessary -->
</ItemGroup>
```

FishNet's initialize methods do not get invoked when the assembly is loaded with a mod loader (such as BepInEx or MelonLoader). A workaround for this is to invoke those methods manually when the mod initializes. Here is a code snippet you can use to do that:

```cs
// Call this when your plugin is being initialized
private void InitFishNet()
{
    // FishNet normally uses RuntimeInitializeOnLoadMethod to invoke these methods, but those do not get invoked when using a mod loader.
    // So we invoke them manually here.
    var types = Assembly.GetExecutingAssembly().GetExportedTypes().Where(t => t.Namespace == "FishNet.Serializing.Generated");

    foreach (var type in types)
    {
        MethodInfo method = type.GetMethod("InitializeOnce", BindingFlags.NonPublic | BindingFlags.Static);
        method.Invoke(null, []);
    }
}
```
