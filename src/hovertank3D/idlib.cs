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
        public const string EXTENSION = "HOV";

        public const int SCREENWIDTH = 40;

        public short egaWriteMode;

        private void EGAWRITEMODE(short x)
        {
            egaWriteMode = x;
        }

        public short egaBitMask;

        private void EGABITMASK(short x)
        {
            egaBitMask = x;
        }

        public short egaMapMask;

        private void EGAMAPMASK(short x)
        {
            egaMapMask = x;
        }

        private static short ABS(short x)
        {
            return (x > 0 ? x : (short) -x);
        }

        private static int LABS(int x)
        {
            return (x > 0 ? x : -x);
        }
    }

}
