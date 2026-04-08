#!/bin/bash
echo "============================================"
echo "  Rain World Key Overlay Workshop Package"
echo "============================================"
echo ""

GAME_PATH="/path/to/Rain World"  # 请修改为实际路径
MOD_DIR="RainWorld_Data/StreamingAssets/mods/keyoverlay"
WORKSHOP_DIR="workshop/keyoverlay"

echo "[1] Building project (Release mode)..."
dotnet build KeyOverlay.csproj -c Release
if [ $? -ne 0 ]; then
    echo "    Build FAILED!"
    exit 1
fi
echo "    Build successful!"

echo ""
echo "[2] Copying DLL to workshop directory..."
cp -f bin/Release/KeyOverlay.dll "$WORKSHOP_DIR/plugins/KeyOverlay.dll"
echo "    Done!"

echo ""
echo "[3] Copying modinfo.json..."
cp -f modinfo.json "$WORKSHOP_DIR/modinfo.json"
echo "    Done!"

echo ""
echo "[4] Installing to Rain World game directory..."
if [ -d "$GAME_PATH/$MOD_DIR" ]; then
    echo "    Removing old version..."
    rm -rf "$GAME_PATH/$MOD_DIR"
fi
mkdir -p "$GAME_PATH/$MOD_DIR/plugins"
cp -f "$WORKSHOP_DIR/plugins/KeyOverlay.dll" "$GAME_PATH/$MOD_DIR/plugins/"
cp -f "$WORKSHOP_DIR/modinfo.json" "$GAME_PATH/$MOD_DIR/"
if [ -f "$WORKSHOP_DIR/thumbnail.png" ]; then
    cp -f "$WORKSHOP_DIR/thumbnail.png" "$GAME_PATH/$MOD_DIR/"
    echo "    thumbnail.png copied!"
fi
echo "    Done!"

echo ""
echo "============================================"
echo "  Workshop Package Ready!"
echo "============================================"
echo ""
echo "Next steps:"
echo "  1. Create thumbnail.png (512x512 recommended)"
echo "  2. Place it in workshop/keyoverlay/"
echo "  3. Start Rain World and go to Remix menu"
echo "  4. Upload Key Overlay to Steam Workshop"
echo ""
echo "See UPLOAD_GUIDE.md for detailed instructions."