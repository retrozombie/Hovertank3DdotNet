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

#define DEBUG_IGNORE_RENDER_FAILS

#if !PROFILE
#define ADAPTIVE
#endif

using System;

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        private void HovDrawInitialise()
        {
            // as: bugfix - Extended sintable length by 1, BuildTables writes one element past the end of the array
            sintable = new fixed_t[ANGLES + ANGLES / 4 + 1];
            costable = new costable_t(sintable);

            for(short i = 0; i < walls.Length; i++)
                walls[i] = new walltype(i);
        }

        /*
        ============================================================================

		               3 - D  DEFINITIONS

        ============================================================================
        */

        private fixed_t focallength = FOCALLENGTH;

        private fixed_t mindist = MINDIST;

        private fixed_t scale;

        private tilept tile = new tilept();

        private tilept focal = new tilept(); // focal point in tiles
        
        private tilept right = new tilept(); // rightmost tile in view

        private short[] segstart = new short[VIEWHEIGHT]; // addline tracks line segment and draws
	    
        private short[] segend = new short[VIEWHEIGHT];
	
        private short[] segcolor = new short[VIEWHEIGHT]; // only when the color changes

        private void ADDLINE(short a, short b, short y, short c)
        {
            if(a > segend[y] + 1)
            {
                if(y > CENTERY)
                    DrawLine((short) (segend[y] + 1), (short) (a - 1), y, 8);
                else
                    DrawLine((short) (segend[y] + 1), (short) (a - 1), y, 0);
            }

            DrawLine(a, a, y, 0);

            if(a + 1 <= b)
                DrawLine((short) (a + 1), b, y, c);

            segend[y] = b;
        }

        private const int MAXWALLS = 100;

        private walltype[] walls = new walltype[MAXWALLS];

        private walltype rightwall;

        //==========================================================================

        //
        // refresh stuff
        //

        //
        // calculate location of screens in video memory so they have the
        // maximum possible distance seperating them (for scaling overflow)
        //

        private const ushort EXTRALINES = (ushort) (0x10000L / SCREENWIDTH - STATUSLINES - VIEWHEIGHT * 3);

        private ushort[] screenloc = new ushort[]
        {
            ((STATUSLINES + EXTRALINES / 4) * SCREENWIDTH) & 0xff00,
            ((STATUSLINES + EXTRALINES / 2 + VIEWHEIGHT) * SCREENWIDTH) & 0xff00,
            ((STATUSLINES + 3 * EXTRALINES / 4 + 2 * VIEWHEIGHT) * SCREENWIDTH) & 0xff00
        };

        private short screenpage;

        private short tics;

        private int lasttimecount;

        private const int SHIFTFRAMES = 256;

        private short[] yshift = new short[SHIFTFRAMES]; // screen sliding variables

        //
        // rendering stuff
        //

        private short firstangle;

        private short lastangle;

        private fixed_t prestep;

        private fixed_t[] sintable;

        private costable_t costable;

        private fixed_t viewx; // the focal point

        private fixed_t viewy;

        private short viewangle;

        private fixed_t viewsin;

        private fixed_t viewcos;

        private short[] zbuffer = new short[VIEWXH + 1]; // holds the height of the wall at that point

        //==========================================================================

        /*
        ==================
        =
        = DrawLine
        =
        = Must be in write mode 2 with all planes enabled
        = The bit mask is left set to the end value, so clear it after all lines are
        = drawn
        =
        ==================
        */
        private void DrawLine(short xl, short xh, short y, short color)
        {
            if(xh < xl)
                Quit("DrawLine: xh<xl");

            if(y < VIEWY)
                Quit("DrawLine: y<VIEWY");

            if(y > VIEWYH)
                Quit("DrawLine: y>VIEWYH");

            // as: converted from asm
            // Adapted for simpler display emulation

            byte[] videoBuffer = _display.VideoBuffer;
            int dstIndex = screenofs * Display.AddressScale + ylookup[y] * Display.ColumnScale + xl;
            byte colorIndex = (byte) color;
            for(int x = xl; x <= xh; x++)
                videoBuffer[dstIndex++] = colorIndex;
        }

        //==========================================================================

        // as: The following drawWall_ variables were static in DrawWall

        private walltype drawWall_wall = new walltype(-1);
  
        private short drawWall_y1l;

        private short drawWall_y1h;

        private short drawWall_y2l;
        
        private short drawWall_y2h;

        /*
        ===================
        =
        = DrawWall
        =
        = Special polygon with vertical edges and symetrical top / bottom
        = Clips horizontally to clipleft/clipright
        = Clips vertically to VIEWY/VIEWYH
        = Should only be called if the wall is at least partially visable
        =
        ==================
        */
        private void DrawWall(walltype wallptr)
        {
            wallptr.CopyTo(drawWall_wall);

            short i = (short) (drawWall_wall.height1 / 2);
            drawWall_y1l = (short) (CENTERY - i);
            drawWall_y1h = (short) (CENTERY + i);

            i = (short) (drawWall_wall.height2 / 2);
            drawWall_y2l = (short) (CENTERY - i);
            drawWall_y2h = (short) (CENTERY + i);

            if(drawWall_wall.x1 > drawWall_wall.leftclip)
                drawWall_wall.leftclip = drawWall_wall.x1;

            if(drawWall_wall.x2 < drawWall_wall.rightclip)
                drawWall_wall.rightclip = drawWall_wall.x2;

            //
            // fill in the zbuffer
            //
            int height = (int) drawWall_wall.height1 << 16;
            int heightstep;
            if(drawWall_wall.x2 != drawWall_wall.x1)
                heightstep = ((int) (drawWall_wall.height2 - drawWall_wall.height1) << 16) / (short) (drawWall_wall.x2 - drawWall_wall.x1);
            else
                heightstep = 0;

            i = (short) (drawWall_wall.leftclip - drawWall_wall.x1);
            if(i != 0)
                height += heightstep * i; // adjust for clipped area

            for(short x = drawWall_wall.leftclip; x <= drawWall_wall.rightclip; x++)
            {
                zbuffer[x] = (short) (height >> 16);
                height += heightstep;
            }

            //
            // draw the wall to the line buffer
            //
            if(drawWall_y1l == drawWall_y2l)
            {
                //
                // rectangle, no slope
                //
                if(drawWall_y1l < VIEWY)
                    drawWall_y1l = VIEWY;

                if(drawWall_y1h > VIEWYH)
                    drawWall_y1h = VIEWYH;

                for(short y = drawWall_y1l; y <= drawWall_y1h; y++)
                    ADDLINE(drawWall_wall.leftclip, drawWall_wall.rightclip, y, (short) drawWall_wall.color);

                return;
            }
            
            if(drawWall_y1l < drawWall_y2l)
            {
                //
                // slopes down to the right
                //
                short slope = (short) (((int) (drawWall_wall.x2 - drawWall_wall.x1) << 6) / (drawWall_y2l - drawWall_y1l)); // in 128ths

                short ysteps = (short) (drawWall_y2l - drawWall_y1l);
                if(drawWall_y1l < VIEWY)
                    ysteps -= (short) (VIEWY - drawWall_y1l);

                short endfrac = (short) (drawWall_wall.x2 << 6);
                for(short y = 1; y < ysteps; y++) // top and bottom slopes
                {
                    endfrac -= slope;
                    short end = (short) (endfrac >> 6);
                    if(end > drawWall_wall.rightclip)
                        end = drawWall_wall.rightclip;
                    else if(end < drawWall_wall.leftclip) // the rest is hidden
                        break;

                    ADDLINE(drawWall_wall.leftclip, end, (short) (drawWall_y2l - y), (short) drawWall_wall.color);
                    ADDLINE(drawWall_wall.leftclip, end, (short) (drawWall_y2h + y), (short) drawWall_wall.color);
                }

                if(drawWall_y2l < VIEWY)
                    drawWall_y2l = VIEWY;

                if(drawWall_y2h > VIEWYH)
                    drawWall_y2h = VIEWYH;

                for(short y = drawWall_y2l; y <= drawWall_y2h; y++) // middle
                    ADDLINE(drawWall_wall.leftclip, drawWall_wall.rightclip, y, (short) drawWall_wall.color);
            }
            else
            {
                //
                // slopes down to the left
                //
                short slope = (short) (((int) (drawWall_wall.x2 - drawWall_wall.x1) << 6) / (drawWall_y1l - drawWall_y2l)); // in 128ths

                short ysteps = (short) (drawWall_y1l - drawWall_y2l);
                if(drawWall_y2l < VIEWY)
                    ysteps -= (short) (VIEWY - drawWall_y2l);

                short endfrac = (short) (drawWall_wall.x1 << 6);
                for(short y = 1; y < ysteps; y++) // top and bottom slopes
                {
                    endfrac += slope;
                    short end = (short) (endfrac >> 6);

                    if(end < drawWall_wall.leftclip)
                        end = drawWall_wall.leftclip;
                    else if(end > drawWall_wall.rightclip) // the rest is hidden
                        break;

                    ADDLINE(end, drawWall_wall.rightclip, (short) (drawWall_y1l - y), (short) drawWall_wall.color);
                    ADDLINE(end, drawWall_wall.rightclip, (short) (drawWall_y1h + y), (short) drawWall_wall.color);
                }
                if(drawWall_y1l < VIEWY)
                    drawWall_y1l = VIEWY;

                if(drawWall_y1h > VIEWYH)
                    drawWall_y1h = VIEWYH;

                for(short y = drawWall_y1l; y <= drawWall_y1h; y++) // middle
                    ADDLINE(drawWall_wall.leftclip, drawWall_wall.rightclip, y, (short) drawWall_wall.color);
            }
        }

        //==========================================================================

        /*
        =================
        =
        = TraceRay
        =
        = Used to find the left and rightmost tile in the view area to be traced from
        = Follows a ray of the given angle from viewx,viewy in the global map until
        = it hits a solid tile
        = sets:
        =   tile.x,tile.y	: tile coordinates of contacted tile
        =   tilecolor	: solid tile's color
        =
        ==================
        */
        private void TraceRay(ushort angle)
        {
            int tracexstep = costable[angle];
            int traceystep = sintable[angle];

            //
            // advance point so it is even with the view plane before we start checking
            //
            fixed_t fixtemp = FixedByFrac(prestep, tracexstep);
            int tracex = FixedAdd(viewx, fixtemp);
            fixtemp = FixedByFrac(prestep, traceystep);
            int tracey = FixedAdd(viewy, fixtemp ^ SIGNBIT);

            if((tracexstep & SIGNBIT) != 0) // use 2's complement, not signed magnitude
                tracexstep = -(tracexstep & ~SIGNBIT);

            if((traceystep & SIGNBIT) != 0) // use 2's complement, not signed magnitude
                traceystep = -(traceystep & ~SIGNBIT);

            tile.x = (short) (tracex >> TILESHIFT); // starting point in tiles
            tile.y = (short) (tracey >> TILESHIFT);

            //
            // we assume viewx,viewy is not inside a solid tile, so go ahead one step
            //

            do // until a solid tile is hit
            {
                short otx = tile.x;
                short oty = tile.y;
                tracex += tracexstep;
                tracey -= traceystep;
                tile.x = (short) (tracex >> TILESHIFT);
                tile.y = (short) (tracey >> TILESHIFT);

                if(tile.x != otx && tile.y != oty &&
                    (tilemap[otx, tile.y] != 0 || tilemap[tile.x, oty] != 0))
                {
                    //
                    // trace crossed two solid tiles, so do a binary search along the line
                    // to find a spot where only one tile edge is crossed
                    //
                    short searchsteps = 0;
                    int searchx = tracexstep;
                    int searchy = traceystep;
                    do
                    {
                        searchx /= 2;
                        searchy /= 2;

                        if(tile.x != otx && tile.y != oty)
                        {
                            // still too far
                            tracex -= searchx;
                            tracey += searchy;
                        }
                        else
                        {
                            // not far enough, no tiles crossed
                            tracex += searchx;
                            tracey -= searchy;
                        }

                        //
                        // if it is REAL close, go for the most clockwise intersection
                        //
                        if(++searchsteps == 16)
                        {
                            tracex = ((int) otx) << TILESHIFT;
                            tracey = ((int) oty) << TILESHIFT;

                            if(tracexstep > 0)
                            {
                                if(traceystep < 0)
                                {
                                    tracex += TILEGLOBAL - 1;
                                    tracey += TILEGLOBAL;
                                }
                                else
                                {
                                    tracex += TILEGLOBAL;
                                }
                            }
                            else
                            {
                                if(traceystep < 0)
                                {
                                    tracex--;
                                    tracey += TILEGLOBAL - 1;
                                }
                                else
                                {
                                    tracey--;
                                }
                            }
                        }

                        tile.x = (short) (tracex >> TILESHIFT);
                        tile.y = (short) (tracey >> TILESHIFT);

                    } while((tile.x != otx && tile.y != oty) || (tile.x == otx && tile.y == oty));
                }
            } while(tilemap[tile.x, tile.y] == 0);
        }

        //==========================================================================

        /*
        ========================
        =
        = FixedByFrac
        =
        = multiply a 16/16 bit fixed point number by a 16 bit fractional number
        = both unsigned (handle signs seperately)
        =
        ========================
        */
        private fixed_t FixedByFrac(fixed_t a, fixed_t b)
        {
            // as: Converted from asm
            ushort si = (ushort) (a >> 16);
            si ^= (ushort) (b >> 16);
            si &= 0x8000; // si is high word of result (sign bit)

            ushort bx = (ushort) b;
            ushort ax = (ushort) a;

            uint dxax = ax;
            dxax *= bx; // fraction * fraction

            ushort di = (ushort) (dxax >> 16); // di is low word of result

            ax = (ushort) (a >> 16);
            ax &= 0x7fff; // strip sign bit

            dxax = ax;
            dxax *= bx; // units*fraction
            ax = (ushort) dxax;
            ushort dx = (ushort) (dxax >> 16);

            if(ax + di > 65535)
                dx++; // adc dx,0

            ax += di;
            dx |= si;

            fixed_t value = dx;
            value <<= 16;
            value |= ax;

            return value;
        }

        /*
        =========================
        =
        = FixedAdd
        =
        = add two 16 bit fixed point numbers
        = to subtract, invert the sign of B before invoking
        =
        =========================
        */
        private fixed_t FixedAdd(fixed_t a, fixed_t b)
        {
            // as: Converted from asm

            ushort ax = (ushort) a;
            ushort dx = (ushort) (a >> 16);

            ushort bx = (ushort) b;
            ushort cx = (ushort) (b >> 16);

            if((dx & 0x8000) != 0) // negative?
            {   // convert a from signed magnitude to 2's compl
                dx &= 0x7fff;
                ax ^= 0xffff;
                dx ^= 0xffff;

                if(ax + 1 > 0xffff)
                    dx++; // adc dx,0

                ax++;
            }

            if((cx & 0x8000) != 0) // negative?
            {   // convert b from signed magnitude to 2's compl
                cx &= 0x7fff;
                bx ^= 0xffff;
                cx ^= 0xffff;

                if(bx + 1 > 0xffff)
                    cx++; // adc cx,0

                bx++;
            }

            // perform the addition
            dx += cx;
            if(ax + bx > 0xffff)
                dx++;

            ax += bx;

            if((dx & 0x8000) != 0)
            {   // value was negative
                // back to signed magnitude
                dx &= 0x7fff;
                ax ^= 0xffff;
                dx ^= 0xffff;

                if(ax + 1 > 0xffff)
                    dx++; // adc dx, 0

                ax++;
            }

            fixed_t value = dx;
            value <<= 16;
            value |= ax;
            return value;
        }

        //==========================================================================

        private const short MINRATIO = 16;

        /*
        ========================
        =
        = TransformPoint
        =
        = Takes paramaters:
        =   gx,gy		: globalx/globaly of point
        =
        = globals:
        =   viewx,viewy		: point of view
        =   viewcos,viewsin	: sin/cos of viewangle
        =
        =
        = defines:
        =   CENTERX		: pixel location of center of view window
        =   TILEGLOBAL		: size of one
        =   FOCALLENGTH		: distance behind viewx/y for center of projection
        =   scale		: conversion from global value to screen value
        =
        = returns:
        =   screenx,screenheight: projected edge location and size
        =
        ========================
        */
        private void TransformPoint(fixed_t gx, fixed_t gy, ref short screenx, ref ushort screenheight)
        {
            //
            // translate point to view centered coordinates
            //
            gx = FixedAdd(gx, viewx | SIGNBIT);
            gy = FixedAdd(gy, viewy | SIGNBIT);

            //
            // calculate newx
            //
            fixed_t gxt = FixedByFrac(gx, viewcos);
            fixed_t gyt = FixedByFrac(gy, viewsin);
            fixed_t nx = FixedAdd(gxt, gyt ^ SIGNBIT);

            //
            // calculate newy
            //
            gxt = FixedByFrac(gx, viewsin);
            gyt = FixedByFrac(gy, viewcos);
            fixed_t ny = FixedAdd(gyt, gxt);

            //
            // calculate perspective ratio
            //
            if(nx < 0)
                nx = 0;

            short ratio = (short) (nx * scale / FOCALLENGTH);

            if(ratio <= MINRATIO)
                ratio = MINRATIO;

            if((ny & SIGNBIT) != 0)
                screenx = (short) (CENTERX - (ny & ~SIGNBIT) / ratio);
            else
                screenx = (short) (CENTERX + ny / ratio);

            screenheight = (ushort) (TILEGLOBAL / ratio);
        }

        //==========================================================================

        private fixed_t TransformX(fixed_t gx, fixed_t gy)
        {
            //
            // translate point to view centered coordinates
            //
            gx = FixedAdd(gx, viewx | SIGNBIT);
            gy = FixedAdd(gy, viewy | SIGNBIT);

            //
            // calculate newx
            //
            fixed_t gxt = FixedByFrac(gx, viewcos);
            fixed_t gyt = FixedByFrac(gy, viewsin);

            return FixedAdd(gxt, gyt ^ SIGNBIT);
        }

        //==========================================================================

        /*
        ==================
        =
        = BuildTables
        =
        = Calculates:
        =
        = scale			projection constant
        = sintable/costable	overlapping fractional tables
        = firstangle/lastangle	angles from focalpoint to left/right view edges
        = prestep		distance from focal point before checking for tiles
        = yshift[]		screen bouncing patters
        =
        ==================
        */

        private const float PI = 3.141592657f;

        private const int ANGLEQUAD = (ANGLES / 4);

        private void BuildTables()
        {
            //
            // calculate scale value so one tile at mindist allmost fills the view vertical
            //
            scale = GLOBAL1 / VIEWWIDTH;
            scale *= focallength;
            scale /= (focallength + mindist);

            //
            // costable overlays sintable with a quarter phase shift
            // ANGLES is assumed to be divisable by four
            //

            float angle = 0;
            float anglestep = PI / 2 / ANGLEQUAD;
            for(short i = 0; i <= ANGLEQUAD; i++)
            {
                fixed_t value = (fixed_t) (GLOBAL1 * Math.Sin(angle));

                sintable[i] = value;
                sintable[i + ANGLES] = value; // as: Out of bounds when i == ANGLEQUAD
                sintable[ANGLES / 2 - i] = value;

                sintable[ANGLES - i] = value | SIGNBIT;
                sintable[ANGLES / 2 + i] = value | SIGNBIT;

                angle += anglestep;
            }

            //
            // figure trace angles for first and last pixel on screen
            //
            angle = (float) Math.Atan((float) VIEWWIDTH / 2 * scale / FOCALLENGTH);
            angle *= ANGLES / (PI * 2);

            short intang = (short) (((short) angle) + 1);
            firstangle = intang;
            lastangle = (short) -intang;

            prestep = (fixed_t) (GLOBAL1 * ((float) FOCALLENGTH / costable[firstangle]));

            //
            // hover screen shifting
            //
            for(short i = 0; i < SHIFTFRAMES; i++)
            {
                angle = (int) ANGLES * i / SHIFTFRAMES;
                fixed_t value = FixedByFrac(7 * GLOBAL1, sintable[(int) angle]);
                yshift[i] = (short) (SCREENWIDTH * (FixedAdd(value, 8 * GLOBAL1) >> 16));
            }

            //
            // misc stuff
            //
            walls[0].x2 = VIEWX - 1;
            walls[0].height2 = 32000;
        }

        //==========================================================================

        /*
        =================
        =
        = StartView
        =
        = Called by player think
        =
        =================
        */
        private void StartView()
        {
            //
            // set up variables for this view
            //
            viewangle = objlist[0].angle;
            viewsin = sintable[viewangle];
            viewcos = costable[viewangle];
            viewx = FixedAdd(objlist[0].x, FixedByFrac(FOCALLENGTH, viewcos) ^ SIGNBIT);
            viewy = FixedAdd(objlist[0].y, FixedByFrac(FOCALLENGTH, viewsin));

            focal.x = (short) (viewx >> TILESHIFT);
            focal.y = (short) (viewy >> TILESHIFT);

            //
            // find the rightmost visable tile in view
            //
            short tracedir = (short) (viewangle + lastangle);

            if(tracedir < 0)
                tracedir += ANGLES;
            else if(tracedir >= ANGLES)
                tracedir -= ANGLES;

            TraceRay((ushort) tracedir);
            right.x = tile.x;
            right.y = tile.y;

            //
            // find the leftmost visable tile in view
            //
            tracedir = (short) (viewangle + firstangle);
            
            if(tracedir < 0)
                tracedir += ANGLES;
            else if(tracedir >= ANGLES)
                tracedir -= ANGLES;
            
            TraceRay((ushort) tracedir);

            //
            // follow the walls from there to the right
            //
            rightwall = walls[1];

#if DEBUG_IGNORE_RENDER_FAILS
            // FIXME as: Prevent rendering errors crashing the game
            try { FollowWalls(); }
            catch { }
#else
            FollowWalls();
#endif
        }

        //==========================================================================

        /*
        =====================
        =
        = DrawWallList
        =
        = Clips and draws all the walls traced this refresh
        =
        =====================
        */
        private void DrawWallList()
        {
            Array.Clear(segstart, 0, segstart.Length); // start lines at 0

            for(int index = 0; index < segend.Length; index++)
                segend[index] = -1; // end lines at -1

            for(int index = 0; index < segcolor.Length; index++)
                segcolor[index] = -1; // with color -1

            rightwall.x1 = VIEWXH + 1;
            rightwall.height1 = 32000;
            walls[rightwall.index + 1].x1 = 32000;

            short leftx = -1;
            for(short wallIndex = 1; wallIndex < rightwall.index && leftx <= VIEWXH; wallIndex++)
            {
                walltype wall = walls[wallIndex];

                if(leftx >= wall.x2)
                    continue;

                short rightclip = wall.x2;

                short checkIndex = (short) (wallIndex + 1);
                walltype check = walls[checkIndex];
                while(check.x1 <= rightclip && check.height1 >= wall.height2)
                {
                    rightclip = (short) (check.x1 - 1);
                    checkIndex++;
                    check = walls[checkIndex];
                }

                if(rightclip > VIEWXH)
                    rightclip = VIEWXH;

                short newleft;
                if(leftx < wall.x1 - 1)
                    newleft = (short) (wall.x1 - 1); // there was black space between walls
                else
                    newleft = leftx;

                if(rightclip > newleft)
                {
                    wall.leftclip = (short) (newleft + 1);
                    wall.rightclip = rightclip;
                    DrawWall(wall);
                    leftx = rightclip;
                }
            }

            //
            // finish all lines to the right edge
            //
            short i;
            for(i = 0; i < CENTERY; i++)
                if(segend[i] < VIEWXH)
                    DrawLine((short) (segend[i] + 1), VIEWXH, i, 0);

            for(; i < VIEWHEIGHT; i++)
                if(segend[i] < VIEWXH)
                    DrawLine((short) (segend[i] + 1), VIEWXH, i, 8);
        }

        //==========================================================================

        private short[] depthsort = new short[MAXOBJECTS];

        private short[] sortheight = new short[MAXOBJECTS];

        private short[] obscreenx = new short[MAXOBJECTS];
	
        private short[] obscreenheight = new short[MAXOBJECTS];
	
        private short[] obshapenum = new short[MAXOBJECTS];

        /*
        =====================
        =
        = DrawScaleds
        =
        = Draws all objects that are visable
        =
        =====================
        */
        private void DrawScaleds()
        {
            short numvisable = 0;

            //
            // calculate base positions of all objects
            //
            for(short objIndex = 1; objIndex <= lastobjIndex; objIndex++)
            {
                objtype obj = objlist[objIndex];
                if(obj._class != 0)
                {
                    viewx = obj.viewx - obj.size; // now value of nearest edge
                    if(viewx >= FOCALLENGTH + MINDIST)
                    {
                        ushort ratio = (ushort) (viewx * scale / FOCALLENGTH);
                        short screenx = (short) (CENTERX + obj.viewy / ratio);
                        short screenheight = (short) (TILEGLOBAL / ratio);
                        if(screenx > -128 && screenx < 320 + 128)
                        {
                            obscreenx[numvisable] = screenx;
                            obscreenheight[numvisable] = screenheight;
                            obshapenum[numvisable] = obj.shapenum;
                            numvisable++;
                        }
                    }
                }
            }

            if(numvisable == 0)
                return;

            //
            // sort in order of increasing height
            //
            short i, j;
            for(i = 0; i < numvisable; i++)
            {
                short least = 32000;
                short leastnum = 0; // as: Prevent C# Use of unassigned variable error
                for(j = 0; j < numvisable; j++)
                {
                    if(obscreenheight[j] < least)
                    {
                        leastnum = j;
                        least = obscreenheight[j];
                    }
                }

                depthsort[i] = leastnum;
                sortheight[i] = least;
                obscreenheight[leastnum] = 32000;
            }

            //
            // draw in order
            //
            for(i = 0; i < numvisable; i++)
            {
                j = depthsort[i];
                SC_ScaleShape(obscreenx[j], (short) (CENTERY + 5), (ushort) sortheight[i], scalesegs[obshapenum[j]]);
            }
        }

        //==========================================================================

        /*
        ====================
        =
        = DrawCrossHairs
        =
        = Should still be in write mode 2
        =
        ====================
        */
        private const short CROSSSIZE = 40;

        private void DrawCrossHairs()
        {
            // as: converted from asm
            // Adapted for simpler display emulation

            byte[] videoBuffer = _display.VideoBuffer;

            // Bitmask = 00111100 @ [20]
            int dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (64 - CROSSSIZE / 2) + 20) * Display.ColumnScale + 2;
            for(int i = 0; i < CROSSSIZE; i++)
            {
                videoBuffer[dstIndex] = 0;
                videoBuffer[dstIndex + 1] = 0;
                videoBuffer[dstIndex + 2] = 0;
                videoBuffer[dstIndex + 3] = 0;
                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }

            // Bitmask = 11111111 @ [18]
            dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (82 - CROSSSIZE / 2) + 18) * Display.ColumnScale;
            for(int i = 0; i < 4; i++)
            {
                for(int j = 0; j < 5 * Display.ColumnScale; j++)
                    videoBuffer[dstIndex + j] = 0;

                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }

            // Bitmask = 11111111 @ [19]
            dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (83 - CROSSSIZE / 2) + 19) * Display.ColumnScale;
            for(int i = 0; i < 2; i++)
            {
                for(int j = 0; j < 3 * Display.ColumnScale; j++)
                    videoBuffer[dstIndex + j] = 15;

                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }

            // Bitmask = 01111111 @ [18]
            dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (83 - CROSSSIZE / 2) + 18) * Display.ColumnScale + 1;
            for(int i = 0; i < 2; i++)
            {
                for(int j = 0; j < 7; j++)
                    videoBuffer[dstIndex + j] = 15;

                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }

            // Bitmask = 11111110 @ [18]
            dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (83 - CROSSSIZE / 2) + 18 + 4) * Display.ColumnScale;
            for(int i = 0; i < 2; i++)
            {
                for(int j = 0; j < 7; j++)
                    videoBuffer[dstIndex + j] = 15;

                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }

            // Bitmask = 00011000 @ [20]
            dstIndex = screenofs * Display.AddressScale + (SCREENWIDTH * (65 - CROSSSIZE / 2) + 20) * Display.ColumnScale + 3;
            for(int i = 0; i < CROSSSIZE - 2; i++)
            {
                videoBuffer[dstIndex] = 15;
                videoBuffer[dstIndex + 1] = 15;
                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }
        }

        //==========================================================================

        /*
        =====================
        =
        = FinishView
        =
        =====================
        */
        private void FinishView()
        {
            if(++screenpage == 3)
                screenpage = 0;

            screenofs = screenloc[screenpage];

            EGAWRITEMODE(2);

            //
            // draw the wall list
            //
            DrawWallList();

            //
            // draw all the scaled images
            //
            DrawScaleds();

            //
            // show screen and time last cycle
            //
            screenofs += (ushort) yshift[(ushort) inttime & 0xff]; // hover effect

            DrawCrossHairs();

            EGAWRITEMODE(0);

            // as: converted from asm
            // Adapted for simpler display emulation

            _display.ScreenStartIndex = screenofs * Display.AddressScale;

#if ADAPTIVE
            while((tics = (short) ((timecount - lasttimecount) / 2)) < 2)
            {
                // as: The timer interrupt increments timecount
            }

            lasttimecount = (int) (timecount & 0xfffffffe);// (~1);
#else
            tics = 2;
#endif

            if(tics > MAXTICS)
                tics = MAXTICS;

        }
    }

}
