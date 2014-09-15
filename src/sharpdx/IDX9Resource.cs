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
using SharpDX.Direct3D9;

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>Represents an object that uses DirectX9 resources.</summary>
    interface IDX9Resource : IDisposable
    {
        /// <summary>Invoked after the device has been created.</summary>
        /// <param name="device">The Direct3D9 Device.</param>
        void DeviceCreated(Device device);

        /// <summary>Invoked when the device is lost.</summary>
        void DeviceLost();

        /// <summary>Invoked after the device is reset.</summary>
        /// <param name="device">The Direct3D9 Device.</param>
        void DeviceReset(Device device);
    }
}
