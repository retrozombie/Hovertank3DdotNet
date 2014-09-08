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
using OpenTK.Graphics.OpenGL;

namespace Hovertank3DdotNet.OpenTK
{
    /// <summary>A texture and an image buffer which is used to update the texture.</summary>
    class TextureBuffer : IDisposable
    {
        /// <summary>Creates a new texture buffer that will contain an image of the specified size.</summary>
        /// <param name="imageWidth">The width of the image.</param>
        /// <param name="imageHeight">The height of the image.</param>
        /// <param name="useLinearFilter">Whether to use linear filtering.</param>
        public TextureBuffer(int imageWidth, int imageHeight, bool useLinearFilter)
        {
            _imageWidth = imageWidth;
            _imageHeight = imageHeight;

            _textureWidth = NextPowerOf2(imageWidth);
            _textureHeight = NextPowerOf2(imageWidth);

            _sMin = 0.0f;
            _sMax = ((float) _imageWidth) / _textureWidth;
            _tMin = 0.0f;
            _tMax = ((float) _imageHeight) / _textureHeight;

            _imageBuffer = new int[_textureWidth * _textureHeight];

            _textureID = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, _textureID);

            int filter = (int) (useLinearFilter ? TextureMinFilter.Linear : TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, filter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, filter);

            GL.TexImage2D<int>(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, _textureWidth, _textureHeight, 0, 
                PixelFormat.Bgra, PixelType.UnsignedByte, _imageBuffer);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            GL.Enable(EnableCap.Texture2D);
        }

        /// <summary>Disposes resources.</summary>
        public void Dispose()
        {
            if(_textureID != 0)
            {
                GL.DeleteTexture(_textureID);
                _textureID = 0;
            }
        }

        /// <summary>The OpenGL texture ID.</summary>
        private int _textureID;

        /// <summary>Gets the OpenGL texture ID.</summary>
        public int TextureID
        {
            get { return _textureID; }
        }

        /// <summary>The image width.</summary>
        private int _imageWidth;

        /// <summary>Gets the image width.</summary>
        public int ImageWidth
        {
            get { return _imageWidth; }
        }

        /// <summary>The image height.</summary>
        private int _imageHeight;

        /// <summary>Gets the image height.</summary>
        public int ImageHeight
        {
            get { return _imageHeight; }
        }

        /// <summary>The texture width.</summary>
        private int _textureWidth;

        /// <summary>Gets the texture width.</summary>
        public int TextureWidth
        {
            get { return _textureWidth; }
        }

        /// <summary>The texture height.</summary>
        private int _textureHeight;

        /// <summary>Gets the texture height.</summary>
        public int TextureHeight
        {
            get { return _textureHeight; }
        }

        /// <summary>The S component for the minimum texture co-ordinate.</summary>
        private float _sMin;

        /// <summary>Gets the S component for the minimum texture co-ordinate.</summary>
        public float SMin
        {
            get { return _sMin; }
        }

        /// <summary>The S component for the maximum texture co-ordinate.</summary>
        private float _sMax;

        /// <summary>Gets the S component for the maximum texture co-ordinate.</summary>
        public float SMax
        {
            get { return _sMax; }
        }

        /// <summary>The T component for the minimum texture co-ordinate.</summary>
        private float _tMin;

        /// <summary>Gets the T component for the minimum texture co-ordinate.</summary>
        public float TMin
        {
            get { return _tMin; }
        }

        /// <summary>The T component for the maximum texture co-ordinate.</summary>
        private float _tMax;

        /// <summary>Gets the T component for the maximum texture co-ordinate.</summary>
        public float TMax
        {
            get { return _tMax; }
        }

        /// <summary>The image buffer.</summary>
        private int[] _imageBuffer;

        /// <summary>Gets the image buffer.</summary>
        public int[] ImageBuffer
        {
            get { return _imageBuffer; }
        }

        /// <summary>Updates the texture using the image buffer.</summary>
        public void UpdateTexture()
        {
            GL.TexSubImage2D<int>(TextureTarget.Texture2D, 0, 0, 0, _textureWidth, _textureHeight, PixelFormat.Bgra, 
                PixelType.UnsignedByte, _imageBuffer);
        }

        /// <summary>Returns the next power of 2 greater than or equal to the specified value.</summary>
        /// <param name="value">A number.</param>
        /// <returns>The next power of 2.</returns>
        private int NextPowerOf2(int value)
        {
            int powerOf2 = 1;

            while(powerOf2 < value)
                powerOf2 <<= 1;

            return powerOf2;
        }

    }
}
