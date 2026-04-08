using System;
using System.Collections.Generic;
using UnityEngine;
using Rewired;

namespace KeyOverlay
{
    public class InputMonitor
    {
        public Dictionary<string, KeyState> KeyStates { get; private set; }
        public int JumpCombo { get; private set; }
        public int ThrowCombo { get; private set; }
        public int GrabCombo { get; private set; }
        public int TotalInputs { get; private set; }
        public bool IsGamepadActive { get; private set; }
        
        private float _lastInputTime;
        private Rewired.Player _rewiredPlayer;
        private bool _rewiredInitialized = false;
        private int _rewiredInitAttempts = 0;
        
        // Rewired action names (Rain World uses these names)
        private static readonly string[] ACTION_NAMES = { "Up", "Down", "Left", "Right", "Jump", "PickUp", "Throw", "Grab" };
        
        public InputMonitor()
        {
            try
            {
                KeyStates = new Dictionary<string, KeyState>();
                
                // Initialize basic keys
                string[] keys = { "Up", "Down", "Left", "Right", "Jump", "Grab", "Throw", "PickUp" };
                foreach (var k in keys) KeyStates[k] = new KeyState(k);
                
                // Initialize gamepad keys
                string[] gpKeys = { "GP_A", "GP_B", "GP_X", "GP_Y", "GP_LeftStick" };
                foreach (var k in gpKeys) KeyStates[k] = new KeyState(k);
                
                KeyOverlayPlugin.Log?.LogInfo("[KeyOverlay] InputMonitor initialized");
            }
            catch (Exception ex)
            {
                KeyOverlayPlugin.Log?.LogError($"[KeyOverlay] InputMonitor init error: {ex.Message}");
            }
        }
        
        private void TryInitializeRewired()
        {
            if (_rewiredInitialized || _rewiredInitAttempts > 10) return;
            
            _rewiredInitAttempts++;
            
            try
            {
                // ReInput.players.GetPlayer(0) is the standard way to get the first player
                if (ReInput.players != null)
                {
                    var player = ReInput.players.GetPlayer(0);
                    if (player != null && player.controllers != null)
                    {
                        _rewiredPlayer = player;
                        _rewiredInitialized = true;
                        KeyOverlayPlugin.Log?.LogInfo($"[KeyOverlay] Rewired player '{player.name}' connected");
                    }
                }
                
                if (!_rewiredInitialized && _rewiredInitAttempts == 5)
                {
                    KeyOverlayPlugin.Log?.LogWarning("[KeyOverlay] Rewired not available after 5 attempts, using Unity Input fallback");
                }
            }
            catch (Exception ex)
            {
                if (_rewiredInitAttempts <= 3)
                    KeyOverlayPlugin.Log?.LogWarning($"[KeyOverlay] Rewired init attempt {_rewiredInitAttempts} failed: {ex.Message}");
            }
        }
        
        public void Update()
        {
            try
            {
                // Try to initialize Rewired (lazy init)
                TryInitializeRewired();
                
                // Detect gamepad
                var joysticks = Input.GetJoystickNames();
                IsGamepadActive = joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]);
                
                // Use Rewired if available, fallback to Unity Input
                if (_rewiredPlayer != null && _rewiredInitialized)
                {
                    UpdateWithRewired();
                }
                else
                {
                    UpdateWithUnityInput();
                }
                
