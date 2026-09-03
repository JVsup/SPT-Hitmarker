# SPT Hitmarker

This is a SPT 4.1.3 port of Drexira's DragonDen.Hitmarker

## Building

Required assemblies are referenced in place from a read-only SPT installation through the `SptPath` MSBuild property.

Create an ignored `Directory.Build.props.user` in the repository root:

```xml
<Project>
  <PropertyGroup>
    <SptPath>X:\Path\To\SPT</SptPath>
  </PropertyGroup>
</Project>
```

Build and package the plugin:

```powershell
pwsh -File .\scripts\build-release.ps1
```

The SPT path can also be supplied:

```powershell
pwsh -File .\scripts\build-release.ps1 -SptPath 'X:\Path\To\SPT'
```

After dependencies have been restored once, pass `-NoRestore` for an offline build. The local verification archive is written to `dist/`.

## Changelog

4.1.0 - initial update for SPT 4.1.3

## License

MIT