using System;
using UnityEngine;
using Menu.Remix.MixedUI;
using Menu.Remix;

namespace KeyOverlay
{
    /// <summary>
    /// Simplified Remix Menu - Just shows F1 instructions and reset button
    /// Author: LanRhyme
    /// </summary>
    public class KeyOverlayRemixMenu : OptionInterface
    {
        private readonly KeyOverlayPlugin _plugin;
        private readonly ConfigWrapper _config;
        private OpTab _mainTab;
        
        public KeyOverlayRemixMenu(KeyOverlayPlugin plugin, ConfigWrapper config)
        {
            _plugin = plugin;
            _config = config;
        }
        
        public override void Initialize()
        {
            base.Initialize();
            
            _mainTab = new OpTab(this, "Settings");
            Tabs = new OpTab[] { _mainTab };
            
            SetupMainTab();
        }
        
        private void SetupMainTab()
        {
            float y = 550f;
            
            // Big title with icon
            var titleIcon = new OpLabel(new Vector2(50f, y), new Vector2(400f, 40f), "⌨ KEY OVERLAY", FLabelAlignment.Center);
            titleIcon.color = new Color(0.9f, 0.7f, 0.3f); // Yellow-ish color
            _mainTab.AddItems(titleIcon);
            y -= 60f;
            
            // Big F1 instruction text
            var f1Label1 = new OpLabel(new Vector2(50f, y), new Vector2(400f, 35f), "Press F1 in-game to configure", FLabelAlignment.Center);
            f1Label1.color = new Color(1f, 1f, 1f);
            _mainTab.AddItems(f1Label1);
            y -= 40f;
            
            var f1Label2 = new OpLabel(new Vector2(50f, y), new Vector2(400f, 30f), "all settings and key bindings", FLabelAlignment.Center);
            f1Label2.color = new Color(0.7f, 0.7f, 0.7f);
            _mainTab.AddItems(f1Label2);
            y -= 80f;
            
            // Separator line (visual)
            var separator = new OpLabel(new Vector2(150f, y), new Vector2(200f, 20f), "─────────────────", FLabelAlignment.Center);
            separator.color = new Color(0.4f, 0.4f, 0.4f);
            _mainTab.AddItems(separator);
            y -= 50f;
            
            // Reset All Settings button
            var resetBtn = new OpSimpleButton(new Vector2(125f, y), new Vector2(250f, 40f), "RESET ALL SETTINGS");
            resetBtn.OnClick += (_) => ResetAllSettings();
            _mainTab.AddItems(resetBtn);
            y -= 60f;
            
            // Info text
            var infoLabel = new OpLabel(new Vector2(50f, y), new Vector2(400f, 60f), 
                "This will reset position, scale,\ncolors, and key bindings to defaults.", 
                FLabelAlignment.Center);
            infoLabel.color = new Color(0.5f, 0.5f, 0.5f);
            _mainTab.AddItems(infoLabel);
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
                _config.SetKeyUp(KeyCode.W);
                _config.SetKeyDown(KeyCode.S);
                _config.SetKeyLeft(KeyCode.A);
                _config.SetKeyRight(KeyCode.D);
                _config.SetKeyJump(KeyCode.Space);
                _config.SetKeyThrow(KeyCode.K);
                _config.SetKeyGrab(KeyCode.L);
                
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