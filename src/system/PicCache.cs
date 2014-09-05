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
    /// <summary>Caches pics that have been converted from planar format to 8BPP format.</summary>
    class PicCache
    {
        /// <summary>Creates a new PicCache.</summary>
        /// <param name="numberOfPics">The number of pictures.</param>
        public PicCache(int numberOfPics)
        {
            _pics = new byte[numberOfPics][];
        }
        
        /// <summary>The pic cache.</summary>
        private byte[][] _pics;

        /// <summary>Gets or sets the pic at the specified index.</summary>
        /// <param name="index">The index of the pic.</param>
        /// <returns>The picture buffer.</returns>
        public byte[] this[int index]
        {
            get { return _pics[index]; }
            set { _pics[index] = value; }
        }

        /// <summary>Converts a planar pic and caches it.</summary>
        /// <param name="index">The index of the pic.</param>
        /// <param name="grseg">The grseg for the pic.</param>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="count">The number of sub images.</param>
        /// <returns>The converted pic.</returns>
        public byte[] CachePic(int index, memptr grseg, int width, int height, int count)
        {
            byte[] buffer = new byte[width * height * count];

            // Draw the picture
            byte[] source = grseg.Buffer;
            int srcIndex = grseg.BaseIndex;
            int byteWidth = width >> 3;
            int dstBaseIndex = 0;
            for(int i = 0; i < count; i++)
            {
                byte planeBit = 1;
                for(int plane = 0; plane < 4; plane++)
                {
                    int dstIndex = dstBaseIndex;
                    for(int y = 0; y < height; y++)
                    {
                        for(int x = 0; x < byteWidth; x++)
                        {
                            byte pixels = source[srcIndex++];

                            if((pixels & 0x80) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x40) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x20) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x10) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;
                            if((pixels & 0x08) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x04) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x02) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;

                            if((pixels & 0x01) != 0)
                                buffer[dstIndex] |= planeBit;

                            dstIndex++;
                        }
                    }
                    planeBit <<= 1;
                }

                dstBaseIndex += width * height;
            }

            _pics[index] = buffer;
            return buffer;
        }

        /// <summary>Converts a planar pic and caches it.</summary>
        /// <param name="index">The index of the pic.</param>
        /// <param name="grseg">The grseg for the pic.</param>
        /// <param name="picType">The pic type.</param>
        /// <returns>The converted pic.</returns>
        public byte[] CachePic(int index, memptr grseg, pictype picType)
        {
            return CachePic(index, grseg, picType.width * Display.ColumnScale, picType.height, 1);
        }

        /// <summary>Converts a planar proportional font character and caches it.</summary>
        /// <param name="index">The index of the pic.</param>
        /// <param name="fontseg">The font pointer.</param>
        /// <returns>The converted pic.</returns>
        public byte[] CachePChar(int index, fontstruct fontseg)
        {
            int offset = fontseg.Get_location(index);
            int byteWidth = (fontseg.Get_width(index) + 7) >> 3;
            int height = fontseg.height;

            byte[] buffer = new byte[byteWidth * height * 2];
            Array.Copy(fontseg.Pointer.Buffer, fontseg.Pointer.BaseIndex + offset, buffer, 0, buffer.Length);

            _pics[index] = buffer;
            return buffer;
        }
    }
}
