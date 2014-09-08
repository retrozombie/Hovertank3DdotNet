/* 
 * Copyright (C) 2014 Andy Stewart
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License along
 * with this program; if not, write to the Free Software Foundation, Inc.,
 * 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 */

using System;
using OpenTK;
using OpenTK.Input;

namespace Hovertank3DdotNet.OpenTK
{
    /// <summary>The OpenTK input system implementation.</summary>
    class OpenTKInputSystem : InputSystem
    {
        /// <summary>Creates a OpenTKInputSystem.</summary>
        /// <param name="gameWindow">The game window.</param>
        public OpenTKInputSystem(GameWindow gameWindow)
        {
            gameWindow.KeyDown += gameWindow_KeyDown;
            gameWindow.KeyUp += gameWindow_KeyUp;

            // From idlibc.c
            _keyToScanCode = new int[((int) Key.LastKey) + 1];
            _keyToScanCode[(int) Key.Escape] = 0x01;
            _keyToScanCode[(int) Key.Space] = 0x39;
            _keyToScanCode[(int) Key.Back] = 0x0e;
            _keyToScanCode[(int) Key.Tab] = 0x0f;
            _keyToScanCode[(int) Key.ControlLeft] = 0x1d;
            _keyToScanCode[(int) Key.ShiftLeft] = 0x2a;
            _keyToScanCode[(int) Key.CapsLock] = 0x3a;
            _keyToScanCode[(int) Key.F1] = 0x3b;
            _keyToScanCode[(int) Key.F2] = 0x3c;
            _keyToScanCode[(int) Key.F3] = 0x3d;
            _keyToScanCode[(int) Key.F4] = 0x3e;
            _keyToScanCode[(int) Key.F5] = 0x3f;
            _keyToScanCode[(int) Key.F6] = 0x40;
            _keyToScanCode[(int) Key.F7] = 0x41;
            _keyToScanCode[(int) Key.F8] = 0x42;
            _keyToScanCode[(int) Key.F9] = 0x43;
            _keyToScanCode[(int) Key.F10] = 0x44;
            _keyToScanCode[(int) Key.F11] = 0x57;
            _keyToScanCode[(int) Key.F12] = 0x58;
            _keyToScanCode[(int) Key.ScrollLock] = 0x46;
            _keyToScanCode[(int) Key.Enter] = 0x1c;
            _keyToScanCode[(int) Key.ShiftRight] = 0x36;
            _keyToScanCode[(int) Key.PrintScreen] = 0x37;
            _keyToScanCode[(int) Key.AltLeft] = 0x38;
            _keyToScanCode[(int) Key.Home] = 0x47;
            _keyToScanCode[(int) Key.PageUp] = 0x49;
            _keyToScanCode[(int) Key.End] = 0x4f;
            _keyToScanCode[(int) Key.PageDown] = 0x51;
            _keyToScanCode[(int) Key.Insert] = 0x52;
            _keyToScanCode[(int) Key.Delete] = 0x53;
            _keyToScanCode[(int) Key.NumLock] = 0x45;
            _keyToScanCode[(int) Key.Up] = 0x48;
            _keyToScanCode[(int) Key.Down] = 0x50;
            _keyToScanCode[(int) Key.Left] = 0x4b;
            _keyToScanCode[(int) Key.Right] = 0x4d;
            _keyToScanCode[(int) Key.Number1] = 0x02;
            _keyToScanCode[(int) Key.Number2] = 0x03;
            _keyToScanCode[(int) Key.Number3] = 0x04;
            _keyToScanCode[(int) Key.Number4] = 0x05;
            _keyToScanCode[(int) Key.Number5] = 0x06;
            _keyToScanCode[(int) Key.Number6] = 0x07;
            _keyToScanCode[(int) Key.Number7] = 0x08;
            _keyToScanCode[(int) Key.Number8] = 0x09;
            _keyToScanCode[(int) Key.Number9] = 0x0a;
            _keyToScanCode[(int) Key.Number0] = 0x0b;
            _keyToScanCode[(int) Key.Minus] = 0x0c;
            _keyToScanCode[(int) Key.Plus] = 0x0d;
            _keyToScanCode[(int) Key.Q] = 0x10;
            _keyToScanCode[(int) Key.W] = 0x11;
            _keyToScanCode[(int) Key.E] = 0x12;
            _keyToScanCode[(int) Key.R] = 0x13;
            _keyToScanCode[(int) Key.T] = 0x14;
            _keyToScanCode[(int) Key.Y] = 0x15;
            _keyToScanCode[(int) Key.U] = 0x16;
            _keyToScanCode[(int) Key.I] = 0x17;
            _keyToScanCode[(int) Key.O] = 0x18;
            _keyToScanCode[(int) Key.P] = 0x19;
            _keyToScanCode[(int) Key.BracketLeft] = 0x1a;
            _keyToScanCode[(int) Key.BracketRight] = 0x1b;
            //_keyToScanCode[(int) Key.Pipe] = 0x1c; // TODO
            _keyToScanCode[(int) Key.A] = 0x1e;
            _keyToScanCode[(int) Key.S] = 0x1f;
            _keyToScanCode[(int) Key.D] = 0x20;
            _keyToScanCode[(int) Key.F] = 0x21;
            _keyToScanCode[(int) Key.G] = 0x22;
            _keyToScanCode[(int) Key.H] = 0x23;
            _keyToScanCode[(int) Key.J] = 0x24;
            _keyToScanCode[(int) Key.K] = 0x25;
            _keyToScanCode[(int) Key.L] = 0x26;
            _keyToScanCode[(int) Key.Semicolon] = 0x27;
            _keyToScanCode[(int) Key.Quote] = 0x28;
            _keyToScanCode[(int) Key.Z] = 0x2c;
            _keyToScanCode[(int) Key.X] = 0x2d;
            _keyToScanCode[(int) Key.C] = 0x2e;
            _keyToScanCode[(int) Key.V] = 0x2f;
            _keyToScanCode[(int) Key.B] = 0x30;
            _keyToScanCode[(int) Key.N] = 0x31;
            _keyToScanCode[(int) Key.M] = 0x32;
            _keyToScanCode[(int) Key.Comma] = 0x33;
            _keyToScanCode[(int) Key.Period] = 0x34;
            // see idlibc.chartable for other keys
        }

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
        }

