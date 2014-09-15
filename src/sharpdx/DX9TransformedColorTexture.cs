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
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct3D9;

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>Represents a vertex which has a transformed position and color and texture coordinates.</summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct DX9TransformedColorTexture
    {
        /// <summary>Initialises a new TransformedColorTexture.</summary>
        /// <param name="x">The x component for the position.</param>
        /// <param name="y">The y component for the position.</param>
        /// <param name="z">The z component for the position.</param>
        /// <param name="rhw">The reciprocal of homogeneous w component for the transformed vertex.</param>
        /// <param name="color">The color.</param>
        /// <param name="tu">The u component for the texture coordinates.</param>
        /// <param name="tv">The v component for the texture coordinates.</param>
        public DX9TransformedColorTexture(float x, float y, float z, float rhw, int color, float tu, float tv)
        {
            _x = x;
            _y = y;
            _z = z;
            _rhw = rhw;
            _color = color;
            _tu = tu;
            _tv = tv;
        }

        /// <summary>Initialises a new TransformedColorTexture.</summary>
        /// <param name="position">A Vector4 containing the position.</param>
        /// <param name="argb">The color ARGB.</param>
        /// <param name="texCoord">A Vector2 containing the texture coordinates.</param>
        public DX9TransformedColorTexture(Vector4 position, int argb, Vector2 texCoord)
        {
            _x = position.X;
            _y = position.Y;
            _z = position.Z;
            _rhw = position.W;
            _color = argb;
            _tu = texCoord.X;
            _tv = texCoord.Y;
        }

        /// <summary>The stride of the structure.</summary>
        public static readonly int Stride = 7 * 4;

        /// <summary>The x component for the position.</summary>
        private float _x;

        /// <summary>Gets or sets the x component for the position.</summary>
        public float X
        {
            get { return _x; }
            set { _x = value; }
        }

        /// <summary>The y component for the position.</summary>
        private float _y;

        /// <summary>Gets or sets the y component for the position.</summary>
        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }

        /// <summary>The z component for the position.</summary>
        private float _z;

        /// <summary>Gets or sets the z component for the position.</summary>
        public float Z
        {
            get { return _z; }
            set { _z = value; }
        }

        /// <summary>The reciprocal of homogeneous w component for the transformed vertex.</summary>
        private float _rhw;

        /// <summary>Gets or sets the reciprocal of homogeneous w component for the transformed vertex.</summary>
        public float Rhw
        {
            get { return _rhw; }
            set { _rhw = value; }
        }

        /// <summary>Gets or sets the position for the vertex as a Vector4.</summary>
        public Vector4 Position
        {
            get { return new Vector4(_x, _y, _z, _rhw); }
            set
            {
                _x = value.X;
                _y = value.Y;
                _z = value.Z;
                _rhw = value.W;
            }
        }

        /// <summary>The color.</summary>
        private int _color;

        /// <summary>Gets or sets the color.</summary>
        public int Color
        {
            get { return _color; }
            set { _color = value; }
        }

        /// <summary>The u component for the texture coordinates.</summary>
        private float _tu;

        /// <summary>Gets or sets the u component for the texture coordinates.</summary>
        public float Tu
        {
            get { return _tu; }
            set { _tu = value; }
        }

        /// <summary>The v component for the texture coordinates.</summary>
        private float _tv;

        /// <summary>Gets or sets the v component for the texture coordinates.</summary>
        public float Tv
        {
            get { return _tv; }
            set { _tv = value; }
        }

        /// <summary>Gets or sets the texture coordinate for the vertex as a Vector2.</summary>
        public Vector2 TexCoord
        {
            get { return new Vector2(_tu, _tv); }
            set
            {
                _tu = value.X;
                _tv = value.Y;
            }
        }

        /// <summary>Returns a string representation of the object.</summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return string.Concat("x = ", _x, ", y = ", _y, ", z = ", _z, ", color = ", _color, ", tu = ", _tu, ", tv = ", _tv);
        }

        /// <summary>The VertexFormat.</summary>
        public const VertexFormat Format = VertexFormat.PositionRhw | VertexFormat.Diffuse | VertexFormat.Texture1;

        /// <summary>Creates and returns a new VertexDeclaration.</summary>
        /// <param name="device">The Direct3D9 Device.</param>
        /// <returns>A VertexDeclaration.</returns>
        public static VertexDeclaration CreateVertexDeclaration(Device device)
        {
            return new VertexDeclaration(device, new VertexElement[]
            {
                new VertexElement(0, 0, DeclarationType.Float4, DeclarationMethod.Default, DeclarationUsage.PositionTransformed, 0), 
                new VertexElement(0, 16, DeclarationType.Color, DeclarationMethod.Default, DeclarationUsage.Color, 0), 
                new VertexElement(0, 20, DeclarationType.Float2, DeclarationMethod.Default, DeclarationUsage.TextureCoordinate, 0), 
                VertexElement.VertexDeclarationEnd
            });
        }
    }

}
