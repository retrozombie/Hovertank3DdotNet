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
using SharpDX.DirectSound;
using SharpDX.Multimedia;

using WF = System.Windows.Forms;

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>
    /// SharpDX implementation of SoundSystem.
    /// Uses DirectSound SecondarySoundBuffers for sound playback.
    /// </summary>
    class SharpDXSoundSystem : SoundSystem, IDisposable
    {
        /// <summary>Creates a new SharpDXSoundSystem.</summary>
        /// <param name="control">The control to associate with DirectSound.</param>
        public SharpDXSoundSystem(WF.Control control)
        {
            _control = control;
        }

        /// <summary>The control to associate with the DirectSound device.</summary>
        private WF.Control _control;

        /// <summary>The direct sound device.</summary>
        private DirectSound _directSound;

        /// <summary>Initialises the sound system.</summary>
        public override void Initialise()
        {
            _directSound = new DirectSound();
            _directSound.SetCooperativeLevel(_control.Handle, CooperativeLevel.Priority);
        }

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
            _control = null;

            if(_directSound != null)
            {
                try { _directSound.Dispose(); }
                catch { }
                _directSound = null;
            }
        }

        /// <summary>Creates a sound.</summary>
        /// <param name="samples">A byte array containing the sample data (Mono 16-bit Signed samples).</param>
        /// <param name="frequency">The sample frequency in herz.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        /// <param name="speakerSound">Whether the sound is a speaker sound otherwise it is a sampled sound.</param>
        /// <returns>The Sound.</returns>
        public override SoundSystem.Sound CreateSound(byte[] samples, int frequency, int soundIndex, bool speakerSound)
        {
            WaveFormat waveFormat = new WaveFormat(frequency, 16, 1);

            SoundBufferDescription soundBufferDescription = new SoundBufferDescription();
            soundBufferDescription.Format = waveFormat;
            soundBufferDescription.Flags = BufferFlags.ControlVolume;
            soundBufferDescription.BufferBytes = samples.Length;

            SecondarySoundBuffer secondarySoundBuffer = new SecondarySoundBuffer(_directSound, soundBufferDescription);
            secondarySoundBuffer.Write(samples, 0, LockFlags.EntireBuffer);

            return new SharpDXSound(secondarySoundBuffer);
        }

        /// <summary>Represents a SharpDX sound.</summary>
        class SharpDXSound : SoundSystem.Sound
        {
            /// <summary>Creates a new SharpDXSound.</summary>
            /// <param name="soundBuffer">The sound buffer.</param>
            public SharpDXSound(SoundBuffer soundBuffer)
            {
                _soundBuffer = soundBuffer;
            }

            /// <summary>The sound buffer.</summary>
            private SoundBuffer _soundBuffer;

            /// <summary>Disposes resources.</summary>
            public override void Dispose()
            {
                if(_soundBuffer != null)
                {
                    try { _soundBuffer.Dispose(); }
                    catch { }
                    _soundBuffer = null;
                }
            }

            /// <summary>Starts the sound playing.</summary>
            /// <param name="volume">The volume (0 to 1).</param>
            public override void Play(float volume)
            {
                _soundBuffer.CurrentPosition = 0;
                _soundBuffer.Volume = DSVolume(volume);
                _soundBuffer.Play(0, PlayFlags.None);
            }

            /// <summary>Stops the sound playing.</summary>
            public override void Stop()
            {
                _soundBuffer.Stop();
            }

            /// <summary>Gets whether the sound is playing.</summary>
            public override bool IsPlaying
            {
                get { return (_soundBuffer.Status == (int) BufferStatus.Playing); }
            }

            /// <summary>Returns the sound buffer volume.</summary>
            /// <param name="amplitude">The amplitude.</param>
            /// <returns>An int.</returns>
            private int DSVolume(float amplitude)
            {
                int range = (int) ((Volume.Maximum - Volume.Minimum) / 3);
                return ((int) Volume.Maximum) + ((int) (range * ((float) Math.Log10(amplitude))));
            }
        }
    }
}
