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

#define EXTEND_SHORT_SPEAKER_SOUNDS

using System;
using System.Diagnostics;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;

namespace Hovertank3DdotNet.WinForms
{
    /// <summary>
    /// Windows Forms implementation of SoundSystem.
    /// Uses System.Media.SoundPlayer for sound playback.
    /// Generates wav format data in memory.
    /// Uses time to determine when a sound has finished playing.
    /// </summary>
    class WinFormsSoundSystem : SoundSystem
    {
        /// <summary>Initialises the sound system.</summary>
        public override void Initialise()
        {
            _soundPlayer = new SoundPlayer();
            _stopwatch = new Stopwatch();
        }

        /// <summary>Disposes resources.</summary>
        public override void Dispose()
        {
            if(_soundPlayer != null)
            {
                try { _soundPlayer.Dispose(); }
                catch { }
                _soundPlayer = null;
            }
        }

        /// <summary>The sound player.</summary>
        private SoundPlayer _soundPlayer;

        /// <summary>The stopwatch.</summary>
        private Stopwatch _stopwatch;

        /// <summary>Starts playback of a sound stream.</summary>
        /// <param name="soundStream">The sound stream.</param>
        /// <param name="volume">The playback volume.</param>
        private void StartSound(Stream soundStream, float volume)
        {
            SetVolume(volume);

            soundStream.Position = 0;
            _soundPlayer.Stream = soundStream;
            _soundPlayer.Play();

            _stopwatch.Reset();
            _stopwatch.Start();
        }

        /// <summary>Stops sound playback.</summary>
        private void StopSound()
        {
            _soundPlayer.Stop();
        }

        /// <summary>Sets the volume.</summary>
        /// <param name="hwo">Handle to window.</param>
        /// <param name="dwVolume">The volume 0 to 0xffff for each word.</param>
        /// <returns>New volume setting.</returns>
        [DllImport("winmm.dll")]
        private static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

        /// <summary>Sets the volume.</summary>
        /// <param name="volume">The volume (0 to 1).</param>
        private static void SetVolume(float volume)
        {
            uint value = (uint) (volume * ushort.MaxValue);
            value |= value << 16;
            waveOutSetVolume(IntPtr.Zero, value);
        }

        /// <summary>Creates a sound.</summary>
        /// <param name="samples">A byte array containing the sample data (Mono 16-bit Signed samples).</param>
        /// <param name="frequency">The sample frequency in Hertz.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        /// <param name="speakerSound">Whether the sound is a speaker sound otherwise it is a sampled sound.</param>
        /// <returns>The Sound.</returns>
        public override SoundSystem.Sound CreateSound(byte[] samples, int frequency, int soundIndex, bool speakerSound)
        {
            // Build a wav format sound in a memory stream
            MemoryStream soundStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(soundStream);

#if EXTEND_SHORT_SPEAKER_SOUNDS
            double minSampleTime = 0.2;
            if(speakerSound)
            {   // Extend short sounds so they can be heard
                double sampleTime = (samples.Length * 0.5) / frequency;
                if(sampleTime < minSampleTime)
                {
                    int extraBytes = (int) ((minSampleTime - sampleTime) * frequency) * 2;
                    Array.Resize<byte>(ref samples, samples.Length + extraBytes);
                }
            }
#endif

            writer.Write(Wav.ID_RIFF);
            writer.Write((int) (12 + 16 + 8 + samples.Length)); // Size of file - 8

            writer.Write(Wav.ID_WAVE);

            writer.Write(Wav.ID_FMT);
            writer.Write((UInt32) 16); // Size of WAVEFORMAT structure
            writer.Write((UInt16) 1); // PCM
            writer.Write((UInt16) 1); // Channels
            writer.Write((UInt32) frequency); // Frequency
            writer.Write((UInt32) frequency * 2); // Bytes / second
            writer.Write((UInt16) 2); // Block align
            writer.Write((UInt16) 16); // Bits
            writer.Write(Wav.ID_DATA);
            writer.Write(samples.Length);
            writer.Write(samples);

            TimeSpan timeSpan = TimeSpan.FromSeconds((soundStream.Length * 0.5) / frequency);

            return new WinFormsSound(this, soundStream, timeSpan);
        }

        /// <summary>WinForms sound implementation.</summary>
        public class WinFormsSound : Sound
        {
            /// <summary>Creates a new WinFormsSound.</summary>
            /// <param name="winFormsSoundSystem">The sound system.</param>
            /// <param name="wavStream">The memory stream for the sound (wav format).</param>
            /// <param name="timeSpan">The time span.</param>
            public WinFormsSound(WinFormsSoundSystem winFormsSoundSystem, Stream wavStream, TimeSpan timeSpan)
            {
                _soundSystem = winFormsSoundSystem;
                _wavStream = wavStream;
                _timeSpan = timeSpan;
            }

            /// <summary>Disposes resources.</summary>
            public override void Dispose()
            {
                _soundSystem = null;

                if(_wavStream != null)
                {
                    try { _wavStream.Dispose(); }
                    catch { }
                    _wavStream = null;
                }
            }

            /// <summary>The sound system.</summary>
            private WinFormsSoundSystem _soundSystem;

            /// <summary>The stream for the sound (wav format).</summary>
            private Stream _wavStream;

            /// <summary>Gets the stream for the sound (wav format).</summary>
            public Stream WavStream
            {
                get { return _wavStream; }
            }

            /// <summary>The time span.</summary>
            private TimeSpan _timeSpan;

            /// <summary>Plays the sound.</summary>
            /// <param name="volume">The playback volume.</param>
            public override void Play(float volume)
            {
                _soundSystem.StartSound(_wavStream, volume);
            }

            /// <summary>Stops the sound playing.</summary>
            public override void Stop()
            {
                _soundSystem.StopSound();
            }

            /// <summary>Gets whether the sound is still playing.</summary>
            public override bool IsPlaying
            {
                get { return (_soundSystem._stopwatch.Elapsed < _timeSpan); }
            }
        }

    }
}
