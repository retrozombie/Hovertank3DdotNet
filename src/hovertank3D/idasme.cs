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
    partial class Hovertank
    {
        /*
        ;============================================================================
        ;
        ;                      EGA Graphic routines
        ;
        ;============================================================================
        */

        private ushort screenofs;

        private ushort linewidth;

        private ushort[] ylookup = new ushort[256];

        /*
        ;==============
        ;
        ; SetScreen
        ;
        ;==============
        */
        private void SetScreen(ushort crtc, ushort pel)
        {
            // as: Functional Description:
            // Wait for any active VBL to finish
            // Set screen start address
            // Wait for VBL to start
            // Set horizontal pixel offset
            // Adapted for simpler display emulation

            _display.ScreenStartIndex = crtc * Display.AddressScale;
            _display.PixelOffset = pel;
        }

        /*
        ;=============
        ;
        ; XPlot
        ;
        ; Xdraws one point
        ;
        ;=============
        */
        private void XPlot(short x, short y, short color)
        {
            // as: Functional Description
            // Xor plot a pixel
            // Adapted for simpler display emulation

            int dstIndex = screenofs * Display.AddressScale + ylookup[y] * Display.ColumnScale + x;
            _display.VideoBuffer[dstIndex] ^= (byte) (color & 0x0f);
        }

        /*
        ;=============
        ;
        ; DrawChar (int xcoord, int ycoord, int charnum)
        ;
        ; xcoord in bytes, ycoord in pixels
        ;
        ; Source is grsegs[STARTTILE8+charnum]
        ;
        ;=============
        */
        private void DrawChar(ushort xcoord, ushort ycoord, short charnum)
        {
            // as: Functional Description
            // Draw an 8x8 4 plane image 1 plane at a time
            // Adapted for simpler display emulation
            // Graphics are converted at load time to 8BPP format

            byte[] fontBuffer = _picCache[STARTTILE8];
            int srcIndex = charnum * 64;
            int dstIndex = screenofs * Display.AddressScale + (ylookup[ycoord] + xcoord) * Display.ColumnScale;
            byte[] videoBuffer = _display.VideoBuffer;
            int stride = linewidth * Display.ColumnScale;
            for(int y = 0; y < 8; y++)
            {
                Array.Copy(fontBuffer, srcIndex, videoBuffer, dstIndex, 8);
                srcIndex += 8;
                dstIndex += stride;
            }
        }

        /*
        ;============
        ;
        ; DrawPic (int xcoord, int ycoord, int picnum)
        ;
        ; xcoord in bytes, ycoord in pixels
        ;
        ;============
        */
        private void DrawPic(ushort xcoord, ushort ycoord, short picnum)
        {
            // as: Functional Description
            // Draw an width x height 4 plane image 1 plane at a time
            // Adapted for simpler display emulation
            // Graphics are converted at load time to 8BPP format

            pictype picType = pictable[picnum];

            byte[] pic = _picCache[STARTPICS + picnum];

            if(pic == null)
                pic = _picCache.CachePic(STARTPICS + picnum, grsegs[STARTPICS + picnum], picType);

            int picWidth = picType.width * Display.ColumnScale;
            int dstIndex = screenofs * Display.AddressScale + (ylookup[ycoord] + xcoord) * Display.ColumnScale;
            int srcIndex = 0;
            byte[] videoBuffer = _display.VideoBuffer;
            for(int y = 0; y < picType.height; y++)
            {
                Array.Copy(pic, srcIndex, videoBuffer, dstIndex, picWidth);
                srcIndex += picWidth;
                dstIndex += _display.Stride;
            }
        }

        /*
        ;============
        ;
        ; Bar (int xl,yl,width,height)
        ;
        ; xcoord in bytes, ycoord in pixels
        ;
        ;============
        */
        private void Bar(ushort xl, ushort yl, ushort wide, ushort height, ushort fill)
        {
            // as: Functional Description
            // Fill a bar starting at the column specified by 'xl', a column width specified by 'wide', 
            // a height in lines specified by 'height' and a pattern written to all planes specified by 'fill'
            // Adapted for simpler display emulation

            int dstIndex = screenofs * Display.AddressScale + (ylookup[yl] + xl) * Display.ColumnScale;
            int stride = (linewidth - wide) * Display.ColumnScale;
            byte[] videoBuffer = _display.VideoBuffer;
            wide *= Display.ColumnScale;
            for(int i = 0; i < height; i++)
            {
                for(int j = 0; j < wide; j++)
                    videoBuffer[dstIndex++] = 15;

                dstIndex += stride;
            }
        }

        /*
        ;============
        ;
        ; CopyEGA
        ;
        ; Must be in latch mode
        ;
        ;============
        */
        private void CopyEGA(ushort wide, ushort height, ushort source, ushort dest)
        {
            // as: Functional Description
            // This copies a rectangular area of the screen from the source offset to the destination offset
            // Required WriteMode 1 to be active and typically MapMask = 15
            // wide = width in bytes (8 pixel blocks)
            // height = number of lines
            // source = source offset
            // source = dest offset
            // i.e. CopyEGA(40, 200, 0, 0x4000) would copy the whole screen from page 0 (0xA0000) to page 1 (0xA4000)
            // Adapted for simpler display emulation

            _display.Copy(wide * 8, height, source * Display.AddressScale, dest * Display.AddressScale);
        }

        // 0-16 mapmask value
        private ushort fontcolor = 15;

        private ushort px;

        private ushort py;

        private fontstruct fontseg;

        /*
        ;==================
        ;
        ; DrawPchar
        ; Draws a proportional character at px,py, and increments px
        ;
        ;==================
        */
        private void DrawPchar(ushort charnum)
        {
            // as: Functional Description:
            // Draw a character from the proportional font and adjust px
            // The asm used shifttables to rotate the image and selected
            // different drawing subroutines based upon the width of the character
            // Pixels were then XORed onto the screen 8 at a time
            // Adapted for simpler display emulation

            int offset = fontseg.Get_location(charnum);
            ushort width = (ushort) (fontseg.Get_width(charnum) & 0xff);
            int height = fontseg.height;
            int srcIndex = fontseg.Pointer.BaseIndex + offset;
            srcIndex += height * ((width + 7) >> 3); // Advance passed mask
            byte[] videoBuffer = _display.VideoBuffer;
            int dstIndex = screenofs * Display.AddressScale + ylookup[py] * Display.ColumnScale + px;
            byte colorIndex = (byte) fontcolor;
            int stride = linewidth * Display.ColumnScale;
            for(int y = 0; y < height; y++)
            {
                byte mask = 0;
                byte data = 0;
                for(int x = 0; x < width; x++)
                {
                    if(mask == 0)
                    {
                        mask = 0x80;
                        data = fontseg.Pointer.GetUInt8(srcIndex++);
                    }

                    if((data & mask) != 0)
                        videoBuffer[dstIndex + x] ^= colorIndex;
                    
                    mask >>= 1;
                }

                dstIndex += stride;
            }

            px += width;
        }
        /*
        ;===========================================================================
        ;
        ;                    SCALING GRAPHICS
        ;
        ;===========================================================================
        */

        /*
        ;================
        ;
        ; doline
        ;
        ; Big unwound scaling routine
        ;
        ;================

        ;==================================================
        ;
        ; void scaleline (int scale, unsigned picseg, unsigned maskseg,
        ;                 unsigned screen, unsigned width)
        ;
        ;==================================================
        */
        private void ScaleLine(ushort pixels, memptr scaleptr, memptr picptr, ushort screen, int x, int width)
        {
            // as: Functional Description:
            // This function modified the code in the subroutine doline to limit the number of pixels drawn to
            // the value specified by "pixels", after doline completed the previous code was then restored
            // Adapted for simpler display emulation
            //
            // dx = linewidth
            // es:di = Pointer to screen
            // ds:si = Pointer to scaling lookup table
            // as:bx = Pointer to picture
            // doline is called
            // doline is restored
            // return
            //
            // doline
            //      This function is up to 256 draw pixel operations, ScaleLine modifies the code depending upon the length of the
            //      line segment to draw writing a "mov ss, cx" followed by a "ret" instruction at the appropriate position
            //  cx <- ss : Save Stack Segment in CX
            //  { // This section repeated 256 times
            //      al <- [ds:si], si++ // This is the scaled pixel index (ds:si = pointer to scaling lookup table)
            //      al <- [ss:bx + al] // Read the pixel (ss:bx = pointer to picture)
            //      al <- [es:di] // Read video ram to latches, write pixel to screen
            //      di += dx // Next line down screen
            //  }
            //  ss <- cx : Restore Stack Segment from CX
            //  return

            int dstIndex = screen * Display.AddressScale + x;
            byte[] videoBuffer = _display.VideoBuffer;
            for(int i = 0; i < pixels; i++)
            {
                byte offset = scaleptr.GetUInt8(i);
                byte color = picptr.GetUInt8(offset);
                
                for(int j = 0; j < width; j++)
                    videoBuffer[dstIndex + j] = color;

                dstIndex += _display.Stride;
            }
        }

    }
}
