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

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    /// <summary>Represents an actor in the game.</summary>
    class objtype
    {
        public short active;

        public classtype _class;

        public fixed_t x;

        public fixed_t y;

        public fixed_t viewx; // x,y in view coordinate space (NOT pixels!)

        public fixed_t viewy;

        public short angle;

        public short hitpoints;

        public short radarx;

        public short radary;

        public short radarcolor;

        public int speed;

        public ushort size; // global radius for hit rect calculation

        public fixed_t xl; // hit rectangle

        public fixed_t xh;

        public fixed_t yl;

        public fixed_t yh;

        public short ticcount;

        public short shapenum;

        public short stage;

        public short temp1;

        public short temp2;

        public dirtype dir;

        public ThinkerFunction think;

        /// <summary>Clears the objtype.</summary>
        /// <remarks>Replaces memset.</remarks>
        public void Clear()
        {
            active = 0;
            _class = 0;
            x = 0;
            y = 0;
            viewx = 0;
            viewy = 0;
            angle = 0;
            hitpoints = 0;
            radarx = 0;
            radary = 0;
            radarcolor = 0;
            speed = 0;
            size = 0;
            xl = 0;
            xh = 0;
            yl = 0;
            yh = 0;
            ticcount = 0;
            shapenum = 0;
            stage = 0;
            temp1 = 0;
            temp2 = 0;
            dir = 0;
            think = null;
        }

        /// <summary>Copies the objtype to another.</summary>
        /// <remarks>For struct assignment.</remarks>
        /// <param name="destination">The destination objtype.</param>
        public void CopyTo(objtype destination)
        {
            destination.active = active;
            destination._class = _class;
            destination.x = x;
            destination.y = y;
            destination.viewx = viewx; // x,y in view coordinate space (NOT pixels!)
            destination.viewy = viewy;
            destination.angle = angle;
            destination.hitpoints = hitpoints;
            destination.radarx = radarx;
            destination.radary = radary;
            destination.radarcolor = radarcolor;
            destination.speed = speed;
            destination.size = size; // global radius for hit rect calculation
            destination.xl = xl; // hit rectangle
            destination.xh = xh;
            destination.yl = yl;
            destination.yh = yh;
            destination.ticcount = ticcount;
            destination.shapenum = shapenum;
            destination.stage = stage;
            destination.temp1 = temp1;
            destination.temp2 = temp2;
            destination.dir = dir;
            destination.think = think;
        }
    }

    /// <summary>Pointer to thinker function.</summary>
    delegate void ThinkerFunction();
}
