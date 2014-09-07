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
using System.Windows.Forms;

namespace Hovertank3DdotNet.WinForms
{
    /// <summary>WinForms implementation of InputSystem.</summary>
    class WinFormsInputSystem : InputSystem
    {
        /// <summary>Creates a new WinFormsInputSystem.</summary>
        /// <param name="form">The form that provides input events.</param>
        public WinFormsInputSystem(Form form)
        {
            // From idlibc.c
            _keyToScanCode = new int[256];
            _keyToScanCode[(int) Keys.Escape] = 0x01;
            _keyToScanCode[(int) Keys.Space] = 0x39;
            _keyToScanCode[(int) Keys.Back] = 0x0e;
            _keyToScanCode[(int) Keys.Tab] = 0x0f;
            _keyToScanCode[(int) Keys.ControlKey] = 0x1d;
            _keyToScanCode[(int) Keys.LShiftKey] = 0x2a;
            _keyToScanCode[(int) Keys.CapsLock] = 0x3a;
            _keyToScanCode[(int) Keys.F1] = 0x3b;
            _keyToScanCode[(int) Keys.F2] = 0x3c;
            _keyToScanCode[(int) Keys.F3] = 0x3d;
            _keyToScanCode[(int) Keys.F4] = 0x3e;
            _keyToScanCode[(int) Keys.F5] = 0x3f;
            _keyToScanCode[(int) Keys.F6] = 0x40;
            _keyToScanCode[(int) Keys.F7] = 0x41;
            _keyToScanCode[(int) Keys.F8] = 0x42;
            _keyToScanCode[(int) Keys.F9] = 0x43;
            _keyToScanCode[(int) Keys.F10] = 0x44;
            _keyToScanCode[(int) Keys.F11] = 0x57;
            _keyToScanCode[(int) Keys.F12] = 0x58;
            _keyToScanCode[(int) Keys.Scroll] = 0x46;
            _keyToScanCode[(int) Keys.Enter] = 0x1c;
            _keyToScanCode[(int) Keys.RShiftKey] = 0x36;
            _keyToScanCode[(int) Keys.PrintScreen] = 0x37;
            _keyToScanCode[(int) Keys.Menu] = 0x38;
            _keyToScanCode[(int) Keys.Home] = 0x47;
            _keyToScanCode[(int) Keys.PageUp] = 0x49;
            _keyToScanCode[(int) Keys.End] = 0x4f;
            _keyToScanCode[(int) Keys.PageDown] = 0x51;
            _keyToScanCode[(int) Keys.Insert] = 0x52;
            _keyToScanCode[(int) Keys.Delete] = 0x53;
            _keyToScanCode[(int) Keys.NumLock] = 0x45;
            _keyToScanCode[(int) Keys.Up] = 0x48;
            _keyToScanCode[(int) Keys.Down] = 0x50;
            _keyToScanCode[(int) Keys.Left] = 0x4b;
            _keyToScanCode[(int) Keys.Right] = 0x4d;
            _keyToScanCode[(int) Keys.D1] = 0x02;
            _keyToScanCode[(int) Keys.D2] = 0x03;
            _keyToScanCode[(int) Keys.D3] = 0x04;
            _keyToScanCode[(int) Keys.D4] = 0x05;
            _keyToScanCode[(int) Keys.D5] = 0x06;
            _keyToScanCode[(int) Keys.D6] = 0x07;
            _keyToScanCode[(int) Keys.D7] = 0x08;
            _keyToScanCode[(int) Keys.D8] = 0x09;
            _keyToScanCode[(int) Keys.D9] = 0x0a;
            _keyToScanCode[(int) Keys.D0] = 0x0b;
            _keyToScanCode[(int) Keys.OemMinus] = 0x0c;
            _keyToScanCode[(int) Keys.Oemplus] = 0x0d;
            _keyToScanCode[(int) Keys.Q] = 0x10;
            _keyToScanCode[(int) Keys.W] = 0x11;
            _keyToScanCode[(int) Keys.E] = 0x12;
            _keyToScanCode[(int) Keys.R] = 0x13;
            _keyToScanCode[(int) Keys.T] = 0x14;
            _keyToScanCode[(int) Keys.Y] = 0x15;
            _keyToScanCode[(int) Keys.U] = 0x16;
            _keyToScanCode[(int) Keys.I] = 0x17;
            _keyToScanCode[(int) Keys.O] = 0x18;
            _keyToScanCode[(int) Keys.P] = 0x19;
            _keyToScanCode[(int) Keys.OemOpenBrackets] = 0x1a;
            _keyToScanCode[(int) Keys.OemCloseBrackets] = 0x1b;
            _keyToScanCode[(int) Keys.OemPipe] = 0x1c;
            _keyToScanCode[(int) Keys.A] = 0x1e;
            _keyToScanCode[(int) Keys.S] = 0x1f;
            _keyToScanCode[(int) Keys.D] = 0x20;
            _keyToScanCode[(int) Keys.F] = 0x21;
            _keyToScanCode[(int) Keys.G] = 0x22;
            _keyToScanCode[(int) Keys.H] = 0x23;
            _keyToScanCode[(int) Keys.J] = 0x24;
            _keyToScanCode[(int) Keys.K] = 0x25;
            _keyToScanCode[(int) Keys.L] = 0x26;
            _keyToScanCode[(int) Keys.OemSemicolon] = 0x27;
            _keyToScanCode[(int) Keys.OemQuotes] = 0x28;
            _keyToScanCode[(int) Keys.Z] = 0x2c;
            _keyToScanCode[(int) Keys.X] = 0x2d;
            _keyToScanCode[(int) Keys.C] = 0x2e;
            _keyToScanCode[(int) Keys.V] = 0x2f;
            _keyToScanCode[(int) Keys.B] = 0x30;
            _keyToScanCode[(int) Keys.N] = 0x31;
            _keyToScanCode[(int) Keys.M] = 0x32;
            _keyToScanCode[(int) Keys.Oemcomma] = 0x33;
            _keyToScanCode[(int) Keys.OemPeriod] = 0x34;
            // see idlibc.chartable for other keys

            form.KeyDown += form_KeyDown;
            form.KeyUp += form_KeyUp;
			
			_joystickConnected = false;
			_joystickButtons = 0;
			_joystickXAxis = 0.0f;
			_joystickYAxis = 0.0f;
        }

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
        }

        /// <summary>Maps a Keys value to a scan code.</summary>
        private int[] _keyToScanCode;

        /// <summary>Returns the equivalent scan code for the key code.</summary>
        /// <param name="keyCode">The keycode.</param>
        /// <returns>The scan code or zero.</returns>
        private int KeyToScanCode(Keys keyCode)
        {
            int index = (int) keyCode;

            int scanCode = 0;
            if(index < _keyToScanCode.Length)
                scanCode = _keyToScanCode[index];

            return scanCode;
        }

        /// <summary>Handles the forms keydown event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The KeyEventArgs.</param>
        private void form_KeyDown(object sender, KeyEventArgs e)
        {
            int scanCode = KeyToScanCode(e.KeyCode);

            if(scanCode != 0)
                _keyDown[scanCode] = true;
        }

        /// <summary>Handles the forms keyup event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The KeyEventArgs.</param>
        private void form_KeyUp(object sender, KeyEventArgs e)
        {
            int scanCode = KeyToScanCode(e.KeyCode);

            if(scanCode != 0)
                _keyDown[scanCode] = false;
        }


    }
}
