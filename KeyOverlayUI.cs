using UnityEngine;

namespace KeyOverlay
{
    public class KeyOverlayUI
    {
        private ConfigWrapper _config;
        private InputMonitor _input;
        private Texture2D _keyTex, _pressedTex;
        private GUIStyle _style;
        private bool _dragging;
        private Vector2 _dragOffset;
        
        public KeyOverlayUI(ConfigWrapper config, InputMonitor input)
        {
            _config = config;
            _input = input;
            _style = new GUIStyle { fontSize = 10, alignment = TextAnchor.MiddleCenter, normal = { textColor = Color.white } };
        }
        
        public void OnGUI()
        {
            if (_keyTex == null)
            {
                _keyTex = MakeTex(new Color(0.3f, 0.3f, 0.35f));
                _pressedTex = MakeTex(new Color(0.9f, 0.7f, 0.2f));
            }
            
            GUI.color = new Color(1, 1, 1, _config.Opacity);
            
            float x = _config.PanelX;
            float y = _config.PanelY;
            float s = _config.Scale;
            float w = 32 * s;
            float h = 32 * s;
            float sp = 4 * s;
            
            GUI.Box(new Rect(x, y, 200 * s, 100 * s), "");
            
            if (_config.ShowMovementKeys && _config.ShowKeyboard)
            {
                DrawKey(x + sp + w + sp, y + sp, w, h, "Up", "W");
                DrawKey(x + sp, y + sp + h + sp, w, h, "Left", "A");
                DrawKey(x + sp + w + sp, y + sp + h + sp, w, h, "Down", "S");
                DrawKey(x + sp + 2 * (w + sp), y + sp + h + sp, w, h, "Right", "D");
            }
            
            if (_config.ShowActionKeys && _config.ShowKeyboard)
            {
                float ax = x + sp + 4 * (w + sp);
                DrawKey(ax, y + sp, w, h, "Jump", "JMP");
                DrawKey(ax, y + sp + h + sp, w, h, "Grab", "GRB");
                DrawKey(ax + w + sp, y + sp, w, h, "Throw", "THR");
            }
            
            if (_config.ShowComboStats)
            {
                GUI.Label(new Rect(x + sp, y + 80 * s, 180 * s, 20 * s), 
                    $"JMP:{_input.JumpCombo} THR:{_input.ThrowCombo} GRB:{_input.GrabCombo}", _style);
            }
            
            GUI.color = Color.white;
            
            var e = Event.current;
            var rect = new Rect(x, y, 200 * s, 100 * s);
            
            if (e.type == EventType.MouseDown && e.button == 0 && rect.Contains(e.mousePosition))
            {
                _dragging = true;
                _dragOffset = e.mousePosition - new Vector2(x, y);
                e.Use();
            }
            if (e.type == EventType.MouseUp && e.button == 0 && _dragging)
            {
                _dragging = false;
                _config.Save();
                e.Use();
            }
            if (e.type == EventType.MouseDrag && _dragging)
            {
                _config.SetPanelX(e.mousePosition.x - _dragOffset.x);
                _config.SetPanelY(e.mousePosition.y - _dragOffset.y);
                e.Use();
            }
        }
        
        private void DrawKey(float x, float y, float w, float h, string keyName, string label)
        {
            var state = _input.GetKeyState(keyName);
            bool pressed = state != null && state.IsPressed;
            
            GUI.DrawTexture(new Rect(x, y, w, h), pressed ? _pressedTex : _keyTex);
            if (_config.ShowKeyNames)
            {
                _style.normal.textColor = pressed ? Color.black : Color.white;
                GUI.Label(new Rect(x, y, w, h), label, _style);
            }
        }
        
        private Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1);
            t.SetPixel(0, 0, c);
            t.Apply();
            return t;
        }
        
        public void RefreshTextures() { _keyTex = null; _pressedTex = null; }
    }
}