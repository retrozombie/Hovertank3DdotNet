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

namespace Hovertank3DdotNet
{
    /// <summary>An abstract base class for providing read-only resources.</summary>
    abstract class ResourceProvider
    {
        /// <summary>Reads and returns the specified resource in a byte array.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>A byte array containing the resource.</returns>
        public abstract byte[] GetBytes(string identifier);

        /// <summary>Returns whether the specified resource exists.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>True if the resource exists.</returns>
        public abstract bool Exists(string identifier);

        /// <summary>Returns the length of the resource.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>The length of the resource in bytes or -1 if it doesn't exist.</returns>
        public abstract int GetLength(string identifier);
    }
}
