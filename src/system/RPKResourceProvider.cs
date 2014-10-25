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
using System.IO;
using net.zombieman.RPKLib;

namespace Hovertank3DdotNet
{
    /// <summary>Provides read-only resources from an RPK file.</summary>
    class RPKResourceProvider : ResourceProvider
    {
        /// <summary>Creates a new RPKResourceProvider.</summary>
        /// <param name="rpkBytes">A byte array containing the resource pack.</param>
        public RPKResourceProvider(byte[] rpkBytes)
        {
            _rpkReader = RPK.Reader.FromStream(new MemoryStream(rpkBytes));
        }

        /// <summary>The RPK reader.</summary>
        private RPK.Reader _rpkReader;

        /// <summary>Reads and returns the specified resource in a byte array.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>A byte array.</returns>
        public override byte[] GetBytes(string identifier)
        {
            return _rpkReader.GetResourceBytes(identifier, true);
        }

        /// <summary>Returns whether the specified resource exists.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>True if the resource exists.</returns>
        public override bool Exists(string identifier)
        {
            return _rpkReader.ResourceDirectoryEntryExists(identifier);
        }

        /// <summary>Returns the length of the resource.</summary>
        /// <param name="identifier">The resource identifier (case insensitive).</param>
        /// <returns>The length of the resource in bytes or -1 if it doesn't exist.</returns>
        public override int GetLength(string identifier)
        {
            RPK.DirectoryEntry directoryEntry;
            if(_rpkReader.TryGetResourceDirectoryEntry(identifier, out directoryEntry))
                return (int) directoryEntry.Length;

            return -1;
        }
    }
}
