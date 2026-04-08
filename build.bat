@echo off
echo ========================================
echo Key Overlay Mod Build Script
echo ========================================
echo.

echo Checking dependencies...
if not exist "D:\Program Files\steam\steamapps\common\Rain World\RainWorld_Data\Managed\Assembly-CSharp.dll" (
    echo ERROR: Rain World Assembly-CSharp.dll not found!
    echo Please ensure Rain World is installed in the correct location.
    pause
    exit /b 1
)

if not exist "D:\Program Files\steam\steamapps\common\Rain World\BepInEx\core\BepInEx.Core.dll" (
    echo ERROR: BepInEx.Core.dll not found!
    echo Please ensure BepInEx is installed in Rain World.
    pause
    exit /b 1
)

echo Dependencies found.
echo.

echo Building Key Overlay mod...
dotnet build KeyOverlay.csproj -c Release

if %ERRORLEVEL% neq 0 (
    echo.
    echo ========================================
    echo Build FAILED!
    echo ========================================
    pause
    exit /b 1
)

echo.
echo ========================================
echo Build SUCCESS!
echo ========================================
echo.

echo Installing mod to Rain World...
echo Copying modinfo.json...
copy /Y "modinfo.json" "D:\Program Files\steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\keyoverlay\modinfo.json"

echo Mod installed successfully!
echo.
echo Location: D:\Program Files\steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\keyoverlay
echo.
echo You can now load the mod in Rain World's "Expansion" menu.
echo ========================================

pause