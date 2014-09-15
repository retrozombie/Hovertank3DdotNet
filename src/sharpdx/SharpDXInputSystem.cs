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
using System.Collections.Generic;
using SharpDX;

using WF = System.Windows.Forms;
using DI = SharpDX.DirectInput;

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>SharpDX input system implementation.</summary>
    class SharpDXInputSystem : InputSystem
    {
        /// <summary>Creates a new SharpDXInputSystem.</summary>
        /// <param name="control">The control to associate with DirectInput.</param>
        public SharpDXInputSystem(WF.Control control)
        {
            _directInput = new DI.DirectInput();

            InitialiseKeyboard(control);

            InitialiseJoystick();
        }

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
            if(_keyboard != null)
            {
                try { _keyboard.Unacquire(); }
                catch { }

                try { _keyboard.Dispose(); }
                catch { _keyboard = null; }
            }

            if(_joystick != null)
            {
                try { _joystick.Unacquire(); }
                catch { }

                try { _joystick.Dispose(); }
                catch { }
            }

            if(_directInput != null)
            {
                try { _directInput.Dispose(); }
                catch { }
                _directInput = null;
            }
        }

        /// <summary>The DirectInput.</summary>
        private DI.DirectInput _directInput;

        /// <summary>The keyboard.</summary>
        private DI.Keyboard _keyboard;

        /// <summary>The keyboard state.</summary>
        private DI.KeyboardState _keyboardState;

        /// <summary>The key to scan code lookup.</summary>
        private int[] _keyToScanCode;

        /// <summary>The joystick device.</summary>
        private DI.Joystick _joystick;

        /// <summary>The joystick state.</summary>
        private DI.JoystickState _joystickState;

        /// <summary>Initialises the keyboard.</summary>
        /// <param name="control">The control to associate with DirectInput.</param>
        private void InitialiseKeyboard(WF.Control control)
        {
            // From idlibc.c
            _keyToScanCode = new int[256];
            _keyToScanCode[(int) DI.Key.Escape] = 0x01;
            _keyToScanCode[(int) DI.Key.Space] = 0x39;
            _keyToScanCode[(int) DI.Key.Back] = 0x0e;
            _keyToScanCode[(int) DI.Key.Tab] = 0x0f;
            _keyToScanCode[(int) DI.Key.LeftControl] = 0x1d;
            _keyToScanCode[(int) DI.Key.LeftShift] = 0x2a;
            _keyToScanCode[(int) DI.Key.Capital] = 0x3a;
            _keyToScanCode[(int) DI.Key.F1] = 0x3b;
            _keyToScanCode[(int) DI.Key.F2] = 0x3c;
            _keyToScanCode[(int) DI.Key.F3] = 0x3d;
            _keyToScanCode[(int) DI.Key.F4] = 0x3e;
            _keyToScanCode[(int) DI.Key.F5] = 0x3f;
            _keyToScanCode[(int) DI.Key.F6] = 0x40;
            _keyToScanCode[(int) DI.Key.F7] = 0x41;
            _keyToScanCode[(int) DI.Key.F8] = 0x42;
            _keyToScanCode[(int) DI.Key.F9] = 0x43;
            _keyToScanCode[(int) DI.Key.F10] = 0x44;
            _keyToScanCode[(int) DI.Key.F11] = 0x57;
            _keyToScanCode[(int) DI.Key.F12] = 0x58;
            _keyToScanCode[(int) DI.Key.ScrollLock] = 0x46;
            _keyToScanCode[(int) DI.Key.Return] = 0x1c;
            _keyToScanCode[(int) DI.Key.RightShift] = 0x36;
            _keyToScanCode[(int) DI.Key.PrintScreen] = 0x37;
            _keyToScanCode[(int) DI.Key.LeftAlt] = 0x38;
            _keyToScanCode[(int) DI.Key.Home] = 0x47;
            _keyToScanCode[(int) DI.Key.PageUp] = 0x49;
            _keyToScanCode[(int) DI.Key.End] = 0x4f;
            _keyToScanCode[(int) DI.Key.PageDown] = 0x51;
            _keyToScanCode[(int) DI.Key.Insert] = 0x52;
            _keyToScanCode[(int) DI.Key.Delete] = 0x53;
            _keyToScanCode[(int) DI.Key.NumberLock] = 0x45;
            _keyToScanCode[(int) DI.Key.Up] = 0x48;
            _keyToScanCode[(int) DI.Key.Down] = 0x50;
            _keyToScanCode[(int) DI.Key.Left] = 0x4b;
            _keyToScanCode[(int) DI.Key.Right] = 0x4d;
            _keyToScanCode[(int) DI.Key.D1] = 0x02;
            _keyToScanCode[(int) DI.Key.D2] = 0x03;
            _keyToScanCode[(int) DI.Key.D3] = 0x04;
            _keyToScanCode[(int) DI.Key.D4] = 0x05;
            _keyToScanCode[(int) DI.Key.D5] = 0x06;
            _keyToScanCode[(int) DI.Key.D6] = 0x07;
            _keyToScanCode[(int) DI.Key.D7] = 0x08;
            _keyToScanCode[(int) DI.Key.D8] = 0x09;
            _keyToScanCode[(int) DI.Key.D9] = 0x0a;
            _keyToScanCode[(int) DI.Key.D0] = 0x0b;
            _keyToScanCode[(int) DI.Key.Minus] = 0x0c;
            _keyToScanCode[(int) DI.Key.Equals] = 0x0d;
            _keyToScanCode[(int) DI.Key.Q] = 0x10;
            _keyToScanCode[(int) DI.Key.W] = 0x11;
            _keyToScanCode[(int) DI.Key.E] = 0x12;
            _keyToScanCode[(int) DI.Key.R] = 0x13;
            _keyToScanCode[(int) DI.Key.T] = 0x14;
            _keyToScanCode[(int) DI.Key.Y] = 0x15;
            _keyToScanCode[(int) DI.Key.U] = 0x16;
            _keyToScanCode[(int) DI.Key.I] = 0x17;
            _keyToScanCode[(int) DI.Key.O] = 0x18;
            _keyToScanCode[(int) DI.Key.P] = 0x19;
            _keyToScanCode[(int) DI.Key.LeftBracket] = 0x1a;
            _keyToScanCode[(int) DI.Key.RightBracket] = 0x1b;
            //_keyToScanCode[(int) DI.Key.OemPipe] = 0x1c; // TODO
            _keyToScanCode[(int) DI.Key.A] = 0x1e;
            _keyToScanCode[(int) DI.Key.S] = 0x1f;
            _keyToScanCode[(int) DI.Key.D] = 0x20;
            _keyToScanCode[(int) DI.Key.F] = 0x21;
            _keyToScanCode[(int) DI.Key.G] = 0x22;
            _keyToScanCode[(int) DI.Key.H] = 0x23;
            _keyToScanCode[(int) DI.Key.J] = 0x24;
            _keyToScanCode[(int) DI.Key.K] = 0x25;
            _keyToScanCode[(int) DI.Key.L] = 0x26;
            _keyToScanCode[(int) DI.Key.Semicolon] = 0x27;
            //_keyToScanCode[(int) DI.Key.OemQuotes] = 0x28; // TODO
            _keyToScanCode[(int) DI.Key.Z] = 0x2c;
            _keyToScanCode[(int) DI.Key.X] = 0x2d;
            _keyToScanCode[(int) DI.Key.C] = 0x2e;
            _keyToScanCode[(int) DI.Key.V] = 0x2f;
            _keyToScanCode[(int) DI.Key.B] = 0x30;
            _keyToScanCode[(int) DI.Key.N] = 0x31;
            _keyToScanCode[(int) DI.Key.M] = 0x32;
            _keyToScanCode[(int) DI.Key.Comma] = 0x33;
            _keyToScanCode[(int) DI.Key.Period] = 0x34;
            // see idlibc.chartable for other keys

            _keyboard = new DI.Keyboard(_directInput);
            _keyboard.SetCooperativeLevel(control, DI.CooperativeLevel.NonExclusive | DI.CooperativeLevel.Background);
            _keyboard.Acquire();

            _keyboardState = _keyboard.GetCurrentState();
        }

        /// <summary>Initialises the joystick.</summary>
        private void InitialiseJoystick()
        {
            List<DI.Joystick> joysticks = GetAttachedJoysticks();

            if(ControllerIndex < joysticks.Count)
            {
                _joystick = joysticks[ControllerIndex];
                _joystickState = _joystick.GetCurrentState();

                System.Diagnostics.Debug.WriteLine("Using joystick: " + _joystick.Information.InstanceName);
            }

            _joystickConnected = (_joystickState != null);
        }

        /// <summary>Gets a list of the attached joysticks.</summary>
        /// <returns>A list of joysticks.</returns>
        private List<DI.Joystick> GetAttachedJoysticks()
        {
            List<DI.Joystick> joysticks = new List<DI.Joystick>();

            foreach(DI.DeviceInstance device in _directInput.GetDevices(DI.DeviceClass.GameControl, DI.DeviceEnumerationFlags.AttachedOnly))
            {
                try
                {
                    DI.Joystick joystick = new DI.Joystick(_directInput, device.InstanceGuid);

                    joystick.Acquire();

                    IList<DI.DeviceObjectInstance> deviceObjects = joystick.GetObjects();
                    for(int i = 0; i < deviceObjects.Count; i++)
                    {
                        DI.DeviceObjectInstance deviceObjectInstance = deviceObjects[i];

                        if((deviceObjectInstance.ObjectId.Flags & DI.DeviceObjectTypeFlags.Axis) != 0)
                            joystick.GetObjectPropertiesById(deviceObjectInstance.ObjectId).Range = new DI.InputRange(-1000, 1000);
                    }


                    joysticks.Add(joystick);
                }
                catch(SharpDXException)
                {
                }
            }

            return joysticks;
        }

        /// <summary>Updates input state.</summary>
        public void Update()
        {
            UpdateKeyboard();

            UpdateJoystick();
        }

        /// <summary>Updates the keyboard.</summary>
        private void UpdateKeyboard()
        {
            try
            {
                _keyboard.GetCurrentState(ref _keyboardState);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateKeyboard: " + ex);

                try
                {
                    _keyboard.Acquire();
                    _keyboard.GetCurrentState(ref _keyboardState);
                }
                catch
                {
                    return;
                }
            }

            _keyDown.SetAll(false);

            int scanCode;
            for(int i = 0; i < _keyboardState.PressedKeys.Count; i++)
                if((scanCode = _keyToScanCode[(int) _keyboardState.PressedKeys[i]]) != 0)
                    _keyDown[scanCode] = true;
        }

        /// <summary>Updates the joystick.</summary>
        private void UpdateJoystick()
        {
            if(_joystick == null)
                return;

            try
            {
                _joystick.GetCurrentState(ref _joystickState);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateJoystick: " + ex);

                try
                {
                    _joystick.Acquire();
                    _joystick.GetCurrentState(ref _joystickState);
                }
                catch
                {
                    return;
                }
            }

            _joystickXAxis = _joystickState.X / 1000.0f;
            _joystickYAxis = _joystickState.Y / 1000.0f;

            _joystickButtons = 0;

            for(int i = 0, value = 1; i < 2 && i < _joystickState.Buttons.Length; i++, value <<= 1)
                if(_joystickState.Buttons[i])
                    _joystickButtons |= value;
        }

    }
}
