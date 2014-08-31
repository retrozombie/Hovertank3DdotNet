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
        // as: This is all handled by SfxPlayer / SoundSystem now

        //
        //	Checks to see if a SoundBlaster is in the system. If the port passed is
        //		-1, then it scans through all possible I/O locations. If the port
        //		passed is 0, then it uses the default (2). If the port is >0, then
        //		it just passes it directly to jmCheckSB()
        //
        private bool jmDetectSoundBlaster(short port)
        {
            // SoundBlaster detection
            return true;
        }

        private void jmStartSB()
        {
            // SoundBlaster startup
        }

        private void jmShutSB()
        {
            // SoundBlaster shutdown
        }

        private void jmSetSamplePtr(SampledSound s)
        {
            // as: Added initialisation of sampled sounds
            _sfxPlayer.InitSampledSound(s.Pointer);
        }
    }

}
