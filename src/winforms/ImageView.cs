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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hovertank3DdotNet.WinForms
{
    /// <summary>A control that displays the game view.</summary>
    class ImageView : Panel
    {
        /// <summary>Creates a new ImageView.</summary>
        public ImageView()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                ControlStyles.Opaque |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint, true);

            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);

            _bitmaps = new Bitmap[NumBitmaps];
            for(int i = 0; i < NumBitmaps; i++)
                _bitmaps[i] = new Bitmap(Display.Width, Display.Height, _pixelFormat);

            _rgb = new int[Display.Width * Display.Height];
            for(int i = 0; i < _rgb.Length; i++)
                _rgb[i] = -16777216; // Black
        }

        /// <summary>The number of bitmaps to use.</summary>
        private int NumBitmaps = 2;

        /// <summary>The image pixel format.</summary>
        private const PixelFormat _pixelFormat = PixelFormat.Format32bppRgb;

        /// <summary>Whether to use linear filtering.</summary>
        private bool _linearFilter;

        /// <summary>Gets or sets whether to use linear filtering.</summary>
        public bool LinearFilter
        {
            get { return _linearFilter; }
            set { _linearFilter = value; }
        }

        /// <summary>The bitmaps.</summary>
        private Bitmap[] _bitmaps;

        /// <summary>The index of the next bitmap to use.</summary>
        private int _bitmapIndex;

        /// <summary>The RGB buffer.</summary>
        private int[] _rgb;

        /// <summary>Updates the view.</summary>
        /// <param name="hovertank">The hovertank game engine.</param>
        public void UpdateView(Hovertank hovertank)
        {
            // Convert display to RGB with emulation of some EGA features
            byte[] videoBuffer = hovertank.Display.VideoBuffer;
            int[] palette = hovertank.Display.Palette;

            int srcIndex = hovertank.Display.ScreenStartIndex + hovertank.Display.PixelOffset;
            int dstIndex = 0;
            for(int y = 0; y < 200; y++)
            {
                if(y == hovertank.Display.SplitScreenLines)
                    srcIndex = 0;

                for(int x = 0; x < 320; x++)
                    _rgb[dstIndex++] = palette[videoBuffer[srcIndex++]];

                srcIndex += hovertank.Display.Stride - 320;
            }

            Bitmap bitmap = _bitmaps[_bitmapIndex++];
            _bitmapIndex %= NumBitmaps;

            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, 320, 200), ImageLockMode.WriteOnly, _pixelFormat);
            Marshal.Copy(_rgb, 0, bitmapData.Scan0, _rgb.Length);
            bitmap.UnlockBits(bitmapData);

            _paintBitmap = bitmap;

            Invalidate();
        }

        /// <summary>The bitmap to use for painting.</summary>
        private Bitmap _paintBitmap;

        /// <summary>Draws the image.</summary>
        /// <param name="e">The PaintEventArgs.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if(_paintBitmap == null)
            {
                using(SolidBrush brush = new SolidBrush(BackColor))
                {
                    e.Graphics.FillRectangle(brush, e.ClipRectangle);
                }
            }
            else
            {
                Rectangle destRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
                Rectangle srcRect = new Rectangle(0, 0, Display.Width, Display.Height);

                e.Graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;
                e.Graphics.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceCopy;

                if(_linearFilter)
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                else
                    e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                e.Graphics.DrawImage(_paintBitmap, destRect, srcRect, GraphicsUnit.Pixel);
            }
        }

    }
}
