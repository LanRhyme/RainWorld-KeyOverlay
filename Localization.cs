using System.Collections.Generic;
using UnityEngine;

namespace KeyOverlay
{
    /// <summary>
    /// Localization system for Key Overlay mod
    /// Supports English and Simplified Chinese
    /// </summary>
    public static class Localization
    {
        public enum Language
        {
            Auto,       // Follow game language
            English,
            ChineseSimplified
        }
        
        private static Language _currentLanguage = Language.Auto;
        private static bool _isChinese = false;
        
        // Localized strings
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
        };
        
        /// <summary>
        /// Initialize localization with game language detection
        /// </summary>
        public static void Initialize(Language configLanguage)
        {
            _currentLanguage = configLanguage;
            
            if (_currentLanguage == Language.Auto)
            {
                // Detect game language from PlayerPrefs or system
                _isChinese = DetectGameLanguage();
            }
            else
            {
                _isChinese = _currentLanguage == Language.ChineseSimplified;
            }
            
            KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Localization initialized: {(_isChinese ? "Chinese" : "English")}");
        }
        
        /// <summary>
        /// Detect if game is using Chinese language
        /// </summary>
        private static bool DetectGameLanguage()
        {
            // Method 1: Check PlayerPrefs for Rain World language setting
            if (PlayerPrefs.HasKey("language"))
            {
                string lang = PlayerPrefs.GetString("language");
                if (lang.Contains("zh") || lang.Contains("CN") || lang.Contains("Chinese"))
                {
                    return true;
                }
            }
            
            // Method 2: Check system language
            if (Application.systemLanguage == SystemLanguage.Chinese || 
                Application.systemLanguage == SystemLanguage.ChineseSimplified ||
                Application.systemLanguage == SystemLanguage.ChineseTraditional)
            {
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// Get localized string by key
        /// </summary>
        public static string Get(string key)
        {
            if (Strings.TryGetValue(key, out var values))
            {
                return _isChinese ? values[1] : values[0];
            }
            return key; // Return key if not found
        }
        
        /// <summary>
        /// Check if current language is Chinese
        /// </summary>
        public static bool IsChinese => _isChinese;
        
        /// <summary>
        /// Get current language setting
        /// </summary>
        public static Language CurrentLanguage => _currentLanguage;
    }
}