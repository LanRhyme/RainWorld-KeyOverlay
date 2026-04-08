using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;
using HarmonyLib;

namespace KeyOverlay
{
    /// <summary>
    /// Main BepInEx plugin - Author: LanRhyme
    /// Steam Workshop ready with Remix menu integration
    /// </summary>
    [BepInPlugin("keyoverlay", "Key Overlay", "1.0.0")]
    [BepInDependency("rwremix", BepInDependency.DependencyFlags.SoftDependency)]
    public class KeyOverlayPlugin : BaseUnityPlugin
    {
        internal static KeyOverlayPlugin Instance;
        internal static ManualLogSource Log;
        
        internal ConfigEntry<float> ConfigPanelX;
        internal ConfigEntry<float> ConfigPanelY;
        internal ConfigEntry<float> ConfigScale;
        internal ConfigEntry<float> ConfigOpacity;
        internal ConfigEntry<bool> ConfigShowKeyboard;
        internal ConfigEntry<bool> ConfigShowGamepad;
        internal ConfigEntry<bool> ConfigShowComboStats;
        internal ConfigEntry<bool> ConfigShowKeyNames;
        internal ConfigEntry<bool> ConfigShowMovementKeys;
        internal ConfigEntry<bool> ConfigShowActionKeys;
        internal ConfigEntry<bool> ConfigShowIconMode; // true = icons, false = key names
        
        // Color settings
        internal ConfigEntry<Color> ConfigKeyColorNormal;
        internal ConfigEntry<Color> ConfigKeyColorPressed;
        internal ConfigEntry<Color> ConfigBorderColor;
        internal ConfigEntry<float> ConfigBorderOpacity;
        internal ConfigEntry<float> ConfigFillOpacity;
        internal ConfigEntry<float> ConfigPressedEffectOpacity;
        
        // Style settings
        internal ConfigEntry<int> ConfigFontSize;
        internal ConfigEntry<float> ConfigBorderWidth;
        
        // Key bindings (KeyCode names)
        internal ConfigEntry<string> ConfigKeyUp;
        internal ConfigEntry<string> ConfigKeyDown;
        internal ConfigEntry<string> ConfigKeyLeft;
        internal ConfigEntry<string> ConfigKeyRight;
        internal ConfigEntry<string> ConfigKeyJump;
        internal ConfigEntry<string> ConfigKeyThrow;
        internal ConfigEntry<string> ConfigKeyGrab;
        
        private InputMonitor _inputMonitor;
        private KeyOverlayUI _ui;
        private PauseMenuIntegration _pauseMenu;
        private ConfigWrapper _configWrapper;
        private KeyOverlayRemixMenu _remixMenu;
        private bool _initialized = false;
        
        private void Awake()
        {
            Instance = this;
            Log = Logger;
            
            Log.LogInfo("[KeyOverlay] Starting...");
            
            try
            {
                InitConfig();
                _configWrapper = new ConfigWrapper(this);
                _inputMonitor = new InputMonitor();
                _ui = new KeyOverlayUI(_configWrapper, _inputMonitor);
                _pauseMenu = new PauseMenuIntegration(_configWrapper, _ui, _inputMonitor);
                
                // Use Harmony to hook OnModsInit for Remix menu registration
                var harmony = new Harmony("keyoverlay");
                harmony.PatchAll(typeof(KeyOverlayPlugin).Assembly);
                
                _initialized = true;
                Log.LogInfo("[KeyOverlay] Loaded successfully!");
            }
            catch (Exception ex)
            {
                Log.LogError($"[KeyOverlay] Init failed: {ex}");
            }
        }
        
