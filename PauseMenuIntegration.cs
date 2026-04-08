using UnityEngine;

namespace KeyOverlay
{
    /// <summary>
    /// F1 Settings Menu - Enhanced Pixel-style UI with mouse support
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
        
        // Pixel color palette (Rain World inspired)
        private static readonly Color PixelBg = new Color(0.08f, 0.08f, 0.1f);
        private static readonly Color PixelBgLight = new Color(0.12f, 0.12f, 0.15f);
        private static readonly Color PixelBorder = new Color(0.25f, 0.25f, 0.28f);
        private static readonly Color PixelHighlight = new Color(1f, 0.82f, 0.35f); // Yellow-gold
        private static readonly Color PixelText = new Color(0.95f, 0.95f, 0.95f);
        private static readonly Color PixelTextDim = new Color(0.6f, 0.6f, 0.65f);
        private static readonly Color PixelAccent = new Color(0.4f, 0.35f, 0.25f);
        
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
        private bool _stylesInitialized = false;
        
        // Textures
        private Texture2D _pixelBorderTex;
        private Texture2D _pixelButtonTex;
        private Texture2D _pixelButtonHoverTex;
        private Texture2D _pixelSliderTex;
        private Texture2D _pixelThumbTex;
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c;
            _ui = u;
            _input = i;
        }
        
        private void InitStyles()
        {
            if (_stylesInitialized) return;
            _stylesInitialized = true;
            
            CreatePixelTextures();
            
            // Window - dark with pixel border
            _windowStyle = new GUIStyle();
            _windowStyle.normal.background = _pixelBorderTex;
            _windowStyle.border = new RectOffset(4, 4, 4, 4);
            
            // Label - clean pixel text
            _labelStyle = new GUIStyle();
            _labelStyle.fontSize = 14;
            _labelStyle.normal.textColor = PixelText;
            _labelStyle.alignment = TextAnchor.MiddleLeft;
            _labelStyle.wordWrap = false;
            
            // Small label for values
            _smallLabelStyle = new GUIStyle(_labelStyle);
            _smallLabelStyle.fontSize = 12;
            _smallLabelStyle.normal.textColor = PixelTextDim;
            
            // Button - pixel border style
            _buttonStyle = new GUIStyle();
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
            
            // Key button - for key bindings
            _keyButtonStyle = new GUIStyle(_buttonStyle);
            _keyButtonStyle.fontSize = 12;
            _keyButtonStyle.normal.background = _pixelSliderTex;
            _keyButtonStyle.hover.background = _pixelButtonHoverTex;
            
            // Toggle
            _toggleStyle = new GUIStyle();
            _toggleStyle.fontSize = 13;
            _toggleStyle.normal.textColor = PixelText;
            _toggleStyle.hover.textColor = PixelHighlight;
            _toggleStyle.alignment = TextAnchor.MiddleLeft;
            
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
            
            // Header - golden pixel title
            _headerStyle = new GUIStyle();
            _headerStyle.fontSize = 18;
            _headerStyle.fontStyle = FontStyle.Bold;
            _headerStyle.normal.textColor = PixelHighlight;
            _headerStyle.alignment = TextAnchor.MiddleCenter;
        }
        
        private void CreatePixelTextures()
        {
            // Pixel border texture (9-slice compatible)
            _pixelBorderTex = CreatePixelBorderTexture(PixelBg, PixelBorder, 16);
            
            // Button textures
            _pixelButtonTex = CreatePixelButtonTexture(PixelBgLight, PixelBorder, false);
            _pixelButtonHoverTex = CreatePixelButtonTexture(new Color(0.18f, 0.18f, 0.22f), PixelHighlight, true);
            
            // Slider textures
            _pixelSliderTex = CreateSolidTexture(new Color(0.2f, 0.2f, 0.24f), 4);
            _pixelThumbTex = CreateSolidTexture(PixelHighlight, 4);
        }
        
        private Texture2D CreatePixelBorderTexture(Color fill, Color border, int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point; // Pixel perfect!
            
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
            
            // Create pixelated button with beveled edges
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    Color c = fill;
                    
                    // Top edge lighter
                    if (y == 0 || y == 1) c = highlight ? border : new Color(fill.r * 1.3f, fill.g * 1.3f, fill.b * 1.3f);
                    // Bottom edge darker
                    else if (y == size - 1 || y == size - 2) c = new Color(fill.r * 0.7f, fill.g * 0.7f, fill.b * 0.7f);
                    // Left edge lighter
                    else if (x == 0 || x == 1) c = highlight ? border : new Color(fill.r * 1.2f, fill.g * 1.2f, fill.b * 1.2f);
                    // Right edge darker
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
            
            // Calculate window position
            float cx = (Screen.width - WindowWidth) / 2;
            float cy = (Screen.height - WindowHeight) / 2;
            
            // Draw window background
            GUI.Box(new Rect(cx, cy, WindowWidth, WindowHeight), "", _windowStyle);
            
            // Inner padding
            GUILayout.BeginArea(new Rect(cx + 16, cy + 16, WindowWidth - 32, WindowHeight - 32));
            
            // Header with pixel decoration
            DrawPixelHeader();
            GUILayout.Space(Spacing);
            
            // Tab buttons
            DrawTabs();
            GUILayout.Space(Spacing * 2);
            
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
        
        private void DrawPixelHeader()
        {
            GUILayout.BeginHorizontal();
            
            // Left decoration
            GUILayout.Label("◄", _headerStyle, GUILayout.Width(20));
            GUILayout.Label("KEY OVERLAY", _headerStyle);
            GUILayout.Label("►", _headerStyle, GUILayout.Width(20));
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawTabs()
        {
            GUILayout.BeginHorizontal();
            
            string[] tabs = { "General", "Position", "Style", "Colors", "Keys" };
            
            for (int i = 0; i < tabs.Length; i++)
            {
                bool isSelected = (_tab == i);
                
                // Highlight selected tab
                Color prevBg = GUI.backgroundColor;
                if (isSelected)
                {
                    GUI.backgroundColor = new Color(1f, 0.85f, 0.4f, 0.3f);
                }
                
                if (GUILayout.Button(tabs[i], _tabButtonStyle, GUILayout.Height(ButtonHeight - 4)))
                {
                    _tab = i;
                }
                
                GUI.backgroundColor = prevBg;
            }
            
            GUILayout.EndHorizontal();
        }
        
        private void DrawGeneralTab()
        {
            // Toggles with pixel checkbox style
            DrawPixelToggle("Show Keyboard Overlay", () => _config.ShowKeyboard, v => _config.SetShowKeyboard(v));
            DrawPixelToggle("Show Gamepad Overlay", () => _config.ShowGamepad, v => _config.SetShowGamepad(v));
            DrawPixelToggle("Show Combo Stats", () => _config.ShowComboStats, v => _config.SetShowComboStats(v));
            DrawPixelToggle("Show Key Names", () => _config.ShowKeyNames, v => _config.SetShowKeyNames(v));
            DrawPixelToggle("Show Movement Keys", () => _config.ShowMovementKeys, v => _config.SetShowMovementKeys(v));
            DrawPixelToggle("Show Action Keys", () => _config.ShowActionKeys, v => _config.SetShowActionKeys(v));
            
            GUILayout.Space(Spacing * 3);
            
            // Action buttons
            if (GUILayout.Button("[ RESET STATS ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                _input?.ResetStats();
            }
        }
        
        private void DrawPixelToggle(string label, System.Func<bool> getter, System.Action<bool> setter)
        {
            GUILayout.BeginHorizontal();
            
            bool value = getter();
            
            // Pixel checkbox box
            Rect checkRect = GUILayoutUtility.GetRect(20, 20, GUILayout.Width(20));
            
            // Draw checkbox background
            GUI.DrawTexture(checkRect, _pixelButtonTex, ScaleMode.StretchToFill, true, 0, Color.white, 0, 0);
            
            // Draw X if checked
            if (value)
            {
                GUI.color = PixelHighlight;
                GUI.Label(new Rect(checkRect.x + 4, checkRect.y + 2, 16, 16), "✓", _labelStyle);
                GUI.color = Color.white;
            }
            
            // Click detection
            if (Event.current.type == EventType.MouseDown && checkRect.Contains(Event.current.mousePosition))
            {
                setter(!value);
                Event.current.Use();
            }
            
            // Label
            GUILayout.Space(8);
            GUILayout.Label(label, _labelStyle);
            
            GUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        
        private void DrawPositionTab()
        {
            // Panel X
            DrawPixelSlider("Panel X", _config.PanelX, 0f, Screen.width, v => {
                _config.SetPanelX(v);
                _ui.RefreshTextures();
            }, "{0:F0}");
            
            // Panel Y
            DrawPixelSlider("Panel Y", _config.PanelY, 0f, Screen.height, v => {
                _config.SetPanelY(v);
                _ui.RefreshTextures();
            }, "{0:F0}");
            
            // Scale
            DrawPixelSlider("Scale", _config.Scale, 0.5f, 3f, v => {
                _config.SetScale(v);
                _ui.RefreshTextures();
            }, "{0:F1}x");
            
            // Opacity
            DrawPixelSlider("Opacity", _config.Opacity, 0.1f, 1f, v => {
                _config.SetOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F1}");
            
            GUILayout.Space(Spacing * 2);
            
            if (GUILayout.Button("[ RESET POSITION ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
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
            DrawPixelSlider("Border Opacity", _config.BorderOpacity, 0f, 1f, v => {
                _config.SetBorderOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider("Fill Opacity", _config.FillOpacity, 0f, 1f, v => {
                _config.SetFillOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider("Pressed Effect", _config.PressedEffectOpacity, 0f, 1f, v => {
                _config.SetPressedEffectOpacity(v);
                _ui.RefreshTextures();
            }, "{0:F2}");
            
            DrawPixelSlider("Border Width", _config.BorderWidth, 0.5f, 4f, v => {
                _config.SetBorderWidth(v);
                _ui.RefreshTextures();
            }, "{0:F1}px");
            
            DrawPixelSlider("Font Size", _config.FontSize, 8, 20, v => {
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
            GUILayout.Label("Normal Color", _labelStyle);
            DrawColorSliders("normal");
            
            GUILayout.Space(Spacing);
            
            GUILayout.Label("Pressed Color", _labelStyle);
            DrawColorSliders("pressed");
            
            GUILayout.Space(Spacing);
            
            GUILayout.Label("Border Color", _labelStyle);
            DrawColorSliders("border");
            
            GUILayout.Space(Spacing * 2);
            
            // Color preview box
            GUILayout.BeginHorizontal();
            GUILayout.Label("Preview:", _labelStyle, GUILayout.Width(70));
            
            Rect previewRect = GUILayoutUtility.GetRect(80, 40, GUILayout.Width(80));
            DrawPreviewBox(previewRect, _config.KeyColorNormal, _config.FillOpacity, _config.BorderColor, _config.BorderOpacity);
            
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
            
            // R
            GUILayout.BeginHorizontal();
            GUI.color = new Color(1f, 0.3f, 0.3f);
            GUILayout.Label("R", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            float newR = GUILayout.HorizontalSlider(c.r, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newR:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            // G
            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.3f, 1f, 0.3f);
            GUILayout.Label("G", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            float newG = GUILayout.HorizontalSlider(c.g, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newG:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            // B
            GUILayout.BeginHorizontal();
            GUI.color = new Color(0.3f, 0.5f, 1f);
            GUILayout.Label("B", _labelStyle, GUILayout.Width(20));
            GUI.color = Color.white;
            float newB = GUILayout.HorizontalSlider(c.b, 0f, 1f, _sliderStyle, _sliderThumbStyle);
            GUILayout.Label($"{newB:F1}", _smallLabelStyle, GUILayout.Width(30));
            GUILayout.EndHorizontal();
            
            if (newR != c.r || newG != c.g || newB != c.b)
            {
                setter(new Color(newR, newG, newB));
                _ui.RefreshTextures();
            }
            
            GUILayout.Space(4);
        }
        
        private void DrawPreviewBox(Rect rect, Color fill, float fillAlpha, Color border, float borderAlpha)
        {
            // Border
            GUI.color = new Color(border.r, border.g, border.b, borderAlpha);
            GUI.DrawTexture(rect, _pixelThumbTex);
            
            // Fill
            GUI.color = new Color(fill.r, fill.g, fill.b, fillAlpha);
            GUI.DrawTexture(new Rect(rect.x + 3, rect.y + 3, rect.width - 6, rect.height - 6), _pixelSliderTex);
            
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
                GUILayout.Label(keyNames[i], _labelStyle, GUILayout.Width(60));
                
                if (GUILayout.Button($"[ {keyCodes[i]} ]", _keyButtonStyle, GUILayout.Height(ButtonHeight - 6)))
                {
                    _waitingForKey = true;
                    _currentBindingIndex = i;
                }
                
                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
            
            GUILayout.Space(Spacing * 2);
            
            if (GUILayout.Button("[ RESET TO DEFAULTS ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
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
            
            GUILayout.BeginArea(new Rect(cx + 20, cy + 20, 280, 140));
            
            GUILayout.Label("◄ KEY BINDING ►", _headerStyle);
            GUILayout.Space(15);
            
            string[] keyNames = { "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab" };
            GUILayout.Label($"Press new key for:", _labelStyle);
            GUILayout.Label($"[ {keyNames[_currentBindingIndex]} ]", _headerStyle);
            GUILayout.Space(10);
            
            GUILayout.Label("Press any key... (Esc to cancel)", _smallLabelStyle);
            
            GUILayout.EndArea();
            
            HandleKeyInput();
        }
        
        private void DrawConfirmDialog()
        {
            float cx = Screen.width / 2 - 160;
            float cy = Screen.height / 2 - 90;
            
            GUI.Box(new Rect(cx, cy, 320, 180), "", _windowStyle);
            
            GUILayout.BeginArea(new Rect(cx + 20, cy + 20, 280, 140));
            
            GUILayout.Label("◄ CONFIRM ►", _headerStyle);
            GUILayout.Space(15);
            
            string[] keyNames = { "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab" };
            GUILayout.Label($"Bind {keyNames[_currentBindingIndex]} to:", _labelStyle);
            GUILayout.Label($"[ {_pendingKey} ]", _headerStyle);
            GUILayout.Space(15);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("[ CONFIRM ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
            {
                SetKeyBinding(_currentBindingIndex, _pendingKey);
                _confirmingKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
                _config.Save();
            }
            if (GUILayout.Button("[ CANCEL ]", _buttonStyle, GUILayout.Height(ButtonHeight)))
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