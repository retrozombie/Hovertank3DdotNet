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
    /// <summary>A display with support for emulation of some EGA features.</summary>
    class Display
    {
        /// <summary>Creates a new Display.</summary>
        public Display()
        {
            _palette = new int[256];
            _videoBuffer = new byte[512 * 1024];
            _stride = 320;
        }

        /// <summary>The scale factor to convert an address offset to an index.</summary>
        public const int AddressScale = 8;

        /// <summary>The scale factor to convert a column offset to an index.</summary>
        public const int ColumnScale = 8;

        /// <summary>The stride.</summary>
        private int _stride;

        /// <summary>Gets or sets the stride.</summary>
        public int Stride
        {
            get { return _stride; }
            set { _stride = value; }
        }

        /// <summary>Gets the display width.</summary>
        public const int Width = 320;

        /// <summary>Gets the display height.</summary>
        public const int Height = 200;

        /// <summary>The color palette (RGB).</summary>
        private int[] _palette;

        /// <summary>Gets the color palette (RGB).</summary>
        public int[] Palette
        {
            get { return _palette; }
        }

        /// <summary>The border color (RGB).</summary>
        private int _borderColor;

        /// <summary>Gets the border color (RGB).</summary>
        public int BorderColor
        {
            get { return _borderColor; }
        }

        /// <summary>The number of colors.</summary>
        public const int Colors = 16;

        /// <summary>The index of the border color.</summary>
        public const int BorderColorIndex = 17;

        /// <summary>Sets a palette color.</summary>
        /// <param name="index">The index of the color.</param>
        /// <param name="argb">The color specified as an ARGB value.</param>
        public void SetPaletteColorARGB(int index, int argb)
        {
            _palette[index] = argb;
        }

        /// <summary>The color lookup table.</summary>
        private static readonly int[] _egaColors = 
        {
            0x000000, 0x0000aa, 0x00aa00, 0x00aaaa,
            0xaa0000, 0xaa00aa, 0xaa5500, 0xaaaaaa,
            0x555555, 0x5555ff, 0x55ff55, 0x55ffff,
            0xff5555, 0xff55ff, 0xffff55, 0xffffff
        };

        /// <summary>Sets a palette color.</summary>
        /// <param name="index">The index of the color.</param>
        /// <param name="egaColor">The color specified as an EGA color value.</param>
        public void SetPaletteColorEGA(int index, byte egaColor)
        {
            int argb = -16777216 | _egaColors[egaColor & 0x0f];

            if(index == BorderColorIndex)
                _borderColor = argb;
            else
                SetPaletteColorARGB(index, argb);
        }

        /// <summary>The video buffer.</summary>
        private byte[] _videoBuffer;

        /// <summary>Gets the video buffer.</summary>
        public byte[] VideoBuffer
        {
            get { return _videoBuffer; }
        }

        /// <summary>Clears the display.</summary>
        public void Clear()
        {
            Array.Clear(_videoBuffer, 0, _videoBuffer.Length);
        }

        /// <summary>Gets or sets the color index at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <returns>A byte.</returns>
        public byte this[int x, int y]
        {
            get { return _videoBuffer[y * _stride + x]; }
            set { _videoBuffer[y * _stride + x] = value; }
        }

        /// <summary>Returns the ARGB color of the pixel at the specified location.</summary>
        /// <param name="x">The x location.</param>
        /// <param name="y">The y location.</param>
        /// <returns>An int.</returns>
        public int GetPixelARGB(int x, int y)
        {
            return _palette[this[x, y]];
        }

        /// <summary>Copies a rectangular area of video memory.</summary>
        /// <param name="width">The width in pixels.</param>
        /// <param name="height">The height in pixels.</param>
        /// <param name="srcIndex">The source index.</param>
        /// <param name="dstIndex">The destination index.</param>
        public void Copy(int width, int height, int srcIndex, int dstIndex)
        {
            for(int y = 0; y < height; y++)
            {
                Array.Copy(_videoBuffer, srcIndex, _videoBuffer, dstIndex, width);
                srcIndex += _stride;
                dstIndex += _stride;
            }
        }

        /// <summary>The start index for the screen.</summary>
        private int _screenStartIndex;

        /// <summary>Gets or sets the start index for the screen.</summary>
        public int ScreenStartIndex
        {
            get { return _screenStartIndex; }
            set { _screenStartIndex = value; }
        }

        /// <summary>The pixel offset for the screen (horizontal).</summary>
        private int _pixelOffset;

        /// <summary>Gets or sets the pixel offset for the screen (horizontal).</summary>
        public int PixelOffset
        {
            get { return _pixelOffset; }
            set { _pixelOffset = value; }
        }

        /// <summary>The split screen lines setting.</summary>
        private int _splitScreenLines;

        /// <summary>Gets or sets the split screen lines setting.</summary>
        /// <remarks>
        /// For split screen the ScreenStartIndex is usually set to be higher in memory.
        /// When drawing, once SplitScreenLines lines have been drawn the screen index is then set to zero.
        /// </remarks>
        public int SplitScreenLines
        {
            get { return _splitScreenLines; }
            set { _splitScreenLines = value; }
        }
    }
}