        private void InitConfig()
        {
            ConfigPanelX = Config.Bind("Display", "PanelX", 136f);
            ConfigPanelY = Config.Bind("Display", "PanelY", 666f);
            ConfigScale = Config.Bind("Display", "Scale", 1.0f);
            ConfigOpacity = Config.Bind("Display", "Opacity", 0.8f);
            ConfigShowKeyboard = Config.Bind("Features", "ShowKeyboard", true);
            ConfigShowGamepad = Config.Bind("Features", "ShowGamepad", true);
            ConfigShowComboStats = Config.Bind("Features", "ShowComboStats", true);
            ConfigShowKeyNames = Config.Bind("Features", "ShowKeyNames", true);
            ConfigShowMovementKeys = Config.Bind("Keys", "MovementKeys", true);
            ConfigShowActionKeys = Config.Bind("Keys", "ActionKeys", true);
            ConfigShowIconMode = Config.Bind("Display", "ShowIconMode", false); // false = key names (W/A/S/D), true = icons (▲▼◄►)
            
            // Color settings - user's custom colors (white theme)
            ConfigKeyColorNormal = Config.Bind("Colors", "KeyColorNormal", new Color(1f, 1f, 1f));
            ConfigKeyColorPressed = Config.Bind("Colors", "KeyColorPressed", new Color(1f, 1f, 1f));
            ConfigBorderColor = Config.Bind("Colors", "BorderColor", new Color(1f, 1f, 1f));
            ConfigBorderOpacity = Config.Bind("Colors", "BorderOpacity", 0.45f);
            ConfigFillOpacity = Config.Bind("Colors", "FillOpacity", 0.1f);
            ConfigPressedEffectOpacity = Config.Bind("Colors", "PressedEffectOpacity", 0.75f);
            
            // Style settings
            ConfigFontSize = Config.Bind("Style", "FontSize", 11);
            ConfigBorderWidth = Config.Bind("Style", "BorderWidth", 1.5f);
            
            // Key bindings - user's WASD configuration
            ConfigKeyUp = Config.Bind("KeyBindings", "Up", "W");
            ConfigKeyDown = Config.Bind("KeyBindings", "Down", "S");
            ConfigKeyLeft = Config.Bind("KeyBindings", "Left", "A");
            ConfigKeyRight = Config.Bind("KeyBindings", "Right", "D");
            ConfigKeyJump = Config.Bind("KeyBindings", "Jump", "Space");
            ConfigKeyThrow = Config.Bind("KeyBindings", "Throw", "K");
            ConfigKeyGrab = Config.Bind("KeyBindings", "Grab", "L");
        }
        
        private void Update()
        {
            if (!_initialized) return;
            try
            {
                _inputMonitor?.Update();
                if (Input.GetKeyDown(KeyCode.F1))
                {
                    if (_pauseMenu != null)
                    {
                        if (_pauseMenu.IsMenuActive) _pauseMenu.CloseMenu();
                        else _pauseMenu.OpenMenu();
                    }
                }
            }
            catch { }
        }
        
        private void OnGUI()
        {
            if (!_initialized) return;
            try
            {
                _ui?.OnGUI();
                _pauseMenu?.OnGUI();
            }
            catch { }
        }
        
        private void OnDestroy()
        {
            try { Config?.Save(); } catch { }
        }
        
