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
        =============================================================================

		            Id Software EGA scaling routines

		                by John Carmack, 4-16-91

		                ------------------------


        These routines implement SPARSE SCALING, a very fast way to draw a given
        shape at various sizes.

        SC_Setup
        --------
        Builds tables used by the drawing routines to select which pixels to draw
        at different scales.  A scale of 256 is regular size, 128 half size, 512
        double size, etc.  Usually you won't need exact precision in scaling, so
        some tables are used for a few scales.  Basetables convert from a given
        pixel offset on the original shape to pixels on the scaled shape, screentables
        convert from a scaled offset to the original offset.

        SC_MakeShape
        ------------
        Converts a standard four plane pic or sprite of the given width/height to
        sparse scaling format.  The pixels are converted from four individual bits
        to segments of byte values (0-15).  Pixels of BACKGROUND color are considered
        masks, and will not apear in the scaled shape.  By considering the shape
        as a list of vertical line segments no masking is needed, as only visable
        pixels are scaled.

        SC_ScaleShape
        -------------
        Draws the shape CENTERED at the given x,y at the given scale, clipped to
        scalexl,scalexh,scaleyl,scaleyh (inclusive).  The drawing is done vertically
        so the bit mask register need only be set once for each line.

        SC_ScaleLine (ASM)
        ------------
        Low level scaling routine to scale a given source of byte values (0-15) to
        a given point on the screen.  No bounds checking.  This is self modifying
        unwound code (coding purists go hide), with a string of 200 scaling
        operations that get a RET stuck in the code after the number of pixels
        that need to be scaled have been.


        DEPENDENCIES
        ------------


        LIMITATIONS
        -----------
        Shapes must never scale over MAXHEIGHT pixels high, or the scale tables are
        invalid.  Greater than 256 character height is impossible without major
        changes in any case.

        The clipping bounds must be between 0 and 320 horizontally or the byte/mask
        tables are invalid.

        The segmented scaling isn't perfect


        POSSIBLE IMPROVEMENTS
        ---------------------
        256 color VGA scaling can be done over twice as fast as 16 color EGA
        because the pixels can be written out without having to load the latches!
        Sparsing the shape with horizontal scans would also make an improvement.

        SCALESTEP probably shouldn't be linear, as size change perception varies
        with the original size

        Seperate horizontal/vertical scaling for stretching effects

        vertical clipping

        Bitmap holography... (coming soon!)

        =============================================================================
        */

        private const ushort MAXPICHEIGHT = 256; // tallest pic to be scaled

        private const byte BACKGROUND = 5; // background pixel for make shape

        private const ushort BASESCALE = 64; // normal size

        private const ushort MAXSCALE = 256; // largest scale possible
    
        private const ushort SCALESTEP = 3;

        private const ushort DISCREETSCALES = (MAXSCALE / SCALESTEP);

        class scaleseg
        {
            public scaleseg(memptr pointer, ushort offset)
            {
                _pointer = pointer;
                _pointer.Offset(offset);
            }

            public scaleseg(memptr pointer)
                : this(pointer, 0)
            {
            }

            private memptr _pointer;

            public memptr Pointer
            {
                get { return _pointer; }
            }

            // relative to top of shape
            public short start
            {
                get { return _pointer.GetInt16(0); }
                set { _pointer.SetInt16(0, value); }
            }

            // pixels in this segment
            public short length
            {
                get { return _pointer.GetInt16(2); }
                set { _pointer.SetInt16(2, value); }
            }

            public const int FieldOffset_next = 4;

            // offset from segment, NULL if last segment
            public ushort next
            {
                get { return _pointer.GetUInt16(FieldOffset_next); }
                set { _pointer.SetUInt16(FieldOffset_next, value); }
            }

            // pixel values
            public byte Get_data(int elementIndex)
            {
                return _pointer.GetUInt8(6 + elementIndex);
            }

            public memptr PointerToData(int elementIndex)
            {
                return new memptr(_pointer.Buffer, _pointer.BaseIndex + 6 + elementIndex);
            }
        }

        class scaleshape
        {
            /// <summary>Creates a new scaleshape.</summary>
            /// <param name="pointer">A memory pointer.</param>
            public scaleshape(memptr pointer)
            {
                _pointer = pointer;
            }

            private memptr _pointer;

            public memptr Pointer
            {
                get { return _pointer; }
            }

            // number of vertical lines
            public short width
            {
                get { return _pointer.GetInt16(0); }
                set { _pointer.SetInt16(0, value); }
            }

            // only used for centering
            public short height
            {
                get { return _pointer.GetInt16(2); }
                set { _pointer.SetInt16(2, value); }
            }

            // offsets from segment to topmost segment on each line, NULL if no pixels on line
            public ushort first(int elementIndex)
            {
                return _pointer.GetUInt16(4 + elementIndex * 2);
            }

            public memptr PointerTofirst(int elementIndex)
            {
                return new memptr(_pointer.Buffer, _pointer.BaseIndex + 4 + elementIndex * 2);
            }
        }

        private const int SCREENPIXELS = 320;

        private byte[] bytetable = new byte[SCREENPIXELS];
        
        private byte[] masktable = new byte[SCREENPIXELS];

        // segment of basetables and screentables

        private memptr basetableseg;

        private memptr screentableseg;

        private ushort[] basetables = new ushort[DISCREETSCALES]; // offsets in basetableseg

        private ushort[] screentables = new ushort[DISCREETSCALES]; // offsets in screentableseg

        private short scalexl = 0;
        
        private short scalexh = 319;
        
        private short scaleyl = 0;

        private short scaleyh = 144;

        private ushort scaleblockwidth;

        private ushort scaleblockheight;

        private ushort scaleblockdest;

        //==========================================================================

        /*
        ===========================
        =
        = SC_Setup
        =
        ===========================
        */
        private void SC_Setup()
        {
            ushort mask, i, step, scale;
            ushort offset1, offset2, size;

            //
            // fast ploting tables
            //
            mask = 128;
            for(i = 0; i < 320; i++)
            {
                bytetable[i] = (byte) (i / 8);
                masktable[i] = (byte) mask;

                mask >>= 1;
                if(mask == 0)
                    mask = 128;
            }

            //
            // fast scaling tables
            //
            offset1 = 0;
            offset2 = 0;

            for(step = 0; step < DISCREETSCALES; step++)
            {
                screentables[step] = offset1;
                scale = (ushort) ((step + 1) * SCALESTEP);
                size = (ushort) (scale * MAXPICHEIGHT / BASESCALE);
                offset1 += (ushort) (size + 1);
                basetables[step] = offset2;
                offset2 += MAXPICHEIGHT;
            }

            MMGetPtr(ref basetableseg, offset2);
            MMGetPtr(ref screentableseg, offset1);

            for(step = 0; step < DISCREETSCALES; step++)
            {
                int basetablesegIndex = basetables[step];
                int screenptrIndex = screentables[step];

                scale = (ushort) ((step + 1) * SCALESTEP);
                size = (ushort) (scale * MAXPICHEIGHT / BASESCALE);

                for(i = 0; i < MAXPICHEIGHT; i++)
                    basetableseg.Buffer[basetablesegIndex++] = (byte) (scale * i / BASESCALE); // basetable

                for(i = 0; i <= size; i++)
                    screentableseg.Buffer[screenptrIndex++] = (byte) (i * BASESCALE / scale); // screentable
            }
        }

        //==========================================================================

        private ushort AsmRotate(byte data, ushort shift)
        {
            // as: converted from asm
            data >>= shift - 1;

            ushort result = 0;
            if((data & 1) != 0)
                result = 1;

            return result;
        }

        /*
        ===========================
        =
        = MakeShape
        =
        = Takes a raw bit map of width bytes by height and creates a scaleable shape
        =
        = Returns the length of the shape in bytes
        =
        ===========================
        */
        private void SC_MakeShape(memptr src, short width, short height, ref memptr shapeseg)
        {
            short pixwidth = (short) (width * 8);

            memptr tempseg_memptr = new memptr(); // as: added
            MMGetPtr(ref tempseg_memptr, pixwidth * (height + 20)); // larger than needed buffer

            scaleshape tempseg = new scaleshape(tempseg_memptr);

            tempseg.width = pixwidth; // pixel dimensions
            tempseg.height = height;

            //
            // convert ega pixels to byte color values in a temp buffer
            //
            // Stored in a collumn format, not rows!
            //
            memptr byteseg = new memptr();
            MMGetPtr(ref byteseg, pixwidth * height);

            memptr byteptr = new memptr(byteseg);

            memptr plane0 = new memptr(src);
            memptr plane1 = new memptr(plane0, width * height);
            memptr plane2 = new memptr(plane1, width * height);
            memptr plane3 = new memptr(plane2, width * height);

            for(short x = 0; x < width; x++)
            {
                for(ushort b = 0; b < 8; b++)
                {
                    ushort shift = (ushort) (8 - b);
                    ushort offset = (ushort) x;

                    for(short y = 0; y < height; y++)
                    {
                        byte by0 = plane0.GetUInt8(offset);
                        byte by1 = plane1.GetUInt8(offset);
                        byte by2 = plane2.GetUInt8(offset);
                        byte by3 = plane3.GetUInt8(offset);
                        offset += (ushort) width;

                        ushort color = 0;

                        // as: converted from asm
                        color |= AsmRotate(by3, shift);
                        color <<= 1;
                        color |= AsmRotate(by2, shift);
                        color <<= 1;
                        color |= AsmRotate(by1, shift);
                        color <<= 1;
                        color |= AsmRotate(by0, shift);

                        byteptr.SetUInt8(0, (byte) color);
                        byteptr.Offset(1);
                    } // Y
                } // B
            } // X

            //
            // convert byte map to sparse scaling format
            //
            memptr saveptr = tempseg.PointerTofirst(pixwidth);

            // start filling in data after all pointers to line segments
            byteptr = new memptr(byteseg); // first pixel in byte array

            for(short x = 0; x < pixwidth; x++)
            {
                //
                // each vertical line can have 0 or more segments of pixels in it
                //
                short y = 0;
                memptr segptr = tempseg.PointerTofirst(x);
                segptr.SetUInt16(0, 0); // in case there are no segments on line
                do
                {
                    // scan for first pixel to be scaled
                    while(y < height && byteptr.GetUInt8(0) == BACKGROUND) // as: bugfix - re-ordered to prevent out of bounds read
                    {
                        byteptr.Offset(1);
                        y++;
                    }

                    if(y == height) // if not, the line is finished
                        continue;

                    //
                    // start a segment by pointing the last link (either shape.first[x] if it
                    // is the first segment, or a seg.next if not) to the current spot in
                    // the tempseg, setting segptr to this segments next link, and copying
                    // all the pixels in the segment
                    //
                    segptr.SetUInt16(0, _sys.FP_OFF(saveptr)); // pointer to start of this segment

                    short start = y;
                    short length = 0;

                    scaleseg scale_seg = new scaleseg(saveptr);
                    
                    memptr dataptr = scale_seg.PointerToData(0);

                    //
                    // copy bytes in the segment to the shape
                    //
                    while(y < height && byteptr.GetUInt8(0) != BACKGROUND) // as: bugfix - re-ordered to prevent out of bounds read
                    {
                        length++;
                        dataptr.SetUInt8(0, byteptr.GetUInt8(0));
                        dataptr.Offset(1);
                        byteptr.Offset(1);
                        y++;
                    }

                    scale_seg.start = start;
                    scale_seg.length = length;
                    scale_seg.next = 0;

                    // get ready for next segment
                    segptr = new memptr(saveptr, scaleseg.FieldOffset_next);

                    saveptr = dataptr; // next free byte to be used

                } while(y < height);
            }

            //
            // allocate exact space needed and copy shape to it, then free buffers
            //
            MMGetPtr(ref shapeseg, _sys.FP_OFF(saveptr));
            Array.Copy(tempseg.Pointer.Buffer, shapeseg.Buffer, _sys.FP_OFF(saveptr));
            MMFreePtr(ref byteseg);
            MMFreePtr(ref tempseg_memptr);
        }

        //==========================================================================

        /*
        ====================
        =
        = SC_ScaleShape
        =
        = Scales the shape centered on x,y to size scale (256=1:1, 512=2:1, etc)
        =
        = Clips to scalexl/scalexh, scaleyl/scaleyh
        = Returns true if something was drawn
        =
        = Must be called in write mode 2!
        =
        ====================
        */
        private bool SC_ScaleShape(short x, short y, ushort scale, memptr shape)
        {
            short scalechop = (short) (scale / SCALESTEP - 1);

            if(scalechop < 0)
                return false; // can't scale this size

            if(scalechop >= DISCREETSCALES)
                scalechop = DISCREETSCALES - 1;

            memptr basetoscreenptr = new memptr(basetableseg, basetables[scalechop]);
            memptr screentobaseptr = new memptr(screentableseg, screentables[scalechop]);
            
            //
            // figure bounding rectangle for scaled image
            //
            scaleshape scaleshape_shape = new scaleshape(shape);
            ushort fullwidth = (ushort) scaleshape_shape.width;
            ushort fullheight = (ushort) scaleshape_shape.height;

            ushort scalewidth = (ushort) (fullwidth * ((scalechop + 1) * SCALESTEP) / BASESCALE);
            ushort scaleheight = basetoscreenptr.GetUInt8(fullheight - 1);

            short xl = (short) (x - scalewidth / 2);
            short xh = (short) (xl + scalewidth - 1);
            short yl = (short) (y - scaleheight / 2);
            short yh = (short) (yl + scaleheight - 1);

            // off screen?
            if(xl > scalexh || xh < scalexl || yl > scaleyh || yh < scaleyl)
                return false;

            //
            // clip to sides of screen
            //
            short sxl;
            if(xl < scalexl)
                sxl = scalexl;
            else
                sxl = xl;

            short sxh;
            if(xh > scalexh)
                sxh = scalexh;
            else
                sxh = xh;

            //
            // clip both sides to zbuffer
            //
            short sx = sxl;
            while(sx <= sxh && zbuffer[sx] > scale) // as: re-ordered to prevent out of bounds read
                sx++;

            sxl = sx;

            sx = sxh;
            while(sx > sxl && zbuffer[sx] > scale) // as: re-ordered to prevent out of bounds read
                sx--;

            sxh = sx;

            if(sxl > sxh)
                return false; // behind a wall

            //
            // save block info for background erasing
            //
            ushort screencorner = (ushort) (screenofs + yl * linewidth);

            scaleblockdest = (ushort) (screencorner + sxl / 8);
            scaleblockwidth = (ushort) (sxh / 8 - sxl / 8 + 1);
            scaleblockheight = (ushort) (yh - yl + 1);
            
            //
            // start drawing
            //
            for(sx = sxl; sx <= sxh; sx++)
            {
                short shapex = screentobaseptr.GetUInt8(sx - xl);
                ushort shapeofs = scaleshape_shape.first(shapex);
                if(shapeofs != 0)
                {
                    short width = 1; // as: added for line drawing
                    ushort mask = masktable[sx];
                    if(scale > BASESCALE)
                    {
                        //
                        // make a multiple pixel scale pass if possible
                        //
                        while(((sx & 7) != 7) && (screentobaseptr.GetUInt8(sx + 1 - xl) == shapex))
                        {
                            sx++;
                            mask |= masktable[sx];
                            width++;
                        }
                    }

                    // Draw vertical spans
                    do
                    {
                        scaleseg shapeptr = new scaleseg(shape, shapeofs);

                        ushort yoffset = basetoscreenptr.GetUInt8(shapeptr.start); // pixels on screen to be skipped

                        short offset = 0;
                        if(width > 1)
                            offset = (short) (width - 1);

                        // as: added start x and width
                        ScaleLine(
                            basetoscreenptr.GetUInt8(shapeptr.length),
                            screentobaseptr,
                            shapeptr.PointerToData(0),
                            (ushort) (screencorner + ylookup[yoffset]),
                            sx - offset, width);

                        shapeofs = shapeptr.next;

                    } while(shapeofs != 0);
                }
            }

            return true;
        }

    }
}
