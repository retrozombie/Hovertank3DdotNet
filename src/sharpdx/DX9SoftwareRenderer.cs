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
    /// <summary>Implements a software based renderer for Hovertank 3D.</summary>
    class DX9SoftwareRenderer : IDX9Resource
    {
        /// <summary>The texture for the view.</summary>
        private Texture _viewTexture;

        /// <summary>Whether IntPtrs are 32-bit.</summary>
        private bool _intPtr32;

        /// <summary>The buffer for expanding spans.</summary>
        private int[] _scanBuffer;
        
        /// <summary>The view width.</summary>
        private int _viewWidth;

        /// <summary>The view height.</summary>
        private int _viewHeight;

        /// <summary>The texture width.</summary>
        private int _textureWidth;

        /// <summary>The texture height.</summary>
        private int _textureHeight;

        /// <summary>Initialises the renderer.</summary>
        /// <param name="engine">The main engine.</param>
        /// <param name="device">The direct3d device.</param>
        /// <param name="viewWidth">The view width.</param>
        /// <param name="viewHeight">The view height.</param>
        public void Initialise(Device device, int viewWidth, int viewHeight)
        {
            _viewWidth = viewWidth;
            _viewHeight = viewHeight;

            _textureWidth = NextPowerOf2(_viewWidth);
            _textureHeight = NextPowerOf2(_viewHeight);

            CreateDeviceObjects(device);
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

        /// <summary>Updates the view texture.</summary>
        /// <param name="display">The hovertank display.</param>
        public void Update(Display display)
        {
            if(_viewTexture == null)
                return;

            byte[] videoBuffer = display.VideoBuffer;
            int[] palette = display.Palette;
            int srcStride = display.Stride;

            bool textureLocked = false;
            try
            {
                int srcOffset = srcStride - Display.Width;

                DataRectangle dataRectangle = _viewTexture.LockRectangle(0, LockFlags.Discard);
                textureLocked = true;

                IntPtr pDest = dataRectangle.DataPointer;

                int srcIndex = display.ScreenStartIndex + display.PixelOffset;
                for(int y = 0; y < _viewHeight; y++)
                {
                    if(y == display.SplitScreenLines)
                        srcIndex = 0;

                    for(int x = 0; x < _viewWidth; x++)
                        _scanBuffer[x] = palette[videoBuffer[srcIndex++]];

                    srcIndex += srcOffset;

                    Marshal.Copy(_scanBuffer, 0, pDest, _viewWidth);

                    if(_intPtr32)
                        pDest = new IntPtr(pDest.ToInt32() + dataRectangle.Pitch);
                    else
                        pDest = new IntPtr(pDest.ToInt64() + dataRectangle.Pitch);
                }
            }
            catch
            {
                System.Diagnostics.Debug.WriteLine("screenTexture.Lock / update failed!");
            }

            if(textureLocked)
            {
                try { _viewTexture.UnlockRectangle(0); }
                catch(Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine("screenTexture.UnlockRectangle failed: " + ex.Message);
                }
            }
        }

        /// <summary>The vertices for drawing the view texture.</summary>
        private DX9TransformedColorTexture[] _vertices;

        /// <summary>The vertex buffer for drawing.</summary>
        private VertexBuffer _vertexBuffer;

        /// <summary>The VertexDeclaration for the vertex format.</summary>
        private VertexDeclaration _vertexDeclaration;

        /// <summary>Whether to use linear filtering.</summary>
        private bool _useLinearFiltering;

        /// <summary>Gets or sets whether to use linear filtering.</summary>
        public bool UseLinearFiltering
        {
            get { return _useLinearFiltering; }
            set { _useLinearFiltering = value; }
        }

        /// <summary>Renders the view.</summary>
        public void Render(Device device)
        {
            device.SetRenderState(RenderState.CullMode, Cull.None);
            device.SetRenderState(RenderState.ZEnable, ZBufferType.DontUseZBuffer);
            device.SetRenderState(RenderState.ZWriteEnable, false);

            device.SetSamplerState(0, SamplerState.MinFilter, TextureFilter.Point);
            device.SetSamplerState(0, SamplerState.MagFilter, _useLinearFiltering ? TextureFilter.Linear : TextureFilter.Point);
            device.SetSamplerState(0, SamplerState.MipFilter, TextureFilter.Point);

            device.SetRenderState(RenderState.ShadeMode, ShadeMode.Flat);
            device.SetRenderState(RenderState.FillMode, FillMode.Solid);

            device.SetRenderState(RenderState.Lighting, false);
            device.SetTexture(0, _viewTexture);

            device.SetStreamSource(0, _vertexBuffer, 0, Marshal.SizeOf(typeof(DX9TransformedColorTexture)));
            device.VertexDeclaration = _vertexDeclaration;
            device.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);
        }

        /// <summary>Creates the device objects.</summary>
        private void CreateDeviceObjects(Device device)
        {
            if(_viewWidth == 0)
                return;

            if(_scanBuffer == null)
            {
                _intPtr32 = (IntPtr.Size == sizeof(Int32));

                _scanBuffer = new int[_viewWidth];
                
                int vertexColor = -1;
                float height = device.Viewport.Height;
                float width = height / 1.2f * 1.6f;

                float tu = ((float) _viewWidth) / ((float) _textureWidth);
                float tv = ((float) _viewHeight) / ((float) _textureHeight);

                float centerX = device.Viewport.Width * 0.5f;
                float centerY = device.Viewport.Height * 0.5f;

                float xLeft = centerX - width * 0.5f - 0.5f;
                float xRight = centerX + width * 0.5f - 0.5f;
                float yTop = centerY - height * 0.5f - 0.5f;
                float yBottom = centerY + height * 0.5f - 0.5f;

                _vertices = new DX9TransformedColorTexture[]
                {
                    new DX9TransformedColorTexture(xLeft, yBottom, 1.0f, 1.0f, vertexColor, 0, tv),
                    new DX9TransformedColorTexture(xLeft, yTop, 1.0f, 1.0f, vertexColor, 0, 0),
                    new DX9TransformedColorTexture(xRight, yBottom, 1.0f, 1.0f, vertexColor, tu, tv),
                    new DX9TransformedColorTexture(xRight, yTop, 1.0f, 1.0f, vertexColor, tu, 0),
                };
            }

            _viewTexture = new Texture(device, _textureWidth, _textureHeight, 1, Usage.Dynamic, Format.A8R8G8B8, Pool.Default);

            _vertexBuffer = new VertexBuffer(device, _vertices.Length * Marshal.SizeOf(typeof(DX9TransformedColorTexture)), Usage.WriteOnly,
                VertexFormat.None, Pool.Managed);

            DataStream stream = _vertexBuffer.Lock(0, 0, LockFlags.None);
            stream.WriteRange(_vertices);
            _vertexBuffer.Unlock();

            _vertexDeclaration = DX9TransformedColorTexture.CreateVertexDeclaration(device);
        }

        /// <summary>Disposes the device objects.</summary>
        private void DisposeDeviceObjects()
        {
            if(_viewTexture != null)
            {
                try { _viewTexture.Dispose(); }
                catch { }
                _viewTexture = null;
            }

            if(_vertexBuffer != null)
            {
                try { _vertexBuffer.Dispose(); }
                catch { }
                _vertexBuffer = null;
            }

            if(_vertexDeclaration != null)
            {
                try { _vertexDeclaration.Dispose(); }
                catch { }
                _vertexDeclaration = null;
            }
        }

        /// <summary>Invoked when the device is created.</summary>
        /// <param name="device">The Direct3D9 Device.</param>
        public void DeviceCreated(Device device)
        {
            CreateDeviceObjects(device);
        }

        /// <summary>Invoked when the device is lost.</summary>
        public void DeviceLost()
        {
            DisposeDeviceObjects();
        }

        /// <summary>Invoked after the device is reset.</summary>
        /// <param name="device">The Direct3D9 Device.</param>
        public void DeviceReset(Device device)
        {
            CreateDeviceObjects(device);
        }

        /// <summary>Disposes resources.</summary>
        public void Dispose()
        {
            DisposeDeviceObjects();
        }

    }
}
