using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace FedoraEngine.Engine.Input
{
    public static class Input
    {
        private static KeyboardState _prevKState;
        private static KeyboardState _kState;

        private static MouseState _prevMouseStates;
        private static MouseState _mouseState;

        public static MouseState CurrentMouseState
        {
            get { return _mouseState; }
        }

        public static Vector2 ScreenMousePosition
        {
            get
            {
                return CurrentMouseState.Position.ToVector2();
            }
        }

        public static Vector2 WorldMousePosition
        {
            get
            {
                return Core.Scene.MainCamera.ScreenToWorld(ScreenMousePosition);
            }
        }

        public static Dictionary<string, Keys[]> KeyMap = new Dictionary<string, Keys[]>
        {
            { "quit", new Keys[] { Keys.Escape, } },
            { "reloadScene", new Keys[] { Keys.Tab, Keys.R, } },
            { "pauseScene", new Keys[] { Keys.RightControl, } },
            { "toggleFullScreen", new Keys[] { Keys.F11, } },
            { "toggleDebugCollisions", new Keys[] { Keys.LeftAlt, } },
            { "debugCameraSprint", new Keys[] { Keys.LeftShift, } },
            { "toggleDebugCamera", new Keys[] { Keys.C, } },
            { "toggleImGui", new Keys[] { Keys.OemTilde, } },
            { "stepForwardOneFrame", new Keys[] { Keys.RightAlt, } },
        };

        public static void UpdateState()
        {
            _prevKState = _kState;
            _kState = Keyboard.GetState();

            _prevMouseStates = _mouseState;
            _mouseState = Mouse.GetState();
        }

        public static bool IsKeyDown(Keys key)
        {
            if (_kState.IsKeyDown(key))
                return true;
            return false;
        }

        public static bool IsKeyUp(Keys key)
        {
            if (_kState.IsKeyUp(key))
                return true;
            return false;
        }

        public static bool IsKeyPressed(Keys key)
        {
            if (_kState.IsKeyDown(key) && !_prevKState.IsKeyDown(key))
                return true;
            return false;
        }

        public static bool IsKeyReleased(Keys key)
        {
            if (!_kState.IsKeyDown(key) && _prevKState.IsKeyDown(key))
                return true;
            return false;
        }

        public static bool IsKeyDown(Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (_kState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        public static bool IsKeyUp(Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (_kState.IsKeyUp(key))
                    return true;
            }
            return false;
        }

        public static bool IsKeyPressed(Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (_kState.IsKeyDown(key) && !_prevKState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        public static bool IsKeyReleased(Keys[] keys)
        {
            foreach (var key in keys)
            {
                if (!_kState.IsKeyDown(key) && _prevKState.IsKeyDown(key))
                    return true;
            }
            return false;
        }

        public static bool IsLeftMousePressed()
        {
            return _mouseState.LeftButton == ButtonState.Pressed && _prevMouseStates.LeftButton != ButtonState.Pressed;
        }

        public static bool IsLeftMouseReleased()
        {
            return _mouseState.LeftButton == ButtonState.Released && _prevMouseStates.LeftButton == ButtonState.Pressed;
        }

        public static bool IsRightMousePressed()
        {
            return _mouseState.RightButton == ButtonState.Pressed && _prevMouseStates.RightButton != ButtonState.Pressed;
        }

        public static bool IsRightMouseReleased()
        {
            return _mouseState.RightButton == ButtonState.Released && _prevMouseStates.RightButton == ButtonState.Pressed;
        }

        public static bool IsLeftMouseDown()
        {
            return _mouseState.LeftButton == ButtonState.Pressed;
        }

        public static bool IsLeftMouseUp()
        {
            return _mouseState.LeftButton == ButtonState.Released;
        }

        public static bool IsRightMouseDown()
        {
            return _mouseState.RightButton == ButtonState.Pressed;
        }

        public static bool IsRightMouseUp()
        {
            return _mouseState.RightButton == ButtonState.Released;
        }

        public static bool IsMiddleMousePressed()
        {
            return _mouseState.MiddleButton == ButtonState.Pressed && _prevMouseStates.MiddleButton != ButtonState.Pressed;
        }

        public static bool IsMiddleMouseReleased()
        {
            return _mouseState.MiddleButton == ButtonState.Released && _prevMouseStates.MiddleButton != ButtonState.Released;
        }

        public static bool IsMiddleMouseDown()
        {
            return _mouseState.MiddleButton == ButtonState.Pressed;
        }

        public static bool IsMiddleMouseUp()
        {
            return _mouseState.MiddleButton == ButtonState.Released;
        }
    }
}
