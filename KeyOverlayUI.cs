using UnityEngine;

namespace KeyOverlay
{
    public class KeyOverlayUI
    {
        private ConfigWrapper _config;
        private InputMonitor _input;
        private Texture2D _whiteTex;  // Single white texture for color control
        private GUIStyle _keyLabelStyle;
        private GUIStyle _statsStyle;
        private bool _dragging;
        private Vector2 _dragOffset;
        
        // Font lookup - Rain World uses a pixel font
        private Font _pixelFont;
        
        public KeyOverlayUI(ConfigWrapper config, InputMonitor input)
        {
            _config = config;
            _input = input;
            
            // Try to load Rain World's pixel font or fallback to built-in
            _pixelFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            _keyLabelStyle = new GUIStyle 
            { 
                font = _pixelFont,
                fontSize = 11, 
                alignment = TextAnchor.MiddleCenter, 
                normal = { textColor = Color.white },
                fontStyle = FontStyle.Bold
            };
            
            _statsStyle = new GUIStyle
            {
                font = _pixelFont,
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.7f, 0.7f, 0.7f) }
            };
        }
        
        public void OnGUI()
        {
            if (_whiteTex == null)
            {
                _whiteTex = new Texture2D(1, 1);
                _whiteTex.SetPixel(0, 0, Color.white);
                _whiteTex.Apply();
            }
            
            float x = _config.PanelX;
            float y = _config.PanelY;
            float s = _config.Scale;
            
            // Compact layout - smaller keys, tighter spacing
            float keySize = 28 * s;
            float spacing = 3 * s;
            float borderWidth = 2 * s;
            
            // Movement keys (WASD layout - compact)
            if (_config.ShowMovementKeys && _config.ShowKeyboard)
            {
                float mx = x;
                float my = y;
                
                // W (Up) - center above middle row
                DrawKey(mx + keySize + spacing, my, keySize, borderWidth, "Up", _input.GetKeyBindingName("Up"));
                
                // A S D (Left, Down, Right) - middle row
                DrawKey(mx, my + keySize + spacing, keySize, borderWidth, "Left", _input.GetKeyBindingName("Left"));
                DrawKey(mx + keySize + spacing, my + keySize + spacing, keySize, borderWidth, "Down", _input.GetKeyBindingName("Down"));
                DrawKey(mx + 2 * (keySize + spacing), my + keySize + spacing, keySize, borderWidth, "Right", _input.GetKeyBindingName("Right"));
            }
            
            // Action keys (Jump, Grab, Throw) - more compact
            if (_config.ShowActionKeys && _config.ShowKeyboard)
            {
                float ax = x + 3.5f * (keySize + spacing);
                float ay = y + spacing;
                
                // Jump on top, Throw next to it
                DrawKey(ax, ay, keySize, borderWidth, "Jump", _input.GetKeyBindingName("Jump"));
                DrawKey(ax + keySize + spacing, ay, keySize, borderWidth, "Throw", _input.GetKeyBindingName("Throw"));
                
                // Grab below Jump
                DrawKey(ax, ay + keySize + spacing, keySize, borderWidth, "Grab", _input.GetKeyBindingName("Grab"));
            }
            
            // Combo stats - compact line
            if (_config.ShowComboStats)
            {
                float statsY = y + 2 * (keySize + spacing) + spacing;
                GUI.color = new Color(1, 1, 1, _config.Opacity);
                _statsStyle.normal.textColor = new Color(0.6f, 0.6f, 0.6f);
                GUI.Label(new Rect(x, statsY, 200 * s, 18 * s), 
                    $"JMP:{_input.JumpCombo}  THR:{_input.ThrowCombo}  GRB:{_input.GrabCombo}", _statsStyle);
                GUI.color = Color.white;
            }
            
            // Drag handling - adjust bounds for compact layout
            float totalWidth = 5 * (keySize + spacing);
            float totalHeight = 2.3f * (keySize + spacing);
            HandleDrag(x, y, totalWidth, totalHeight);
        }
        
        private void DrawKey(float x, float y, float size, float borderWidth, string keyName, string label)
        {
            var state = _input.GetKeyState(keyName);
            bool pressed = state != null && state.IsPressed;
            
            // Single panel alpha for overall transparency
            float panelAlpha = _config.Opacity;
            
            // Determine colors based on pressed state
            Color fillColor;
            Color borderColor;
            float fillAlpha;
            
            if (pressed)
            {
                fillColor = _config.KeyColorPressed;
                borderColor = _config.KeyColorPressed * 0.7f;  // Slightly darker border when pressed
                fillAlpha = _config.PressedEffectOpacity * panelAlpha;
            }
            else
            {
                fillColor = _config.KeyColorNormal;
                borderColor = _config.BorderColor;
                fillAlpha = _config.FillOpacity * panelAlpha;
            }
            
            // Border (outer rectangle)
            GUI.color = new Color(borderColor.r, borderColor.g, borderColor.b, _config.BorderOpacity * panelAlpha);
            GUI.DrawTexture(new Rect(x, y, size, size), _whiteTex);
            
            // Fill (inner rectangle, offset by border width)
            GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillAlpha);
            GUI.DrawTexture(new Rect(x + borderWidth, y + borderWidth, size - 2 * borderWidth, size - 2 * borderWidth), _whiteTex);
            
            // Key label text
            if (_config.ShowKeyNames && !string.IsNullOrEmpty(label))
            {
                // Text color based on contrast with fill
                Color textColor = GetContrastTextColor(fillColor, pressed);
                GUI.color = new Color(textColor.r, textColor.g, textColor.b, panelAlpha);
                _keyLabelStyle.normal.textColor = textColor;
                GUI.Label(new Rect(x, y, size, size), label, _keyLabelStyle);
            }
            
            // Reset GUI color
            GUI.color = Color.white;
        }
        
        private Color GetContrastTextColor(Color bgColor, bool pressed)
        {
            // Simple contrast calculation - use white for dark backgrounds, black for light
            float brightness = (bgColor.r + bgColor.g + bgColor.b) / 3f;
            
            if (pressed)
            {
                // When pressed (usually yellow/orange), use black text
                return new Color(0.1f, 0.1f, 0.1f);
            }
            else
            {
                // When not pressed, use white text for dark keys
                return brightness < 0.5f ? Color.white : new Color(0.2f, 0.2f, 0.2f);
            }
        }
        
        private void HandleDrag(float x, float y, float w, float h)
        {
            var e = Event.current;
            var rect = new Rect(x, y, w, h);
            
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
        
        public void RefreshTextures() 
        { 
            // No longer needed with simple white texture approach
        }
    }
}