using UnityEngine;

namespace KeyOverlay
{
    public class PauseMenuIntegration
    {
        private ConfigWrapper _config;
        private KeyOverlayUI _ui;
        private InputMonitor _input;
        public bool IsMenuActive { get; private set; }
        private int _sel = 0;
        private int _page = 0; // 0: main, 1: colors, 2: keybindings
        
        // Key binding states
        private bool _waitingForKey = false;      // Step 1: waiting for user to press a key
        private bool _confirmingKey = false;      // Step 2: waiting for Enter to confirm
        private int _currentBindingIndex = -1;
        private KeyCode _pendingKey = KeyCode.None; // The key user pressed, waiting for confirmation
        
        private readonly string[] _mainOpts = {
            "Scale", "Opacity", "Font Size", "Border Width", "Keyboard", "Gamepad", "Stats", "Names", "Movement", "Actions",
            "Color Settings...", "Key Bindings...", "Reset Pos", "Reset Stats"
        };
        
        private readonly string[] _colorOpts = {
            "Normal Color R", "Normal Color G", "Normal Color B",
            "Pressed Color R", "Pressed Color G", "Pressed Color B",
            "Border Color R", "Border Color G", "Border Color B",
            "Border Opacity", "Fill Opacity", "Pressed Effect Opacity",
            "Back to Main..."
        };
        
        private readonly string[] _keyBindOpts = {
            "Up", "Down", "Left", "Right", "Jump", "Throw", "Grab",
            "Back to Main..."
        };
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c; _ui = u; _input = i;
        }
        
        public void OnGUI()
        {
            if (!IsMenuActive) return;
            
            if (_waitingForKey)
            {
                DrawWaitingForKey();
                HandleKeyBindInput();
                return;
            }
            
            if (_confirmingKey)
            {
                DrawConfirmingKey();
                HandleConfirmInput();
                return;
            }
            
            if (_page == 0) DrawMainMenu();
            else if (_page == 1) DrawColorMenu();
            else if (_page == 2) DrawKeyBindMenu();
            
            HandleInput();
        }
        
        private void DrawWaitingForKey()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 60;
            
            GUI.Box(new Rect(cx, cy, 300, 120), "KEY BINDING");
            
            var style = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            GUI.Label(new Rect(cx, cy + 35, 300, 25), $"Press new key for {_keyBindOpts[_currentBindingIndex]}", style);
            
