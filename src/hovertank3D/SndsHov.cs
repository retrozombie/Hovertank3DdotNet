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
        public const ushort INTROSND = 0; // as: Added for convenience

        //
        // From Sound Editor v1.31 by Lane Roath
        //

        public const ushort FIRESND = 1;

        public const ushort BUMPWALLSND = 2;

        public const ushort AFTERBURNSND = 3;

        public const ushort SHOOTWALLSND = 4;

        public const ushort SHOOTTHINGSND = 5;

        public const ushort SAVEHOSTAGESND = 6;

        public const ushort HOSTAGEDEADSND = 7;

        public const ushort LOWTIMESND = 8;

        public const ushort TAKEDAMAGESND = 9;

        public const ushort NUKESND = 10;

        public const ushort WARPGATESND = 11;

        public const ushort PLAYERDEADSND = 12;

        public const ushort GUNREADYSND = 13;

        public const ushort MAXPOWERSND = 14;

        public const ushort LASTHOSTAGESND = 15;

        public const ushort ARMORUPSND = 16;

        public const ushort TIMESCORESND = 17;

        public const ushort GUYSCORESND = 18;

        public const ushort STARTGAMESND = 19;

        public const ushort HIGHSCORESND = 20;

        // as: Support for extra sound effects

        // Mutant attack and death sounds use existing sounds: TAKEDAMAGE and SHOOTTHING
        // Tank missile hitting wall uses existing sound SHOOTWALLSND

        /// <summary>Mutant attacks player.</summary>
        public const ushort SNDEX_MUTEDAMAGE = TAKEDAMAGESND;

        /// <summary>Mutant dies.</summary>
        public const ushort SNDEX_MUTEDIE = SHOOTTHINGSND;

        /// <summary>Drone attacks player (explosion).</summary>
        public const ushort SNDEX_DRONEDAMAGE = 21;

        /// <summary>Drone dies.</summary>
        public const ushort SNDEX_DRONEDIE = 22;

        /// <summary>Tank shoots a missile.</summary>
        public const ushort SNDEX_TANKFIRE = 23;

        /// <summary>Tank missile hits player.</summary>
        public const ushort SNDEX_TANKDAMAGE = 24;

        /// <summary>Tank dies.</summary>
        public const ushort SNDEX_TANKDIE = 25;

        /// <summary>Last male refugee killed.</summary>
        public const ushort SNDEX_LASTDEAD1 = 26;

        /// <summary>Last female refugee killed.</summary>
        public const ushort SNDEX_LASTDEAD2 = 27;

        /// <summary>Last male refugee killed by player.</summary>
        public const ushort SNDEX_LASTDEAD3 = 28;

        /// <summary>Last female refugee killed by player.</summary>
        public const ushort SNDEX_LASTDEAD4 = 29;

        /// <summary>Female refugee saved.</summary>
        public const ushort SNDEX_SAVHOSTAGE2 = 30;

        /// <summary>Last refugee saved (female).</summary>
        public const ushort SNDEX_LSTHOSTAGE2 = 31;

        /// <summary>Female refugee killed.</summary>
        public const ushort SNDEX_HSTAGEDEAD2 = 32;

        /// <summary>Player killed male refugee.</summary>
        public const ushort SNDEX_HSTAGEDEAD3 = 33;

        /// <summary>Player killed female refugee.</summary>
        public const ushort SNDEX_HSTAGEDEAD4 = 34;

        /// <summary>Player fires fully charged weapon.</summary>
        public const ushort SNDEX_FIRE2 = 35;

        /// <summary>Tank missile hits wall.</summary>
        public const ushort SNDEX_TSHOTWALL = SHOOTWALLSND;

        /// <summary>Player missile hits wall.</summary>
        public const ushort SNDEX_PSHOTWALL = 36;

        /// <summary>Player fully charged missile hits wall.</summary>
        public const ushort SNDEX_PSHOTWALL2 = 37;

        /// <summary>Player picks up a shield object.</summary>
        public const ushort SNDEX_SHIELDUP = 38;

        /// <summary>The total number of sounds.</summary>
        public const int SNDEX_NUMSOUNDS = 1 + 20 + 18;
    }
}
