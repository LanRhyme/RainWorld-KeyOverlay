using UnityEngine;
using System.IO;

namespace KeyOverlay
{
    /// <summary>
    /// F1 Settings Menu - Draggable Pixel-style UI with custom font
    /// Author: LanRhyme
    /// </summary>
    public class PauseMenuIntegration
    {
        private ConfigWrapper _config;
        private KeyOverlayUI _ui;
        private InputMonitor _input;
        public bool IsMenuActive { get; private set; }
        
        private int _tab = 0;
        
        // Key binding states
        private bool _waitingForKey = false;
        private bool _confirmingKey = false;
        private int _currentBindingIndex = -1;
        private KeyCode _pendingKey = KeyCode.None;
        
        // UI dimensions
        private const float WindowWidth = 420f;
        private const float WindowHeight = 520f;
        private const float ButtonHeight = 32f;
        private const float Spacing = 8f;
        private const float TitleBarHeight = 30f;
        
        // Window position (draggable)
        private float _windowX = -1f;
        private float _windowY = -1f;
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        
        // Pixel color palette
        private static readonly Color PixelBg = new Color(0.08f, 0.08f, 0.1f);
        private static readonly Color PixelBgLight = new Color(0.12f, 0.12f, 0.15f);
        private static readonly Color PixelBorder = new Color(0.25f, 0.25f, 0.28f);
        private static readonly Color PixelHighlight = new Color(1f, 0.82f, 0.35f);
        private static readonly Color PixelText = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color PixelTextDim = new Color(0.6f, 0.6f, 0.65f);
        
        // Styles
        private GUIStyle _windowStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _sliderStyle;
        private GUIStyle _sliderThumbStyle;
        private GUIStyle _tabButtonStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _smallLabelStyle;
        private GUIStyle _keyButtonStyle;
        private GUIStyle _titleBarStyle;
        private bool _stylesInitialized = false;
        
        // Custom font
        private Font _pixelFont;
        
        // Textures
        private Texture2D _pixelBorderTex;
        private Texture2D _pixelButtonTex;
        private Texture2D _pixelButtonHoverTex;
        private Texture2D _pixelSliderTex;
        private Texture2D _pixelThumbTex;
        private Texture2D _whiteTexture;
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c;
            _ui = u;
            _input = i;
            
            // Initialize window position to center
            if (_windowX < 0)
            {
                _windowX = (Screen.width - WindowWidth) / 2;
                _windowY = (Screen.height - WindowHeight) / 2;
            }
        }
        
        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;
            
            LoadCustomFont();
            CreatePixelTextures();
            
            // White texture for color preview
            _whiteTexture = new Texture2D(1, 1);
            _whiteTexture.SetPixel(0, 0, Color.white);
            _whiteTexture.Apply();
            
            // Window style
            _windowStyle = new GUIStyle();
            _windowStyle.normal.background = _pixelBorderTex;
            _windowStyle.border = new RectOffset(4, 4, 4, 4);
            
            // Label with custom font
            _labelStyle = new GUIStyle();
            _labelStyle.font = _pixelFont;
            _labelStyle.fontSize = 14;
            _labelStyle.normal.textColor = PixelText;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            
            // Small label
            _smallLabelStyle = new GUIStyle(_labelStyle);
            _smallLabelStyle.fontSize = 12;
            _smallLabelStyle.normal.textColor = PixelTextDim;
            
            // Button
            _buttonStyle = new GUIStyle();
            _buttonStyle.font = _pixelFont;
            _buttonStyle.fontSize = 13;
            _buttonStyle.normal.background = _pixelButtonTex;
            _buttonStyle.normal.textColor = PixelText;
            _buttonStyle.hover.background = _pixelButtonHoverTex;
            _buttonStyle.hover.textColor = PixelHighlight;
            _buttonStyle.active.background = _pixelThumbTex;
            _buttonStyle.active.textColor = Color.white;
            _buttonStyle.border = new RectOffset(3, 3, 3, 3);
            _buttonStyle.padding = new RectOffset(10, 10, 6, 6);
            _buttonStyle.alignment = TextAnchor.MiddleCenter;
            
            // Key button
            _keyButtonStyle = new GUIStyle(_buttonStyle);
            _keyButtonStyle.fontSize = 12;
            _keyButtonStyle.normal.background = _pixelSliderTex;
            
