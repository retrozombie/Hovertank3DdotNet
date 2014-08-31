/* Hovertank 3-D Source Code
 * Copyright (C) 1993-2014 Flat Rock Software
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
    /// <summary>Level definition.</summary>
    /// <remarks>
    /// Defines a game level, a rectangular array of wall colors and a rectangular array of objects (actors).
    /// Originally defined in IDLIB.H.
    /// The games 20 levels are stored in the files named: LEVEL##.HOV.
    /// </remarks>
    class LevelDef
    {
        /// <summary>Creates a new LevelDef.</summary>
        /// <param name="pointer">The pointer to the level data.</param>
        public LevelDef(memptr pointer)
        {
            _pointer = pointer;
        }

        /// <summary>The pointer to the level data.</summary>
        private memptr _pointer;

        /// <summary>Gets the pointer to the level data.</summary>
        public memptr Pointer
        {
            get { return _pointer; }
        }

        /// <summary>Gets or sets the width of the level.</summary>
        /// <remarks>int width</remarks>
        public short width
        {
            get { return _pointer.GetInt16(0); }
            set { _pointer.SetInt16(0, value); }
        }

        /// <summary>Gets or sets the height of the level.</summary>
        /// <remarks>int height</remarks>
        public short height
        {
            get { return _pointer.GetInt16(2); }
            set { _pointer.SetInt16(2, value); }
        }

        /// <remarks>int planes</remarks>
        public short planes
        {
            get { return _pointer.GetInt16(4); }
            set { _pointer.SetInt16(4, value); }
        }

        /// <remarks>int screenx</remarks>
        public short screenx
        {
            get { return _pointer.GetInt16(6); }
            set { _pointer.SetInt16(6, value); }
        }

        /// <remarks>int screeny</remarks>
        public short screeny
        {
            get { return _pointer.GetInt16(8); }
            set { _pointer.SetInt16(8, value); }
        }

        /// <remarks>int screenw</remarks>
        public short screenw
        {
            get { return _pointer.GetInt16(10); }
            set { _pointer.SetInt16(10, value); }
        }

        /// <remarks>int screenh</remarks>
        public short screenh
        {
            get { return _pointer.GetInt16(12); }
            set { _pointer.SetInt16(12, value); }
        }

        /// <summary>Gets or sets the size of the walls array in bytes.</summary>
        /// <remarks>unsigned planesize</remarks>
        public ushort planesize
        {
            get { return _pointer.GetUInt16(14); }
            set { _pointer.SetUInt16(14, value); }
        }

        // as: Added some extra helpers for level editing

        /// <summary>Gets the wall color at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <returns>The wall color (see System\LevelWalls).</returns>
        public ushort GetWallColor(int x, int y)
        {
            if(width == 0 || height == 0 || planesize == 0)
                throw new Exception("width, height or planesize have not been set!");

            return _pointer.GetUInt16(32 + (width * y + x) * 2);
        }

        /// <summary>Sets the wall color at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <param name="value">The wall color (see System\LevelWalls).</param>
        public void SetWallColor(int x, int y, ushort value)
        {
            if(width == 0 || height == 0 || planesize == 0)
                throw new Exception("width, height or planesize have not been set!");

            _pointer.SetUInt16(32 + (width * y + x) * 2, value);
        }

        /// <summary>Gets the object type at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <returns>The object type (see System\LevelObjects).</returns>
        public ushort GetObjectType(int x, int y)
        {
            if(width == 0 || height == 0 || planesize == 0)
                throw new Exception("width, height or planesize have not been set!");

            return _pointer.GetUInt16(32 + planesize + (width * y + x) * 2);
        }

        /// <summary>Sets the object type at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <param name="value">The object type (see System\LevelObjects).</param>
        public void SetObjectType(int x, int y, ushort value)
        {
            if(width == 0 || height == 0 || planesize == 0)
                throw new Exception("width, height or planesize have not been set!");

            _pointer.SetUInt16(32 + planesize + (width * y + x) * 2, value);
        }

        /// <summary>Returns a string representation of the object.</summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Concat("width = ", width.ToString(), ", height = ", height.ToString(), ", planes = ", planes.ToString(),
                ", screenx = ", screenx.ToString(), ", screeny = ", screeny.ToString(), ", screenw = ", screenw.ToString(),
                ", screenh = ", screenh.ToString(), ", planesize = ", planesize.ToString());
        }
    }
}
