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
    /// <summary>Graphics data header.</summary>
    /// <remarks>
    /// Originally defined in HOVMAIN.C.
    /// This data structure stores offsets to:
    ///     dictionary - The huffman node dictionary.
    ///     dataoffsets - An array of offsets for the chunks found in the file EGAGRAPH.HOV.
    /// The file EGAHEAD.HOV contains the data for this structure.
    /// </remarks>
    class grheadtype
    {
        /// <summary>Creates a new grheadtype.</summary>
        /// <param name="buffer">A byte array containing the data.</param>
        public grheadtype(byte[] buffer)
        {
            _pointer = new memptr(buffer, 0);
        }

        /// <summary>The pointer to the data structure.</summary>
        private memptr _pointer;

        /// <summary>Gets the byte array containing the EGAHEAD data.</summary>
        public byte[] Buffer
        {
            get { return _pointer.Buffer; }
        }

        /// <summary>Gets or sets the header size.</summary>
        /// <remarks>int headersize</remarks>
        public short headersize
        {
            get { return _pointer.GetInt16(0); }
            set { _pointer.SetInt16(0, value); }
        }

        /// <summary>Gets or sets the offset to the huffman dictionary.</summary>
        /// <remarks>long dictionary</remarks>
        public int dictionary
        {
            get { return _pointer.GetInt32(2); }
            set { _pointer.SetInt32(2, value); }
        }

        /// <summary>Gets or sets the offset to the data offsets array.</summary>
        /// <remarks>long dataoffsets</remarks>
        public int dataoffsets
        {
            get { return _pointer.GetInt32(6); }
            set { _pointer.SetInt32(6, value); }
        }
    }
}
