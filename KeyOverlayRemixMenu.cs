using System;
using System.Collections.Generic;
using UnityEngine;
using Menu.Remix.MixedUI;
using Menu.Remix;

namespace KeyOverlay
{
    /// <summary>
    /// Remix Menu - Configure menu toggle key and other settings
    /// Author: LanRhyme
    /// </summary>
    public class KeyOverlayRemixMenu : OptionInterface
    {
        private readonly KeyOverlayPlugin _plugin;
        private readonly ConfigWrapper _config;
        private OpTab _mainTab;
        
        // Configurable for menu key binding
        public readonly Configurable<string> MenuKeyConfig;
        
        // Key binding state
        private bool _waitingForKey = false;
        private OpSimpleButton _menuKeyBtn;
        private OpLabel _waitingLabel;
        private OpLabel _currentKeyLabel;
        
        public KeyOverlayRemixMenu(KeyOverlayPlugin plugin, ConfigWrapper config)
        {
            _plugin = plugin;
            _config = config;
            
            // Bind menu key configuration using OptionInterface's config container
            string initialKey = plugin.ConfigMenuKey?.Value ?? "F1";
            MenuKeyConfig = this.config.Bind("MenuKey", initialKey);
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            // Refresh language detection when Remix menu opens
            Localization.Refresh();
            
            _mainTab = new OpTab(this, "Settings");
            Tabs = new OpTab[] { _mainTab };
            
            SetupMainTab();
        }
        
        private void SetupMainTab()
        {
            float y = 550f;
            
            // Title
            var title = new OpLabel(new Vector2(50f, y), new Vector2(400f, 35f), Localization.Get("Remix_Title"), FLabelAlignment.Center);
            title.color = new Color(0.9f, 0.7f, 0.3f);
            _mainTab.AddItems(title);
            y -= 50f;
            
            // Separator
            var sep1 = new OpLabel(new Vector2(150f, y), new Vector2(200f, 20f), "─────────────────", FLabelAlignment.Center);
            sep1.color = new Color(0.4f, 0.4f, 0.4f);
            _mainTab.AddItems(sep1);
            y -= 35f;
            
            // Menu Toggle Key section title
            var menuKeyTitle = new OpLabel(new Vector2(50f, y), new Vector2(400f, 25f), Localization.Get("Remix_MenuKeySection"), FLabelAlignment.Center);
            menuKeyTitle.color = new Color(0.8f, 0.8f, 0.8f);
            _mainTab.AddItems(menuKeyTitle);
            y -= 30f;
            
            // Current key label
            string currentKey = MenuKeyConfig.Value ?? "F1";
            _currentKeyLabel = new OpLabel(new Vector2(150f, y), new Vector2(200f, 25f), $"{Localization.Get("Remix_CurrentKey")}: [ {currentKey} ]", FLabelAlignment.Center);
            _currentKeyLabel.color = new Color(0.5f, 0.7f, 0.9f);
            _mainTab.AddItems(_currentKeyLabel);
            y -= 30f;
            
            // Key binding button
            _menuKeyBtn = new OpSimpleButton(new Vector2(100f, y), new Vector2(300f, 35f), Localization.Get("Remix_ClickToSet"));
            _menuKeyBtn.OnClick += (_) => StartKeyBinding();
            _mainTab.AddItems(_menuKeyBtn);
            y -= 40f;
            
            // Hint label (shows current status)
            _waitingLabel = new OpLabel(new Vector2(50f, y), new Vector2(400f, 25f), Localization.Get("Remix_DefaultKeyF1"), FLabelAlignment.Center);
            _waitingLabel.color = new Color(0.5f, 0.5f, 0.5f);
            _mainTab.AddItems(_waitingLabel);
            y -= 50f;
            
            // Separator
            var sep2 = new OpLabel(new Vector2(150f, y), new Vector2(200f, 20f), "─────────────────", FLabelAlignment.Center);
            sep2.color = new Color(0.4f, 0.4f, 0.4f);
            _mainTab.AddItems(sep2);
            y -= 40f;
            
            // In-game config hint
            var inGameHint = new OpLabel(new Vector2(50f, y), new Vector2(400f, 25f), Localization.Get("Remix_InGameConfig"), FLabelAlignment.Center);
            inGameHint.color = new Color(0.7f, 0.7f, 0.7f);
            _mainTab.AddItems(inGameHint);
            y -= 25f;
            
            var inGameDesc = new OpLabel(new Vector2(50f, y), new Vector2(400f, 25f), Localization.Get("Remix_ConfigPanelDesc"), FLabelAlignment.Center);
            inGameDesc.color = new Color(0.5f, 0.5f, 0.5f);
            _mainTab.AddItems(inGameDesc);
            y -= 50f;
            
            // Separator
            var sep3 = new OpLabel(new Vector2(150f, y), new Vector2(200f, 20f), "─────────────────", FLabelAlignment.Center);
            sep3.color = new Color(0.4f, 0.4f, 0.4f);
            _mainTab.AddItems(sep3);
            y -= 50f;
            
            // Reset All Settings button
            var resetBtn = new OpSimpleButton(new Vector2(100f, y), new Vector2(300f, 35f), Localization.Get("Remix_ResetAll"));
            resetBtn.OnClick += (_) => ResetAllSettings();
            _mainTab.AddItems(resetBtn);
            y -= 45f;
            
            // Reset description
            var resetDesc = new OpLabel(new Vector2(50f, y), new Vector2(400f, 40f), Localization.Get("Remix_ResetDesc"), FLabelAlignment.Center);
            resetDesc.color = new Color(0.4f, 0.4f, 0.4f);
            _mainTab.AddItems(resetDesc);
        }
        
