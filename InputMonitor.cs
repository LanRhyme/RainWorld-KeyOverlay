using System;
using System.Collections.Generic;
using UnityEngine;

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
        public float CPS { get; private set; } // Clicks Per Second
        
        private float _lastInputTime;
        private List<float> _inputTimes = new List<float>(); // For CPS calculation
        
        public InputMonitor()
        {
            KeyStates = new Dictionary<string, KeyState>();
            
            // Initialize basic keys
            string[] keys = { "Up", "Down", "Left", "Right", "Jump", "Grab", "Throw" };
            foreach (var k in keys) KeyStates[k] = new KeyState(k);
            
            // Initialize gamepad keys
            string[] gpKeys = { "GP_A", "GP_B", "GP_X", "GP_Y", "GP_LeftStick" };
            foreach (var k in gpKeys) KeyStates[k] = new KeyState(k);
            
            KeyOverlayPlugin.Log?.LogInfo("[KeyOverlay] InputMonitor initialized");
        }
        
        public void Update()
        {
            try
            {
                // Detect gamepad
                var joysticks = Input.GetJoystickNames();
                IsGamepadActive = joysticks.Length > 0 && !string.IsNullOrEmpty(joysticks[0]);
                
                // Check Unity Input
                UpdateInputsCombined();
                
                // Calculate CPS (inputs per second over last 1 second)
                float now = Time.time;
                _inputTimes.RemoveAll(t => now - t > 1f);
                CPS = _inputTimes.Count;
                
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
        
        private void UpdateInputsCombined()
        {
            // Get configured key bindings from ConfigWrapper
            var cfg = ConfigWrapper.Instance;
            
            // Unity Input - use configured keys + fallback to arrow keys for movement
            bool up, down, left, right, jump, grab, throw_;
            
            if (cfg != null)
            {
                // Movement: configured key OR arrow keys OR WASD (multiple fallbacks)
                up = Input.GetKey(cfg.KeyUp) || Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W);
                down = Input.GetKey(cfg.KeyDown) || Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S);
                left = Input.GetKey(cfg.KeyLeft) || Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A);
                right = Input.GetKey(cfg.KeyRight) || Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D);
                
                // Actions: configured key OR common alternatives
                jump = Input.GetKey(cfg.KeyJump) || Input.GetKey(KeyCode.Space);
                grab = Input.GetKey(cfg.KeyGrab) || Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.LeftShift);
                throw_ = Input.GetKey(cfg.KeyThrow) || Input.GetKey(KeyCode.C);
            }
else
            {
                // Fallback hardcoded - WASD + arrow keys + common actions
                up = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
                down = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
                left = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
                right = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
                jump = Input.GetKey(KeyCode.Space);
                grab = Input.GetKey(KeyCode.L) || Input.GetKey(KeyCode.LeftShift);
                throw_ = Input.GetKey(KeyCode.K) || Input.GetKey(KeyCode.C);
            }
            
            UpdateKey("Up", up);
            UpdateKey("Down", down);
            UpdateKey("Left", left);
            UpdateKey("Right", right);
            UpdateKey("Jump", jump);
            UpdateKey("Grab", grab);
            UpdateKey("Throw", throw_);
            
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
                _inputTimes.Add(Time.time); // For CPS calculation
                
                if (name == "Jump" || name == "GP_A") JumpCombo++;
                else if (name == "Throw" || name == "GP_B") ThrowCombo++;
                else if (name == "Grab" || name == "GP_X") GrabCombo++;
            }
        }
        
        public KeyState GetKeyState(string name)
        {
            return KeyStates.ContainsKey(name) ? KeyStates[name] : null;
        }
        
        public void ResetStats()
        {
            JumpCombo = 0;
            ThrowCombo = 0;
            GrabCombo = 0;
            TotalInputs = 0;
            _inputTimes.Clear();
            CPS = 0;
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