            // Toggle
            _toggleStyle = new GUIStyle();
            _toggleStyle.font = _pixelFont;
            _toggleStyle.fontSize = 13;
            _toggleStyle.normal.textColor = PixelText;
            
            // Slider
            _sliderStyle = new GUIStyle();
            _sliderStyle.normal.background = _pixelSliderTex;
            _sliderStyle.border = new RectOffset(2, 2, 2, 2);
            _sliderStyle.fixedHeight = 12f;
            
            _sliderThumbStyle = new GUIStyle();
            _sliderThumbStyle.normal.background = _pixelThumbTex;
            _sliderThumbStyle.hover.background = _pixelButtonHoverTex;
            _sliderThumbStyle.border = new RectOffset(2, 2, 2, 2);
            _sliderThumbStyle.fixedWidth = 16f;
            _sliderThumbStyle.fixedHeight = 16f;
            
            // Tab button
            _tabButtonStyle = new GUIStyle(_buttonStyle);
            _tabButtonStyle.fontSize = 12;
            _tabButtonStyle.padding = new RectOffset(8, 8, 5, 5);
            
            // Header
            _headerStyle = new GUIStyle();
            _headerStyle.font = _pixelFont;
            _headerStyle.fontSize = 18;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = PixelHighlight;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
            
            // Title bar (draggable area)
            _titleBarStyle = new GUIStyle();
            _titleBarStyle.font = _pixelFont;
            _titleBarStyle.fontSize = 14;
            _titleBarStyle.fontStyle = FontStyle.Bold;
            _titleBarStyle.normal.textColor = PixelHighlight;
            _titleBarStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        private void LoadCustomFont()
        {
            // Unity's Font class cannot load TTF directly from file path
            // Use system fonts with pixel-friendly settings
            
            string[] pixelFontNames = {
                "Cubic_11",  // If installed on system
                "Consolas",  // Windows pixel-friendly font
                "Courier New",
                "Lucida Console",
                "Arial"
            };
            
            foreach (string fontName in pixelFontNames)
            {
                try
                {
                    _pixelFont = Font.CreateDynamicFontFromOSFont(fontName, 14);
                    if (_pixelFont != null)
                    {
                        Debug.Log($"[KeyOverlay] Using font: {fontName}");
                        return;
                    }
                }
                catch { }
            }
            
            // Fallback: use Unity's default font
            _pixelFont = GUI.skin.font;
            Debug.Log("[KeyOverlay] Using default GUI font");
        }
        
        private void CreatePixelTextures()
        {
            _pixelBorderTex = CreatePixelBorderTexture(PixelBg, PixelBorder, 16);
            _pixelButtonTex = CreatePixelButtonTexture(PixelBgLight, PixelBorder, false);
            _pixelButtonHoverTex = CreatePixelButtonTexture(new Color(0.18f, 0.18f, 0.22f), PixelHighlight, true);
            _pixelSliderTex = CreateSolidTexture(new Color(0.2f, 0.2f, 0.24f), 4);
            _pixelThumbTex = CreateSolidTexture(PixelHighlight, 4);
        }
        
        private Texture2D CreatePixelBorderTexture(Color fill, Color border, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[size * size];
            int borderSize = 2;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool isBorder = x < borderSize || x >= size - borderSize || 
                                    y < borderSize || y >= size - borderSize;
                    pixels[y * size + x] = isBorder ? border : fill;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
        
        private Texture2D CreatePixelButtonTexture(Color fill, Color border, bool highlight)
        {
            int size = 12;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[size * size];
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = fill;
                    if (y == 0 || y == 1) c = highlight ? border : new Color(fill.r * 1.3f, fill.g * 1.3f, fill.b * 1.3f);
                    else if (y == size - 1 || y == size - 2) c = new Color(fill.r * 0.7f, fill.g * 0.7f, fill.b * 0.7f);
                    else if (x == 0 || x == 1) c = highlight ? border : new Color(fill.r * 1.2f, fill.g * 1.2f, fill.b * 1.2f);
                    else if (x == size - 1 || x == size - 2) c = new Color(fill.r * 0.8f, fill.g * 0.8f, fill.b * 0.8f);
                    pixels[y * size + x] = c;
                }
            }
            
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
        
