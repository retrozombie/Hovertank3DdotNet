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
    /// <summary>Records the dimensions of an image.</summary>
    /// <remarks>
    /// Originally defined in IDLIB.H.
    /// The images and dimensions are stored seperately:
    /// The chunk STRUCTPIC (in EGAGRAPH.HOV) contains the data for the array: pictable.
    /// </remarks>
    struct pictype
    {
        /// <summary>Creates a new pictype.</summary>
        /// <param name="pointer">The pointer to the data structure.</param>
        public pictype(memptr pointer)
        {
            width = pointer.GetInt16(0);
            height = pointer.GetInt16(2);
        }

        /// <summary>The width of the image in columns.</summary>
        /// <remarks>int width</remarks>
        public short width;

        /// <summary>The height of the image in pixels.</summary>
        /// <remarks>int height</remarks>
        public short height;

        /// <summary>Returns a string representation of the object.</summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Concat("width = ", width.ToString(), ", height = ", height.ToString());
        }

        /// <summary>The size of the structure in bytes.</summary>
        public const int SizeOf = 4;
    }
}
