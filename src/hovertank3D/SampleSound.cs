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
    /// <summary>Samples sound.</summary>
    class SampledSound
    {
        /// <summary>Creates a new SampledSound.</summary>
        /// <param name="pointer">The pointer to the SampledSound structure.</param>
        public SampledSound(memptr pointer)
        {
            _pointer = pointer;
        }

        /// <summary>The pointer to the SampledSound data.</summary>
        private memptr _pointer;

        /// <summary>Gets the pointer to the SampledSound data.</summary>
        public memptr Pointer
        {
            get { return _pointer; }
        }

        /// <summary>Gets or sets the offset to the sample data.</summary>
        public uint offset
        {
            get { return _pointer.GetUInt32(0); }
            set { _pointer.SetUInt32(0, value); }
        }

        /// <summary>Gets or sets the length of the sample data.</summary>
        public uint length
        {
            get { return _pointer.GetUInt32(4); }
            set { _pointer.SetUInt32(4, value); }
        }

        /// <summary>Gets or sets the sample frequency.</summary>
        public uint hertz
        {
            get { return _pointer.GetUInt32(8); }
            set { _pointer.SetUInt32(8, value); }
        }

        /// <summary>Gets or sets the number of bits per sample.</summary>
        public byte bits
        {
            get { return _pointer.GetUInt8(12); }
            set { _pointer.SetUInt8(12, value); }
        }

        /// <summary>Gets or sets the reference.</summary>
        public byte reference
        {
            get { return _pointer.GetUInt8(13); }
            set { _pointer.SetUInt8(13, value); }
        }

        /// <summary>The size of the SampledSound structure in bytes.</summary>
        public const int SizeOf = 14;
    }
}
