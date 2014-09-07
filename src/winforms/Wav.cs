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

namespace Hovertank3DdotNet.WinForms
{
    /// <summary>Wav chunk ids.</summary>
    class Wav
    {
        /// <summary>RIFF chunk identifier.</summary>
        public const uint ID_RIFF = 0x46464952;

        /// <summary>Wave chunk identifier.</summary>
        public const uint ID_WAVE = 0x45564157;

        /// <summary>Format chunk identifier.</summary>
        public const uint ID_FMT = 0x20746D66;

        /// <summary>Data chunk identifier.</summary>
        public const uint ID_DATA = 0x61746164;
    }
}
