/* 
 * Resource Package Library
 * Copyright (c) 2014, Andy Stewart
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the author nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace net.zombieman.RPKLib
{
    /// <summary>File Format (.rpk) - Resource PacKage - Copyright (c)2014 Andy Stewart.</summary>
    /// <remarks>
    /// Usage:
    /// 
    /// Create a resource package:
    ///     RPK.Writer writer = new RPK.Writer("APP", 0);
    ///     writer.AddResource("Example/Resource", 0, new FileResource("resourcefilename")); // or MemoryResource
    ///     writer.Write("output.rpk"); // or BinaryWriter or Stream
    /// 
    /// Read a resource package:
    ///     RPK.Reader reader = RPK.Reader.FromFile("output.rpk"); // or FromStream
    ///     // Check app / version
    ///     if(reader.ApplicationID == "APP" && reader.Version == 0)
    ///     {
    ///         byte[] resource = reader.GetResourceBytes("Example/Resource");
    ///     }
    ///     
    /// -- Header --
    /// ascii[4] RPK\0 ID
    /// u32 Length (of data from here to end of this RPK i.e. total length - 8)
    /// u32 Root_Directory_Offset (relative to BasePosition (start of RPK + 8))
    /// u8 StringLength - The maximum length of an identifier string, actual max length = (n * 4) + 4.
    /// u8 MaxDepth - The maximum sub directory depth - 1 (1 to 256).
    /// ascii[3] Application_ID (i.e. 4 letter code describing the purpose of this package - application defined)
    /// u8 Version (application defined version)
    /// u32 CRC32 of Header
    ///
    /// u8[?] Resource 1 (resource entries)
    /// ...
    /// u8[?] Resource n
    ///
    /// -- Root Directory / Sub Directory -- (located at Root_Directory_Offset)
    /// u32 DIR\0 (Directory ID)
    /// u16 Number_of_Entries
    /// DirectoryEntry[] Entries
    /// u32 CRC32 of Directory
    ///
    /// -- DirectoryEntry --
    /// u8 Type (255 = RPK Subdirectory (reserved), 0 ... 254 = application defined resource type)
    /// u24 Length
    /// u32 Offset (relative to BasePosition (start of RPK + 8))
    /// u8 ID_Length (Length of encoded ID string)
    /// utf8[31] ID - entry is encoded with UTF8 and must fit in 31 bytes
    /// u32 CRC32 of DirectoryEntry
    /// </remarks>
    class RPK
    {
        /// <summary>RPK static constructor.</summary>
        static RPK()
        {
            crc32Table = new uint[256];
            
            for(uint i = 0; i < 256; i++)
            {
                uint remainder = i;
                for(uint j = 0; j < 8; j++)
                {
                    if((remainder & 1) != 0)
                    {
                        remainder >>= 1;
                        remainder ^= 0xEDB88320;
                    }
                    else
                    {
                        remainder >>= 1;
                    }
                }

                crc32Table[i] = remainder;
            }
        }
        
        /// <summary>The CRC32 table.</summary>
        private static readonly uint[] crc32Table;

        /// <summary>The initial CRC32.</summary>
        private const uint InitialCRC32 = 0xffffffff;

        /// <summary>Updates a CRC32.</summary>
        /// <param name="crc32">The CRC to update.</param>
        /// <param name="value">The byte to include in the CRC.</param>
        private static void UpdateCRC32(ref uint crc32, byte value)
        {
            crc32 = (crc32 >> 8) ^ crc32Table[(crc32 & 0xff) ^ value];
        }

        /// <summary>Updates a CRC32.</summary>
        /// <param name="crc32">The CRC to update.</param>
        /// <param name="value">The ushort to include in the CRC.</param>
        private static void UpdateCRC32(ref uint crc32, ushort value)
        {
            UpdateCRC32(ref crc32, (byte) value);
            value >>= 8;
            UpdateCRC32(ref crc32, (byte) value);
        }

        /// <summary>Updates a CRC32.</summary>
        /// <param name="crc32">The CRC to update.</param>
        /// <param name="value">The uint to include in the CRC.</param>
        private static void UpdateCRC32(ref uint crc32, uint value)
        {
            UpdateCRC32(ref crc32, (byte) value);
            value >>= 8;
            UpdateCRC32(ref crc32, (byte) value);
            value >>= 8;
            UpdateCRC32(ref crc32, (byte) value);
            value >>= 8;
            UpdateCRC32(ref crc32, (byte) value);
        }

        /// <summary>Updates a CRC32 with the specified range of a byte array.</summary>
        /// <param name="crc32">The CRC to update.</param>
        /// <param name="buffer">The source buffer.</param>
        /// <param name="startIndex">The index of the first element for the range within the buffer.</param>
        /// <param name="length">The length of the range to include in the CRC.</param>
        private static void UpdateCRC32(ref uint crc32, byte[] buffer, int startIndex, int length)
        {
            int endIndex = startIndex + length;

            while(startIndex < endIndex)
                UpdateCRC32(ref crc32, buffer[startIndex++]);
        }

        /// <summary>Calculates and returns the CRC32 for the specified range of a byte array.</summary>
        /// <param name="buffer">The source buffer.</param>
        /// <param name="startIndex">The index of the first element for the range within the buffer.</param>
        /// <param name="length">The length of the range to include in the CRC.</param>
        /// <returns>The CRC32.</returns>
        public static uint CalculateCRC32(byte[] buffer, int startIndex, int length)
        {
            uint crc32 = InitialCRC32;
            UpdateCRC32(ref crc32, buffer, startIndex, length);
            return ~crc32;
        }

        /// <summary>Encodes a string using UTF8 and updates the CRC32 to include the encoded data.</summary>
        /// <param name="text">The string to encode.</param>
        /// <param name="maxLength">The max length.</param>
        /// <param name="crc32">The CRC to update.</param>
        /// <returns>A byte array.</returns>
        private static byte[] EncodeUTF8(string text, int maxLength, ref uint crc32)
        {
            if(string.IsNullOrEmpty(text))
                throw new Exception("text is null or empty!");

            if(text.Length > maxLength)
                throw new Exception("text is too long!");

            byte[] buffer = new byte[maxLength + 1];
            buffer[0] = (byte) Encoding.UTF8.GetByteCount(text);
            Encoding.UTF8.GetBytes(text, 0, text.Length, buffer, 1);
            UpdateCRC32(ref crc32, buffer, 0, buffer.Length);
            return buffer;
        }

        /// <summary>Splits a path into identifiers.</summary>
        /// <param name="path">The path.</param>
        /// <param name="maxIDLength">The maximum identifier length.</param>
        /// <returns>The list of identifiers.</returns>
        private static List<string> SplitPath(string path, int maxIDLength)
        {
            List<string> ids = null;

            int index1 = path.IndexOf(PathSeparatorChar);
            if(index1 != 0)
                throw new Exception("Invalid path, expecting leading separator!");

            index1++;
            int index2 = path.IndexOf(PathSeparatorChar, index1);
            while(index2 != -1)
            {
                string id = path.Substring(index1, index2 - index1);
                DirectoryEntry.CheckID(id, maxIDLength);

                if(ids == null)
                    ids = new List<string>();

                ids.Add(id);

                index1 = index2 + 1;
                index2 = path.IndexOf(PathSeparatorChar, index1);
            }

            return ids;
        }

        /// <summary>Returns the resource identifier part of the path.</summary>
        /// <param name="path">The path.</param>
        /// <returns>A string.</returns>
        private static string GetID(string path)
        {
            int index = path.LastIndexOf(PathSeparatorChar);

            if(index == -1)
                return path;

            return path.Substring(index + 1);
        }
        
        /// <summary>The identifier for the root directory.</summary>
        private const string RootDirectoryID = "";

        /// <summary>Provides methods for locating / reading resources from an RPK.</summary>
        public class Reader
        {
            /// <summary>Creates a new RPK.Reader.</summary>
            /// <param name="stream">The stream.</param>
            /// <param name="basePosition">The base position.</param>
            private Reader(Stream stream, uint basePosition)
            {
                _stream = stream;
                _basePosition = basePosition;
            }

            /// <summary>The stream.</summary>
            private Stream _stream;

            /// <summary>The base position.</summary>
            private uint _basePosition;

            /// <summary>Gets the base position.</summary>
            public uint BasePosition
            {
                get { return _basePosition; }
            }

            /// <summary>The maximum identifier length.</summary>
            private int _maxIDLength;

            /// <summary>Gets the maximum identifier length.</summary>
            public int MaxIDLength
            {
                get { return _maxIDLength; }
            }

            /// <summary>The maximum directory depth.</summary>
            private int _maxDepth;

            /// <summary>Gets the maximum directory depth.</summary>
            public int MaxDepth
            {
                get { return _maxDepth; }
            }

            /// <summary>The application ID.</summary>
            private string _applicationID;

            /// <summary>Gets the application ID.</summary>
            public string ApplicationID
            {
                get { return _applicationID; }
            }

            /// <summary>The application version.</summary>
            private byte _version;

            /// <summary>Gets the application version.</summary>
            public byte Version
            {
                get { return _version; }
            }

            /// <summary>The root directory.</summary>
            private Directory<DirectoryEntry> _rootDirectory;

            /// <summary>Gets the root directory.</summary>
            public Directory<DirectoryEntry> RootDirectory
            {
                get { return _rootDirectory; }
            }

            /// <summary>Returns whether the specified directory entry exists.</summary>
            /// <param name="path">The path for the directory entry.</param>
            /// <returns>True if the directory entry exists.</returns>
            public bool ResourceDirectoryEntryExists(string path)
            {
                DirectoryEntry directoryEntry;
                return TryGetResourceDirectoryEntry(path, out directoryEntry);
            }

            /// <summary>Returns whether the specified directory entry exists.</summary>
            /// <param name="path">The path for the directory entry.</param>
            /// <param name="directoryEntry">The variable that will receive the directory entry.</param>
            /// <returns>True if the entry exists and directoryEntry has been set.</returns>
            public bool TryGetResourceDirectoryEntry(string path, out DirectoryEntry directoryEntry)
            {
                if(string.IsNullOrEmpty(path))
                {
                    directoryEntry = null;
                    return false;
                }

                List<string> ids = null;
                if(path[0] == PathSeparatorChar)
                    ids = SplitPath(path, _maxIDLength);

                Directory<DirectoryEntry> directory = _rootDirectory;
                if(ids != null)
                {
                    if(ids.Count > _maxDepth)
                        throw new Exception("Path directories exceeds max depth!");

                    int index = 0;
                    do
                    {
                        string directoryID = ids[index];

                        Directory<DirectoryEntry> existingDirectory;
                        if(!directory.TryGetDirectory(directoryID, out existingDirectory))
                            throw new Exception("Directory '" + path + "' not found!");

                        directory = existingDirectory;
                        index++;

                    } while(index < ids.Count);
                }

                string id = GetID(path);
                return TryGetItem(directory.Items, id, out directoryEntry);
            }


            /// <summary>Gets the directory entry for the specified path.</summary>
            /// <param name="path">The path.</param>
            /// <returns>A DirectoryEntry.</returns>
            public DirectoryEntry GetResourceDirectoryEntry(string path)
            {
                DirectoryEntry directoryEntry;
                if(!TryGetResourceDirectoryEntry(path, out directoryEntry))
                    throw new Exception("Item '" + path + "' not found!");

                return directoryEntry;
            }

            /// <summary>Returns a byte array containing the specified resource.</summary>
            /// <param name="path">The path for the resource.</param>
            /// <param name="dontVerifyCRC">Whether the resources CRC32 will not be verified.</param>
            /// <returns>A byte array.</returns>
            public byte[] GetResourceBytes(string path, bool dontVerifyCRC)
            {
                DirectoryEntry directoryEntry = GetResourceDirectoryEntry(path);

                _stream.Seek(_basePosition + directoryEntry.Offset, SeekOrigin.Begin);

                byte[] buffer = new byte[directoryEntry.Length];
                _stream.Read(buffer, 0, buffer.Length);

                if(!dontVerifyCRC)
                {
                    uint crc32 = CalculateCRC32(buffer, 0, buffer.Length);
                   
                    if(crc32 != directoryEntry.CRC32)
                        throw new Exception("CRC mismatch for resource '" + path + "'!");
                }

                return buffer;
            }

            /// <summary>Returns a byte array containing the specified resource, the CRC is verified.</summary>
            /// <param name="path">The path for the resource.</param>
            /// <returns>A byte array.</returns>
            public byte[] GetResourceBytes(string path)
            {
                return GetResourceBytes(path, false);
            }

            /// <summary>Returns the index of the specified item.</summary>
            /// <param name="directoryItems">The list of directory entries.</param>
            /// <param name="id">The resource identifier.</param>
            /// <returns>The index or -1 if not found.</returns>
            private int IndexOfItem(List<DirectoryEntry> directoryItems, string id)
            {
                for(int i = 0; i < directoryItems.Count; i++)
                    if(string.Compare(id, directoryItems[i].ID, StringComparison.OrdinalIgnoreCase) == 0)
                        return i;

                return -1;
            }

            /// <summary>Tries to get the specified item.</summary>
            /// <param name="directoryItems">The list of items.</param>
            /// <param name="id">The item identifier.</param>
            /// <param name="item">The variable that will receive the item.</param>
            /// <returns>True if the item exists and item has been set.</returns>
            private bool TryGetItem(List<DirectoryEntry> directoryItems, string id, out DirectoryEntry item)
            {
                int index = IndexOfItem(directoryItems, id);

                if(index != -1)
                {
                    item = directoryItems[index];
                    return true;
                }

                item = null;
                return false;
            }

            /// <summary>A lookup that maps a path to a directory entry.</summary>
            private Dictionary<string, DirectoryEntry> _lookup;

            /// <summary>Creates a lookup for directory entries.</summary>
            private void CreateLookup()
            {
                _lookup = new Dictionary<string, DirectoryEntry>(StringComparer.OrdinalIgnoreCase);
                RecurseBuildLookup(_lookup, _rootDirectory, RootDirectoryID);
            }

            /// <summary>Recursively builds a lookup for directory entries.</summary>
            /// <param name="lookup">The dictionary to add to.</param>
            /// <param name="directory">The current directory.</param>
            /// <param name="path">The current path.</param>
            internal static void RecurseBuildLookup(Dictionary<string, DirectoryEntry> lookup, Directory<DirectoryEntry> directory, string path)
            {
                path += PathSeparator;
                for(int i = 0; i < directory.Items.Count; i++)
                {
                    DirectoryEntry directoryEntry = directory.Items[i];
                    string fullPath = path + directoryEntry.ID;
                    lookup.Add(fullPath, directoryEntry);
                }

                for(int i = 0; i < directory.Directories.Count; i++)
                    RecurseBuildLookup(lookup, directory.Directories[i], path + directory.Directories[i].ID);
            }

            /// <summary>Returns a string representation of the object.</summary>
            /// <returns>A string.</returns>
            public override string ToString()
            {
                return string.Concat("ApplicationID = ", _applicationID, ", Version = ", _version.ToString(), 
                    ", Entries = ", _lookup.Count.ToString());
            }

            /// <summary>Creates and returns a new RPK.Reader for the specified stream.</summary>
            /// <param name="stream">A stream which contains RPK formatted data.</param>
            /// <returns>An RPK.Reader.</returns>
            public static Reader FromStream(Stream stream)
            {
                BinaryReader binaryReader = new BinaryReader(stream, Encoding.UTF8);

                uint crc32 = InitialCRC32;

                // Check ID
                int id = binaryReader.ReadInt32();
                if(id != RPKID)
                    throw new Exception("Expecting an RPK!");

                UpdateCRC32(ref crc32, RPKID);

                uint length = binaryReader.ReadUInt32();

                long basePosition = stream.Position;

                if(basePosition + length < stream.Length)
                    throw new Exception("Invalid length, stream is too short!");

                UpdateCRC32(ref crc32, length);

                uint directoryOffset = binaryReader.ReadUInt32();

                if(directoryOffset >= stream.Length - 10) // ID:4 + Length:2 + CRC32:4
                    throw new Exception("Invalid offset, stream is too short!");

                UpdateCRC32(ref crc32, directoryOffset);

                Reader reader = new Reader(stream, (uint) basePosition);

                byte value = binaryReader.ReadByte();
                reader._maxIDLength = (value * 4) + 4;
                UpdateCRC32(ref crc32, value);

                value = binaryReader.ReadByte();
                reader._maxDepth = value + 1;
                UpdateCRC32(ref crc32, value);

                StringBuilder s = new StringBuilder();
                value = binaryReader.ReadByte();
                s.Append((char) value);
                UpdateCRC32(ref crc32, value);

                value = binaryReader.ReadByte();
                s.Append((char) value);
                UpdateCRC32(ref crc32, value);

                value = binaryReader.ReadByte();
                s.Append((char) value);
                UpdateCRC32(ref crc32, value);

                reader._applicationID = s.ToString();

                value = binaryReader.ReadByte();
                UpdateCRC32(ref crc32, value);
                reader._version = value;

                crc32 = ~crc32;
                uint fileCRC32 = binaryReader.ReadUInt32();
                if(fileCRC32 != crc32)
                    throw new Exception("CRC mismatch (header)!");

                stream.Seek(basePosition + directoryOffset, SeekOrigin.Begin);

                reader._rootDirectory = new Directory<DirectoryEntry>(RootDirectoryID, null);

                RecurseLoadDirectory(binaryReader, basePosition, reader._rootDirectory, 0, reader);

                reader.CreateLookup();

                return reader;
            }

            /// <summary>Recursively reads the RPK directory structure.</summary>
            /// <param name="binaryReader">The BinaryReader.</param>
            /// <param name="basePosition">The base position.</param>
            /// <param name="parentDirectory">The parent directory.</param>
            /// <param name="depth">The current directory depth.</param>
            /// <param name="reader">The RPK.Reader.</param>
            private static void RecurseLoadDirectory(BinaryReader binaryReader, long basePosition,
                Directory<DirectoryEntry> parentDirectory, int depth, Reader reader)
            {
                if(depth > reader._maxDepth)
                    throw new Exception("Exceeded maximum directory depth!");

                int id = binaryReader.ReadInt32();
                if(id != DirectoryID)
                    throw new Exception("Expecting a directory!");

                uint crc32 = InitialCRC32;
                UpdateCRC32(ref crc32, DirectoryID);

                ushort entries = binaryReader.ReadUInt16();
                UpdateCRC32(ref crc32, entries);

                for(int i = 0; i < entries; i++)
                {
                    DirectoryEntry directoryEntry = new DirectoryEntry(reader._maxIDLength, binaryReader, ref crc32);

                    if(directoryEntry.Type == DirectoryEntry.TypeSubDirectory)
                    {
                        long position = binaryReader.BaseStream.Position;

                        Directory<DirectoryEntry> directory = new Directory<DirectoryEntry>(directoryEntry.ID, parentDirectory);
                        parentDirectory.Directories.Add(directory);

                        binaryReader.BaseStream.Seek(basePosition + directoryEntry.Offset, SeekOrigin.Begin);
                        RecurseLoadDirectory(binaryReader, basePosition, directory, depth + 1, reader);

                        binaryReader.BaseStream.Position = position;
                        continue;
                    }

                    parentDirectory.AddItem(directoryEntry.ID, directoryEntry);
                }

                crc32 = ~crc32;
                uint fileCRC32 = binaryReader.ReadUInt32();
                if(fileCRC32 != crc32)
                    throw new Exception("CRC mismatch (directory)!");
            }

            /// <summary>Creates and returns a new RPK.Reader for the specified file.</summary>
            /// <param name="fileName">The name of the file (RPK format).</param>
            /// <returns>An RPK.Reader.</returns>
            public static Reader FromFile(string fileName)
            {
                return FromStream(new FileStream(fileName, FileMode.Open, FileAccess.Read));
            }
        }

        /// <summary>The id "RPK\0".</summary>
        private const int RPKID = 0x004B5052;

        /// <summary>The id "DIR\0".</summary>
        private const int DirectoryID = 0x00524944;

        /// <summary>The path separator character.</summary>
        public const char PathSeparatorChar = '/';

        /// <summary>The path separator character as a string.</summary>
        public const string PathSeparator = "/";

        /// <summary>The default maximum identifier length.</summary>
        private const int DefaultMaxIDLength = 32;

        /// <summary>The default maximum directory depth.</summary>
        private const int DefaultMaxDepth = 2;
       
        /// <summary>The directory entry for a resource.</summary>
        public class DirectoryEntry
        {
            /// <summary>Creates a new DirectoryEntry.</summary>
            /// <param name="maxIDLength">The maximum identifier length.</param>
            internal DirectoryEntry(int maxIDLength)
            {
                _maxIDLength = maxIDLength;
            }

            /// <summary>Creates a new DirectoryEntry.</summary>
            /// <param name="maxIDLength">The maximum identifier length.</param>
            /// <param name="type">The type of resource.</param>
            /// <param name="length">The length of the resource.</param>
            /// <param name="offset">The offset to the resource from the base position.</param>
            /// <param name="id">The identifier for the resource.</param>
            /// <param name="crc32">The CRC32.</param>
            internal DirectoryEntry(int maxIDLength, byte type, uint length, uint offset, string id, uint crc32)
                : this(maxIDLength)
            {
                Type = type;
                Length = length;
                Offset = offset;
                ID = id;
                CRC32 = crc32;
            }

            /// <summary>Creates a new DirectoryEntry.</summary>
            /// <param name="maxIDLength">The maximum identifier length.</param>
            /// <param name="binaryReader">The binary reader.</param>
            /// <param name="crc32">The CRC32.</param>
            internal DirectoryEntry(int maxIDLength, BinaryReader binaryReader, ref uint crc32)
                : this(maxIDLength)
            {
                uint lengthType = binaryReader.ReadUInt32();
                UpdateCRC32(ref crc32, lengthType);

                _type = (byte) lengthType;
                
                lengthType >>= 8;
                _length = lengthType;

                _offset = binaryReader.ReadUInt32();
                UpdateCRC32(ref crc32, _offset);

                byte[] idBytes = binaryReader.ReadBytes(_maxIDLength + 1);
                if(idBytes[0] == 0 || idBytes[0] > _maxIDLength)
                    throw new Exception("Invalid ID length!");

                _id = Encoding.UTF8.GetString(idBytes, 1, idBytes[0]);
                UpdateCRC32(ref crc32, idBytes, 0, idBytes.Length);

                _crc32 = binaryReader.ReadUInt32();
                UpdateCRC32(ref crc32, _crc32);
            }

            /// <summary>The maximum identifier length.</summary>
            private int _maxIDLength;

            /// <summary>The type of resource.</summary>
            internal byte _type;

            /// <summary>Gets or sets the type of resource.</summary>
            public byte Type
            {
                get { return _type; }
                set
                {
                    CheckType(value);

                    _type = value;
                }
            }

            /// <summary>The length of the resource.</summary>
            private uint _length;

            /// <summary>Gets or sets the length of the resource.</summary>
            public uint Length
            {
                get { return _length; }
                set
                {
                    if(value > 16777215)
                        throw new Exception("Invalid length (too big)!");

                    _length = value;
                }
            }

            /// <summary>The offset to the resource from the base position.</summary>
            private uint _offset;

            /// <summary>Gets or sets the offset to the resource from the base position.</summary>
            public uint Offset
            {
                get { return _offset; }
                set { _offset = value; }
            }

            /// <summary>The resource identifier.</summary>
            private string _id;

            /// <summary>Gets or sets the resource identifier.</summary>
            public string ID
            {
                get { return _id; }
                set
                {
                    CheckID(value, _maxIDLength);

                    _id = value;
                } 
            }

            /// <summary>The CRC32 for the resource.</summary>
            private uint _crc32;

            /// <summary>Gets or sets the CRC32 for the resource.</summary>
            public uint CRC32
            {
                get { return _crc32; }
                set { _crc32 = value; }
            }

            /// <summary>Checks the resource type.</summary>
            /// <param name="type">The resource type.</param>
            public static void CheckType(byte type)
            {
                if(type == SubDirectoryEntry.TypeSubDirectory)
                    throw new Exception("Type is reserved!");
            }

            /// <summary>Checks the resource identifier.</summary>
            /// <param name="id">The resource identifier.</param>
            /// <param name="maxIDLength">The maximum identifier length.</param>
            public static void CheckID(string id, int maxIDLength)
            {
                if(string.IsNullOrEmpty(id))
                    throw new Exception("Invalid ID (empty)!");

                if(id.Length > maxIDLength)
                    throw new Exception("Invalid ID (too long)!");

                for(int i = 0; i < id.Length; i++)
                    if(!DirectoryEntry.IsValidIDCharacter(id[i]))
                        throw new Exception("Invalid ID (invalid characters)!");
            }

            /// <summary>Writes the directory entry to a binary writer.</summary>
            /// <param name="writer">The BinaryWriter.</param>
            /// <param name="crc32">The directory's CRC32.</param>
            internal void WriteBinary(BinaryWriter writer, ref uint crc32)
            {
                uint lengthType = _length;
                lengthType <<= 8;
                lengthType |= _type;

                writer.Write(lengthType);
                UpdateCRC32(ref crc32, lengthType);
                writer.Write(_offset);
                UpdateCRC32(ref crc32, _offset);
                writer.Write(EncodeUTF8(_id, _maxIDLength, ref crc32));
                writer.Write(_crc32);
                UpdateCRC32(ref crc32, _crc32);
            }

            /// <summary>Returns a string representation of the object.</summary>
            /// <returns>A string.</returns>
            public override string ToString()
            {
                return string.Concat("Type = ", _type, ", Length = ", _length.ToString(),
                    ", Offset = ", _offset.ToString(), ", ID = ", _id.ToString() + 
                    ", CRC32 = 0x" + _crc32.ToString("X8"));
            }

            /// <summary>The reserved resource type number for a sub directory.</summary>
            public const byte TypeSubDirectory = 0xff;

            /// <summary>Returns whether the specified character is valid for use within an identifier.</summary>
            /// <param name="chr">A character.</param>
            /// <returns>True if the character is useable.</returns>
            public static bool IsValidIDCharacter(char chr)
            {
                if(char.IsLetterOrDigit(chr))
                    return true;

                switch(chr)
                {
                    case ' ':
                    case '_':
                    case '-':
                    case '+':
                    case '=':
                    case '(':
                    case ')':
                    case '#':
                    case '@':
                    case '.':
                    case ',':
                    case '!':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        return true;
                }

                return false;
            }
        }

        /// <summary>The directory entry for a sub directory.</summary>
        class SubDirectoryEntry : DirectoryEntry
        {
            /// <summary>Creates a new DirectoryEntry.</summary>
            /// <param name="maxIDLength">The maximum identifier length.</param>
            /// <param name="length">The length of the resource.</param>
            /// <param name="offset">The offset to the resource from the base position.</param>
            /// <param name="id">The identifier for the resource.</param>
            /// <param name="crc32">The CRC32.</param>
            internal SubDirectoryEntry(int maxIDLength, uint length, uint offset, string id, uint crc32)
                : base(maxIDLength)
            {
                _type = TypeSubDirectory;
                Length = length;
                Offset = offset;
                ID = id;
                CRC32 = crc32;
            }
        }

        /// <summary>Provides methods for creating an RPK.</summary>
        public class Writer
        {
            /// <summary>Creates a new RPK writer.</summary>
            /// <param name="applicationID">The application ID (3 characters).</param>
            /// <param name="version">The application version.</param>
            /// <param name="maxIDLength">The max directory / resource identifier string length (4 to 1024, must be a multiple of 4).</param>
            /// <param name="maxDepth">The maximum directory depth (1 to 256).</param>
            public Writer(string applicationID, byte version, int maxIDLength, int maxDepth)
            {
                if(string.IsNullOrEmpty(applicationID))
                    throw new Exception("applicationID cannot be null!");

                if(applicationID.Length != 3)
                    throw new Exception("applicationID must be 3 characters!");

                for(int i = 0; i < 3; i++)
                {
                    if(!char.IsLetterOrDigit(applicationID[i]))
                        throw new Exception("applicationID must consist of letters or digits!");

                    ushort chrValue = (ushort) applicationID[i];
                    if(chrValue < 32 || chrValue > 127)
                        throw new Exception("applicationID must consist of ASCII characters!");
                }

                _applicationID = applicationID;
                _version = version;

                if(maxIDLength < 4 || maxIDLength > 1024)
                    throw new Exception("maxStringLength out of range (must be 4 to 1024)!");

                if((maxIDLength & 1) != 0)
                    throw new Exception("maxStringLength must be an even number!");

                _maxIDLength = maxIDLength;

                if(maxDepth < 1 || maxDepth > 256)
                    throw new Exception("maxDepth out of range (must be 1 to 256)!");

                _maxDepth = maxDepth;

                _rootDirectory = new Directory<ResourceIDAndType>(RootDirectoryID, null);

                _resourceBuffer = new byte[65536];
            }

            /// <summary>Creates a new RPK writer.</summary>
            /// <param name="applicationID">The application ID (3 characters).</param>
            /// <param name="version">The application version.</param>
            public Writer(string applicationID, byte version)
                : this(applicationID, version, DefaultMaxIDLength, DefaultMaxDepth)
            {
            }

            /// <summary>The maximum identifier length.</summary>
            private int _maxIDLength;

            /// <summary>Gets the maximum identifier length.</summary>
            public int MaxIDLength
            {
                get { return _maxIDLength; }
            }

            /// <summary>The maximum directory depth.</summary>
            private int _maxDepth;

            /// <summary>Gets the maximum directory depth.</summary>
            public int MaxDepth
            {
                get { return _maxDepth; }
            }

            /// <summary>The application ID.</summary>
            private string _applicationID;

            /// <summary>Gets the application ID.</summary>
            public string ApplicationID
            {
                get { return _applicationID; }
            }

            /// <summary>The application version.</summary>
            private byte _version;

            /// <summary>Gets the application version.</summary>
            public byte Version
            {
                get { return _version; }
            }

            /// <summary>The root directory.</summary>
            private Directory<ResourceIDAndType> _rootDirectory;

            /// <summary>The buffer for reading resources.</summary>
            private byte[] _resourceBuffer;

            /// <summary>Adds a resource to the package.</summary>
            /// <param name="path">The path.</param>
            /// <param name="type">The resource type.</param>
            /// <param name="resource">The resource.</param>
            public void AddResource(string path, byte type, Resource resource)
            {
                DirectoryEntry.CheckType(type);

                Directory<ResourceIDAndType> parentDirectory;
                string id;
                if(path[0] == PathSeparatorChar)
                {
                    id = GetID(path);
                    parentDirectory = GetDirectoryForPath(path);
                }
                else
                {
                    id = path;
                    parentDirectory = _rootDirectory;
                }

                DirectoryEntry.CheckID(id, _maxIDLength);

                ResourceIDAndType resourceIDAndType = new ResourceIDAndType(id, 0, resource);
                parentDirectory.AddItem(id, resourceIDAndType);
            }

            /// <summary>Recursively writes resources to the writer stream directory entries on the way.</summary>
            /// <param name="writerStream">The writer stream.</param>
            /// <param name="basePosition">The base position.</param>
            /// <param name="resourceDirectory">The current directory.</param>
            /// <param name="directoryEntries">The directory entries.</param>
            private void RecurseWriteItems(Stream writerStream, long basePosition, Directory<ResourceIDAndType> resourceDirectory, 
                Directory<DirectoryEntry> directoryEntries)
            {
                for(int i = 0; i < resourceDirectory.Items.Count; i++)
                {
                    ResourceIDAndType resourceIDAndType = resourceDirectory.Items[i];

                    uint offset1 = (uint) (writerStream.Position - basePosition);

                    uint crc32 = InitialCRC32;
                    int bytesRead = resourceIDAndType.Resource.ResourceRead(_resourceBuffer, 0, _resourceBuffer.Length);
                    while(bytesRead != 0)
                    {
                        writerStream.Write(_resourceBuffer, 0, bytesRead);
                        UpdateCRC32(ref crc32, _resourceBuffer, 0, bytesRead);

                        bytesRead = resourceIDAndType.Resource.ResourceRead(_resourceBuffer, 0, _resourceBuffer.Length);
                    }
                    crc32 = ~crc32;

                    uint offset2 = (uint) (writerStream.Position - basePosition);

                    DirectoryEntry directoryEntry = new DirectoryEntry(_maxIDLength, resourceIDAndType.Type, offset2 - offset1, 
                        offset1, resourceIDAndType.ID, crc32);

                    directoryEntries.AddItem(resourceIDAndType.ID, directoryEntry);
                }

                for(int i = 0; i < resourceDirectory.Directories.Count; i++)
                {
                    Directory<ResourceIDAndType> directory = resourceDirectory.Directories[i];
                    Directory<DirectoryEntry> parentDirectoryEntries = directoryEntries.CreateDirectory(directory.ID);

                    RecurseWriteItems(writerStream, basePosition, directory, parentDirectoryEntries);
                }
            }

            /// <summary>Writes a small number of bytes (max 4) to the writer stream, updating a CRC32 with the data.</summary>
            /// <param name="writerStream">The writer stream.</param>
            /// <param name="crc32">The CRC32 to update.</param>
            /// <param name="value">The value to write.</param>
            /// <param name="bytes">The number of bytes.</param>
            private void WriteBytes(Stream writerStream, ref uint crc32, int value, int bytes)
            {
                writerStream.WriteByte((byte) value);
                UpdateCRC32(ref crc32, (byte) value);

                while(--bytes > 0)
                {
                    value >>= 8;
                    writerStream.WriteByte((byte) value);
                    UpdateCRC32(ref crc32, (byte) value);
                }
            }

            /// <summary>Recursively writes directories to a binary writer.</summary>
            /// <param name="writer">The BinaryWriter.</param>
            /// <param name="basePosition">The base position.</param>
            /// <param name="directoryEntries">The current directory entries.</param>
            /// <returns>A sub directory entry for the items written.</returns>
            private SubDirectoryEntry RecurseWriteDirectories(BinaryWriter writer, long basePosition, 
                Directory<DirectoryEntry> directoryEntries)
            {
                Stream writerStream = writer.BaseStream;

                // Write deepest directories out first
                List<SubDirectoryEntry> subDirectories = null;
                if(directoryEntries.Directories.Count > 0)
                {
                    subDirectories = new List<SubDirectoryEntry>(directoryEntries.Directories.Count);

                    for(int i = 0; i < directoryEntries.Directories.Count; i++)
                        subDirectories.Add(RecurseWriteDirectories(writer, basePosition, directoryEntries.Directories[i]));
                }

                int count = directoryEntries.Directories.Count + directoryEntries.Items.Count;
                if(count > ushort.MaxValue)
                    throw new Exception("Directory contains too many items / directories!");

                uint offset1 = (uint) (writerStream.Position - basePosition);

                uint crc32 = InitialCRC32;
                WriteBytes(writerStream, ref crc32, DirectoryID, 4);
                WriteBytes(writerStream, ref crc32, count, 2);

                if(subDirectories != null)
                {
                    for(int i = 0; i < subDirectories.Count; i++)
                        subDirectories[i].WriteBinary(writer, ref crc32);
                }

                for(int i = 0; i < directoryEntries.Items.Count; i++)
                    directoryEntries.Items[i].WriteBinary(writer, ref crc32);

                crc32 = ~crc32;
                writer.Write(crc32);

                uint offset2 = (uint) (writerStream.Position - basePosition);

                string id = (directoryEntries.ParentDirectory == null ? "root" : directoryEntries.ID);
                return new SubDirectoryEntry(_maxIDLength, offset2 - offset1, offset1, id, crc32);
            }

            /// <summary>Writes resources to a binary writer.</summary>
            /// <param name="writer">The BinaryWriter.</param>
            public void Write(BinaryWriter writer)
            {
                writer.Write(RPKID);

                writer.Write(0); // length - will be set later

                long basePosition = writer.BaseStream.Position;

                writer.Write(0); // directoryOffset - will be set later

                writer.Write((byte) ((_maxIDLength - 4) / 4));
                writer.Write((byte) (_maxDepth - 1));

                writer.Write((byte) _applicationID[0]);
                writer.Write((byte) _applicationID[1]);
                writer.Write((byte) _applicationID[2]);
                
                writer.Write(_version);

                writer.Write(0); // CRC32 - will be set later

                Directory<DirectoryEntry> directoryEntries = new Directory<DirectoryEntry>("", null);
                RecurseWriteItems(writer.BaseStream, basePosition, _rootDirectory, directoryEntries);

                SubDirectoryEntry rootSubDirectoryEntry = RecurseWriteDirectories(writer, basePosition, directoryEntries);

                long endPosition = writer.BaseStream.Position;
                uint length = (uint) (endPosition - basePosition);

                writer.BaseStream.Seek(basePosition - 4, SeekOrigin.Begin);
                writer.Write(length);
                writer.Write(rootSubDirectoryEntry.Offset);

                uint crc32 = InitialCRC32;
                UpdateCRC32(ref crc32, RPKID);
                UpdateCRC32(ref crc32, length);
                UpdateCRC32(ref crc32, rootSubDirectoryEntry.Offset);

                UpdateCRC32(ref crc32, (byte) ((_maxIDLength - 4) / 4));
                UpdateCRC32(ref crc32, (byte) (_maxDepth - 1));
                UpdateCRC32(ref crc32, (byte) _applicationID[0]);
                UpdateCRC32(ref crc32, (byte) _applicationID[1]);
                UpdateCRC32(ref crc32, (byte) _applicationID[2]);
                UpdateCRC32(ref crc32, _version);

                writer.BaseStream.Seek(6, SeekOrigin.Current);

                crc32 = ~crc32;
                writer.Write(crc32);

                writer.BaseStream.Seek(endPosition, SeekOrigin.Begin);
            }

            /// <summary>Writes resources to a stream.</summary>
            /// <param name="stream">The stream.</param>
            public void Write(Stream stream)
            {
                Write(new BinaryWriter(stream));
            }

            /// <summary>Writes resources to a file.</summary>
            /// <param name="fileName">The name of the file to create / overwrite.</param>
            public void Write(string fileName)
            {
                using(FileStream fileStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    Write(fileStream);
                }
            }

            /// <summary>Returns the directory entry for the specified path.</summary>
            /// <param name="path">The path.</param>
            /// <returns>A directory entry.</returns>
            private Directory<ResourceIDAndType> GetDirectoryForPath(string path)
            {
                List<string> ids = SplitPath(path, _maxIDLength);

                Directory<ResourceIDAndType> currentDirectory = _rootDirectory;
                if(ids != null)
                {
                    if(ids.Count > _maxDepth)
                        throw new Exception("Path directories exceeds max depth!");

                    int index = 0;
                    do
                    {
                        string directoryID = ids[index];

                        Directory<ResourceIDAndType> existingDirectory;
                        if(!currentDirectory.TryGetDirectory(directoryID, out existingDirectory))
                            existingDirectory = currentDirectory.CreateDirectory(directoryID);

                        currentDirectory = existingDirectory;
                        index++;

                    } while(index < ids.Count);
                }

                return currentDirectory;
            }

            /// <summary>A resource, it's identifier and type.</summary>
            class ResourceIDAndType
            {
                /// <summary>Creates a new ResourceIDAndType.</summary>
                /// <param name="id">The resource identifier.</param>
                /// <param name="type">The resource type.</param>
                /// <param name="resource">The resource.</param>
                public ResourceIDAndType(string id, byte type, Resource resource)
                {
                    _id = id;
                    _type = type;
                    _resource = resource;
                }

                /// <summary>The resource identifier.</summary>
                private string _id;

                /// <summary>Gets the resource identifier.</summary>
                public string ID
                {
                    get { return _id; }
                }

                /// <summary>The resource type.</summary>
                private byte _type;

                /// <summary>Gets the resource type.</summary>
                public byte Type
                {
                    get { return _type; }
                }

                /// <summary>The resource.</summary>
                private Resource _resource;

                /// <summary>Gets the resource.</summary>
                public Resource Resource
                {
                    get { return _resource; }
                }
            }
        }

        /// <summary>A base class for resources.</summary>
        public abstract class Resource
        {
            /// <summary>Reads the resource.</summary>
            /// <param name="buffer">The buffer to write to.</param>
            /// <param name="offset">The offset within the buffer to write to.</param>
            /// <param name="count">The number of bytes to write.</param>
            /// <returns>The number of bytes written to the buffer, zero indicates the end of the resource has been reached.</returns>
            public abstract int ResourceRead(byte[] buffer, int offset, int count);
        }

        /// <summary>A memory based resource.</summary>
        public class MemoryResource : Resource
        {
            /// <summary>Creates a new MemoryResource.</summary>
            /// <param name="buffer">The buffer containing the data.</param>
            public MemoryResource(byte[] buffer)
            {
                _buffer = buffer;
            }

            /// <summary>The buffer containing the data.</summary>
            private byte[] _buffer;

            /// <summary>The read position.</summary>
            private int _readPosition;

            /// <summary>Reads the resource.</summary>
            /// <param name="buffer">The buffer to write to.</param>
            /// <param name="offset">The offset within the buffer to write to.</param>
            /// <param name="count">The number of bytes to write.</param>
            /// <returns>The number of bytes written to the buffer, zero indicates the end of the resource has been reached.</returns>
            public override int ResourceRead(byte[] buffer, int offset, int count)
            {
                int bytesRemaining = _buffer.Length - _readPosition;

                if(count > bytesRemaining)
                    count = bytesRemaining;

                if(count != 0)
                {
                    Array.Copy(_buffer, _readPosition, buffer, offset, count);
                    _readPosition += count;
                }
             
                return count;
            }
        }

        /// <summary>A file based resource.</summary>
        public class FileResource : Resource
        {
            /// <summary>Creates a new FileResource.</summary>
            /// <param name="fileName">The name of the file containing the resource.</param>
            public FileResource(string fileName)
            {
                _fileName = fileName;
            }

            /// <summary>The name of the data file.</summary>
            private string _fileName;

            /// <summary>The file stream.</summary>
            private FileStream _fileStream;

            /// <summary>Whether reading has completed.</summary>
            private bool _readComplete;

            /// <summary>Reads the resource.</summary>
            /// <param name="buffer">The buffer to write to.</param>
            /// <param name="offset">The offset within the buffer to write to.</param>
            /// <param name="count">The number of bytes to write.</param>
            /// <returns>The number of bytes written to the buffer, zero indicates the end of the resource has been reached.</returns>
            public override int ResourceRead(byte[] buffer, int offset, int count)
            {
                if(_readComplete)
                    return 0;

                if(_fileStream == null)
                    _fileStream = new FileStream(_fileName, FileMode.Open, FileAccess.Read);

                long bytesRemaining = _fileStream.Length - _fileStream.Position;

                if(count > bytesRemaining)
                    count = (int) bytesRemaining;

                if(count != 0)
                {
                    _fileStream.Read(buffer, offset, count);
                }
                else
                {
                    _fileStream.Close();
                    _readComplete = true;
                }

                return count;
            }
        }

        /// <summary>A generic directory.</summary>
        /// <typeparam name="T">The entry type.</typeparam>
        public class Directory<T>
        {
            /// <summary>Creates a new directory entry.</summary>
            /// <param name="id">The identifier for the directory.</param>
            /// <param name="parentDirectory">The parent directory.</param>
            public Directory(string id, Directory<T> parentDirectory)
            {
                _id = id;
                _parentDirectory = parentDirectory;
                _directories = new List<Directory<T>>();
                _items = new List<T>();
                _itemIDs = new List<string>();
            }

            /// <summary>The identifier for the directory.</summary>
            private string _id;

            /// <summary>Gets the identifier for the directory.</summary>
            public string ID
            {
                get { return _id; }
            }

            /// <summary>The parent directory.</summary>
            private Directory<T> _parentDirectory;

            /// <summary>Gets the parent directory.</summary>
            public Directory<T> ParentDirectory
            {
                get { return _parentDirectory; }
            }

            /// <summary>The sub directories within the directory.</summary>
            private List<Directory<T>> _directories;

            /// <summary>Gets the sub directories within the directory.</summary>
            internal List<Directory<T>> Directories
            {
                get { return _directories; }
            }

            /// <summary>Creates and returns a new sub directory.</summary>
            /// <param name="id">The identifier for the directory.</param>
            /// <returns>The directory.</returns>
            public Directory<T> CreateDirectory(string id)
            {
                if(DirectoryExists(id))
                    throw new Exception("A directory with the identifier '" + id + "' already exists!");

                if(ItemExists(id))
                    throw new Exception("An item with the identifier '" + id + "' already exists!");

                Directory<T> directory = new Directory<T>(id, this);
                _directories.Add(directory);
                return directory;
            }

            /// <summary>Returns the index of the specified directory.</summary>
            /// <param name="id">The identifier of the directory to find.</param>
            /// <returns>The index or -1 if not found.</returns>
            private int IndexOfDirectory(string id)
            {
                for(int i = 0; i < _directories.Count; i++)
                    if(string.Compare(id, _directories[i].ID, StringComparison.OrdinalIgnoreCase) == 0)
                        return i;

                return -1;
            }

            /// <summary>Retrieves the specified directory.</summary>
            /// <param name="id">The identifier for the directory.</param>
            /// <param name="directory">The variable that will receive the directory.</param>
            /// <returns>True if successful.</returns>
            public bool TryGetDirectory(string id, out Directory<T> directory)
            {
                int index = IndexOfDirectory(id);

                if(index != -1)
                {
                    directory = _directories[index];
                    return true;
                }

                directory = null;
                return false;
            }

            /// <summary>Returns whether a directory with the specified identifier exists.</summary>
            /// <param name="id">The identifier for the directory.</param>
            /// <returns>True if the directory exists.</returns>
            public bool DirectoryExists(string id)
            {
                return (IndexOfDirectory(id) != -1);
            }

            /// <summary>Gets the full path for the specified directory.</summary>
            public string FullPath
            {
                get
                {
                    StringBuilder s = new StringBuilder();

                    Directory<T> directory = this;
                    while(directory != null)
                    {
                        s.Insert(0, PathSeparator);
                        s.Insert(0, directory._id);

                        directory = directory._parentDirectory;
                    }

                    return s.ToString();
                }
            }

            /// <summary>The items within the directory.</summary>
            private List<T> _items;

            /// <summary>The identifiers for the items within the directory.</summary>
            private List<string> _itemIDs;

            /// <summary>Gets the items within the directory.</summary>
            internal List<T> Items
            {
                get { return _items; }
            }

            /// <summary>Adds an item to the directory.</summary>
            /// <param name="id">The identifier for the item.</param>
            /// <param name="item">The item.</param>
            public void AddItem(string id, T item)
            {
                if(ItemExists(id))
                    throw new Exception("An item with the identifier '" + id + "' already exists!");

                if(DirectoryExists(id))
                    throw new Exception("A directory with the identifier '" + id + "' already exists!");

                _items.Add(item);
                _itemIDs.Add(id);
            }

            /// <summary>Returns the index of the specified item.</summary>
            /// <param name="id">The identifier of the item to find.</param>
            /// <returns>The index or -1 if not found.</returns>
            private int IndexOfItem(string id)
            {
                for(int i = 0; i < _directories.Count; i++)
                    if(string.Compare(id, _itemIDs[i], StringComparison.OrdinalIgnoreCase) == 0)
                        return i;

                return -1;
            }

            /// <summary>Retrieves the specified item.</summary>
            /// <param name="id">The identifier for the item.</param>
            /// <param name="item">The variable that will receive the item.</param>
            /// <returns>True if successful.</returns>
            public bool TryGetItem(string id, out T item)
            {
                int index = IndexOfItem(id);
                
                if(index != -1)
                {
                    item = _items[index];
                    return true;
                }

                item = default(T);
                return false;
            }

            /// <summary>Returns whether an item with the specified identifier exists.</summary>
            /// <param name="id">The identifier for the item.</param>
            /// <returns>True if the item exists.</returns>
            public bool ItemExists(string id)
            {
                return (IndexOfItem(id) != -1);
            }

            /// <summary>Returns a string representation of the object.</summary>
            /// <returns>A string.</returns>
            public override string ToString()
            {
                string id = (_id == string.Empty ? PathSeparator : _id);

                return string.Concat("'", id, "', Directories = [", _directories.Count, "], Items = [", 
                    _items.Count, "]");
            }

        }
    }
}