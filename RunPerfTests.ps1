[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [string]
    $Filter = "*"
)

echo "perf: Performance tests started with Filter = $Filter"

Push-Location $PSScriptRoot

$artifactsPath = "$PSScriptRoot\artifacts\perftests"

if (Test-Path "$artifactsPath") {
    echo "perf: Cleaning $artifactsPath"
    Remove-Item "$artifactsPath" -Force -Recurse
}

New-Item -Path "$artifactsPath" -ItemType Directory

$perfTestProjectPath = "$PSScriptRoot/test/Serilog.Sinks.MSSqlServer.PerformanceTests"
Push-Location "$perfTestProjectPath"

echo "perf: Running performance test project in $perfTestProjectPath"
& dotnet run -c Release -- -f $Filter

cp ".\BenchmarkDotNet.Artifacts\results\*.*" "$artifactsPath\"
Pop-Location

Pop-Location
