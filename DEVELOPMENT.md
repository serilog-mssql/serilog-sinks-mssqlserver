# Creating Releases

## Creating a Pre-release (-dev suffix)

Whenever the `dev` branch is updated (after merging a pull request), the `Release` action is triggered. This action builds a nuget package with a prerelease identifier of the format `-dev-nnnnn` appended to the version number. This package is automatically published on nuget.org.

## Creating a latest Release

### Normal Update (no major version change)

1. On the `dev` branch, update CHANGES.md and `VersionPrefix` in Serilog.Sinks.MSSqlServer.csproj.

1. Create a PR to merge the `dev` branch into `main`. The `Release` action will be triggered. This action builds a nuget package and publishes it on nuget.org. Additionally a release is created in the GitHub repo. The release summary will be taken from the description of the PR, so best thing is to put something similar to the version summary in CHANGES.md in there.

1. After the release is done, increase the patch version number in `VersionPrefix` in Serilog.Sinks.MSSqlServer.csproj on the `dev` branch. This ensures that the next dev release will have a higher version number than the latest release.

### Major Release (major version change)

1. On the `dev` branch, update CHANGES.md and increase the major version in `VersionPrefix` in Serilog.Sinks.MSSqlServer.csproj. Also set `EnablePackageValidation` to false because on an intial release of a new major version you don't have a baseline version yet on nuget.org to compare with.

1. Create a PR to merge the `dev` branch into `main`. The `Release` action will be triggered. This works the same as described above under [Normal Update](#normal-update-no-major-version-change).

1. After the release is done make some changes in Serilog.Sinks.MSSqlServer.csproj on the `dev` branch. Set `EnablePackageValidation` back to true and `PackageValidationBaselineVersion` to the version of the new major release you just created (e.g. 7.0.0). Then also increase the patch version number in `VersionPrefix` (e.g. 7.0.1).
