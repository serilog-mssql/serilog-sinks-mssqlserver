[CmdletBinding()]
param (
    [Parameter(Mandatory = $false)]
    [Switch]
    $SkipTests
)

echo "build: Build started"

Push-Location $PSScriptRoot

if(Test-Path .\artifacts) {
	echo "build: Cleaning .\artifacts"
	Remove-Item .\artifacts -Force -Recurse
}

& dotnet restore --no-cache

$branch = @{ $true = $env:GITHUB_REF_NAME; $false = $(git symbolic-ref --short -q HEAD) }[$env:GITHUB_REF_NAME -ne $NULL];
$revision = @{ $true = "{0:00000}" -f [convert]::ToInt32("0" + $env:GITHUB_RUN_NUMBER, 10); $false = "local" }[$env:GITHUB_RUN_NUMBER -ne $NULL];
$suffix = @{ $true = ""; $false = "$($branch.Substring(0, [math]::Min(10,$branch.Length)))-$revision"}[$branch -ne "dev" -and $revision -ne "local"]

echo "build: Version suffix is $suffix"

foreach ($src in ls src/*) {
    Push-Location $src

    echo "build: Packaging project in $src"

    if ($suffix) {
        & dotnet pack -c Release -o ..\..\artifacts --version-suffix=$suffix
    } else {
        & dotnet pack -c Release -o ..\..\artifacts
    }
    if($LASTEXITCODE -ne 0) { exit 1 }

    Pop-Location
}

if ($SkipTests -eq $false) {
    foreach ($test in ls test/*.PerformanceTests) {
        Push-Location $test

        echo "build: Building performance test project in $test"

        & dotnet build -c Release
        if($LASTEXITCODE -ne 0) { exit 2 }

        Pop-Location
    }

    foreach ($test in ls test/*.Tests) {
        Push-Location $test

        echo "build: Testing project in $test"

        & dotnet test -c Release
        if($LASTEXITCODE -ne 0) { exit 3 }

        Pop-Location
    }
}

Pop-Location
