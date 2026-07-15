# razo-winslim

Windows 11 GUI tool to free RAM/CPU by disabling unnecessary services,
scheduled tasks, telemetry, startup apps, and preinstalled bloatware. Every
tweak is individually toggleable and revertible.

## Build

    dotnet build

## Test

    dotnet test

## Run (requires Administrator - one UAC prompt at launch)

    dotnet run --project src/RazoWinslim/RazoWinslim.csproj

## Publish a portable single-file exe

    dotnet publish src/RazoWinslim/RazoWinslim.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish

The resulting `publish/` folder contains `RazoWinslim.exe` plus a `Catalog/`
subfolder with the tweak catalog - no .NET install required on the target
machine. Copy the whole `publish/` folder (not just the exe) when moving it
to another machine.

## Manual verification before each release

See `docs/manual-smoke-test.md` - real service/task/registry/Appx changes
cannot be safely automated in CI and must be checked on a real Windows 11 VM.
