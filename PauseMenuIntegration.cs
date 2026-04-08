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
        
        private readonly string[] _opts = {
            "Scale", "Opacity", "Keyboard", "Gamepad", "Stats", "Names", "Movement", "Actions", "Reset Pos", "Reset Stats"
        };
        
        public PauseMenuIntegration(ConfigWrapper c, KeyOverlayUI u, InputMonitor i)
        {
            _config = c; _ui = u; _input = i;
        }
        
        public void OnGUI()
        {
            if (!IsMenuActive) return;
            
            float cx = Screen.width / 2 - 150;
            float cy = Screen.height / 2 - 200;
            
            GUI.Box(new Rect(cx, cy, 300, 400), "KEY OVERLAY SETTINGS");
            
            var style = new GUIStyle { fontSize = 14, normal = { textColor = Color.white } };
            var selStyle = new GUIStyle { fontSize = 14, normal = { textColor = new Color(0.9f, 0.7f, 0.2f) } };
            
            for (int i = 0; i < _opts.Length; i++)
            {
                float y = cy + 30 + i * 30;
                string txt = _opts[i];
                
                if (i == 0) txt += $": {_config.Scale:F1}";
                else if (i == 1) txt += $": {_config.Opacity:F1}";
                else if (i >= 2 && i <= 7) txt += GetToggle(i);
                
                GUI.Label(new Rect(cx + 30, y, 200, 25), (i == _sel ? "> " : "  ") + txt, i == _sel ? selStyle : style);
            }
            
            GUI.Label(new Rect(cx, cy + 370, 300, 25), "F1:Close | Arrows:Navigate | Enter:Toggle");
            
            HandleInput();
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
            
            if (e.keyCode == KeyCode.UpArrow) { _sel = (_sel - 1 + _opts.Length) % _opts.Length; e.Use(); }
            else if (e.keyCode == KeyCode.DownArrow) { _sel = (_sel + 1) % _opts.Length; e.Use(); }
            else if (e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow) { Adjust(e.keyCode == KeyCode.RightArrow ? 1 : -1); e.Use(); }
            else if (e.keyCode == KeyCode.Return) { Apply(); e.Use(); }
            else if (e.keyCode == KeyCode.Escape) { CloseMenu(); e.Use(); }
        }
        
        private void Adjust(int dir)
        {
            if (_sel == 0) _config.SetScale(_config.Scale + dir * 0.1f);
            else if (_sel == 1) _config.SetOpacity(_config.Opacity + dir * 0.1f);
            else if (_sel >= 2 && _sel <= 7) Toggle(_sel);
            _ui.RefreshTextures();
        }
        
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
            if (_sel == 8) { _config.SetPanelX(200f); _config.SetPanelY(100f); }
            else if (_sel == 9) _input.ResetStats();
            _config.Save();
        }
        
        public void OpenMenu() { IsMenuActive = true; }
        public void CloseMenu() { IsMenuActive = false; _config.Save(); }
    }
}