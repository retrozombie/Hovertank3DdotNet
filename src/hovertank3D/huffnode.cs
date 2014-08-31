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
    /// <summary>Huffman tree node.</summary>
    /// <remarks>
    /// Originally defined in IDLIB.H.
    /// Used for decompressing the graphics / chunks stored in EGAGRAPH.HOV.
    /// </remarks>
    class huffnode
    {
        /// <summary>Creates a new huffnode.</summary>
        /// <param name="buffer">The byte array containing the data for the huffnode.</param>
        /// <param name="offset">The offset to the huffnode.</param>
        public huffnode(byte[] buffer, int offset)
        {
            _pointer = new memptr(buffer, offset);
        }

        /// <summary>Creates a new huffnode for the first entry in the grhead huffnode dictionary (array).</summary>
        /// <param name="grhead">The grhead structure.</param>
        public huffnode(grheadtype grhead)
            : this(grhead.Buffer, grhead.dictionary)
        {
        }

        /// <summary>Creates a new huffnode for the nth element in an array of huffnodes.</summary>
        /// <param name="array">The huffnode that is the first element in the array.</param>
        /// <param name="elementIndex">The element index.</param>
        public huffnode(huffnode array, int elementIndex)
            : this(array._pointer.Buffer, array._pointer.BaseIndex + SizeOf * elementIndex)
        {
        }

        /// <summary>Creates a shallow copy of a huffnode.</summary>
        /// <param name="other">Another huffnode to copy.</param>
        public huffnode(huffnode other)
            : this(other._pointer.Buffer, other._pointer.BaseIndex)
        {
        }

        /// <summary>The pointer to the huffnode.</summary>
        private memptr _pointer;

        /// <summary>Gets the pointer to the huffnode.</summary>
        public memptr Pointer
        {
            get { return _pointer; }
        }

        /// <summary>Gets or sets the value or index to use when the bit is a zero.</summary>
        /// <remarks>
        /// Values 0 to 255 are leaves (uncompressed values), values > 255 are huffnode node indices offset by +256.
        /// unsigned bit0
        /// </remarks>
        public ushort bit0
        {
            get { return _pointer.GetUInt16(0); }
            set { _pointer.SetUInt16(0, value); }
        }

        /// <summary>Gets or sets the value or index to use when the bit is a one.</summary>
        /// <remarks>
        /// Values 0 to 255 are leaves (uncompressed values), values > 255 are huffnode node indices offset by +256.
        /// unsigned bit1
        /// </remarks>
        public ushort bit1
        {
            get { return _pointer.GetUInt16(2); }
            set { _pointer.SetUInt16(2, value); }
        }

        /// <summary>The size of the structure in bytes.</summary>
        public const int SizeOf = 4;
    }
}
