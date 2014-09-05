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
    /// <summary>A base class for sound systems.</summary>
    abstract class SoundSystem : IDisposable
    {
        /// <summary>Initialises the sound system.</summary>
        public abstract void Initialise();

        /// <summary>Disposes resources.</summary>
        public abstract void Dispose();

        /// <summary>Creates a sound.</summary>
        /// <param name="samples">A byte array containing the sample data (Mono 16-bit Signed samples).</param>
        /// <param name="frequency">The sample frequency in Hertz.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        /// <param name="speakerSound">Whether the sound is a speaker sound otherwise it is a sampled sound.</param>
        /// <returns>The Sound.</returns>
        public abstract SoundSystem.Sound CreateSound(byte[] samples, int frequency, int soundIndex, bool speakerSound);

        /// <summary>A base class for platform specific sounds.</summary>
        public abstract class Sound : IDisposable
        {
            /// <summary>Starts the sound playing.</summary>
            /// <param name="volume">The volume (0 to 1).</param>
            public abstract void Play(float volume);

            /// <summary>Stops the sound playing.</summary>
            public abstract void Stop();

            /// <summary>Gets whether the sound is playing.</summary>
            public abstract bool IsPlaying
            {
                get;
            }

            /// <summary>Disposes resources.</summary>
            public abstract void Dispose();
        }
    }
}
