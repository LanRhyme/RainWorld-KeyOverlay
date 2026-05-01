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
        
        // Joystick indicator state
        private Vector2 _joystickPos = Vector2.zero; // Current joystick position (-1 to 1)
        private Vector2 _joystickSmoothPos = Vector2.zero; // Smoothed position
        private float _joystickSmoothSpeed = 12f; // Smoothing speed
        
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
            
            // Joystick indicator (right side of keys - no overlap)
            if (_config.ShowKeyboard && _config.ShowJoystick)
            {
                // Calculate position: right edge of keys + spacing + half joystick radius
                float keyPanelRightEdge = x + 3 * (keySize + spacing) + keySize;
                float joystickRadius = keySize * 0.9f;
                float joystickX = keyPanelRightEdge + spacing * 2 + joystickRadius;
                
                // Vertically centered with key panel (2 rows)
                float keyPanelHeight = 2 * keySize + spacing;
                float joystickY = y + keyPanelHeight / 2;
                
                float joystickSize = keySize * 1.8f;
                DrawJoystickIndicator(joystickX, joystickY, joystickSize, s);
            }
            
            // Combo stats
            if (_config.ShowComboStats)
            {
                float statsY = y + 2 * (keySize + spacing) + spacing / 2;
                GUI.color = new Color(1, 1, 1, _config.Opacity);
                GUI.Label(new Rect(x, statsY, 300 * s, 16 * s), 
                    $"{Localization.Get("CPS")}:{_input.CPS:F1} | {Localization.Get("JMP")}:{_input.JumpCombo} {Localization.Get("THR")}:{_input.ThrowCombo} {Localization.Get("GRB")}:{_input.GrabCombo}", _statsStyle);
                GUI.color = Color.white;
            }
            
            // Drag handling
            float totalWidth = 7 * (keySize + spacing);
            float totalHeight = 2.2f * (keySize + spacing);
            HandleDrag(x, y, totalWidth, totalHeight);
        }
        
        private void DrawJoystickIndicator(float cx, float cy, float size, float scale)
        {
            float panelAlpha = _config.Opacity;
            int style = _config.OverlayStyle;
            
            // Get movement input state (using user's configured key bindings)
            bool up = _input.IsKeyPressed("Up");
            bool down = _input.IsKeyPressed("Down");
            bool left = _input.IsKeyPressed("Left");
            bool right = _input.IsKeyPressed("Right");
            
            // Calculate target joystick position (-1 to 1)
            Vector2 targetPos = Vector2.zero;
            if (up) targetPos.y -= 1;
            if (down) targetPos.y += 1;
            if (left) targetPos.x -= 1;
            if (right) targetPos.x += 1;
            
            if (targetPos.magnitude > 1f)
                targetPos.Normalize();
            
            // Smooth interpolation
            _joystickSmoothPos = Vector2.Lerp(_joystickSmoothPos, targetPos, Time.deltaTime * _joystickSmoothSpeed);
            
            // Draw outer circle (background)
            float outerRadius = size / 2;
            
            Color borderColor = _config.BorderColor;
            float borderAlpha = _config.BorderOpacity * panelAlpha;
            if (style == 3) borderAlpha = 0; // Ghost
            Color borderCol = new Color(borderColor.r, borderColor.g, borderColor.b, borderAlpha);
            
            Color fillColor = _config.KeyColorNormal;
            float fillAlpha = _config.FillOpacity * panelAlpha;
            if (style == 2 && !(up || down || left || right)) fillAlpha = 0; // Minimalist
            Color fillCol = new Color(fillColor.r, fillColor.g, fillColor.b, fillAlpha);
            
            if (style == 0) // Classic Pixel
            {
                float pixelSize = 2 * scale;
                DrawPixelCircle(cx, cy, outerRadius, pixelSize, borderCol, fillCol, true);
            }
            else // Modern/Minimalist/Ghost (Smoother)
            {
                // We don't have a circle texture, so we use the DrawPixelCircle but with smaller "pixel" size for smoothness
                DrawPixelCircle(cx, cy, outerRadius, scale * 0.5f, borderCol, fillCol, true);
            }
            
            // Draw inner circle (joystick position indicator)
            float innerRadius = size / 5;
            float innerOffset = outerRadius - innerRadius - 2 * scale;
            float innerCx = cx + _joystickSmoothPos.x * innerOffset;
            float innerCy = cy + _joystickSmoothPos.y * innerOffset;
            
            // Inner circle color (highlighted when moving)
            Color innerCol = (up || down || left || right) ? _config.KeyColorPressed : borderColor;
            float innerAlpha = (up || down || left || right) ? _config.PressedEffectOpacity : borderAlpha;
            if (style == 3 && !(up || down || left || right)) innerAlpha = 0.3f * panelAlpha; // Ghost subtle inner
            
            Color innerBorderCol = new Color(innerCol.r, innerCol.g, innerCol.b, innerAlpha);
            Color innerFillCol = new Color(innerCol.r, innerCol.g, innerCol.b, innerAlpha * 0.8f);
            
            if (style == 0)
            {
                DrawPixelCircle(innerCx, innerCy, innerRadius, 2 * scale, innerBorderCol, innerFillCol, false);
            }
            else
            {
                DrawPixelCircle(innerCx, innerCy, innerRadius, scale * 0.5f, innerBorderCol, innerFillCol, false);
            }
            
            GUI.color = Color.white;
        }
        
        private void DrawPixelCircle(float cx, float cy, float radius, float pixelSize, Color borderColor, Color fillColor, bool fill)
        {
            int r = Mathf.RoundToInt(radius / pixelSize);
            int cxInt = Mathf.RoundToInt(cx / pixelSize);
            int cyInt = Mathf.RoundToInt(cy / pixelSize);
            
            if (fill)
            {
                GUI.color = fillColor;
                for (int py = -r; py <= r; py++)
                {
                    int rowWidth = Mathf.RoundToInt(Mathf.Sqrt(r * r - py * py));
                    if (rowWidth > 0)
                    {
                        float drawX = (cxInt - rowWidth) * pixelSize;
                        float drawY = (cyInt + py) * pixelSize;
                        float width = (2 * rowWidth + 1) * pixelSize;
                        GUI.DrawTexture(new Rect(drawX, drawY, width, pixelSize), _whiteTex);
                    }
                }
            }
            
            GUI.color = borderColor;
            DrawPixelCircleBorder(cxInt, cyInt, r, pixelSize);
        }
        
        private void DrawPixelCircleBorder(int cx, int cy, int r, float pixelSize)
        {
            int x = 0;
            int y = r;
            int d = 3 - 2 * r;
            
            while (x <= y)
            {
                DrawBorderPixels(cx, cy, x, y, pixelSize);
                
                if (d < 0)
                {
                    d = d + 4 * x + 6;
                }
                else
                {
                    d = d + 4 * (x - y) + 10;
                    y--;
                }
                x++;
            }
        }
        
        private void DrawBorderPixels(int cx, int cy, int x, int y, float pixelSize)
        {
            DrawPixel(cx + x, cy + y, pixelSize);
            DrawPixel(cx - x, cy + y, pixelSize);
            DrawPixel(cx + x, cy - y, pixelSize);
            DrawPixel(cx - x, cy - y, pixelSize);
            DrawPixel(cx + y, cy + x, pixelSize);
            DrawPixel(cx - y, cy + x, pixelSize);
            DrawPixel(cx + y, cy - x, pixelSize);
            DrawPixel(cx - y, cy - x, pixelSize);
        }
        
        private void DrawPixel(int px, int py, float pixelSize)
        {
            GUI.DrawTexture(new Rect(px * pixelSize, py * pixelSize, pixelSize, pixelSize), _whiteTex);
        }
        
        private void DrawKey(float x, float y, float size, float borderWidth, string keyName, string icon)
        {
            var state = _input.GetKeyState(keyName);
            bool pressed = state != null && state.IsPressed;
            
            float panelAlpha = _config.Opacity;
            int style = _config.OverlayStyle; // 0=Classic, 1=Minimalist, 2=Ghost
            
            // Fill color based on pressed state
            Color fillColor = pressed ? _config.KeyColorPressed : _config.KeyColorNormal;
            float fillAlpha = pressed ? _config.PressedEffectOpacity : _config.FillOpacity;
            
            // Adjust transparency for styles
            if (style == 1) // Minimalist
            {
                if (!pressed) fillAlpha = 0;
            }
            else if (style == 2) // Ghost
            {
                fillAlpha *= pressed ? 0.6f : 0.2f;
            }
            
            fillAlpha *= panelAlpha;
            
            // Draw fill (background)
            if (fillAlpha > 0)
            {
                GUI.color = new Color(fillColor.r, fillColor.g, fillColor.b, fillAlpha);
                GUI.DrawTexture(new Rect(x, y, size, size), _whiteTex);
            }
            
            // Draw border
            float effectiveBorderWidth = borderWidth;
            if (style == 1) effectiveBorderWidth = Mathf.Max(1f, borderWidth * 0.5f); // Thinner for minimalist
            if (style == 2) effectiveBorderWidth = 0; // No border for ghost
            
            if (effectiveBorderWidth > 0)
            {
                Color borderColor = _config.BorderColor;
                float borderAlpha = _config.BorderOpacity * panelAlpha;
                if (style == 1 && !pressed) borderAlpha *= 0.7f; // Dimmer border for minimalist
                
                GUI.color = new Color(borderColor.r, borderColor.g, borderColor.b, borderAlpha);
                
                // Top border
                GUI.DrawTexture(new Rect(x, y, size, effectiveBorderWidth), _whiteTex);
                // Bottom border
                GUI.DrawTexture(new Rect(x, y + size - effectiveBorderWidth, size, effectiveBorderWidth), _whiteTex);
                // Left border
                GUI.DrawTexture(new Rect(x, y, effectiveBorderWidth, size), _whiteTex);
                // Right border
                GUI.DrawTexture(new Rect(x + size - effectiveBorderWidth, y, effectiveBorderWidth, size), _whiteTex);
            }
            
            // Icon text
            if (_config.ShowKeyNames)
            {
                Color textColor = pressed ? new Color(0.1f, 0.1f, 0.1f) : Color.white;
                if (style >= 1 && fillColor.grayscale < 0.5f) textColor = Color.white;
                
                GUI.color = new Color(textColor.r, textColor.g, textColor.b, panelAlpha);
                
                var iconStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = _config.FontSize,
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = GUI.color }
                };
                
                string displayText = _config.GetKeyDisplayName(keyName);
                GUI.Label(new Rect(x, y, size, size), displayText, iconStyle);
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