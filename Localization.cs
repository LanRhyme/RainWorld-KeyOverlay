using System.Collections.Generic;
using UnityEngine;
using RWCustom;

namespace KeyOverlay
{
    /// <summary>
    /// Localization system for Key Overlay mod
    /// Supports all Rain World languages with fallback to English
    /// </summary>
    public static class Localization
    {
        public enum Language
        {
            Auto,           // Follow game language
            English,
            French,
            German,
            Spanish,
            Portuguese,     // Brazilian Portuguese
            Russian,
            Japanese,
            Korean,
            ChineseSimplified,
            ChineseTraditional,
            Polish,
            Turkish,
            Italian
        }
        
        private static Language _currentLanguage = Language.Auto;
        private static int _languageIndex = 0; // 0 = English (default)
        private static bool _initialized = false;
        
        // Language indices matching Rain World's language system
        // Rain World uses: 0=English, 1=French, 2=German, 3=Spanish, 4=Portuguese, 
        //                  5=Russian, 6=Japanese, 7=Korean, 8=ChineseSimp, 9=ChineseTrad,
        //                  10=Polish, 11=Turkish, 12=Italian
        private static readonly Dictionary<Language, int> LanguageIndices = new Dictionary<Language, int>
        {
            { Language.English, 0 },
            { Language.French, 1 },
            { Language.German, 2 },
            { Language.Spanish, 3 },
            { Language.Portuguese, 4 },
            { Language.Russian, 5 },
            { Language.Japanese, 6 },
            { Language.Korean, 7 },
            { Language.ChineseSimplified, 8 },
            { Language.ChineseTraditional, 9 },
            { Language.Polish, 10 },
            { Language.Turkish, 11 },
            { Language.Italian, 12 }
        };
        
