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
        private int _colorPage = 0; // 0: main, 1: colors
        
        private readonly string[] _mainOpts = {
            "Scale", "Opacity", "Keyboard", "Gamepad", "Stats", "Names", "Movement", "Actions",
            "Color Settings...", "Reset Pos", "Reset Stats"
        };
        
        private readonly string[] _colorOpts = {
            "Normal Color R", "Normal Color G", "Normal Color B",
            "Pressed Color R", "Pressed Color G", "Pressed Color B",
            "Border Color R", "Border Color G", "Border Color B",
            "Border Opacity", "Fill Opacity", "Pressed Effect Opacity",
            "Back to Main..."
        };
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c; _ui = u; _input = i;
        }
        
        public void OnGUI()
        {
            if (!IsMenuActive) return;
            
            if (_colorPage == 0)
            {
                DrawMainMenu();
            }
            else
            {
                DrawColorMenu();
            }
            
            HandleInput();
        }
        
        private void DrawMainMenu()
        {
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 220;
            
            GUI.Box(new Rect(cx, cy, 300, 450), "KEY OVERLAY SETTINGS");
            
            var style = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
            var selStyle = new GUIStyle { fontSize = 14, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            
            for (int i = 0; i < _mainOpts.Length; i++)
            {
                float y = cy + 30 + i * 30;
                string txt = _mainOpts[i];
                
                if (i == 0) txt += $": {_config.Scale:F1}";
                else if (i == 1) txt += $": {_config.Opacity:F1}";
                else if (i >= 2 && i <= 7) txt += GetToggle(i);
                
                GUI.Label(new Rect(cx + 30, y, 200, 25), (i == _sel ? "> " : "  ") + txt, i == _sel ? selStyle : style);
            }
            
            GUI.Label(new Rect(cx, cy + 420, 300, 25), "F1:Close | Arrows:Navigate | Enter:Toggle");
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
                case 2: v = _config.ShowKeyboard; break;
                case 3: v = _config.ShowGamepad; break;
                case 4: v = _config.ShowComboStats; break;
                case 5: v = _config.ShowKeyNames; break;
                case 6: v = _config.ShowMovementKeys; break;
                case 7: v = _config.ShowActionKeys; break;
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
            return _colorPage == 0 ? _mainOpts.Length : _colorOpts.Length;
        }
        
        private void Adjust(int dir)
        {
            if (_colorPage == 0)
            {
                if (_sel == 0) _config.SetScale(_config.Scale + dir * 0.1f);
                else if (_sel == 1) _config.SetOpacity(_config.Opacity + dir * 0.1f);
                else if (_sel >= 2 && _sel <= 7) Toggle(_sel);
            }
            else
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
                case 2: _config.SetShowKeyboard(!_config.ShowKeyboard); break;
                case 3: _config.SetShowGamepad(!_config.ShowGamepad); break;
                case 4: _config.SetShowComboStats(!_config.ShowComboStats); break;
                case 5: _config.SetShowKeyNames(!_config.ShowKeyNames); break;
                case 6: _config.SetShowMovementKeys(!_config.ShowMovementKeys); break;
                case 7: _config.SetShowActionKeys(!_config.ShowActionKeys); break;
            }
        }
        
        private void Apply()
        {
            if (_colorPage == 0)
            {
                if (_sel == 8) // Color Settings
                {
                    _colorPage = 1;
                    _sel = 0;
                }
                else if (_sel == 9) // Reset Pos
                {
                    _config.SetPanelX(200f);
                    _config.SetPanelY(100f);
                }
                else if (_sel == 10) // Reset Stats
                {
                    _input.ResetStats();
                }
            }
            else
            {
                if (_sel == 12) // Back
                {
                    _colorPage = 0;
                    _sel = 8;
                }
            }
            _config.Save();
        }
        
        public void OpenMenu() { IsMenuActive = true; _colorPage = 0; _sel = 0; }
        public void CloseMenu() { IsMenuActive = false; _colorPage = 0; _config.Save(); }
    }
}