using UnityEngine;

namespace KeyOverlay
{
    public class KeyOverlayUI
    {
        private ConfigWrapper _config;
        private InputMonitor _input;
        private Texture2D _whiteTex;
        private GUIStyle _statsStyle;
        private bool _dragging;
        private Vector2 _dragOffset;
        
        // Pixel art arrow icons (Unicode)
        private const string ARROW_UP = "▲";
        private const string ARROW_DOWN = "▼";
        private const string ARROW_LEFT = "◄";
        private const string ARROW_RIGHT = "►";
        private const string ICON_JUMP = "●";  // Jump icon
        private const string ICON_THROW = "◆"; // Throw icon
        private const string ICON_GRAB = "■";  // Grab icon
        private const string ICON_PICKUP = "▲"; // PickUp icon (same as up arrow)
        
        public KeyOverlayUI(ConfigWrapper config, InputMonitor input)
        {
            _config = config;
            _input = input;
            
            _statsStyle = new GUIStyle
            {
                fontSize = 10,
                alignment = TextAnchor.MiddleLeft,
                normal = { textColor = new Color(0.6f, 0.6f, 0.6f) }
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
            
            _statsStyle.fontSize = Mathf.Max(8, _config.FontSize - 2);
            
            float x = _config.PanelX;
            float y = _config.PanelY;
            float s = _config.Scale;
            
            float keySize = 26 * s;
            float spacing = 2 * s;
            float borderWidth = _config.BorderWidth * s;
            
            // Movement keys (WASD) - using arrow icons
            if (_config.ShowMovementKeys && _config.ShowKeyboard)
            {
                float mx = x;
                float my = y;
                
                // W (Up)
                DrawKey(mx + keySize + spacing, my, keySize, borderWidth, "Up", ARROW_UP);
                
                // A S D
                DrawKey(mx, my + keySize + spacing, keySize, borderWidth, "Left", ARROW_LEFT);
                DrawKey(mx + keySize + spacing, my + keySize + spacing, keySize, borderWidth, "Down", ARROW_DOWN);
                DrawKey(mx + 2 * (keySize + spacing), my + keySize + spacing, keySize, borderWidth, "Right", ARROW_RIGHT);
            }
            
            // Action keys - Jump right of W, Grab above Throw, Throw right of D
            if (_config.ShowActionKeys && _config.ShowKeyboard)
            {
                // Jump - right of W (top row)
                float jumpX = x + 2 * (keySize + spacing);
                DrawKey(jumpX, y, keySize, borderWidth, "Jump", ICON_JUMP);
                
                // Grab - above Throw (top row, same column as Throw)
                float grabX = x + 3 * (keySize + spacing);
                DrawKey(grabX, y, keySize, borderWidth, "Grab", ICON_GRAB);
                
                // Throw - right of D (bottom row)
                float throwX = x + 3 * (keySize + spacing);
                DrawKey(throwX, y + keySize + spacing, keySize, borderWidth, "Throw", ICON_THROW);
            }
            
            // Combo stats
            if (_config.ShowComboStats)
            {
                float statsY = y + 2 * (keySize + spacing) + spacing / 2;
                GUI.color = new Color(1, 1, 1, _config.Opacity);
                GUI.Label(new Rect(x, statsY, 300 * s, 16 * s), 
                    $"CPS:{_input.CPS:F1} | JMP:{_input.JumpCombo} THR:{_input.ThrowCombo} GRB:{_input.GrabCombo}", _statsStyle);
                GUI.color = Color.white;
            }
            
            // Drag handling
            float totalWidth = 5 * (keySize + spacing);
            float totalHeight = 2.2f * (keySize + spacing);
            HandleDrag(x, y, totalWidth, totalHeight);
        }
        
        private void DrawKey(float x, float y, float size, float borderWidth, string keyName, string icon)
        {
            var state = _input.GetKeyState(keyName);
            bool pressed = state != null && state.IsPressed;
            
            float panelAlpha = _config.Opacity;
            
            // Fill color based on pressed state
            Color fillColor = pressed ? _config.KeyColorPressed : _config.KeyColorNormal;
            float fillAlpha = pressed ? _config.PressedEffectOpacity : _config.FillOpacity;
            fillAlpha *= panelAlpha;
            
            // Draw fill first (background)
            if (fillAlpha > 0)
            {
                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillAlpha);
                GUI.DrawTexture(new Rect(x, y, size, size), _whiteTex);
            }
            
            // Draw border lines (on top of fill)
            if (borderWidth > 0)
            {
                Color borderColor = _config.BorderColor;
                float borderAlpha = _config.BorderOpacity * panelAlpha;
                GUI.color = new Color(borderColor.r, borderColor.g, borderColor.b, borderAlpha);
                
                // Top border
                GUI.DrawTexture(new Rect(x, y, size, borderWidth), _whiteTex);
                // Bottom border
                GUI.DrawTexture(new Rect(x, y + size - borderWidth, size, borderWidth), _whiteTex);
                // Left border
                GUI.DrawTexture(new Rect(x, y, borderWidth, size), _whiteTex);
                // Right border
                GUI.DrawTexture(new Rect(x + size - borderWidth, y, borderWidth, size), _whiteTex);
            }
            
            // Icon text
            if (_config.ShowKeyNames)
            {
                Color textColor = pressed ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
                GUI.color = new Color(textColor.r, textColor.g, textColor.b, panelAlpha);
                
                // Use built-in label style for pixel icons
                var iconStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _config.FontSize,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = GUI.color }
                };
                GUI.Label(new Rect(x, y, size, size), icon, iconStyle);
            }
            
            GUI.color = Color.white;
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
        
        public void RefreshTextures() { }
    }
}