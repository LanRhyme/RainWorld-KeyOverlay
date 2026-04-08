@echo off
echo ============================================
echo   Rain World Key Overlay Workshop Package
echo ============================================
echo.

set GAME_PATH=D:\Program Files\steam\steamapps\common\Rain World
set MOD_DIR=RainWorld_Data\StreamingAssets\mods\keyoverlay
set WORKSHOP_DIR=workshop\keyoverlay

echo [1] Building project (Release mode)...
dotnet build KeyOverlay.csproj -c Release
if errorlevel 1 (
    echo     Build FAILED!
    pause
    exit /b 1
)
echo     Build successful!

echo.
echo [2] Copying DLL to workshop directory...
copy /Y "bin\Release\KeyOverlay.dll" "%WORKSHOP_DIR%\plugins\KeyOverlay.dll"
echo     Done!

echo.
echo [3] Copying modinfo.json to workshop directory...
copy /Y "modinfo.json" "%WORKSHOP_DIR%\modinfo.json"
echo     Done!

echo.
echo [4] Installing to Rain World game directory...
if exist "%GAME_PATH%\%MOD_DIR%" (
    echo     Removing old version...
    rmdir /S /Q "%GAME_PATH%\%MOD_DIR%"
)
mkdir "%GAME_PATH%\%MOD_DIR%\plugins"
copy /Y "%WORKSHOP_DIR%\plugins\KeyOverlay.dll" "%GAME_PATH%\%MOD_DIR%\plugins\"
copy /Y "%WORKSHOP_DIR%\modinfo.json" "%GAME_PATH%\%MOD_DIR%\"
if exist "%WORKSHOP_DIR%\Cubic_11.ttf" (
    copy /Y "%WORKSHOP_DIR%\Cubic_11.ttf" "%GAME_PATH%\%MOD_DIR%\"
    echo     Cubic_11.ttf copied!
)
if exist "%WORKSHOP_DIR%\thumbnail.png" (
    copy /Y "%WORKSHOP_DIR%\thumbnail.png" "%GAME_PATH%\%MOD_DIR%\"
    echo     thumbnail.png copied!
)
echo     Done!

echo.
echo ============================================
echo   Workshop Package Ready!
echo ============================================
echo.
echo Workshop files location: %WORKSHOP_DIR%
echo Game installation location: %GAME_PATH%\%MOD_DIR%
echo.
echo Next steps:
echo   1. Create thumbnail.png (512x512 recommended)
echo   2. Place it in workshop\keyoverlay\
echo   3. Start Rain World
echo   4. Go to Remix menu
echo   5. Upload Key Overlay to Steam Workshop
echo.
echo See UPLOAD_GUIDE.md for detailed instructions.
echo ============================================
pause