        internal void RegisterRemixMenu()
        {
            if (_remixMenu != null) return;
            try
            {
                _remixMenu = new KeyOverlayRemixMenu(this, _configWrapper);
                MachineConnector.SetRegisteredOI("keyoverlay", _remixMenu);
                Log.LogInfo("[KeyOverlay] Remix menu registered");
            }
            catch (Exception ex)
            {
                Log.LogWarning($"[KeyOverlay] Remix menu registration failed: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Harmony patch to register Remix menu when mods initialize
    /// </summary>
    [HarmonyPatch(typeof(RainWorld), "OnModsInit")]
    static class RainWorldOnModsInitPatch
    {
        static bool _initialized = false;
        
        static void Postfix()
        {
            if (_initialized) return;
            _initialized = true;
            
            try
            {
                KeyOverlayPlugin.Instance?.RegisterRemixMenu();
            }
            catch (Exception ex)
            {
                KeyOverlayPlugin.Log?.LogWarning($"[KeyOverlay] OnModsInit patch error: {ex.Message}");
            }
        }
    }
    
    public class ConfigWrapper
    {
        private KeyOverlayPlugin p;
        public static ConfigWrapper Instance { get; private set; }
        
        public float PanelX => p?.ConfigPanelX?.Value ?? 136f;
        public float PanelY => p?.ConfigPanelY?.Value ?? 666f;
        public float Scale => p?.ConfigScale?.Value ?? 1.0f;
        public float Opacity => p?.ConfigOpacity?.Value ?? 0.8f;
        public bool ShowKeyboard => p?.ConfigShowKeyboard?.Value ?? true;
        public bool ShowGamepad => p?.ConfigShowGamepad?.Value ?? true;
        public bool ShowComboStats => p?.ConfigShowComboStats?.Value ?? true;
        public bool ShowKeyNames => p?.ConfigShowKeyNames?.Value ?? true;
        public bool ShowMovementKeys => p?.ConfigShowMovementKeys?.Value ?? true;
        public bool ShowActionKeys => p?.ConfigShowActionKeys?.Value ?? true;
        public bool ShowIconMode => p?.ConfigShowIconMode?.Value ?? true; // true = icons, false = key names
        
        // Get display name for a key (returns key binding name or icon)
        public string GetKeyDisplayName(string keyName)
        {
            if (ShowIconMode)
            {
                // Return icons
                switch (keyName)
                {
                    case "Up": return "▲";
                    case "Down": return "▼";
                    case "Left": return "◄";
                    case "Right": return "►";
                    case "Jump": return "●";
                    case "Throw": return "◆";
                    case "Grab": return "■";
                    default: return "?";
                }
            }
            else
            {
                // Return actual key binding name
                KeyCode key = GetKeyCodeForName(keyName);
                return FormatKeyName(key);
            }
        }
        
        private KeyCode GetKeyCodeForName(string keyName)
        {
            switch (keyName)
            {
                case "Up": return KeyUp;
                case "Down": return KeyDown;
                case "Left": return KeyLeft;
                case "Right": return KeyRight;
                case "Jump": return KeyJump;
                case "Throw": return KeyThrow;
                case "Grab": return KeyGrab;
                default: return KeyCode.None;
            }
        }
        
        private string FormatKeyName(KeyCode key)
        {
            string name = key.ToString();
            // Format common keys - max 3 characters
            if (name == "Space") return "SPC";
            if (name == "LeftShift" || name == "RightShift") return "SFT";
            if (name == "LeftControl" || name == "RightControl") return "CTL";
            if (name == "LeftAlt" || name == "RightAlt") return "ALT";
            if (name.StartsWith("Alpha")) return name.Substring(5); // Alpha1 -> 1
            if (name == "Return") return "ENT";
            if (name == "Backspace") return "BSP";
            if (name == "Tab") return "TAB";
            if (name == "Escape") return "ESC";
            if (name == "Insert") return "INS";
            if (name == "Delete") return "DEL";
            if (name == "Home") return "HOM";
            if (name == "End") return "END";
            if (name == "PageUp") return "PUP";
            if (name == "PageDown") return "PDN";
            // Arrow keys - use single character
            if (name == "UpArrow") return "↑";
            if (name == "DownArrow") return "↓";
            if (name == "LeftArrow") return "←";
            if (name == "RightArrow") return "→";
            // Limit to 3 chars if longer
            if (name.Length > 3) name = name.Substring(0, 3);
            return name; // W, A, S, D, K, L etc.
        }
        
        // Color properties
        public Color KeyColorNormal => p?.ConfigKeyColorNormal?.Value ?? new Color(1f, 1f, 1f);
        public Color KeyColorPressed => p?.ConfigKeyColorPressed?.Value ?? new Color(1f, 1f, 1f);
        public Color BorderColor => p?.ConfigBorderColor?.Value ?? new Color(1f, 1f, 1f);
        public float BorderOpacity => p?.ConfigBorderOpacity?.Value ?? 0.45f;
        public float FillOpacity => p?.ConfigFillOpacity?.Value ?? 0.1f;
        public float PressedEffectOpacity => p?.ConfigPressedEffectOpacity?.Value ?? 0.75f;
        
        // Style properties
        public int FontSize => p?.ConfigFontSize?.Value ?? 11;
        public float BorderWidth => p?.ConfigBorderWidth?.Value ?? 1.5f;
        
        // Key binding properties (returns KeyCode)
        public KeyCode KeyUp => ParseKeyCode(p?.ConfigKeyUp?.Value, KeyCode.W);
        public KeyCode KeyDown => ParseKeyCode(p?.ConfigKeyDown?.Value, KeyCode.S);
        public KeyCode KeyLeft => ParseKeyCode(p?.ConfigKeyLeft?.Value, KeyCode.A);
        public KeyCode KeyRight => ParseKeyCode(p?.ConfigKeyRight?.Value, KeyCode.D);
        public KeyCode KeyJump => ParseKeyCode(p?.ConfigKeyJump?.Value, KeyCode.Space);
        public KeyCode KeyThrow => ParseKeyCode(p?.ConfigKeyThrow?.Value, KeyCode.K);
        public KeyCode KeyGrab => ParseKeyCode(p?.ConfigKeyGrab?.Value, KeyCode.L);
        
        private KeyCode ParseKeyCode(string name, KeyCode defaultKey)
        {
            if (string.IsNullOrEmpty(name)) return defaultKey;
            if (name.ToLower() == "none") return defaultKey;
            try { return (KeyCode)Enum.Parse(typeof(KeyCode), name, true); }
            catch { return defaultKey; }
        }
        
        public ConfigWrapper(KeyOverlayPlugin plugin) { p = plugin; Instance = this; }
        
        public void SetPanelX(float v) { if (p?.ConfigPanelX != null) p.ConfigPanelX.Value = v; }
        public void SetPanelY(float v) { if (p?.ConfigPanelY != null) p.ConfigPanelY.Value = v; }
        public void SetScale(float v) { if (p?.ConfigScale != null) p.ConfigScale.Value = Mathf.Clamp(v, 0.5f, 3f); }
        public void SetOpacity(float v) { if (p?.ConfigOpacity != null) p.ConfigOpacity.Value = Mathf.Clamp(v, 0.1f, 1f); }
        public void SetShowKeyboard(bool v) { if (p?.ConfigShowKeyboard != null) p.ConfigShowKeyboard.Value = v; }
        public void SetShowGamepad(bool v) { if (p?.ConfigShowGamepad != null) p.ConfigShowGamepad.Value = v; }
        public void SetShowComboStats(bool v) { if (p?.ConfigShowComboStats != null) p.ConfigShowComboStats.Value = v; }
        public void SetShowKeyNames(bool v) { if (p?.ConfigShowKeyNames != null) p.ConfigShowKeyNames.Value = v; }
        public void SetShowMovementKeys(bool v) { if (p?.ConfigShowMovementKeys != null) p.ConfigShowMovementKeys.Value = v; }
        public void SetShowActionKeys(bool v) { if (p?.ConfigShowActionKeys != null) p.ConfigShowActionKeys.Value = v; }
        public void SetShowIconMode(bool v) { if (p?.ConfigShowIconMode != null) p.ConfigShowIconMode.Value = v; }
        
        public void SetBorderOpacity(float v) { if (p?.ConfigBorderOpacity != null) p.ConfigBorderOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        public void SetFillOpacity(float v) { if (p?.ConfigFillOpacity != null) p.ConfigFillOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        public void SetPressedEffectOpacity(float v) { if (p?.ConfigPressedEffectOpacity != null) p.ConfigPressedEffectOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        public void SetFontSize(int v) { if (p?.ConfigFontSize != null) p.ConfigFontSize.Value = Mathf.Clamp(v, 8, 20); }
        public void SetBorderWidth(float v) { if (p?.ConfigBorderWidth != null) p.ConfigBorderWidth.Value = Mathf.Clamp(v, 0.5f, 4f); }
        
        // Color setters
        public void SetKeyColorNormal(Color v) { if (p?.ConfigKeyColorNormal != null) p.ConfigKeyColorNormal.Value = v; }
        public void SetKeyColorPressed(Color v) { if (p?.ConfigKeyColorPressed != null) p.ConfigKeyColorPressed.Value = v; }
        public void SetBorderColor(Color v) { if (p?.ConfigBorderColor != null) p.ConfigBorderColor.Value = v; }
        
        public void SetKeyUp(KeyCode v) { if (p?.ConfigKeyUp != null) p.ConfigKeyUp.Value = v.ToString(); }
        public void SetKeyDown(KeyCode v) { if (p?.ConfigKeyDown != null) p.ConfigKeyDown.Value = v.ToString(); }
        public void SetKeyLeft(KeyCode v) { if (p?.ConfigKeyLeft != null) p.ConfigKeyLeft.Value = v.ToString(); }
        public void SetKeyRight(KeyCode v) { if (p?.ConfigKeyRight != null) p.ConfigKeyRight.Value = v.ToString(); }
        public void SetKeyJump(KeyCode v) { if (p?.ConfigKeyJump != null) p.ConfigKeyJump.Value = v.ToString(); }
        public void SetKeyThrow(KeyCode v) { if (p?.ConfigKeyThrow != null) p.ConfigKeyThrow.Value = v.ToString(); }
        public void SetKeyGrab(KeyCode v) { if (p?.ConfigKeyGrab != null) p.ConfigKeyGrab.Value = v.ToString(); }
        
        public void Save() => p?.Config?.Save();
    }
}