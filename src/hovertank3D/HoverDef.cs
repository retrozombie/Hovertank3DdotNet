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
using System.Collections.Generic;

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        public const short MAXTICS = 8;

        public const short NUMLEVELS = 20;

        /*
        =============================================================================

			              REFRESH

        =============================================================================
        */

        public const short VIEWX = 0; // corner of view window

        public const short VIEWY = 0;

        public const short VIEWWIDTH = (40 * 8); // size of view window

        public const short VIEWHEIGHT = (18 * 8);

        public const short VIEWXH = (VIEWX + VIEWWIDTH - 1);

        public const short VIEWYH = (VIEWY + VIEWHEIGHT - 1);

        public const short CENTERX = (VIEWX + VIEWWIDTH / 2); // middle of view window

        public const short CENTERY = (VIEWY + VIEWHEIGHT / 2);

        public const ushort STATUSLINES = (9 * 8); // dash board

        public const fixed_t GLOBAL1 = (1 << 16);

        public const fixed_t TILEGLOBAL = GLOBAL1;

        public const int TILESHIFT = 16;

        public const ushort MINDIST = (2 * GLOBAL1 / 5);

        public const fixed_t FOCALLENGTH = (TILEGLOBAL); // in global coordinates

        public const short ANGLES = 360; // must be divisable by 4

        public const int MAPSIZE = 64; // maps are 64*64 max

        public const short MAXOBJECTS = 100; // max number of tanks, etc / map

        //
        // 1  sign bit
        // 15 bits units
        // 16 bits fractional
        //
        public const fixed_t SIGNBIT = int.MinValue; // 0x80000000

        public const short NORTH = 0;

        public const short EAST = 1;

        public const short SOUTH = 2;

        public const short WEST = 3;

        /*
        =============================================================================

		              DASH INSTRUMENTS

        =============================================================================
        */

        public const short RADARX = 284; // center of radar

        public const short RADARY = 36;

        public const short RADARSIZE = 26; // each way

        public const fixed_t RADARRANGE = (TILEGLOBAL * 18); // each way

        public const fixed_t RADARSCALE = (RADARRANGE / RADARSIZE);

        public const short DANGERHIGH = 90;
    }
}
