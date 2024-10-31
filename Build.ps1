[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [Switch]
    $SkipTests,

    [Parameter(Mandatory = $false)]
    [Switch]
    $SkipPerfTests,

    [Parameter(Mandatory = $false)]
    [Switch]
    $SkipSamples
)

echo "build: Build started"

try
{
    Push-Location "$PSScriptRoot"

    if (Test-Path .\artifacts)
    {
        echo "build: Cleaning .\artifacts"
        Remove-Item .\artifacts -Force -Recurse
    }

    echo "build: Restoring packages for solution"
    & dotnet restore --no-cache
    if ($LASTEXITCODE -ne 0)
    {
        echo "Error returned by dotnet restore. Aborting build."
        exit 1
    }

    $branch = @{ $true = $env:GITHUB_REF_NAME; $false = $( git symbolic-ref --short -q HEAD ) }[$env:GITHUB_REF_NAME -ne $NULL]
    $revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:GITHUB_RUN_NUMBER, 10); $false = "local" }[$env:GITHUB_RUN_NUMBER -ne $NULL]
    $suffix = @{ $true = ""; $false = "$($branch.Substring(0,[math]::Min(10, $branch.Length)) )-$revision" }[$branch -ne "dev" -and $revision -ne "local"]

    echo "build: Version suffix is $suffix"

    $sinkProjectPath = "$PSScriptRoot/src/Serilog.Sinks.MSSqlServer"
    try
    {
        Push-Location "$sinkProjectPath"

        echo "build: Packaging sink main project in $sinkProjectPath"
        if ($suffix)
        {
            & dotnet pack -c Release -o ..\..\artifacts --version-suffix=$suffix
        }
        else
        {
            & dotnet pack -c Release -o ..\..\artifacts
        }
        if ($LASTEXITCODE -ne 0)
        {
            echo "Error returned by dotnet pack. Aborting build."
            exit 1
        }
    }
    finally
    {
        Pop-Location
    }

    if ($SkipTests -eq $false)
    {
        $testProjectPath = "$PSScriptRoot/test/Serilog.Sinks.MSSqlServer.Tests"
        try
        {
            Push-Location "$testProjectPath"

            echo "build: Testing project in $testProjectPath"
            & dotnet test -c Release --collect "XPlat Code Coverage"
            if ($LASTEXITCODE -ne 0)
            {
                exit 2
            }

        }
        finally
        {
            Pop-Location
        }
    }

    if ($SkipPerfTests -eq $false)
    {
        # The performance benchmark tests should at least build without errors during PR validation
        $perfTestProjectPath = "$PSScriptRoot/test/Serilog.Sinks.MSSqlServer.PerformanceTests"
        try
        {
            Push-Location "$perfTestProjectPath"

            echo "build: Building performance test project in $perfTestProjectPath"
            & dotnet build -c Release
            if ($LASTEXITCODE -ne 0)
            {
                exit 3
            }
        }
        finally
        {
            Pop-Location
        }
    }

    if ($SkipSamples -eq $false)
    {
        foreach ($src in Get-ChildItem "$PSScriptRoot/sample/*.csproj" -File -Recurse)
        {
            try
            {
                Push-Location $src.DirectoryName

                echo "build: Building sample project $( $src.FullName )"
                & dotnet build -c Release -o ..\..\artifacts
                if ($LASTEXITCODE -ne 0)
                {
                    echo "Error returned by dotnet build. Aborting build."
                    exit 4
                }
            }
            finally
            {
                Pop-Location
            }
        }
    }

}
finally
{
    Pop-Location
}
