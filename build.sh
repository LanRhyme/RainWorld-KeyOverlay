#!/bin/bash

echo "========================================"
echo "Key Overlay Mod Build Script"
echo "========================================"
echo

echo "Checking dependencies..."
if [ ! -f "/d/Program Files/steam/steamapps/common/Rain World/RainWorld_Data/Managed/Assembly-CSharp.dll" ]; then
    echo "ERROR: Rain World Assembly-CSharp.dll not found!"
    echo "Please ensure Rain World is installed in the correct location."
    exit 1
fi

if [ ! -f "/d/Program Files/steam/steamapps/common/Rain World/BepInEx/core/BepInEx.Core.dll" ]; then
    echo "ERROR: BepInEx.Core.dll not found!"
    echo "Please ensure BepInEx is installed in Rain World."
    exit 1
fi

echo "Dependencies found."
echo

echo "Building Key Overlay mod..."
dotnet build KeyOverlay.csproj -c Release

if [ $? -ne 0 ]; then
    echo
    echo "========================================"
    echo "Build FAILED!"
    echo "========================================"
    exit 1
fi

echo
echo "========================================"
echo "Build SUCCESS!"
echo "========================================"
echo

echo "Installing mod to Rain World..."
echo "Copying modinfo.json..."
cp -f "modinfo.json" "/d/Program Files/steam/steamapps/common/Rain World/RainWorld_Data/StreamingAssets/mods/keyoverlay/modinfo.json"

echo "Mod installed successfully!"
echo
echo "Location: D:\Program Files\steam\steamapps\common\Rain World\RainWorld_Data\StreamingAssets\mods\keyoverlay"
echo
echo "You can now load the mod in Rain World's 'Expansion' menu."
echo "========================================"