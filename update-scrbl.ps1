#!/usr/bin/env pwsh
# update-scrbl.ps1 - Update script for scrbl global tool

param(
    [switch]$Force,
    [switch]$Verbose,
    [switch]$SkipBuild
)

$Green = "Green"
$Yellow = "Yellow"
$Red = "Red"
$Cyan = "Cyan"
$Gray = "Gray"

function Write-Status {
    param($Message, $Color = "White")
    Write-Host $Message -ForegroundColor $Color
}

function Write-Error {
    param($Message)
    Write-Host "❌ $Message" -ForegroundColor $Red
}

function Write-Success {
    param($Message)
    Write-Host "✅ $Message" -ForegroundColor $Green
}

function Write-Info {
    param($Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $Cyan
}

function Write-Warning {
    param($Message)
    Write-Host "⚠️  $Message" -ForegroundColor $Yellow
}

try {
    Write-Status "🚀 Updating scrbl tool..." $Green
    Write-Status ""

    $ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $ProjectDir = $ScriptDir

    if (-not (Test-Path "$ProjectDir\scrbl.csproj")) {
        Write-Error "scrbl.csproj not found in $ProjectDir"
        Write-Info "Make sure you're running this script from the scrbl project directory"
        exit 1
    }

    Write-Info "Project directory: $ProjectDir"
    Set-Location $ProjectDir

    $toolList = dotnet tool list --global 2>$null
    $isInstalled = $toolList | Select-String "scrbl"
    
    if ($isInstalled) {
        Write-Info "Current installation found: $($isInstalled.Line.Trim())"
    } else {
        Write-Warning "scrbl is not currently installed as a global tool"
    }

    if (-not $SkipBuild) {
        Write-Status "🔨 Building project..." $Yellow
        
        # Clean first
        $cleanResult = dotnet clean --configuration Release 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Clean failed, continuing anyway..."
            if ($Verbose) { Write-Host $cleanResult }
        }

        # Build
        $buildResult = dotnet build --configuration Release 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Build failed!"
            Write-Host $buildResult
            exit 1
        }
        Write-Success "Build completed"

        # Pack
        Write-Status "📦 Packing tool..." $Yellow
        $packResult = dotnet pack --configuration Release --no-build 2>&1
        if ($LASTEXITCODE -ne 0) {
            Write-Error "Pack failed!"
            Write-Host $packResult
            exit 1
        }
        Write-Success "Pack completed"
    } else {
        Write-Info "Skipping build (--SkipBuild specified)"
    }

    # Check if package exists
    $packagePath = ".\scrbl"
    if (-not (Test-Path $packagePath)) {
        Write-Error "Package directory not found: $packagePath"
        Write-Info "Make sure the pack completed successfully"
        exit 1
    }

    $packageFiles = Get-ChildItem "$packagePath\*.nupkg" | Sort-Object LastWriteTime -Descending
    if ($packageFiles.Count -eq 0) {
        Write-Error "No .nupkg files found in $packagePath"
        exit 1
    }

    $latestPackage = $packageFiles[0]
    Write-Info "Latest package: $($latestPackage.Name)"

    # Uninstall current version
    if ($isInstalled -or $Force) {
        Write-Status "🗑️  Uninstalling current version..." $Yellow
        $uninstallResult = dotnet tool uninstall --global scrbl 2>&1
        if ($LASTEXITCODE -eq 0) {
            Write-Success "Uninstalled successfully"
        } else {
            Write-Warning "Uninstall failed or tool wasn't installed"
            if ($Verbose) { Write-Host $uninstallResult }
        }
    }

    # Install new version
    Write-Status "📥 Installing new version..." $Yellow
    $installResult = dotnet tool install --global --add-source $packagePath scrbl 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Installation failed!"
        Write-Host $installResult
        
        Write-Info ""
        Write-Info "Troubleshooting suggestions:"
        Write-Info "1. Make sure no scrbl processes are running"
        Write-Info "2. Try running with elevated privileges"
        Write-Info "3. Check if the package is corrupted"
        Write-Info "4. Try: dotnet tool uninstall --global scrbl first"
        
        exit 1
    }

    Write-Success "Installation completed!"

    Write-Status "🔍 Verifying installation..." $Yellow
    $verifyResult = scrbl --help 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "Verification successful!"
        
        $versionResult = scrbl --version 2>$null
        if ($LASTEXITCODE -eq 0 -and $versionResult) {
            Write-Info "Version: $versionResult"
        }
    } else {
        Write-Warning "Verification failed - tool may not be properly installed"
        if ($Verbose) { Write-Host $verifyResult }
    }

    Write-Status ""
    Write-Success "🎉 scrbl has been updated successfully!"
    Write-Info "You can now use 'scrbl --help' to see available commands"

    Write-Status ""
    Write-Status "💡 Quick start:" $Cyan
    Write-Status "   scrbl setup <path>     # Configure notes directory" $Gray
    Write-Status "   scrbl create -d        # Create daily template" $Gray
    Write-Status "   scrbl write 'content'  # Add content to notes" $Gray
    Write-Status "   scrbl edit             # Edit notes in editor" $Gray

} catch {
    Write-Error "An unexpected error occurred: $($_.Exception.Message)"
    if ($Verbose) {
        Write-Host $_.Exception.StackTrace
    }
    exit 1
}
