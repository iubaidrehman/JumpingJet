Write-Host "Building Active Session Manager (Single File, Self-Contained)..." -ForegroundColor Cyan

# Clean previous builds
dotnet clean --configuration Release

# Publish as a self-contained, single-file executable for Windows x64
dotnet publish -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:PublishReadyToRun=true

Write-Host "Build complete. Check bin\Release\net10.0-windows\win-x64\publish\" -ForegroundColor Green