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
using System.Collections;

namespace Hovertank3DdotNet
{
    /// <summary>A base class for input systems.</summary>
    abstract class InputSystem : IDisposable
    {
        /// <summary>Initialises a new InputSystem.</summary>
        protected InputSystem()
        {
            _keyDown = new BitArray(128);
            _prevKeyDown = new BitArray(128);
        }

        /// <summary>Disposes resources.</summary>
        public abstract void Dispose();

        /// <summary>The index of the game controller to use.</summary>
        private int _controllerIndex;

        /// <summary>Gets or sets the index of the game controller to use.</summary>
        public int ControllerIndex
        {
            get { return _controllerIndex; }
            set { _controllerIndex = value; }
        }

        /// <summary>Records which keys are down.</summary>
        protected BitArray _keyDown;

        /// <summary>Records which keys were down previously.</summary>
        private BitArray _prevKeyDown;

        /// <summary>Whether a joystick is connected.</summary>
        protected bool _joystickConnected;

        /// <summary>Gets whether a joystick is present.</summary>
        public bool JoystickConnected
        {
            get { return _joystickConnected; }
        }

        /// <summary>The button states for the joystick.</summary>
        protected int _joystickButtons;

        /// <summary>Gets the button states for the joystick.</summary>
        public int JoystickButtons
        {
            get { return _joystickButtons; }
        }

        /// <summary>The x-axis measurement for joystick 1.</summary>
        protected float _joystickXAxis;

        /// <summary>Gets the x-axis measurement for joystick 1.</summary>
        public float JoystickXAxis
        {
            get { return _joystickXAxis; }
        }

        /// <summary>The y-axis measurement for joystick 1.</summary>
        protected float _joystickYAxis;

        /// <summary>Gets the y-axis measurement for joystick 1.</summary>
        public float JoystickYAxis
        {
            get { return _joystickYAxis; }
        }

        /// <summary>Updates keyboard input.</summary>
        /// <param name="hovertank">The hovertank game.</param>
        public virtual void UpdateKeyboardInput(Hovertank hovertank)
        {
            int lastScanCode = 0;
            BitArray hovertankKeyDown = hovertank.keydown;
            for(int i = 0; i < hovertankKeyDown.Length; i++)
            {
                if(_keyDown[i] && !_prevKeyDown[i])
                {
                    hovertankKeyDown[i] = true;
                    lastScanCode = i;
                }

                if(!_keyDown[i] && _prevKeyDown[i])
                    hovertankKeyDown[i] = false;
            }

            if(lastScanCode != 0)
            {
                hovertank.NBKscan = (ushort) (lastScanCode | 0x80);
                hovertank.NBKascii = hovertank.scanascii[hovertank.NBKscan & 0x7f];
            }

            // Copy _prevKeyDown to _keyDown
            _prevKeyDown.SetAll(false);
            _prevKeyDown.Or(_keyDown);
        }
    }
}
