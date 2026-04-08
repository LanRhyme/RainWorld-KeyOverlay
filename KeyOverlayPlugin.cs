using System;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace KeyOverlay
{
    /// <summary>
    /// Main BepInEx plugin - Author: LanRhyme
    /// </summary>
    [BepInPlugin("keyoverlay", "Key Overlay", "1.0.0")]
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
        
        private InputMonitor _inputMonitor;
        private KeyOverlayUI _ui;
        private PauseMenuIntegration _pauseMenu;
        private ConfigWrapper _configWrapper;
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
            ConfigPanelX = Config.Bind("Display", "PanelX", 200f);
            ConfigPanelY = Config.Bind("Display", "PanelY", 100f);
            ConfigScale = Config.Bind("Display", "Scale", 1.0f);
            ConfigOpacity = Config.Bind("Display", "Opacity", 0.8f);
            ConfigShowKeyboard = Config.Bind("Features", "ShowKeyboard", true);
            ConfigShowGamepad = Config.Bind("Features", "ShowGamepad", true);
            ConfigShowComboStats = Config.Bind("Features", "ShowComboStats", true);
            ConfigShowKeyNames = Config.Bind("Features", "ShowKeyNames", true);
            ConfigShowMovementKeys = Config.Bind("Keys", "MovementKeys", true);
            ConfigShowActionKeys = Config.Bind("Keys", "ActionKeys", true);
            
            // Color settings - default pixel-art style colors
            ConfigKeyColorNormal = Config.Bind("Colors", "KeyColorNormal", new Color(0.2f, 0.2f, 0.25f));
            ConfigKeyColorPressed = Config.Bind("Colors", "KeyColorPressed", new Color(0.95f, 0.75f, 0.25f));
            ConfigBorderColor = Config.Bind("Colors", "BorderColor", new Color(0.1f, 0.1f, 0.12f));
            ConfigBorderOpacity = Config.Bind("Colors", "BorderOpacity", 0.9f);
            ConfigFillOpacity = Config.Bind("Colors", "FillOpacity", 0.7f);
            ConfigPressedEffectOpacity = Config.Bind("Colors", "PressedEffectOpacity", 1.0f);
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
                if (Input.GetKeyDown(KeyCode.F2))
                {
                    if (ConfigPanelX != null) ConfigPanelX.Value = 200f;
                    if (ConfigPanelY != null) ConfigPanelY.Value = 100f;
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
    }
    
    public class ConfigWrapper
    {
        private KeyOverlayPlugin p;
        public float PanelX => p?.ConfigPanelX?.Value ?? 200f;
        public float PanelY => p?.ConfigPanelY?.Value ?? 100f;
        public float Scale => p?.ConfigScale?.Value ?? 1.0f;
        public float Opacity => p?.ConfigOpacity?.Value ?? 0.8f;
        public bool ShowKeyboard => p?.ConfigShowKeyboard?.Value ?? true;
        public bool ShowGamepad => p?.ConfigShowGamepad?.Value ?? true;
        public bool ShowComboStats => p?.ConfigShowComboStats?.Value ?? true;
        public bool ShowKeyNames => p?.ConfigShowKeyNames?.Value ?? true;
        public bool ShowMovementKeys => p?.ConfigShowMovementKeys?.Value ?? true;
        public bool ShowActionKeys => p?.ConfigShowActionKeys?.Value ?? true;
        
        // Color properties
        public Color KeyColorNormal => p?.ConfigKeyColorNormal?.Value ?? new Color(0.2f, 0.2f, 0.25f);
        public Color KeyColorPressed => p?.ConfigKeyColorPressed?.Value ?? new Color(0.95f, 0.75f, 0.25f);
        public Color BorderColor => p?.ConfigBorderColor?.Value ?? new Color(0.1f, 0.1f, 0.12f);
        public float BorderOpacity => p?.ConfigBorderOpacity?.Value ?? 0.9f;
        public float FillOpacity => p?.ConfigFillOpacity?.Value ?? 0.7f;
        public float PressedEffectOpacity => p?.ConfigPressedEffectOpacity?.Value ?? 1.0f;
        
        public ConfigWrapper(KeyOverlayPlugin plugin) => p = plugin;
        
        public void SetPanelX(float v) { if (p?.ConfigPanelX != null) p.ConfigPanelX.Value = v; }
        public void SetPanelY(float v) { if (p?.ConfigPanelY != null) p.ConfigPanelY.Value = v; }
        public void SetScale(float v) { if (p?.ConfigScale != null) p.ConfigScale.Value = Mathf.Clamp(v, 1f, 3f); }
        public void SetOpacity(float v) { if (p?.ConfigOpacity != null) p.ConfigOpacity.Value = Mathf.Clamp(v, 0.1f, 1f); }
        public void SetShowKeyboard(bool v) { if (p?.ConfigShowKeyboard != null) p.ConfigShowKeyboard.Value = v; }
        public void SetShowGamepad(bool v) { if (p?.ConfigShowGamepad != null) p.ConfigShowGamepad.Value = v; }
        public void SetShowComboStats(bool v) { if (p?.ConfigShowComboStats != null) p.ConfigShowComboStats.Value = v; }
        public void SetShowKeyNames(bool v) { if (p?.ConfigShowKeyNames != null) p.ConfigShowKeyNames.Value = v; }
        public void SetShowMovementKeys(bool v) { if (p?.ConfigShowMovementKeys != null) p.ConfigShowMovementKeys.Value = v; }
        public void SetShowActionKeys(bool v) { if (p?.ConfigShowActionKeys != null) p.ConfigShowActionKeys.Value = v; }
        
        // Color setters
        public void SetKeyColorNormal(Color v) { if (p?.ConfigKeyColorNormal != null) p.ConfigKeyColorNormal.Value = v; }
        public void SetKeyColorPressed(Color v) { if (p?.ConfigKeyColorPressed != null) p.ConfigKeyColorPressed.Value = v; }
        public void SetBorderColor(Color v) { if (p?.ConfigBorderColor != null) p.ConfigBorderColor.Value = v; }
        public void SetBorderOpacity(float v) { if (p?.ConfigBorderOpacity != null) p.ConfigBorderOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        public void SetFillOpacity(float v) { if (p?.ConfigFillOpacity != null) p.ConfigFillOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        public void SetPressedEffectOpacity(float v) { if (p?.ConfigPressedEffectOpacity != null) p.ConfigPressedEffectOpacity.Value = Mathf.Clamp(v, 0f, 1f); }
        
        public void Save() => p?.Config?.Save();
    }
}