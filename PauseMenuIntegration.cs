using UnityEngine;
using System.IO;

namespace KeyOverlay
{
    /// <summary>
    /// Refined Material Design Settings Menu
    /// Fixes layout overflows and language mapping logic
    /// </summary>
    public class PauseMenuIntegration
    {
        private ConfigWrapper _config;
        private KeyOverlayUI _ui;
        private InputMonitor _input;
        public bool IsMenuActive { get; private set; }
        
        private int _tab = 0;
        private Vector2 _scrollPos;
        
        // Key binding states
        private bool _waitingForKey = false;
        private bool _confirmingKey = false;
        private int _currentBindingIndex = -1;
        private KeyCode _pendingKey = KeyCode.None;
        
        // Window position
        private float _windowX = -1f;
        private float _windowY = -1f;
        private bool _isDragging = false;
        private Vector2 _dragOffset;
        
        // Material dimensions
        private const float WindowWidth = 480f; // Slightly wider for better spacing
        private const float WindowHeight = 580f;
        private const float TitleBarHeight = 64f;
        private const float SideBarWidth = 140f;
        private const float ContentPadding = 20f;
        private const float InnerWidth = WindowWidth - SideBarWidth - (ContentPadding * 2);
        
        // Material Color Palette
        private static readonly Color MatSurface = new Color(0.08f, 0.08f, 0.09f, 0.98f);
        private static readonly Color MatSurfaceVariant = new Color(0.14f, 0.14f, 0.16f);
        private static readonly Color MatPrimary = new Color(0.75f, 0.55f, 1.0f);
        private static readonly Color MatText = new Color(0.95f, 0.95f, 0.98f);
        private static readonly Color MatTextDim = new Color(0.6f, 0.6f, 0.65f);
        private static readonly Color MatDivider = new Color(1f, 1f, 1f, 0.05f);
        
        // Styles
        private GUIStyle _windowStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _navButtonStyle;
        private GUIStyle _sliderStyle;
        private GUIStyle _sliderThumbStyle;
        private GUIStyle _headerStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _cardStyle;
        private bool _stylesInitialized = false;
        
        // Textures
        private Texture2D _surfaceTex;
        private Texture2D _cardTex;
        private Texture2D _buttonTex;
        private Texture2D _accentTex;
        private Texture2D _shadowTex;
        private Texture2D _whiteTexture;
        private Font _mainFont;
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c;
            _ui = u;
            _input = i;
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
            
            _surfaceTex = CreateRoundedTexture(MatSurface, Color.clear, 64, 16);
            _cardTex = CreateRoundedTexture(MatSurfaceVariant, Color.clear, 32, 10);
            _buttonTex = CreateRoundedTexture(new Color(1f, 1f, 1f, 0.05f), Color.clear, 32, 6);
            _accentTex = CreateRoundedTexture(MatPrimary, Color.clear, 32, 6);
            _shadowTex = CreateShadowTexture(64, 16, 16);
            
            _whiteTexture = new Texture2D(1, 1);
            _whiteTexture.SetPixel(0, 0, Color.white);
            _whiteTexture.Apply();
            
            _windowStyle = new GUIStyle { normal = { background = _surfaceTex }, border = new RectOffset(16, 16, 16, 16) };
            _cardStyle = new GUIStyle { normal = { background = _cardTex }, border = new RectOffset(10, 10, 10, 10), padding = new RectOffset(12, 12, 12, 12) };
            
            _labelStyle = new GUIStyle { font = _mainFont, fontSize = 14, normal = { textColor = MatText }, alignment = TextAnchor.MiddleLeft, wordWrap = false };
            _titleStyle = new GUIStyle(_labelStyle) { fontSize = 22, fontStyle = FontStyle.Bold, normal = { textColor = MatPrimary } };
            _headerStyle = new GUIStyle(_labelStyle) { fontSize = 11, fontStyle = FontStyle.Bold, normal = { textColor = MatPrimary } };
            