            var hintStyle = new GUIStyle { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };
            GUI.Label(new Rect(cx, cy + 65, 300, 25), "Press any key...", hintStyle);
            GUI.Label(new Rect(cx, cy + 85, 300, 25), "Esc: Cancel", hintStyle);
        }
        
        private void DrawConfirmingKey()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 60;
            
            GUI.Box(new Rect(cx, cy, 300, 120), "CONFIRM BINDING");
            
            var style = new GUIStyle { fontSize = 14, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            GUI.Label(new Rect(cx, cy + 35, 300, 25), $"Bind {_keyBindOpts[_currentBindingIndex]} to: {_pendingKey}", style);
            
            var hintStyle = new GUIStyle { fontSize = 12, alignment = TextAnchor.MiddleCenter, normal = { textColor = new Color(0.6f, 0.6f, 0.6f) } };
            GUI.Label(new Rect(cx, cy + 65, 300, 25), "Enter: Confirm | Esc: Cancel", hintStyle);
            GUI.Label(new Rect(cx, cy + 85, 300, 25), "Press other key to change", hintStyle);
        }
        
        private void DrawMainMenu()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 250;
            
            GUI.Box(new Rect(cx, cy, 300, 540), "KEY OVERLAY SETTINGS");
            
            var style = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
            var selStyle = new GUIStyle { fontSize = 14, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            
            for (int i = 0; i < _mainOpts.Length; i++)
            {
                float y = cy + 30 + i * 30;
                string txt = _mainOpts[i];
                
                if (i == 0) txt += $": {_config.Scale:F1}";
                else if (i == 1) txt += $": {_config.Opacity:F1}";
                else if (i == 2) txt += $": {_config.FontSize}";
                else if (i == 3) txt += $": {_config.BorderWidth:F1}";
                else if (i >= 4 && i <= 9) txt += GetToggle(i);
                
                GUI.Label(new Rect(cx + 30, y, 200, 25), (i == _sel ? "> " : "  ") + txt, i == _sel ? selStyle : style);
            }
            
            GUI.Label(new Rect(cx, cy + 500, 300, 25), "F1:Close | Arrows:Navigate | Enter:Toggle");
        }
        
        private void DrawColorMenu()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 230;
            
            GUI.Box(new Rect(cx, cy, 300, 470), "COLOR SETTINGS");
            
            var style = new GUIStyle { fontSize = 12, normal = { textColor = Color.white } };
            var selStyle = new GUIStyle { fontSize = 12, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            
            for (int i = 0; i < _colorOpts.Length; i++)
            {
                float y = cy + 30 + i * 30;
                string txt = _colorOpts[i];
                float val = GetColorValue(i);
                txt += $": {val:F2}";
                
                GUI.Label(new Rect(cx + 30, y, 200, 25), (i == _sel ? "> " : "  ") + txt, i == _sel ? selStyle : style);
            }
            
            // Preview box
            float px = cx + 220;
            float py = cy + 30;
            DrawPreviewKey(px, py);
            
            GUI.Label(new Rect(cx, cy + 440, 300, 25), "Arrows:Adjust | Enter:Back");
        }
        
        private void DrawPreviewKey(float x, float y)
        {
            float size = 40;
            
            // Border
            GUI.color = new Color(_config.BorderColor.r, _config.BorderColor.g, _config.BorderColor.b, _config.BorderOpacity);
            GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
            
            // Fill
            GUI.color = new Color(_config.KeyColorNormal.r, _config.KeyColorNormal.g, _config.KeyColorNormal.b, _config.FillOpacity);
            GUI.DrawTexture(new Rect(x + 2, y + 2, size - 4, size - 4), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }
        
        private void DrawKeyBindMenu()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 150;
            
            GUI.Box(new Rect(cx, cy, 300, 270), "KEY BINDINGS");
            
            var style = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
            var selStyle = new GUIStyle { fontSize = 14, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            
            for (int i = 0; i < _keyBindOpts.Length; i++)
            {
                float y = cy + 30 + i * 30;
                string txt = _keyBindOpts[i];
                
                if (i < 7) txt += $": {GetKeyBindingName(i)}";
                
                GUI.Label(new Rect(cx + 30, y, 200, 25), (i == _sel ? "> " : "  ") + txt, i == _sel ? selStyle : style);
            }
            
            GUI.Label(new Rect(cx, cy + 240, 300, 25), "Enter:Change | Esc:Back");
        }
        
        private string GetKeyBindingName(int i)
        {
            switch (i)
            {
                case 0: return _config.KeyUp.ToString();
                case 1: return _config.KeyDown.ToString();
                case 2: return _config.KeyLeft.ToString();
                case 3: return _config.KeyRight.ToString();
                case 4: return _config.KeyJump.ToString();
                case 5: return _config.KeyThrow.ToString();
                case 6: return _config.KeyGrab.ToString();
                default: return "";
            }
        }
        
        private void HandleKeyBindInput()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            // Cancel with Escape
            if (e.keyCode == KeyCode.Escape)
            {
                _waitingForKey = false;
                _confirmingKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
                e.Use();
                return;
            }
            
            // Ignore navigation keys
            if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow ||
                e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow ||
                e.keyCode == KeyCode.Return)
            {
                return;
            }
            
            // Get the key pressed
            KeyCode key = e.keyCode;
            if (key == KeyCode.None)
            {
                if (e.character != '\0')
                {
                    key = CharToKeyCode(e.character);
                }
                if (key == KeyCode.None) return;
            }
            
            // Store the key and transition to confirming state
            _pendingKey = key;
            _waitingForKey = false;
            _confirmingKey = true;
            e.Use();
        }
        
        private void HandleConfirmInput()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            // Cancel with Escape
            if (e.keyCode == KeyCode.Escape)
            {
                _confirmingKey = false;
                _waitingForKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
                e.Use();
                return;
            }
            
            // Confirm with Enter
            if (e.keyCode == KeyCode.Return)
            {
                SetKeyBinding(_currentBindingIndex, _pendingKey);
                _confirmingKey = false;
                _currentBindingIndex = -1;
                _pendingKey = KeyCode.None;
                _config.Save();
                e.Use();
                return;
            }
            
            // Change key - press another key
            if (e.keyCode != KeyCode.None && e.keyCode != KeyCode.UpArrow && 
                e.keyCode != KeyCode.DownArrow && e.keyCode != KeyCode.LeftArrow && 
                e.keyCode != KeyCode.RightArrow)
            {
                _pendingKey = e.keyCode;
                e.Use();
                return;
            }
            
            // Character key
            if (e.keyCode == KeyCode.None && e.character != '\0')
            {
                KeyCode key = CharToKeyCode(e.character);
                if (key != KeyCode.None)
                {
                    _pendingKey = key;
                    e.Use();
                }
            }
        }
        
        private KeyCode CharToKeyCode(char c)
        {
            // Convert character to KeyCode
            c = char.ToUpper(c);
            
            // Letters
            if (c >= 'A' && c <= 'Z')
                return (KeyCode)((int)KeyCode.A + (c - 'A'));
            
            // Numbers
            if (c >= '0' && c <= '9')
                return (KeyCode)((int)KeyCode.Alpha0 + (c - '0'));
            
            // Special keys
            switch (c)
            {
                case ' ': return KeyCode.Space;
                case '\n': return KeyCode.Return;
                case '\t': return KeyCode.Tab;
                case '-': return KeyCode.Minus;
                case '=': return KeyCode.Equals;
                case '[': return KeyCode.LeftBracket;
                case ']': return KeyCode.RightBracket;
                case '\\': return KeyCode.Backslash;
                case ';': return KeyCode.Semicolon;
                case '\'': return KeyCode.Quote;
                case ',': return KeyCode.Comma;
                case '.': return KeyCode.Period;
                case '/': return KeyCode.Slash;
            }
            
            return KeyCode.None;
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
        
        private float GetColorValue(int i)
        {
            switch (i)
            {
                case 0: return _config.KeyColorNormal.r;
                case 1: return _config.KeyColorNormal.g;
                case 2: return _config.KeyColorNormal.b;
                case 3: return _config.KeyColorPressed.r;
                case 4: return _config.KeyColorPressed.g;
                case 5: return _config.KeyColorPressed.b;
                case 6: return _config.BorderColor.r;
                case 7: return _config.BorderColor.g;
                case 8: return _config.BorderColor.b;
                case 9: return _config.BorderOpacity;
                case 10: return _config.FillOpacity;
                case 11: return _config.PressedEffectOpacity;
                default: return 0;
            }
        }
        
        private string GetToggle(int i)
        {
            bool v = false;
            switch (i)
            {
                case 4: v = _config.ShowKeyboard; break;
                case 5: v = _config.ShowGamepad; break;
                case 6: v = _config.ShowComboStats; break;
                case 7: v = _config.ShowKeyNames; break;
                case 8: v = _config.ShowMovementKeys; break;
                case 9: v = _config.ShowActionKeys; break;
            }
            return $": {(v ? "ON" : "OFF")}";
        }
        
        private void HandleInput()
        {
            var e = Event.current;
            if (e.type != EventType.KeyDown) return;
            
            if (e.keyCode == KeyCode.UpArrow) { _sel = (_sel - 1 + GetOptsLength()) % GetOptsLength(); e.Use(); }
            else if (e.keyCode == KeyCode.DownArrow) { _sel = (_sel + 1) % GetOptsLength(); e.Use(); }
            else if (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow) { Adjust(e.keyCode == KeyCode.RightArrow ? 1 : -1); e.Use(); }
            else if (e.keyCode == KeyCode.Return) { Apply(); e.Use(); }
            else if (e.keyCode == KeyCode.Escape) { CloseMenu(); e.Use(); }
        }
        
        private int GetOptsLength()
        {
            if (_page == 0) return _mainOpts.Length;
            if (_page == 1) return _colorOpts.Length;
            return _keyBindOpts.Length;
        }
        
        private void Adjust(int dir)
        {
            if (_page == 0)
            {
                if (_sel == 0) _config.SetScale(_config.Scale + dir * 0.1f);
                else if (_sel == 1) _config.SetOpacity(_config.Opacity + dir * 0.1f);
                else if (_sel == 2) _config.SetFontSize(_config.FontSize + dir);
                else if (_sel == 3) _config.SetBorderWidth(_config.BorderWidth + dir * 0.5f);
                else if (_sel >= 4 && _sel <= 9) Toggle(_sel);
            }
            else if (_page == 1)
            {
                AdjustColorValue(_sel, dir * 0.05f);
            }
            _ui.RefreshTextures();
        }
        
        private void AdjustColorValue(int i, float delta)
        {
            switch (i)
            {
                case 0: _config.SetKeyColorNormal(new Color(Clamp01(_config.KeyColorNormal.r + delta), _config.KeyColorNormal.g, _config.KeyColorNormal.b)); break;
                case 1: _config.SetKeyColorNormal(new Color(_config.KeyColorNormal.r, Clamp01(_config.KeyColorNormal.g + delta), _config.KeyColorNormal.b)); break;
                case 2: _config.SetKeyColorNormal(new Color(_config.KeyColorNormal.r, _config.KeyColorNormal.g, Clamp01(_config.KeyColorNormal.b + delta))); break;
                case 3: _config.SetKeyColorPressed(new Color(Clamp01(_config.KeyColorPressed.r + delta), _config.KeyColorPressed.g, _config.KeyColorPressed.b)); break;
                case 4: _config.SetKeyColorPressed(new Color(_config.KeyColorPressed.r, Clamp01(_config.KeyColorPressed.g + delta), _config.KeyColorPressed.b)); break;
                case 5: _config.SetKeyColorPressed(new Color(_config.KeyColorPressed.r, _config.KeyColorPressed.g, Clamp01(_config.KeyColorPressed.b + delta))); break;
                case 6: _config.SetBorderColor(new Color(Clamp01(_config.BorderColor.r + delta), _config.BorderColor.g, _config.BorderColor.b)); break;
                case 7: _config.SetBorderColor(new Color(_config.BorderColor.r, Clamp01(_config.BorderColor.g + delta), _config.BorderColor.b)); break;
                case 8: _config.SetBorderColor(new Color(_config.BorderColor.r, _config.BorderColor.g, Clamp01(_config.BorderColor.b + delta))); break;
                case 9: _config.SetBorderOpacity(_config.BorderOpacity + delta); break;
                case 10: _config.SetFillOpacity(_config.FillOpacity + delta); break;
                case 11: _config.SetPressedEffectOpacity(_config.PressedEffectOpacity + delta); break;
            }
        }
        
        private float Clamp01(float v) => Mathf.Clamp(v, 0f, 1f);
        
        private void Toggle(int i)
        {
            switch (i)
            {
                case 4: _config.SetShowKeyboard(!_config.ShowKeyboard); break;
                case 5: _config.SetShowGamepad(!_config.ShowGamepad); break;
                case 6: _config.SetShowComboStats(!_config.ShowComboStats); break;
                case 7: _config.SetShowKeyNames(!_config.ShowKeyNames); break;
                case 8: _config.SetShowMovementKeys(!_config.ShowMovementKeys); break;
                case 9: _config.SetShowActionKeys(!_config.ShowActionKeys); break;
            }
        }
        
        private void Apply()
        {
            if (_page == 0) // Main menu
            {
                if (_sel == 10) // Color Settings
                {
                    _page = 1;
                    _sel = 0;
                }
                else if (_sel == 11) // Key Bindings
                {
                    _page = 2;
                    _sel = 0;
                }
                else if (_sel == 12) // Reset Pos
                {
                    _config.SetPanelX(200f);
                    _config.SetPanelY(100f);
                }
                else if (_sel == 13) // Reset Stats
                {
                    _input.ResetStats();
                }
            }
            else if (_page == 1) // Color menu
            {
                if (_sel == 12) // Back
                {
                    _page = 0;
                    _sel = 10;
                }
            }
            else if (_page == 2) // Key Bindings menu
            {
                if (_sel == 7) // Back
                {
                    _page = 0;
                    _sel = 11;
                }
                else if (_sel < 7) // Change key binding
                {
                    _waitingForKey = true;
                    _currentBindingIndex = _sel;
                }
            }
            _config.Save();
        }
        
        public void OpenMenu() { IsMenuActive = true; _page = 0; _sel = 0; }
        public void CloseMenu() { IsMenuActive = false; _page = 0; _config.Save(); }
    }
}