        // Localized strings - format: { English, Chinese }
        // For other languages, fall back to English for now
        private static readonly Dictionary<string, string[]> Strings = new Dictionary<string, string[]>
        {
            // General tab
            { "Show Keyboard Overlay", new[] { "Show Keyboard Overlay", "显示键盘覆盖层" } },
            { "Show Gamepad Overlay", new[] { "Show Gamepad Overlay", "显示手柄覆盖层" } },
            { "Show Combo Stats", new[] { "Show Combo Stats", "显示连击统计" } },
            { "Show Key Names", new[] { "Show Key Names", "显示按键名称" } },
            { "Use Icons", new[] { "Use Icons", "使用图标" } },
            { "Show Movement Keys", new[] { "Show Movement Keys", "显示移动键" } },
            { "Show Action Keys", new[] { "Show Action Keys", "显示动作键" } },
            { "Show Joystick Indicator", new[] { "Show Joystick Indicator", "显示摇杆指示器" } },
            { "RESET STATS", new[] { "RESET STATS", "重置统计" } },
            
            // Position tab
            { "Position", new[] { "Position", "位置" } },
            { "Position X", new[] { "Position X", "位置 X" } },
            { "Position Y", new[] { "Position Y", "位置 Y" } },
            { "Scale", new[] { "Scale", "缩放" } },
            { "Opacity", new[] { "Opacity", "透明度" } },
            { "RESET POSITION", new[] { "RESET POSITION", "重置位置" } },
            
            // Style tab
            { "Style", new[] { "Style", "样式" } },
            { "Border Opacity", new[] { "Border Opacity", "边框透明度" } },
            { "Fill Opacity", new[] { "Fill Opacity", "填充透明度" } },
            { "Pressed Effect", new[] { "Pressed Effect", "按下效果" } },
            { "Border Width", new[] { "Border Width", "边框宽度" } },
            { "Font Size", new[] { "Font Size", "字体大小" } },
            { "Overlay Style", new[] { "Key Style", "按键样式" } },
            { "Style_Classic", new[] { "Classic", "经典像素" } },
            { "Style_Minimalist", new[] { "Minimal", "极简风格" } },
            { "Style_Ghost", new[] { "Ghost", "幽灵发光" } },
            
            // Material Headers & Categories
            { "Visibility", new[] { "Visibility", "显示设置" } },
            { "Layout", new[] { "Layout", "位置布局" } },
            { "Appearance", new[] { "Appearance", "视觉效果" } },
            { "System", new[] { "System", "系统设置" } },
            { "Directional", new[] { "Directional", "移动按键" } },
            { "X Axis", new[] { "X Axis", "水平位置" } },
            { "Y Axis", new[] { "Y Axis", "垂直位置" } },
            { "Keyboard", new[] { "Keyboard", "键盘显示" } },
            { "Gamepad", new[] { "Gamepad", "手柄显示" } },
            { "Combo Stats", new[] { "Combo Stats", "连击统计" } },
            { "Key Names", new[] { "Key Names", "按键名称" } },
            { "Toggles", new[] { "Visibility", "显示设置" } },
            { "Transform", new[] { "Layout", "位置布局" } },
            { "Aesthetics", new[] { "Appearance", "视觉效果" } },
            { "Global", new[] { "System", "全局设置" } },
            { "Movement", new[] { "Directional", "移动按键" } },
            { "X Position", new[] { "X Axis", "水平位置" } },
            { "Y Position", new[] { "Y Axis", "垂直位置" } },
            { "Show Keyboard", new[] { "Keyboard", "键盘显示" } },
            { "Show Gamepad", new[] { "Gamepad", "手柄显示" } },
            
            // Colors tab
            { "Colors", new[] { "Colors", "颜色" } },
            { "Normal Color", new[] { "Normal Color", "正常颜色" } },
            { "Pressed Color", new[] { "Pressed Color", "按下颜色" } },
            { "Border Color", new[] { "Border Color", "边框颜色" } },
            { "Red", new[] { "Red", "红" } },
            { "Green", new[] { "Green", "绿" } },
            { "Blue", new[] { "Blue", "蓝" } },
            
            // Keys tab
            { "Keys", new[] { "Keys", "按键" } },
            { "Menu Key", new[] { "Menu Key", "菜单按键" } },
            { "Up", new[] { "Up", "上" } },
            { "Down", new[] { "Down", "下" } },
            { "Left", new[] { "Left", "左" } },
            { "Right", new[] { "Right", "右" } },
            { "Jump", new[] { "Jump", "跳跃" } },
            { "Throw", new[] { "Throw", "投掷" } },
            { "Grab", new[] { "Grab", "抓取" } },
            { "Press any key...", new[] { "Press any key...", "请按键..." } },
            { "Press Enter to confirm", new[] { "Press Enter to confirm", "按回车确认" } },
            { "RESET TO DEFAULTS", new[] { "RESET TO DEFAULTS", "恢复默认" } },
            
            // Menu
            { "Key Overlay Settings", new[] { "Key Overlay Settings", "按键显示设置" } },
            { "General", new[] { "General", "常规" } },
            { "Language", new[] { "Language", "语言" } },
            { "Auto (Follow Game)", new[] { "Auto (Follow Game)", "自动（跟随游戏）" } },
            { "English", new[] { "English", "英语" } },
            { "Chinese (Simplified)", new[] { "Chinese (Simplified)", "简体中文" } },
            
            // CPS and stats
            { "CPS", new[] { "CPS", "CPS" } },
            { "JMP", new[] { "JMP", "跳" } },
            { "THR", new[] { "THR", "投" } },
            { "GRB", new[] { "GRB", "抓" } },
            
            // Remix Menu strings
            { "Remix_Title", new[] { "⌨ KEY OVERLAY", "⌨ 按键显示" } },
            { "Remix_MenuKeySection", new[] { "Menu Toggle Key", "菜单开关按键" } },
            { "Remix_ClickToSet", new[] { "Click button to change key", "点击按钮更改按键" } },
            { "Remix_PressAnyKey", new[] { "Press any key...", "请按下任意键..." } },
            { "Remix_KeySetSuccess", new[] { "Key set successfully!", "按键设置成功！" } },
            { "Remix_CurrentKey", new[] { "Current Key", "当前按键" } },
            { "Remix_EscapeCancel", new[] { "Press ESC to cancel", "按 ESC 取消" } },
            { "Remix_InGameConfig", new[] { "Press the menu key in-game to open settings panel", "游戏中按下菜单键打开设置面板" } },
            { "Remix_ConfigPanelDesc", new[] { "Configure position, colors, key bindings, and more", "配置位置、颜色、按键绑定等" } },
            { "Remix_ResetAll", new[] { "RESET ALL SETTINGS", "重置所有设置" } },
            { "Remix_ResetDesc", new[] { "This will reset all settings including:\nposition, colors, and key bindings", "这将重置所有设置包括：\n位置、颜色和按键绑定" } },
            { "Remix_DefaultKeyF1", new[] { "Default: F1", "默认：F1" } },
        };
        