        private void StartKeyBinding()
        {
            _waitingForKey = true;
            _menuKeyBtn.text = Localization.Get("Remix_PressAnyKey");
            _waitingLabel.text = Localization.Get("Remix_EscapeCancel");
            _waitingLabel.color = new Color(1f, 0.6f, 0.2f);
        }
        
        public override void Update()
        {
            base.Update();
            
            if (_waitingForKey)
            {
                // Check for key press
                foreach (KeyCode key in GetCommonKeys())
                {
                    if (Input.GetKeyDown(key))
                    {
                        SetMenuKey(key);
                        _waitingForKey = false;
                        break;
                    }
                }
                
                // Cancel with Escape
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    CancelKeyBinding();
                }
            }
        }
        
        private void SetMenuKey(KeyCode key)
        {
            string keyName = key.ToString();
            MenuKeyConfig.Value = keyName;
            _config.SetMenuKey(key);
            _config.Save();
            
            _menuKeyBtn.text = Localization.Get("Remix_ClickToSet");
            _currentKeyLabel.text = $"{Localization.Get("Remix_CurrentKey")}: [ {keyName} ]";
            _waitingLabel.text = Localization.Get("Remix_KeySetSuccess");
            _waitingLabel.color = new Color(0.2f, 0.8f, 0.2f);
            
            KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Menu key set to {keyName}");
        }
        
        private void CancelKeyBinding()
        {
            _waitingForKey = false;
            string currentKey = MenuKeyConfig.Value ?? "F1";
            _menuKeyBtn.text = Localization.Get("Remix_ClickToSet");
            _currentKeyLabel.text = $"{Localization.Get("Remix_CurrentKey")}: [ {currentKey} ]";
            _waitingLabel.text = Localization.Get("Remix_DefaultKeyF1");
            _waitingLabel.color = new Color(0.5f, 0.5f, 0.5f);
        }
        
        private IEnumerable<KeyCode> GetCommonKeys()
        {
            // Function keys
            for (int i = 1; i <= 12; i++)
            {
                yield return (KeyCode)Enum.Parse(typeof(KeyCode), $"F{i}");
            }
            
            // Letter keys
            for (char c = 'A'; c <= 'Z'; c++)
            {
                yield return (KeyCode)Enum.Parse(typeof(KeyCode), c.ToString());
            }
            
            // Number keys (top row)
            for (int i = 0; i <= 9; i++)
            {
                yield return (KeyCode)Enum.Parse(typeof(KeyCode), $"Alpha{i}");
            }
            
            // Special keys
            yield return KeyCode.Space;
            yield return KeyCode.Return;
            yield return KeyCode.Tab;
            yield return KeyCode.Backspace;
            yield return KeyCode.Insert;
            yield return KeyCode.Delete;
            yield return KeyCode.Home;
            yield return KeyCode.End;
            yield return KeyCode.PageUp;
            yield return KeyCode.PageDown;
            
            // Arrow keys
            yield return KeyCode.UpArrow;
            yield return KeyCode.DownArrow;
            yield return KeyCode.LeftArrow;
            yield return KeyCode.RightArrow;
            
            // Modifier keys
            yield return KeyCode.LeftShift;
            yield return KeyCode.RightShift;
            yield return KeyCode.LeftControl;
            yield return KeyCode.RightControl;
            yield return KeyCode.LeftAlt;
            yield return KeyCode.RightAlt;
        }
        
        private void ResetAllSettings()
        {
            if (_plugin == null || _config == null) return;
            
            try
            {
                // Reset position
                _config.SetPanelX(136f);
                _config.SetPanelY(666f);
                _config.SetScale(1.0f);
                _config.SetOpacity(0.8f);
                
                // Reset visibility
                _config.SetShowKeyboard(true);
                _config.SetShowGamepad(true);
                _config.SetShowComboStats(true);
                _config.SetShowKeyNames(true);
                _config.SetShowMovementKeys(true);
                _config.SetShowActionKeys(true);
                
                // Reset style
                _config.SetBorderOpacity(0.45f);
                _config.SetFillOpacity(0.1f);
                _config.SetPressedEffectOpacity(0.75f);
                _config.SetBorderWidth(1.5f);
                _config.SetFontSize(11);
                
                // Reset colors (white theme)
                _config.SetKeyColorNormal(new Color(1f, 1f, 1f));
                _config.SetKeyColorPressed(new Color(1f, 1f, 1f));
                _config.SetBorderColor(new Color(1f, 1f, 1f));
                
                // Reset key bindings
                _config.SetMenuKey(KeyCode.F1);
                _config.SetKeyUp(KeyCode.W);
                _config.SetKeyDown(KeyCode.S);
                _config.SetKeyLeft(KeyCode.A);
                _config.SetKeyRight(KeyCode.D);
                _config.SetKeyJump(KeyCode.Space);
                _config.SetKeyThrow(KeyCode.K);
                _config.SetKeyGrab(KeyCode.L);
                
                // Update Remix menu display
                MenuKeyConfig.Value = "F1";
                _currentKeyLabel.text = $"{Localization.Get("Remix_CurrentKey")}: [ F1 ]";
                
                // Save
                _config.Save();
                
                KeyOverlayPlugin.Log?.LogInfo("[KeyOverlay] All settings reset to defaults via Remix menu");
            }
            catch (Exception ex)
            {
                KeyOverlayPlugin.Log?.LogError($"[KeyOverlay] Reset failed: {ex}");
            }
        }
    }
}