                // Combo reset
                if (Time.time - _lastInputTime > 0.5f)
                {
                    JumpCombo = 0;
                    ThrowCombo = 0;
                    GrabCombo = 0;
                }
            }
            catch { }
        }
        
        private void UpdateWithRewired()
        {
            try
            {
                // Use Rewired's action-based input by name (more reliable than IDs)
                UpdateKey("Up", GetRewiredButton("Up"));
                UpdateKey("Down", GetRewiredButton("Down"));
                UpdateKey("Left", GetRewiredButton("Left"));
                UpdateKey("Right", GetRewiredButton("Right"));
                UpdateKey("Jump", GetRewiredButton("Jump"));
                UpdateKey("Grab", GetRewiredButton("Grab") || GetRewiredButton("PickUp"));
                UpdateKey("Throw", GetRewiredButton("Throw"));
                UpdateKey("PickUp", GetRewiredButton("PickUp"));
                
                // Gamepad detection via Rewired
                var joystick = _rewiredPlayer.controllers.GetController(ControllerType.Joystick, 0);
                if (joystick != null)
                {
                    UpdateKey("GP_A", joystick.GetButton(0));
                    UpdateKey("GP_B", joystick.GetButton(1));
                    UpdateKey("GP_X", joystick.GetButton(2));
                    UpdateKey("GP_Y", joystick.GetButton(3));
                }
            }
            catch { }
        }
        
        private bool GetRewiredButton(string actionName)
        {
            try
            {
                return _rewiredPlayer.GetButton(actionName);
            }
            catch
            {
                return false;
            }
        }
        
        private void UpdateWithUnityInput()
        {
            // Fallback to Unity Input when Rewired is not available
            // Keyboard movement
            UpdateKey("Left", Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow));
            UpdateKey("Right", Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow));
            UpdateKey("Up", Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow));
            UpdateKey("Down", Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow));
            
            // Keyboard actions - Rain World defaults
            UpdateKey("Jump", Input.GetKey(KeyCode.Space));
            UpdateKey("Grab", Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.X));
            UpdateKey("Throw", Input.GetKey(KeyCode.C));
            UpdateKey("PickUp", Input.GetKey(KeyCode.X));
            
            // Gamepad
            UpdateKey("GP_A", Input.GetKey(KeyCode.JoystickButton0));
            UpdateKey("GP_B", Input.GetKey(KeyCode.JoystickButton1));
            UpdateKey("GP_X", Input.GetKey(KeyCode.JoystickButton2));
            UpdateKey("GP_Y", Input.GetKey(KeyCode.JoystickButton3));
        }
        
        private void UpdateKey(string name, bool pressed)
        {
            if (!KeyStates.ContainsKey(name)) return;
            
            var state = KeyStates[name];
            bool wasPressed = state.IsPressed;
            state.Update(pressed);
            
            if (pressed && !wasPressed)
            {
                TotalInputs++;
                _lastInputTime = Time.time;
                
                if (name == "Jump" || name == "GP_A") JumpCombo++;
                else if (name == "Throw" || name == "GP_B") ThrowCombo++;
                else if (name == "Grab" || name == "GP_X") GrabCombo++;
            }
        }
        
        public KeyState GetKeyState(string name)
        {
            return KeyStates.ContainsKey(name) ? KeyStates[name] : null;
        }
        
        public string GetKeyBindingName(string actionName)
        {
            // Get the actual key binding name from Rewired
            if (_rewiredPlayer != null && _rewiredInitialized)
            {
                try
                {
                    // Get action by name
                    var action = ReInput.mapping.GetAction(actionName);
                    if (action != null)
                    {
                        // Get the first element assigned to this action
                        var elementMap = _rewiredPlayer.controllers.maps.GetFirstElementMapWithAction(ControllerType.Keyboard, action.id, true);
                        if (elementMap != null)
                        {
                            return FormatKeyName(elementMap.elementIdentifierName);
                        }
                    }
                }
                catch { }
            }
            
            // Fallback names
            return GetDefaultKeyName(actionName);
        }
        
        private string FormatKeyName(string rawName)
        {
            // Clean up key name for display
            if (string.IsNullOrEmpty(rawName)) return rawName;
            
            // Common key name formatting
            switch (rawName)
            {
                case "Space": return "SPC";
                case "Left Shift": return "LSH";
                case "Right Shift": return "RSH";
                case "Left Control": return "CTL";
                case "Right Control": return "CTL";
                default: 
                    // Shorten long names
                    if (rawName.Length > 4) return rawName.Substring(0, 4);
                    return rawName;
            }
        }
        
        private string GetDefaultKeyName(string name)
        {
            switch (name)
            {
                case "Up": return "W";
                case "Down": return "S";
                case "Left": return "A";
                case "Right": return "D";
                case "Jump": return "SPC";
                case "Grab": return "GRB";
                case "Throw": return "THR";
                default: return name;
            }
        }
        
        public void ResetStats()
        {
            JumpCombo = 0;
            ThrowCombo = 0;
            GrabCombo = 0;
            TotalInputs = 0;
        }
    }
    
    public class KeyState
    {
        public string Name { get; private set; }
        public bool IsPressed { get; private set; }
        public bool WasPressed { get; private set; }
        public float PressDuration { get; private set; }
        public Vector2 StickPosition { get; set; }
        
        public KeyState(string name)
        {
            Name = name;
            IsPressed = false;
            WasPressed = false;
            PressDuration = 0f;
            StickPosition = Vector2.zero;
        }
        
        public void Update(bool pressed)
        {
            WasPressed = IsPressed;
            IsPressed = pressed;
            PressDuration = pressed ? PressDuration + Time.deltaTime : 0f;
        }
    }
}