        /// <summary>
        /// Initialize localization with game language detection
        /// </summary>
        public static void Initialize(Language configLanguage)
        {
            _currentLanguage = configLanguage;
            
            if (_currentLanguage == Language.Auto)
            {
                // Detect game language from Rain World's settings
                _languageIndex = DetectGameLanguageIndex();
            }
            else
            {
                // Use configured language
                _languageIndex = LanguageIndices.TryGetValue(_currentLanguage, out int idx) ? idx : 0;
            }
            
            _initialized = true;
            KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Localization initialized with language index: {_languageIndex}");
        }
        
        /// <summary>
        /// Refresh language detection (call when game language might have changed)
        /// </summary>
        public static void Refresh()
        {
            if (_currentLanguage == Language.Auto)
            {
                int newIndex = DetectGameLanguageIndex();
                if (newIndex != _languageIndex)
                {
                    _languageIndex = newIndex;
                    KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Language refreshed to index: {_languageIndex}");
                }
            }
        }
        
        /// <summary>
        /// Detect Rain World's current language setting
        /// </summary>
        private static int DetectGameLanguageIndex()
        {
            try
            {
                // Try to get Rain World's language setting
                if (Custom.rainWorld != null && Custom.rainWorld.options != null)
                {
                    // Rain World stores language as LanguageID enum
                    int gameLang = (int)Custom.rainWorld.options.language;
                    KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Detected game language index: {gameLang}");
                    return gameLang;
                }
            }
            catch (System.Exception ex)
            {
                KeyOverlayPlugin.Log?.LogWarning($"[KeyOverlay] Failed to detect game language: {ex.Message}");
            }
            
            // Fallback: Check PlayerPrefs
            if (PlayerPrefs.HasKey("language"))
            {
                string lang = PlayerPrefs.GetString("language").ToLower();
                if (lang.Contains("zh") || lang.Contains("cn")) return 8; // Chinese Simplified
                if (lang.Contains("tw") || lang.Contains("traditional")) return 9; // Chinese Traditional
                if (lang.Contains("ja") || lang.Contains("jp")) return 6; // Japanese
                if (lang.Contains("ko") || lang.Contains("kr")) return 7; // Korean
                if (lang.Contains("fr")) return 1; // French
                if (lang.Contains("de")) return 2; // German
                if (lang.Contains("es")) return 3; // Spanish
                if (lang.Contains("pt") || lang.Contains("br")) return 4; // Portuguese
                if (lang.Contains("ru")) return 5; // Russian
                if (lang.Contains("pl")) return 10; // Polish
                if (lang.Contains("tr")) return 11; // Turkish
                if (lang.Contains("it")) return 12; // Italian
            }
            
            // Fallback: Check system language
            switch (Application.systemLanguage)
            {
                case SystemLanguage.ChineseSimplified: return 8;
                case SystemLanguage.ChineseTraditional: return 9;
                case SystemLanguage.Japanese: return 6;
                case SystemLanguage.Korean: return 7;
                case SystemLanguage.French: return 1;
                case SystemLanguage.German: return 2;
                case SystemLanguage.Spanish: return 3;
                case SystemLanguage.Portuguese: return 4;
                case SystemLanguage.Russian: return 5;
                case SystemLanguage.Polish: return 10;
                case SystemLanguage.Turkish: return 11;
                case SystemLanguage.Italian: return 12;
            }
            
            return 0; // Default to English
        }
        
        /// <summary>
        /// Get localized string by key
        /// Always returns a valid string (falls back to English or key)
        /// </summary>
        public static string Get(string key)
        {
            // Auto-refresh language detection if in Auto mode and not yet properly initialized
            if (_currentLanguage == Language.Auto && !_initialized)
            {
                Refresh();
                _initialized = true;
            }
            
            if (Strings.TryGetValue(key, out var values))
            {
                // For Chinese (Simplified = 8, Traditional = 9), use index 1 (Chinese translation)
                // For all other languages, use index 0 (English)
                if (_languageIndex == 8 || _languageIndex == 9)
                {
                    return values[1]; // Chinese
                }
                return values[0]; // English (fallback for all other languages)
            }
            // Return key itself if translation not found - ensures something is always displayed
            return key;
        }
        
        /// <summary>
        /// Check if current language is Chinese
        /// </summary>
        public static bool IsChinese => _languageIndex == 8 || _languageIndex == 9;
        
        /// <summary>
        /// Get current language setting
        /// </summary>
        public static Language CurrentLanguage => _currentLanguage;
        
        /// <summary>
        /// Get current language index
        /// </summary>
        public static int CurrentLanguageIndex => _languageIndex;
    }
}