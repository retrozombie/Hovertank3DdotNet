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
    /// <summary>The walltype structure.</summary>
    class walltype
    {
        /// <summary>Creates a new walltype.</summary>
        /// <param name="index">The array element index.</param>
        public walltype(short index)
        {
            _index = index;
        }

        /// <summary>The array element index.</summary>
        /// <remarks>as: added index property for pointer simulation.</remarks>
        private short _index;

        /// <summary>Gets the array element index.</summary>
        public short index
        {
            get { return _index; }
        }

        // first pixel of wall (may not be visable)
        public short x1;

        public short x2;

        public short leftclip;

        public short rightclip;

        public ushort height1;

        public ushort height2;

        public ushort color;

        /// <summary>Copies the walltype to another.</summary>
        /// <remarks>For struct assignment.</remarks>
        /// <param name="destination">The destination walltype.</param>
        public void CopyTo(walltype destination)
        {
            destination._index = _index;
            destination.x1 = x1;
            destination.x2 = x2;
            destination.leftclip = leftclip;
            destination.rightclip = rightclip;
            destination.height1 = height1;
            destination.height2 = height2;
            destination.color = color;
        }

        /// <summary>Returns a string representation of the object.</summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Concat(
                "index = ", _index.ToString(),
                ", x1 = ", x1.ToString(),
                ", x2 = ", x2.ToString(),
                ", leftclip = ", leftclip.ToString(),
                ", rightclip = ", rightclip.ToString(),
                ", height1 = ", height1.ToString(),
                ", height2 = ", height2.ToString(),
                ", color = ", color.ToString()
                );
        }
    }
}