            _buttonStyle = new GUIStyle { font = _mainFont, fontSize = 13, normal = { background = _buttonTex, textColor = MatText }, hover = { textColor = MatPrimary }, alignment = TextAnchor.MiddleCenter, border = new RectOffset(6, 6, 6, 6) };
            _navButtonStyle = new GUIStyle(_buttonStyle) { alignment = TextAnchor.MiddleLeft, padding = new RectOffset(20, 8, 0, 0) };
            
            _sliderStyle = new GUIStyle { normal = { background = CreateRoundedTexture(new Color(1f, 1f, 1f, 0.1f), Color.clear, 16, 4) }, fixedHeight = 4f, border = new RectOffset(4, 4, 4, 4) };
            _sliderThumbStyle = new GUIStyle { normal = { background = _accentTex }, fixedWidth = 12f, fixedHeight = 12f, border = new RectOffset(4, 4, 4, 4) };
        }
        
        private void LoadCustomFont()
        {
            string[] fonts = { "Inter", "Segoe UI", "Roboto", "Verdana", "Arial" };
            foreach (var f in fonts) { _mainFont = Font.CreateDynamicFontFromOSFont(f, 14); if (_mainFont != null) return; }
            _mainFont = GUI.skin.font;
        }
        
        public void OnGUI()
        {
            if (!IsMenuActive) return;
            InitStyles();
            HandleDragging();
            
            if (_waitingForKey) { DrawKeyBindingDialog(); return; }
            if (_confirmingKey) { DrawConfirmDialog(); return; }
            
            float cx = _windowX, cy = _windowY;
            
            // Draw Window
            GUI.color = new Color(1, 1, 1, 0.6f);
            GUI.DrawTexture(new Rect(cx - 20, cy - 20, WindowWidth + 40, WindowHeight + 40), _shadowTex);
            GUI.color = Color.white;
            GUI.Box(new Rect(cx, cy, WindowWidth, WindowHeight), "", _windowStyle);
            
            DrawSideBar(cx, cy);
            GUI.Label(new Rect(cx + SideBarWidth + 24, cy + 22, 300, 30), Localization.Get(GetTabKey(_tab)), _titleStyle);
            
            // Content Area
            Rect contentRect = new Rect(cx + SideBarWidth, cy + TitleBarHeight, WindowWidth - SideBarWidth, WindowHeight - TitleBarHeight);
            GUILayout.BeginArea(contentRect);
            _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUIStyle.none, GUIStyle.none);
            GUILayout.BeginVertical(new GUIStyle { padding = new RectOffset((int)ContentPadding, (int)ContentPadding, 0, (int)ContentPadding) });
            
            switch (_tab) {
                case 0: DrawTabGeneral(); break;
                case 1: DrawTabDisplay(); break;
                case 2: DrawTabStyle(); break;
                case 3: DrawTabColors(); break;
                case 4: DrawTabKeys(); break;
            }
            
            GUILayout.EndVertical();
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
        
        private void DrawSideBar(float cx, float cy)
        {
            GUI.Label(new Rect(cx + 36, cy + 24, 40, 40), "⌨", new GUIStyle(_titleStyle) { fontSize = 32 });
            string[] tabs = { "General", "Position", "Style", "Colors", "Keys" };
            for (int i = 0; i < tabs.Length; i++) {
                bool active = _tab == i;
                Rect r = new Rect(cx, cy + 110 + i * 52, SideBarWidth, 48);
                if (active) {
                    GUI.color = new Color(MatPrimary.r, MatPrimary.g, MatPrimary.b, 0.12f);
                    GUI.DrawTexture(r, _whiteTexture);
                    GUI.color = MatPrimary;
                    GUI.DrawTexture(new Rect(r.x, r.y, 4, r.height), _whiteTexture);
                }
                GUI.color = Color.white;
                var style = new GUIStyle(_navButtonStyle) { normal = { textColor = active ? MatPrimary : MatTextDim } };
                if (GUI.Button(r, Localization.Get(tabs[i]), style)) _tab = i;
            }
        }
        
