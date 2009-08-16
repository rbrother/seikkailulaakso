#region File Description

//-----------------------------------------------------------------------------
// InputState.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------

#endregion

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Net.Brotherus
{
    /// <summary>
    /// Helper for reading input from keyboard and gamepad. This class tracks both
    /// the current and previous state of both input devices, and implements query
    /// properties for high level input actions such as "move up through the menu"
    /// or "pause the game".
    /// </summary>
    public class InputState
    {
        public GamePadState CurrentGamePadState;
        public KeyboardState CurrentKeyboardState;
        public MouseState CurrentMouseState;

        public GamePadState LastGamePadState;
        public KeyboardState LastKeyboardState;
        public MouseState LastMouseState;

        /// <summary>
        /// Reads the latest state of the keyboard and gamepad.
        /// </summary>
        public void Update()
        {
            LastKeyboardState = CurrentKeyboardState;
            LastGamePadState = CurrentGamePadState;
            LastMouseState = CurrentMouseState;
            CurrentKeyboardState = Keyboard.GetState();
            CurrentGamePadState = GamePad.GetState(PlayerIndex.One);
            CurrentMouseState = Mouse.GetState();
        }

        /// <summary>
        /// Helper for checking if a key was newly pressed during this update.
        /// </summary>
        public bool IsNewKeyPress(Keys key)
        {
            return (CurrentKeyboardState.IsKeyDown(key) && LastKeyboardState.IsKeyUp(key));
        }

        public bool IsKeyLifted(Keys key)
        {
            return (CurrentKeyboardState.IsKeyUp(key) && LastKeyboardState.IsKeyDown(key));
        }
    }
}