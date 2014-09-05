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

namespace Hovertank3DdotNet
{
    /// <summary>Defines level object names and helper methods for level objects.</summary>
    static class LevelObjects
    {
        public const ushort Nothing = 0;

        public const ushort MaleRefugee = 1;

        public const ushort Drone = 2;

        public const ushort Tank = 3;

        public const ushort Mutant = 4;

        public const ushort Shield = 5;

        public const ushort FemaleRefugee = 6;

        public const ushort WarpGate = 0xfe;

        public const ushort Player = 0xff;

        public static ushort PlayerFacingAngle(int angle)
        {
            if(angle < 0 || angle > 359)
                throw new Exception("Angle out of range!");

            return (ushort) (Player | (angle / (Hovertank.ANGLES / 4)) << 8);
        }
    }
}