        private void DrawTabGeneral()
        {
            DrawHeader("Language");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            int currentLang = _config.Language;
            // Map: UI Index -> Enum Index (0:Auto, 1:English, 2:ChineseSimplified)
            int[] langMap = { 0, 1, 8 }; 
            string[] options = { Localization.Get("Auto (Follow Game)"), "English", "简体中文" };
            
            for (int i = 0; i < options.Length; i++) {
                // Check if current config matches this map index
                bool isActive = (currentLang == langMap[i]);
                if (currentLang == 9 && langMap[i] == 8) isActive = true; // Traditional maps to Simplified if only Simp is in UI
                
                if (DrawMatRadioButton(options[i], isActive)) {
                    _config.SetLanguage(langMap[i]);
                }
                if (i < options.Length - 1) DrawDivider();
            }
            GUILayout.EndVertical();

            DrawHeader("Visibility");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawToggle("Keyboard", () => _config.ShowKeyboard, v => _config.SetShowKeyboard(v));
            DrawDivider();
            DrawToggle("Gamepad", () => _config.ShowGamepad, v => _config.SetShowGamepad(v));
            DrawDivider();
            DrawToggle("Combo Stats", () => _config.ShowComboStats, v => _config.SetShowComboStats(v));
            DrawDivider();
            DrawToggle("Key Names", () => _config.ShowKeyNames, v => _config.SetShowKeyNames(v));
            if (_config.ShowKeyNames) {
                DrawDivider();
                DrawToggle("  " + Localization.Get("Use Icons"), () => _config.ShowIconMode, v => _config.SetShowIconMode(v));
            }
            DrawDivider();
            DrawToggle("Joystick Indicator", () => _config.ShowJoystick, v => _config.SetShowJoystick(v));
            GUILayout.EndVertical();
            
            GUILayout.Space(20);
            if (GUILayout.Button("RESET STATISTICS", _buttonStyle, GUILayout.Width(InnerWidth - 24), GUILayout.Height(36))) _input?.ResetStats();
        }

        private bool DrawMatRadioButton(string label, bool active) {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(label, active ? new GUIStyle(_labelStyle) { normal = { textColor = MatPrimary } } : _labelStyle, GUILayout.Width(InnerWidth - 70));
            GUILayout.FlexibleSpace();
            bool result = GUILayout.Toggle(active, "", GUI.skin.toggle);
            GUILayout.EndHorizontal();
            return result && !active; 
        }
        
        private void DrawTabDisplay()
        {
            DrawHeader("Layout");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawSlider("X Position", _config.PanelX, 0, Screen.width, v => _config.SetPanelX(v));
            DrawSlider("Y Position", _config.PanelY, 0, Screen.height, v => _config.SetPanelY(v));
            DrawSlider("Scale", _config.Scale, 0.5f, 3f, v => _config.SetScale(v));
            DrawSlider("Opacity", _config.Opacity, 0.1f, 1f, v => _config.SetOpacity(v));
            GUILayout.EndVertical();
        }
        
