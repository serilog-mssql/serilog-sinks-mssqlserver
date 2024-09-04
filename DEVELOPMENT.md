# Creating Releases

## Creating a dev Release

Whenever the `dev` branch is updated (after merging a pull request), the `Release` action is triggered. This action builds a nuget package with a prerelease identifier of the format `-dev-nnnnn` appended to the version number. This package is automatically published on nuget.org.

## Creating a latest Release

1. On the `dev` branch, update CHANGES.md and `VersionPrefix` and `PackageValidationBaselineVersion` (if applicable, e.g. major version was increased) in Serilog.Sinks.MSSqlServer.csproj.

1. Merge the `dev` branch into `main`. The `Release` action will be triggered. This action builds a nuget package and publishes it on nuget.org. Additionally a release is created in the GitHub repo.

1. Edit the GitHub release and copy the release notes from CHANGES.md.

1. After the release is done, increase the version number in Serilog.Sinks.MSSqlServer.csproj. This ensures that the next dev release will have a higher version number than the latest release.
