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
    /// <summary>Variable width (proportional) font structure.</summary>
    /// <remarks>
    /// This data structure stores information for a variable width bitmap font.
    /// Originally defined in IDLIB.H.
    /// See DrawPchar for usage (draws a character with a single color).
    /// Character data is found at location[n] (relative to the start of the data block).
    ///   Each character definition has the data for a mask and an image.
    ///   The mask and image data are stored 1 bit per pixel so:
    ///     bytes_per_row = (width[n] + 7) / 8
    ///     length = bytes_per_row * height * 2
    /// The chunk STARTFONT contains the data for this font.
    /// Undefined characters have location 0 and width 0.
    /// </remarks>
    class fontstruct
    {
        /// <summary>Creates a new fontstruct.</summary>
        /// <param name="pointer">A pointer to the font data.</param>
        public fontstruct(memptr pointer)
        {
            _pointer = pointer;
        }

        /// <summary>The pointer to the font data.</summary>
        private memptr _pointer;

        /// <summary>Gets the pointer to the font data.</summary>
        public memptr Pointer
        {
            get { return _pointer; }
        }

        /// <summary>Gets or sets the font character height in pixels.</summary>
        /// <remarks>int height</remarks>
        public short height
        {
            get { return _pointer.GetInt16(0); }
            set { _pointer.SetInt16(0, value); }
        }

        /// <summary>Gets the specified character location (array element).</summary>
        /// <remarks>
        /// The location is the offset in bytes to the character data from the start of the font data block.
        /// int location[256]
        /// </remarks>
        public short Get_location(int elementIndex)
        {
            return _pointer.GetInt16(2 + elementIndex * 2);
        }

        /// <summary>Sets the specified character location (array element).</summary>
        public void Set_location(int elementIndex, Int16 location)
        {
            _pointer.SetInt16(2 + elementIndex * 2, location);
        }

        /// <summary>Gets the specified character width (array element).</summary>
        /// <remarks>char width[256]</remarks>
        public sbyte Get_width(int elementIndex)
        {
            return _pointer.GetInt8(2 + 512 + elementIndex);
        }

        /// <summary>Sets the specified character width (array element).</summary>
        public void Set_width(int elementIndex, sbyte width)
        {
            _pointer.SetInt8(2 + 512 + elementIndex, width);
        }
    }
}
