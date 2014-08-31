#define DEBUG_INDEX_OUT_OF_RANGE

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
    /// <summary>Represents a memptr, contains functionality for emulating C pointers.</summary>
    struct memptr
    {
        /// <summary>Initialises a new memptr.</summary>
        /// <param name="buffer">The buffer to point to.</param>
        public memptr(byte[] buffer)
        {
            _buffer = buffer;
            _baseIndex = 0;
        }

        /// <summary>Initialises a new memptr.</summary>
        /// <param name="buffer">The buffer to point to.</param>
        /// <param name="baseIndex">The base index.</param>
        public memptr(byte[] buffer, int baseIndex)
        {
            _buffer = buffer;
            _baseIndex = baseIndex;
        }

        /// <summary>Initialises a new memptr from another memptr.</summary>
        /// <param name="other">Another memory pointer.</param>
        /// <param name="offset">The offset in bytes.</param>
        public memptr(memptr other, int offset)
        {
            _buffer = other.Buffer;
            _baseIndex = other.BaseIndex + offset;
        }

        /// <summary>Initialises a new memptr.</summary>
        /// <param name="other">Another memory pointer to copy.</param>
        public memptr(memptr other)
            : this(other, 0)
        {
        }

        /// <summary>The memory buffer.</summary>
        private byte[] _buffer;

        /// <summary>Gets or sets the memory buffer.</summary>
        public byte[] Buffer
        {
            get { return _buffer; }
            set { _buffer = value; }
        }

        /// <summary>The base index.</summary>
        private int _baseIndex;

        /// <summary>Gets or sets the base index.</summary>
        public int BaseIndex
        {
            get { return _baseIndex; }
            set { _baseIndex = value; }
        }

        /// <summary>Gets whether the pointer is null.</summary>
        public bool IsNull
        {
            get { return (_buffer == null); }
        }

        /// <summary>Sets the pointer to null.</summary>
        public void SetNull()
        {
            _buffer = null;
        }

        /// <summary>Gets a byte at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>A byte.</returns>
        public byte GetUInt8(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetUInt8 Index out of range!");
#endif

            return _buffer[_baseIndex + offset];
        }

        /// <summary>Sets a byte at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetUInt8(int offset, byte value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetUInt8 Index out of range!");
#endif

            _buffer[_baseIndex + offset] = value;
        }

        /// <summary>Gets a signed byte at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>A byte.</returns>
        public sbyte GetInt8(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetInt8 Index out of range!");
#endif

            return (sbyte) _buffer[_baseIndex + offset];
        }

        /// <summary>Sets a signed byte at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetInt8(int offset, sbyte value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetInt8 Index out of range!");
#endif

            _buffer[_baseIndex + offset] = (byte) value;
        }

        /// <summary>Gets an unsigned 16-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>A UInt16.</returns>
        public UInt16 GetUInt16(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 1 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetUInt16 Index out of range!");
#endif

            offset += _baseIndex;
            UInt16 value = _buffer[offset + 1];
            value <<= 8;
            value |= _buffer[offset];
            return value;
        }

        /// <summary>Sets an unsigned 16-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetUInt16(int offset, UInt16 value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 1 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetUInt16 Index out of range!");
#endif

            offset += _baseIndex;
            _buffer[offset] = (byte) value;
            _buffer[offset + 1] = (byte) (value >> 8);
        }

        /// <summary>Gets a signed 16-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>An Int16.</returns>
        public Int16 GetInt16(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 1 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetInt16 Index out of range!");
#endif

            offset += _baseIndex;
            Int16 value = _buffer[offset + 1];
            value <<= 8;
            value |= (Int16) _buffer[offset];
            return value;
        }

        /// <summary>Sets a signed 16-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetInt16(int offset, Int16 value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 1 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetInt16 Index out of range!");
#endif

            offset += _baseIndex;
            _buffer[offset] = (byte) value;
            _buffer[offset + 1] = (byte) (value >> 8);
        }

        /// <summary>Gets an unsigned 32-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>A UInt32.</returns>
        public UInt32 GetUInt32(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 3 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetUInt32 Index out of range!");
#endif
            offset += _baseIndex;
            UInt32 value = _buffer[offset + 3];
            value <<= 8;
            value |= _buffer[offset + 2];
            value <<= 8;
            value |= _buffer[offset + 1];
            value <<= 8;
            value |= _buffer[offset];
            return value;
        }

        /// <summary>Sets an unsigned 32-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetUInt32(int offset, UInt32 value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 3 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetUInt32 Index out of range!");
#endif
            offset += _baseIndex;
            _buffer[offset] = (byte) value;
            value >>= 8;
            _buffer[offset + 1] = (byte) value;
            value >>= 8;
            _buffer[offset + 2] = (byte) value;
            value >>= 8;
            _buffer[offset + 3] = (byte) value;
        }

        /// <summary>Gets a signed 32-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <returns>An Int32.</returns>
        public Int32 GetInt32(int offset)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 3 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("GetInt32 Index out of range!");
#endif

            offset += _baseIndex;
            Int32 value = _buffer[offset + 3];
            value <<= 8;
            value |= _buffer[offset + 2];
            value <<= 8;
            value |= _buffer[offset + 1];
            value <<= 8;
            value |= _buffer[offset];
            return value;
        }

        /// <summary>Sets a signed 32-bit value at the specified offset.</summary>
        /// <param name="offset">The offset in bytes.</param>
        /// <param name="value">The new value.</param>
        public void SetInt32(int offset, Int32 value)
        {
#if DEBUG_INDEX_OUT_OF_RANGE
            if(_baseIndex + offset < 0 || _baseIndex + offset + 3 >= _buffer.Length)
                System.Diagnostics.Debug.WriteLine("SetInt32 Index out of range!");
#endif

            offset += _baseIndex;
            _buffer[offset] = (byte) value;
            value >>= 8;
            _buffer[offset + 1] = (byte) value;
            value >>= 8;
            _buffer[offset + 2] = (byte) value;
            value >>= 8;
            _buffer[offset + 3] = (byte) value;
        }

        /// <summary>Moves the pointer.</summary>
        /// <param name="bytes">The offset in bytes.</param>
        public void Offset(int bytes)
        {
            _baseIndex += bytes;
        }

        /// <summary>Returns a string representation of the object.</summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            if(_buffer == null)
                return "NULL";

            return string.Concat("[", _buffer.Length.ToString(), "]+", _baseIndex.ToString());
        }

    }
}