        private void DrawTabStyle()
        {
            DrawHeader("Appearance");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            int cur = _config.OverlayStyle;
            string[] opts = { "Classic", "Minimal", "Ghost" };
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label("Key Style", _labelStyle, GUILayout.Width(InnerWidth - 100));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(opts[cur % 3], _buttonStyle, GUILayout.Width(80), GUILayout.Height(26))) {
                _config.SetOverlayStyle((cur + 1) % 3); _ui?.RefreshTextures();
            }
            GUILayout.EndHorizontal();
            DrawDivider();
            DrawSlider("Border Opacity", _config.BorderOpacity, 0, 1, v => _config.SetBorderOpacity(v));
            DrawSlider("Fill Opacity", _config.FillOpacity, 0, 1, v => _config.SetFillOpacity(v));
            DrawSlider("Border Width", _config.BorderWidth, 0.5f, 5, v => _config.SetBorderWidth(v));
            GUILayout.EndVertical();
        }
        
        private void DrawTabColors()
        {
            DrawHeader("Preview");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth), GUILayout.Height(100));
            Rect r = GUILayoutUtility.GetRect(InnerWidth - 24, 80);
            DrawColorPreview(r);
            GUILayout.EndVertical();

            DrawHeader("Normal State");
            DrawColorGroup("normal");
            GUILayout.Space(12);
            DrawHeader("Pressed State");
            DrawColorGroup("pressed");
            GUILayout.Space(12);
            DrawHeader("Border Color");
            DrawColorGroup("border");
        }

        private void DrawColorGroup(string t) {
            Color c;
            if (t == "normal") c = _config.KeyColorNormal;
            else if (t == "pressed") c = _config.KeyColorPressed;
            else c = _config.BorderColor;
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawSlider("Red", c.r, 0, 1, v => SetCol(t, new Color(v, c.g, c.b)));
            DrawSlider("Green", c.g, 0, 1, v => SetCol(t, new Color(c.r, v, c.b)));
            DrawSlider("Blue", c.b, 0, 1, v => SetCol(t, new Color(c.r, c.g, v)));
            GUILayout.EndVertical();
        }

        private void DrawColorPreview(Rect r) {
            float pad = 12;
            float w = (r.width - pad * 3) / 2;
            float h = r.height - pad * 2 - 15;
            Rect r1 = new Rect(r.x + pad, r.y + pad, w, h);
            GUI.color = new Color(_config.KeyColorNormal.r, _config.KeyColorNormal.g, _config.KeyColorNormal.b, _config.FillOpacity);
            GUI.DrawTexture(r1, _whiteTexture);
            GUI.color = new Color(_config.BorderColor.r, _config.BorderColor.g, _config.BorderColor.b, _config.BorderOpacity);
            DrawBorder(r1, _config.BorderWidth);
            Rect r2 = new Rect(r1.xMax + pad, r.y + pad, w, h);
            GUI.color = new Color(_config.KeyColorPressed.r, _config.KeyColorPressed.g, _config.KeyColorPressed.b, _config.PressedEffectOpacity);
            GUI.DrawTexture(r2, _whiteTexture);
            GUI.color = new Color(_config.BorderColor.r, _config.BorderColor.g, _config.BorderColor.b, _config.BorderOpacity);
            DrawBorder(r2, _config.BorderWidth);
            GUI.color = Color.white;
            GUI.Label(new Rect(r1.x, r1.yMax + 2, w, 15), "Normal", new GUIStyle(_labelStyle) { fontSize = 10, alignment = TextAnchor.MiddleCenter });
            GUI.Label(new Rect(r2.x, r2.yMax + 2, w, 15), "Pressed", new GUIStyle(_labelStyle) { fontSize = 10, alignment = TextAnchor.MiddleCenter });
        }

        private void DrawBorder(Rect r, float t) {
            GUI.DrawTexture(new Rect(r.x, r.y, r.width, t), _whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.yMax - t, r.width, t), _whiteTexture);
            GUI.DrawTexture(new Rect(r.x, r.y, t, r.height), _whiteTexture);
            GUI.DrawTexture(new Rect(r.xMax - t, r.y, t, r.height), _whiteTexture);
        }
        
        private void DrawTabKeys()
        {
            DrawHeader("System");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawKeyRow("Menu Key", _config.MenuKey, 0);
            GUILayout.EndVertical();
            GUILayout.Space(12);
            DrawHeader("Movement");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawKeyRow("Up", _config.KeyUp, 1);
            DrawKeyRow("Down", _config.KeyDown, 2);
            DrawKeyRow("Left", _config.KeyLeft, 3);
            DrawKeyRow("Right", _config.KeyRight, 4);
            GUILayout.EndVertical();
            GUILayout.Space(12);
            DrawHeader("Actions");
            GUILayout.BeginVertical(_cardStyle, GUILayout.Width(InnerWidth));
            DrawKeyRow("Jump", _config.KeyJump, 5);
            DrawKeyRow("Throw", _config.KeyThrow, 6);
            DrawKeyRow("Grab", _config.KeyGrab, 7);
            GUILayout.EndVertical();
        }
        
        private void DrawHeader(string s) { GUILayout.Space(8); GUILayout.Label(s.ToUpper(), _headerStyle, GUILayout.Width(InnerWidth)); GUILayout.Space(4); }
        private void DrawDivider() { Rect r = GUILayoutUtility.GetRect(InnerWidth - 24, 1); GUI.color = MatDivider; GUI.DrawTexture(r, _whiteTexture); GUI.color = Color.white; }
        
        private void DrawToggle(string l, System.Func<bool> g, System.Action<bool> s) {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(l, _labelStyle, GUILayout.Width(InnerWidth - 80)); GUILayout.FlexibleSpace();
            bool v = g(); if (GUILayout.Button(v ? "ON" : "OFF", v ? new GUIStyle(_buttonStyle) { normal = { textColor = MatPrimary } } : _buttonStyle, GUILayout.Width(50), GUILayout.Height(24))) s(!v);
            GUILayout.EndHorizontal();
        }
        
        private void DrawSlider(string l, float v, float min, float max, System.Action<float> s) {
            GUILayout.BeginVertical(GUILayout.Width(InnerWidth - 24));
            GUILayout.BeginHorizontal(); 
            GUILayout.Label(l, new GUIStyle(_labelStyle) { fontSize = 12, normal = { textColor = MatTextDim } }); 
            GUILayout.FlexibleSpace(); 
            GUILayout.Label(v.ToString("F1"), _labelStyle); 
            GUILayout.EndHorizontal();
            float n = GUILayout.HorizontalSlider(v, min, max, _sliderStyle, _sliderThumbStyle);
            if (Mathf.Abs(n - v) > 0.01f) { s(n); _ui?.RefreshTextures(); }
            GUILayout.EndVertical(); GUILayout.Space(8);
        }

        private void SetCol(string t, Color c) {
            if (t == "normal") _config.SetKeyColorNormal(c);
            else if (t == "pressed") _config.SetKeyColorPressed(c);
            else _config.SetBorderColor(c);
            _ui?.RefreshTextures();
        }

        private void DrawKeyRow(string l, KeyCode k, int i) {
            GUILayout.BeginHorizontal(GUILayout.Height(32));
            GUILayout.Label(l, _labelStyle, GUILayout.Width(InnerWidth - 110)); GUILayout.FlexibleSpace();
            if (GUILayout.Button($"[ {k} ]", _buttonStyle, GUILayout.Width(90), GUILayout.Height(24))) { _waitingForKey = true; _currentBindingIndex = i; }
            GUILayout.EndHorizontal();
        }
        
        private string GetTabKey(int t) { string[] k = { "General", "Position", "Style", "Colors", "Keys" }; return k[t]; }
        
        private Texture2D CreateRoundedTexture(Color fill, Color border, int size, int radius) {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            float r = (float)radius;
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dx = 0, dy = 0; bool inC = false;
                    if (x < radius && y < radius) { dx = r - x; dy = r - y; inC = true; }
                    else if (x >= size - radius && y < radius) { dx = x - (size - r - 1); dy = r - y; inC = true; }
                    else if (x < radius && y >= size - radius) { dx = r - x; dy = y - (size - r - 1); inC = true; }
                    else if (x >= size - radius && y >= size - radius) { dx = x - (size - r - 1); dy = y - (size - r - 1); inC = true; }
                    float a = 1f; if (inC) { float d = Mathf.Sqrt(dx * dx + dy * dy); if (d > r) a = 0; else if (d > r - 1f) a = 1f - (d - (r - 1f)); }
                    pixels[y * size + x] = new Color(fill.r, fill.g, fill.b, fill.a * a);
                }
            }
            tex.SetPixels(pixels); tex.Apply(); return tex;
        }

        private Texture2D CreateShadowTexture(int size, int radius, float blur) {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] pixels = new Color[size * size];
            for (int y = 0; y < size; y++) {
                for (int x = 0; x < size; x++) {
                    float dx = 0, dy = 0;
                    if (x < radius && y < radius) { dx = radius - x; dy = radius - y; }
                    else if (x >= size - radius && y < radius) { dx = x - (size - radius - 1); dy = radius - y; }
                    else if (x < radius && y >= size - radius) { dx = radius - x; dy = y - (size - radius - 1); }
                    else if (x >= size - radius && y >= size - radius) { dx = x - (size - radius - 1); dy = y - (size - radius - 1); }
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float ed = (dx > 0 || dy > 0) ? d - radius : -Mathf.Min(Mathf.Min(x - radius, size - radius - 1 - x), Mathf.Min(y - radius, size - radius - 1 - y));
                    pixels[y * size + x] = new Color(0, 0, 0, Mathf.Clamp01(1f - (ed + blur) / (blur * 2f)) * 0.4f);
                }
            }
            tex.SetPixels(pixels); tex.Apply(); return tex;
        }

        private void HandleDragging() {
            Event e = Event.current;
            if (e.type == EventType.MouseDown && e.button == 0 && new Rect(_windowX, _windowY, WindowWidth, TitleBarHeight).Contains(e.mousePosition)) {
                _isDragging = true; _dragOffset = e.mousePosition - new Vector2(_windowX, _windowY); e.Use();
            } else if (e.type == EventType.MouseUp && e.button == 0) {
                _isDragging = false;
            } else if (e.type == EventType.MouseDrag && _isDragging) {
                _windowX = Mathf.Clamp(e.mousePosition.x - _dragOffset.x, 0, Screen.width - WindowWidth);
                _windowY = Mathf.Clamp(e.mousePosition.y - _dragOffset.y, 0, Screen.height - WindowHeight);
                e.Use();
            }
        }

        private void DrawKeyBindingDialog() {
            GUI.Box(new Rect(Screen.width/2-150, Screen.height/2-50, 300, 100), "", _windowStyle);
            GUI.Label(new Rect(Screen.width/2-130, Screen.height/2-30, 260, 60), "Press any key to bind...", _titleStyle);
            HandleKeyInput();
        }

        private void DrawConfirmDialog() {
            GUI.Box(new Rect(Screen.width/2-150, Screen.height/2-60, 300, 120), "", _windowStyle);
            GUI.Label(new Rect(Screen.width/2-130, Screen.height/2-40, 260, 30), $"Bind to {_pendingKey}?", _labelStyle);
            if (GUI.Button(new Rect(Screen.width/2-130, Screen.height/2, 120, 30), "OK", _buttonStyle)) { SetBinding(_currentBindingIndex, _pendingKey); _confirmingKey = false; _config.Save(); }
            if (GUI.Button(new Rect(Screen.width/2+10, Screen.height/2, 120, 30), "Cancel", _buttonStyle)) { _confirmingKey = false; _waitingForKey = false; }
        }

        private void HandleKeyInput() {
            var e = Event.current; if (e.type != EventType.KeyDown) return;
            if (e.keyCode == KeyCode.Escape) { _waitingForKey = false; e.Use(); return; }
            if (e.keyCode != KeyCode.None) { _pendingKey = e.keyCode; _waitingForKey = false; _confirmingKey = true; e.Use(); }
        }

        private void SetBinding(int i, KeyCode k) {
            switch (i) {
                case 0: _config.SetMenuKey(k); break;
                case 1: _config.SetKeyUp(k); break;
                case 2: _config.SetKeyDown(k); break;
                case 3: _config.SetKeyLeft(k); break;
                case 4: _config.SetKeyRight(k); break;
                case 5: _config.SetKeyJump(k); break;
                case 6: _config.SetKeyThrow(k); break;
                case 7: _config.SetKeyGrab(k); break;
            }
        }

        public void OpenMenu() { Localization.Refresh(); IsMenuActive = true; }
        public void CloseMenu() { IsMenuActive = false; _config.Save(); }
    }
}
