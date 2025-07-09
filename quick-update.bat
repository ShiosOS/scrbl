@echo off
REM 

echo Updating scrbl tool...
echo.

REM
cd /d "%~dp0"

REM
if not exist "scrbl.csproj" (
    echo Error: scrbl.csproj not found!
    echo Make sure this script is in the scrbl project directory.
    pause
    exit /b 1
)

echo Uninstalling current version...
dotnet tool uninstall --global scrbl >nul 2>&1

echo Building and packing...
dotnet pack -c Release >nul 2>&1
if errorlevel 1 (
    echo Error: Build/pack failed!
    pause
    exit /b 1
)

echo Installing new version...
dotnet tool install --global --add-source ./scrbl scrbl
if errorlevel 1 (
    echo Error: Installation failed!
    pause
    exit /b 1
)

echo.
echo ✅ scrbl updated successfully!
echo Try: scrbl --help
echo.
pause