        private Texture2D CreateSolidTexture(Color color, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;
            
            Color[] pixels = new Color[size * size];
            for (int i = 0; i < pixels.Length; i++)
                pixels[i] = color;
            
            tex.SetPixels(pixels);
            tex.Apply();
            return tex;
        }
        
        public void OnGUI()
        {
            if (!IsMenuActive) return;
            
            InitStyles();
            
            // Handle dragging
            HandleDragging();
            
            if (_waitingForKey)
            {
                DrawKeyBindingDialog();
                return;
            }
            
            if (_confirmingKey)
            {
                DrawConfirmDialog();
                return;
            }
            
            // Draw main window
            float cx = _windowX;
            float cy = _windowY;
            
            // Window background
            GUI.Box(new Rect(cx, cy, WindowWidth, WindowHeight), "", _windowStyle);
            
            // Title bar (draggable)
            Rect titleRect = new Rect(cx, cy, WindowWidth, TitleBarHeight);
            GUI.Box(titleRect, "", new GUIStyle { normal = { background = _pixelBorderTex } });
            GUI.Label(new Rect(cx, cy + 5, WindowWidth, 20), "◄ " + Localization.Get("Key Overlay Settings") + " ►", _titleBarStyle);
            
            // Content area
            GUILayout.BeginArea(new Rect(cx + 16, cy + TitleBarHeight + 8, WindowWidth - 32, WindowHeight - TitleBarHeight - 24));
            
            // Tab buttons
            DrawTabs();
            GUILayout.Space(Spacing);
            
            // Tab content
            switch (_tab)
            {
                case 0: DrawGeneralTab(); break;
                case 1: DrawPositionTab(); break;
                case 2: DrawStyleTab(); break;
                case 3: DrawColorsTab(); break;
                case 4: DrawKeysTab(); break;
            }
            
            GUILayout.EndArea();
        }
        
