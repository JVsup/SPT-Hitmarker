[CmdletBinding()]
param(
    [string] $SptPath,

    [switch] $NoRestore
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repositoryRoot = (Resolve-Path -LiteralPath (Join-Path $PSScriptRoot '..')).Path
$projectRoot = Join-Path $repositoryRoot 'src\SPT.Hitmarker'
$projectPath = Join-Path $projectRoot 'SPT.Hitmarker.csproj'
$localPropsPath = Join-Path $repositoryRoot 'Directory.Build.props.user'
$nugetConfigPath = Join-Path $repositoryRoot 'NuGet.Config'

if ([string]::IsNullOrWhiteSpace($SptPath) -and (Test-Path -LiteralPath $localPropsPath -PathType Leaf)) {
    [xml] $localProps = Get-Content -LiteralPath $localPropsPath -Raw
    $configuredPath = @($localProps.Project.PropertyGroup | ForEach-Object { $_.SptPath } | Where-Object { $_ })
    if ($configuredPath.Count -gt 0) {
        $SptPath = [string] $configuredPath[0]
    }
}

if ([string]::IsNullOrWhiteSpace($SptPath)) {
    throw 'SPT path is not configured. Pass -SptPath or create Directory.Build.props.user.'
}

$resolvedSptPath = (Resolve-Path -LiteralPath $SptPath).Path

$msbuildProperty = "-p:SptPath=$resolvedSptPath"
if (-not $NoRestore) {
    & dotnet restore $projectPath --configfile $nugetConfigPath $msbuildProperty
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed with exit code $LASTEXITCODE."
    }
}

& dotnet build $projectPath --configuration Release --no-restore $msbuildProperty '-p:ContinuousIntegrationBuild=true'
if ($LASTEXITCODE -ne 0) {
    throw "dotnet build failed with exit code $LASTEXITCODE."
}

$pluginDll = Join-Path $projectRoot 'bin\Release\netstandard2.1\SPT.Hitmarker.dll'
if (-not (Test-Path -LiteralPath $pluginDll -PathType Leaf)) {
    throw "Expected plugin output not found: $pluginDll"
}

$licenseFile = Join-Path $projectRoot 'LICENSE-DragonDen.Hitmarker.txt'
$soundFiles = @(
    'Hitmarker1.wav',
    'Hitmarker2.wav',
    'Hitmarker3.wav',
    'Kill.wav'
) | ForEach-Object { Join-Path (Join-Path $projectRoot 'Sounds') $_ }
$uiFiles = @(
    'Hitmarker.png',
    'Hitmarker_Headshot.png',
    'Hitmarker_Kill.png'
) | ForEach-Object { Join-Path (Join-Path $projectRoot 'UI') $_ }

foreach ($requiredFile in @($licenseFile) + $soundFiles + $uiFiles) {
    if (-not (Test-Path -LiteralPath $requiredFile -PathType Leaf)) {
        throw "Expected package input not found: $requiredFile"
    }
}

$artifactsRoot = Join-Path $repositoryRoot 'artifacts'
$packageRoot = Join-Path $artifactsRoot 'package'
$pluginPackageRoot = Join-Path $packageRoot 'BepInEx\plugins\SPT.Hitmarker'
$soundsPackageRoot = Join-Path $pluginPackageRoot 'Sounds'
$uiPackageRoot = Join-Path $pluginPackageRoot 'UI'
$distRoot = Join-Path $repositoryRoot 'dist'
$zipPath = Join-Path $distRoot 'SPT-Hitmarker-4.1.0.zip'

foreach ($safePath in @($artifactsRoot, $distRoot)) {
    $fullPath = [System.IO.Path]::GetFullPath($safePath)
    if (-not $fullPath.StartsWith($repositoryRoot + [System.IO.Path]::DirectorySeparatorChar, [StringComparison]::OrdinalIgnoreCase)) {
        throw "Refusing to modify a path outside the repository: $fullPath"
    }
}

if (Test-Path -LiteralPath $packageRoot) {
    Remove-Item -LiteralPath $packageRoot -Recurse -Force
}
New-Item -ItemType Directory -Path $soundsPackageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $uiPackageRoot -Force | Out-Null
New-Item -ItemType Directory -Path $distRoot -Force | Out-Null

Copy-Item -LiteralPath $pluginDll -Destination $pluginPackageRoot
Copy-Item -LiteralPath $licenseFile -Destination $pluginPackageRoot
Copy-Item -LiteralPath $soundFiles -Destination $soundsPackageRoot
Copy-Item -LiteralPath $uiFiles -Destination $uiPackageRoot

if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
Compress-Archive -Path (Join-Path $packageRoot '*') -DestinationPath $zipPath -CompressionLevel Optimal

$hash = Get-FileHash -LiteralPath $zipPath -Algorithm SHA256
Write-Host "Release created: $zipPath"
Write-Host "SHA256: $($hash.Hash)"
