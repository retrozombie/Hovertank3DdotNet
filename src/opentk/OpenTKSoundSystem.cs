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
using OpenTK;
using OpenTK.Audio;
using OpenTK.Audio.OpenAL;

namespace Hovertank3DdotNet.OpenTK
{
    /// <summary>
    /// OpenTK implementation of SoundSystem.
    /// Creates an AudioContext.
    /// Configures the Listener.
    /// Creates a Source for sound playback.
    /// Creates buffers for each sound.
    /// </summary>
    class OpenTKSoundSystem : SoundSystem, IDisposable
    {
        /// <summary>Initialises the sound system.</summary>
        public override void Initialise()
        {
            _audioContext = new AudioContext();
            _audioSourceID = AL.GenSource();

            AL.Listener(ALListener3f.Position, 0.0f, 0.0f, 0.0f);
            Vector3 listenerAt = -Vector3.UnitZ;
            Vector3 listenerUp = Vector3.UnitY;
            AL.Listener(ALListenerfv.Orientation, ref listenerAt, ref listenerUp);
        }

        /// <summary>The audio context.</summary>
        private AudioContext _audioContext;

        /// <summary>The ID for the audio source.</summary>
        private int _audioSourceID;

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
            if(_audioSourceID != 0)
            {
                try { AL.DeleteSource(_audioSourceID); }
                catch { }
            }

            if(_audioContext != null)
            {
                try { _audioContext.Dispose(); }
                catch { }
                _audioContext = null;
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
            int bufferID = AL.GenBuffer();

            AL.BufferData(bufferID, ALFormat.Mono16, samples, samples.Length, frequency);

            if(AL.GetError() != ALError.NoError)
                throw new Exception("CreateSound: AL.BufferData failed!");
            
            return new OpenTKSound(this, bufferID);
        }

        /// <summary>Starts playback of a sound.</summary>
        /// <param name="sound">The sound.</param>
        /// <param name="volume">The playback volume.</param>
        private void StartSound(OpenTKSound sound, float volume)
        {
            AL.Source(_audioSourceID, ALSourcei.Buffer, sound.BufferID);
            AL.Source(_audioSourceID, ALSourceb.Looping, false);
            AL.Source(_audioSourceID, ALSource3f.Position, 0.0f, 0.0f, 0.0f);
            AL.Source(_audioSourceID, ALSourcef.Gain, volume);
            AL.SourcePlay(_audioSourceID);
        }

        /// <summary>Stops sound playback.</summary>
        private void StopSound()
        {
            AL.SourceStop(_audioSourceID);
        }

        /// <summary>Gets whether sound is playing.</summary>
        private bool IsSoundPlaying
        {
            get { return (AL.GetSourceState(_audioSourceID) == ALSourceState.Playing); }
        }

        /// <summary>Represents an OpenTK sound.</summary>
        class OpenTKSound : SoundSystem.Sound
        {
            /// <summary>Creates a new OpenTK sound.</summary>
            /// <param name="soundSystem">The sound system.</param>
            /// <param name="bufferID">The buffer ID.</param>
            public OpenTKSound(OpenTKSoundSystem soundSystem, int bufferID)
            {
                _soundSystem = soundSystem;
                _bufferID = bufferID;
            }

            /// <summary>The sound system.</summary>
            private OpenTKSoundSystem _soundSystem;

            /// <summary>The OpenAL buffer ID.</summary>
            private int _bufferID;

            /// <summary>Gets the OpenAL buffer ID.</summary>
            public int BufferID
            {
                get { return _bufferID; }
            }

            /// <summary>Disposes resources.</summary>
            public override void Dispose()
            {
                if(_bufferID != 0)
                {
                    AL.DeleteBuffer(_bufferID);
                    _bufferID = 0;
                }
            }

            /// <summary>Starts the sound playing.</summary>
            /// <param name="volume">The volume (0 to 1).</param>
            public override void Play(float volume)
            {
                _soundSystem.StartSound(this, volume);
            }

            /// <summary>Stops the sound playing.</summary>
            public override void Stop()
            {
                _soundSystem.StopSound();
            }

            /// <summary>Gets whether the sound is playing.</summary>
            public override bool IsPlaying
            {
                get { return _soundSystem.IsSoundPlaying; }
            }
        }

    }
}