        private void HandleDragging()
        {
            Event e = Event.current;
            
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                Rect titleRect = new Rect(_windowX, _windowY, WindowWidth, TitleBarHeight);
                if (titleRect.Contains(e.mousePosition))
                {
                    _isDragging = true;
                    _dragOffset = e.mousePosition - new Vector2(_windowX, _windowY);
                    e.Use();
                }
            }
            else if (e.type == EventType.MouseUp && e.button == 0)
            {
                _isDragging = false;
            }
            else if (e.type == EventType.MouseDrag && _isDragging)
            {
                _windowX = e.mousePosition.x - _dragOffset.x;
                _windowY = e.mousePosition.y - _dragOffset.y;
                
                // Clamp to screen
                _windowX = Mathf.Clamp(_windowX, 0, Screen.width - WindowWidth);
                _windowY = Mathf.Clamp(_windowY, 0, Screen.height - WindowHeight);
                
                e.Use();
            }
        }
        
        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            
            string[] tabKeys = { "General", "Position", "Style", "Colors", "Keys" };
            
            for (int i = 0; i < tabKeys.Length; i++)
            {
                bool isSelected = (_tab == i);
                
                Color prevBg = GUI.backgroundColor;
                if (isSelected)
                {
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.4f, 0.3f);
                }
                
                if (GUILayout.Button(Localization.Get(tabKeys[i]), _tabButtonStyle, GUILayout.Height(ButtonHeight - 8)))
                {
                    _tab = i;
                }
                
                GUI.backgroundColor = prevBg;
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawGeneralTab()
        {
            // Language selector at top
            DrawLanguageSelector();
            
            DrawPixelToggle(Localization.Get("Show Keyboard Overlay"), () => _config.ShowKeyboard, v => _config.SetShowKeyboard(v));
            DrawPixelToggle(Localization.Get("Show Gamepad Overlay"), () => _config.ShowGamepad, v => _config.SetShowGamepad(v));
            DrawPixelToggle(Localization.Get("Show Combo Stats"), () => _config.ShowComboStats, v => _config.SetShowComboStats(v));
            DrawPixelToggle(Localization.Get("Show Key Names"), () => _config.ShowKeyNames, v => _config.SetShowKeyNames(v));
            if (_config.ShowKeyNames)
            {
                DrawPixelToggle("  " + Localization.Get("Use Icons"), () => _config.ShowIconMode, v => _config.SetShowIconMode(v));
            }
            DrawPixelToggle(Localization.Get("Show Movement Keys"), () => _config.ShowMovementKeys, v => _config.SetShowMovementKeys(v));
            DrawPixelToggle(Localization.Get("Show Action Keys"), () => _config.ShowActionKeys, v => _config.SetShowActionKeys(v));
            DrawPixelToggle(Localization.Get("Show Joystick Indicator"), () => _config.ShowJoystick, v => _config.SetShowJoystick(v));
            
            GUILayout.Space(Spacing * 2);
            
            if (GUILayout.Button("[ " + Localization.Get("RESET STATS") + " ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                _input?.ResetStats();
            }
        }
        
        private void DrawLanguageSelector()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(Localization.Get("Language") + ":", _labelStyle, GUILayout.Width(80));
            
            int lang = _config.Language;
            string[] options = new[] { 
                Localization.Get("Auto (Follow Game)"), 
                Localization.Get("English"), 
                Localization.Get("Chinese (Simplified)") 
            };
            
            int newLang = GUILayout.SelectionGrid(lang, options, 1, _buttonStyle);
            if (newLang != lang)
            {
                _config.SetLanguage(newLang);
            }
            
            GUILayout.EndHorizontal();
            GUILayout.Space(Spacing);
        }
        
        private void DrawPixelToggle(string label, System.Func<bool> getter, System.Action<bool> setter)
        {
            GUILayout.BeginHorizontal();
            
            bool value = getter();
            
            // Checkbox
            Rect checkRect = GUILayoutUtility.GetRect(22, 22, GUILayout.Width(22));
            GUI.DrawTexture(checkRect, _pixelButtonTex, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
            
            if (value)
            {
                GUI.color = PixelHighlight;
                GUI.Label(new Rect(checkRect.x + 5, checkRect.y + 2, 16, 16), "✓", _labelStyle);
                GUI.color = Color.white;
            }
            
            if (Event.current.type == EventType.MouseDown && checkRect.Contains(Event.current.mousePosition))
            {
                setter(!value);
                Event.current.Use();
            }
            
            GUILayout.Space(6);
            GUILayout.Label(label, _labelStyle);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        
        private void DrawPositionTab()
        {
            DrawPixelSlider(Localization.Get("Position X"), _config.PanelX, 0f, Screen.width, v => {
                _config.SetPanelX(v);
                _ui.RefreshTextures();
            }, "{0:F0}");
            
            DrawPixelSlider(Localization.Get("Position Y"), _config.PanelY, 0f, Screen.height, v => {
                _config.SetPanelY(v);
                _ui.RefreshTextures();
            }, "{0:F0}");
            
            DrawPixelSlider(Localization.Get("Scale"), _config.Scale, 0.5f, 3f, v => {
                _config.SetScale(v);
                _ui.RefreshTextures();
            }, "{0:F1}x");
            
            DrawPixelSlider(Localization.Get("Opacity"), _config.Opacity, 0.1f, 1f, v => {
                _config.SetOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F1}");
            
            GUILayout.Space(Spacing);
            
            if (GUILayout.Button("[ " + Localization.Get("RESET POSITION") + " ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                _config.SetPanelX(136f);
                _config.SetPanelY(666f);
                _config.SetScale(1f);
                _config.SetOpacity(0.8f);
                _ui.RefreshTextures();
            }
        }
        
        private void DrawStyleTab()
        {
            DrawPixelSlider(Localization.Get("Border Opacity"), _config.BorderOpacity, 0f, 1f, v => {
                _config.SetBorderOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider(Localization.Get("Fill Opacity"), _config.FillOpacity, 0f, 1f, v => {
                _config.SetFillOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider(Localization.Get("Pressed Effect"), _config.PressedEffectOpacity, 0f, 1f, v => {
                _config.SetPressedEffectOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider(Localization.Get("Border Width"), _config.BorderWidth, 0.5f, 4f, v => {
                _config.SetBorderWidth(v);
                _ui.RefreshTextures();
            }, "{0:F1}px");
            
            DrawPixelSlider(Localization.Get("Font Size"), _config.FontSize, 8, 20, v => {
                _config.SetFontSize((int)v);
                _ui.RefreshTextures();
            }, "{0:F0}");
        }
        
        private void DrawPixelSlider(string label, float value, float min, float max, System.Action<float> onChange, string format)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _labelStyle, GUILayout.Width(120));
            GUILayout.FlexibleSpace();
            GUILayout.Label(string.Format(format, value), _smallLabelStyle, GUILayout.Width(50));
            GUILayout.EndHorizontal();
            
            float newValue = GUILayout.HorizontalSlider(value, min, max, _sliderStyle, _sliderThumbStyle, GUILayout.Height(20));
            
            if (Mathf.Abs(newValue - value) > 0.001f)
            {
                onChange(newValue);
            }
            
            GUILayout.Space(4);
        }
        
        private void DrawColorsTab()
        {
            GUILayout.Label(Localization.Get("Normal Color"), _labelStyle);
            DrawColorSliders("normal");
            
            GUILayout.Space(Spacing);
            
            GUILayout.Label(Localization.Get("Pressed Color"), _labelStyle);
            DrawColorSliders("pressed");
            
            GUILayout.Space(Spacing);
            
            GUILayout.Label(Localization.Get("Border Color"), _labelStyle);
            DrawColorSliders("border");
            
            GUILayout.Space(Spacing);
            
            // Color preview
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preview:", _labelStyle, GUILayout.Width(70));
            
            Rect previewRect = GUILayoutUtility.GetRect(100, 50, GUILayout.Width(100));
            DrawColorPreview(previewRect);
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawColorSliders(string colorType)
        {
            Color c;
            System.Action<Color> setter;
            
            switch (colorType)
            {
                case "normal": 
                    c = _config.KeyColorNormal; 
                    setter = v => _config.SetKeyColorNormal(v);
                    break;
                case "pressed": 
                    c = _config.KeyColorPressed; 
                    setter = v => _config.SetKeyColorPressed(v);
                    break;
                case "border": 
                    c = _config.BorderColor; 
                    setter = v => _config.SetBorderColor(v);
                    break;
                default: return;
            }
            
            float newR = c.r, newG = c.g, newB = c.b;
            
            // R slider
            GUILayout.BeginHorizontal();
            GUI.color = new Color(1f, 0.3f, 0.3f);
            GUILayout.Label("R", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            newR = GUILayout.HorizontalSlider(c.r, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newR:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            // G slider
            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.3f, 1f, 0.3f);
            GUILayout.Label("G", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            newG = GUILayout.HorizontalSlider(c.g, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newG:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            // B slider
            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.3f, 0.5f, 1f);
            GUILayout.Label("B", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            newB = GUILayout.HorizontalSlider(c.b, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newB:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            if (Mathf.Abs(newR - c.r) > 0.001f || Mathf.Abs(newG - c.g) > 0.001f || Mathf.Abs(newB - c.b) > 0.001f)
            {
                setter(new Color(newR, newG, newB));
                _ui.RefreshTextures();
            }
            
            GUILayout.Space(4);
        }
        
        private void DrawColorPreview(Rect rect)
        {
            // Draw border
            GUI.color = new Color(_config.BorderColor.r, _config.BorderColor.g, _config.BorderColor.b, _config.BorderOpacity);
            GUI.DrawTexture(rect, _whiteTexture);
            
            // Draw fill
            Rect innerRect = new Rect(rect.x + 4, rect.y + 4, rect.width - 8, rect.height - 8);
            GUI.color = new Color(_config.KeyColorNormal.r, _config.KeyColorNormal.g, _config.KeyColorNormal.b, _config.FillOpacity);
            GUI.DrawTexture(innerRect, _whiteTexture);
            
            // Draw pressed indicator
            Rect pressedRect = new Rect(rect.x + rect.width/2 - 10, rect.y + rect.height/2 - 5, 20, 10);
            GUI.color = new Color(_config.KeyColorPressed.r, _config.KeyColorPressed.g, _config.KeyColorPressed.b, _config.PressedEffectOpacity);
            GUI.DrawTexture(pressedRect, _whiteTexture);
            
            GUI.color = Color.white;
        }
        
        private void DrawKeysTab()
        {
            string[] keyNames = { "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab" };
            KeyCode[] keyCodes = { 
                _config.KeyUp, _config.KeyDown, _config.KeyLeft, _config.KeyRight, 
                _config.KeyJump, _config.KeyThrow, _config.KeyGrab 
            };
            
            for (int i = 0; i < keyNames.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(Localization.Get(keyNames[i]), _labelStyle, GUILayout.Width(60));
                
                if (GUILayout.Button($"[ {keyCodes[i]} ]", _keyButtonStyle, GUILayout.Height(ButtonHeight - 6)))
                {
                    _waitingForKey = true;
                    _currentBindingIndex = i;
                }
                
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            
            GUILayout.Space(Spacing);
            
            if (GUILayout.Button("[ " + Localization.Get("RESET STATS") + " ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                _config.SetKeyUp(KeyCode.W);
                _config.SetKeyDown(KeyCode.S);
                _config.SetKeyLeft(KeyCode.A);
                _config.SetKeyRight(KeyCode.D);
                _config.SetKeyJump(KeyCode.Space);
                _config.SetKeyThrow(KeyCode.K);
                _config.SetKeyGrab(KeyCode.L);
            }
        }
        
        private void DrawKeyBindingDialog()
        {
            float cx = Screen.width / 2 - 160;
            float cy = Screen.height / 2 - 90;
            
            GUI.Box(new Rect(cx, cy, 320, 180), "", _windowStyle);
            
            GUILayout.BeginArea(new Rect(cx + 20, cy + 30, 280, 130));
            
            GUILayout.Label("◄ " + Localization.Get("Keys") + " ►", _headerStyle);
            GUILayout.Space(15);
            
            string[] keyNames = { "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab" };
            GUILayout.Label(Localization.Get("Press any key..."), _labelStyle);
            GUILayout.Label($"[ {Localization.Get(keyNames[_currentBindingIndex])} ]", _headerStyle);
            GUILayout.Space(10);
            
            GUILayout.Label(Localization.Get("Press any key..."), _smallLabelStyle);
            
            GUILayout.EndArea();
            
            HandleKeyInput();
        }
        
        private void DrawConfirmDialog()
        {
            float cx = Screen.width / 2 - 160;
            float cy = Screen.height / 2 - 90;
            
            GUI.Box(new Rect(cx, cy, 320, 180), "", _windowStyle);
            
            GUILayout.BeginArea(new Rect(cx + 20, cy + 30, 280, 130));
            
            GUILayout.Label("◄ " + Localization.Get("Keys") + " ►", _headerStyle);
            GUILayout.Space(15);
            
            string[] keyNames = { "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab" };
            GUILayout.Label($"{Localization.Get(keyNames[_currentBindingIndex])}:", _labelStyle);
            GUILayout.Label($"[ {_pendingKey} ]", _headerStyle);
            GUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("[ OK ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                SetKeyBinding(_currentBindingIndex, _pendingKey);
                _confirmingKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
                _config.Save();
            }
            if (GUILayout.Button("[ X ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                _confirmingKey = false;
                _waitingForKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
            }
            GUILayout.EndHorizontal();
            
            GUILayout.EndArea();
        }
        
        private void HandleKeyInput()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            if (e.keyCode == KeyCode.Escape)
            {
                _waitingForKey = false;
                _currentBindingIndex = -1;
                e.Use();
                return;
            }
            
            if (e.keyCode != KeyCode.None && e.keyCode != KeyCode.UpArrow && 
                e.keyCode != KeyCode.DownArrow && e.keyCode != KeyCode.LeftArrow && 
                e.keyCode != KeyCode.RightArrow && e.keyCode != KeyCode.Return)
            {
                _pendingKey = e.keyCode;
                _waitingForKey = false;
                _confirmingKey = true;
                e.Use();
            }
        }
        
        private void SetKeyBinding(int index, KeyCode key)
        {
            switch (index)
            {
                case 0: _config.SetKeyUp(key); break;
                case 1: _config.SetKeyDown(key); break;
                case 2: _config.SetKeyLeft(key); break;
                case 3: _config.SetKeyRight(key); break;
                case 4: _config.SetKeyJump(key); break;
                case 5: _config.SetKeyThrow(key); break;
                case 6: _config.SetKeyGrab(key); break;
            }
        }
        
        public void OpenMenu()
        {
            IsMenuActive = true;
            _tab = 0;
        }
        
        public void CloseMenu()
        {
            IsMenuActive = false;
            _config.Save();
        }
    }
}