        /// <summary>Maps a Keys value to a scan code.</summary>
        private int[] _keyToScanCode;

        /// <summary>Returns equivalent the scan code for the specified key.</summary>
        /// <param name="key">A Key.</param>
        /// <returns>The scan code or zero.</returns>
        private int KeyToScanCode(Key key)
        {
            int scanCode = 0;

            if((int) key < _keyToScanCode.Length)
                scanCode = _keyToScanCode[(int) key];

            return scanCode;
        }

        /// <summary>Handles the game window's KeyDown event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            int scanCode = KeyToScanCode(e.Key);

            if(scanCode != 0)
                _keyDown[scanCode] = true;
        }

        /// <summary>Handles the game window's KeyUp event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_KeyUp(object sender, KeyboardKeyEventArgs e)
        {
            int scanCode = KeyToScanCode(e.Key);

            if(scanCode != 0)
                _keyDown[scanCode] = false;
        }

        /// <summary>Updates input.</summary>
        public void Update()
        {
            JoystickState joystickState = Joystick.GetState(ControllerIndex);
            _joystickConnected = joystickState.IsConnected;
            if(_joystickConnected)
            {
                _joystickXAxis = joystickState.GetAxis(JoystickAxis.Axis0);
                _joystickYAxis = -joystickState.GetAxis(JoystickAxis.Axis1);

                _joystickButtons = 0;

                if(joystickState.GetButton(JoystickButton.Button0) == ButtonState.Pressed)
                    _joystickButtons |= 1;

                if(joystickState.GetButton(JoystickButton.Button1) == ButtonState.Pressed)
                    _joystickButtons |= 2;
            }
        }

    }
}
