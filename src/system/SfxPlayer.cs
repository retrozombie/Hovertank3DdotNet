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
using System.Collections.Generic;

namespace Hovertank3DdotNet
{
    /// <summary>A base class for sound players.</summary>
    class SfxPlayer : IDisposable
    {
        /// <summary>Creates a new SfxPlayer.</summary>
        public SfxPlayer()
        {
            _sampledSounds = new List<SoundSystem.Sound>();
            _speakerSounds = new List<SoundSystem.Sound>();

            SampledSoundVolume = 0.5f;
            SpeakerVolume = 0.25f;
            SpeakerSampleFrequency = 44100;
            SpeakerSampleAmplitude = 0.5f;

            // as: Support for extra sound effects
            _soundLinks = new byte[Hovertank.SNDEX_NUMSOUNDS];
            for(byte i = 0; i < 21; i++)
                _soundLinks[i] = i;

            // Link new sounds to originals
            _soundLinks[Hovertank.SNDEX_DRONEDAMAGE] = (byte) Hovertank.TAKEDAMAGESND;
            _soundLinks[Hovertank.SNDEX_DRONEDIE] = (byte) Hovertank.SHOOTTHINGSND;
            _soundLinks[Hovertank.SNDEX_TANKFIRE] = (byte) Hovertank.FIRESND;
            _soundLinks[Hovertank.SNDEX_TANKDAMAGE] = (byte) Hovertank.TAKEDAMAGESND;
            _soundLinks[Hovertank.SNDEX_TANKDIE] = (byte) Hovertank.SHOOTTHINGSND;
            _soundLinks[Hovertank.SNDEX_LASTDEAD1] = (byte) Hovertank.WARPGATESND;
            _soundLinks[Hovertank.SNDEX_LASTDEAD2] = (byte) Hovertank.WARPGATESND;
            _soundLinks[Hovertank.SNDEX_LASTDEAD3] = (byte) Hovertank.WARPGATESND;
            _soundLinks[Hovertank.SNDEX_LASTDEAD4] = (byte) Hovertank.WARPGATESND;
            _soundLinks[Hovertank.SNDEX_SAVHOSTAGE2] = (byte) Hovertank.SAVEHOSTAGESND;
            _soundLinks[Hovertank.SNDEX_LSTHOSTAGE2] = (byte) Hovertank.LASTHOSTAGESND;
            _soundLinks[Hovertank.SNDEX_HSTAGEDEAD2] = (byte) Hovertank.HOSTAGEDEADSND;
            _soundLinks[Hovertank.SNDEX_HSTAGEDEAD3] = (byte) Hovertank.HOSTAGEDEADSND;
            _soundLinks[Hovertank.SNDEX_HSTAGEDEAD4] = (byte) Hovertank.HOSTAGEDEADSND;
            _soundLinks[Hovertank.SNDEX_FIRE2] = (byte) Hovertank.FIRESND;
            _soundLinks[Hovertank.SNDEX_PSHOTWALL] = (byte) Hovertank.SHOOTWALLSND;
            _soundLinks[Hovertank.SNDEX_PSHOTWALL2] = (byte) Hovertank.SHOOTWALLSND;
            _soundLinks[Hovertank.SNDEX_SHIELDUP] = (byte) Hovertank.ARMORUPSND;
        }

        /// <summary>Initialises the SfxPlayer.</summary>
        /// <param name="sys">The system.</param>
        /// <param name="soundSystem">The sound system.</param>
        public void Initialise(Sys sys, SoundSystem soundSystem)
        {
            _sys = sys;
            _soundSystem = soundSystem;
            try { _soundSystem.Initialise(); }
            catch(Exception ex)
            {
                _sys.Log("SoundSystem failed to initialise: " + ex.Message);

                // Try to continue with no sound
                _soundSystem = new NullSoundSystem();
                _soundSystem.Initialise();
            }

            SoundSystem.Sound introSound = CreateIntroSound();

            _sampledSounds.Add(introSound);
            _speakerSounds.Add(introSound);
        }

        /// <summary>Disposes resources.</summary>
        public void Dispose()
        {
            for(int i = 0; i < _sampledSounds.Count; i++)
            {
                if(_sampledSounds[i] != null)
                {
                    try { _sampledSounds[i].Dispose(); }
                    catch { }
                }
            }
            _sampledSounds.Clear();

            for(int i = 0; i < _speakerSounds.Count; i++)
            {
                if(_speakerSounds[i] != null)
                {
                    try { _speakerSounds[i].Dispose(); }
                    catch { }
                }
            }
            _speakerSounds.Clear();
        }

        /// <summary>The system.</summary>
        private Sys _sys;

        /// <summary>The sound system.</summary>
        private SoundSystem _soundSystem;

        /// <summary>The list of sampled sounds.</summary>
        private List<SoundSystem.Sound> _sampledSounds;

        /// <summary>The table of linked sounds.</summary>
        private byte[] _soundLinks;

        /// <summary>Gets the sound links array.</summary>
        public byte[] SoundLinks
        {
            get { return _soundLinks; }
        }

        /// <summary>Initialises the sampled sound player.</summary>
        /// <param name="sampledSoundsPointer">The pointer to the sampled sounds data.</param>
        public void InitSampledSound(memptr sampledSoundsPointer)
        {
            int count = 0;
            bool[] soundUsed = new bool[_soundLinks.Length];
            for(int i = MinSoundIndex; i <= MaxSoundIndex; i++)
            {
                int soundIndex = _soundLinks[i];
                if(!soundUsed[soundIndex])
                {
                    soundUsed[soundIndex] = true;
                    count++;
                }
            }

            for(int i = 0; i < count; i++)
                _sampledSounds.Add(CreateSampledSound(sampledSoundsPointer, MinSoundIndex + i));
        }

        /// <summary>Creates a sampled sound.</summary>
        /// <param name="sampledSoundsPointer">A memory pointer for sampled sound data.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        /// <returns>A Sound.</returns>
        private SoundSystem.Sound CreateSampledSound(memptr sampledSoundsPointer, int soundIndex)
        {
            SampledSound sfxInfo = GetSampledSound(sampledSoundsPointer, soundIndex);
            byte[] buffer = ConvertSamplesU8toS16(sfxInfo);
            return _soundSystem.CreateSound(buffer, (int) sfxInfo.hertz, soundIndex, false);
        }

        /// <summary>The list of sampled sounds.</summary>
        private List<SoundSystem.Sound> _speakerSounds;

        /// <summary>Initialises the speaker sound player.</summary>
        /// <param name="speakerSoundsPointer">The pointer to the speaker sounds data.</param>
        public void InitSpeakerSound(memptr speakerSoundsPointer)
        {
            int count = 0;
            bool[] soundUsed = new bool[_soundLinks.Length];
            for(int i = MinSoundIndex; i <= MaxSoundIndex; i++)
            {
                int soundIndex = _soundLinks[i];
                if(!soundUsed[soundIndex])
                {
                    soundUsed[soundIndex] = true;
                    count++;
                }
            }

            for(int i = 0; i < count; i++)
                _speakerSounds.Add(CreateSpeakerSound(speakerSoundsPointer, MinSoundIndex + i));
        }

        /// <summary>Creates the intro sound.</summary>
        private SoundSystem.Sound CreateIntroSound()
        {
            if(!_sys.SimulateIntroSound)
                return null;

            byte[] samples = CreateIntroSoundSamplesS16(_speakerSampleFrequency, _speakerSampleAmplitude);
            return _soundSystem.CreateSound(samples, _speakerSampleFrequency, Hovertank.INTROSND, true);
        }

        /// <summary>Creates a speaker sound.</summary>
        /// <param name="soundsPointer">A memory pointer to the speaker sound data.</param>
        /// <param name="soundIndex">The index of the sound.</param>
        /// <returns>A Sound.</returns>
        private SoundSystem.Sound CreateSpeakerSound(memptr soundsPointer, int soundIndex)
        {
            spksndtype speakerSound = GetSpeakerSound(soundsPointer, soundIndex);
            byte[] samples = GetSpeakerSamplesS16(speakerSound, soundsPointer, _speakerSampleFrequency, _speakerSampleAmplitude);
            return _soundSystem.CreateSound(samples, _speakerSampleFrequency, soundIndex, true);
        }

        /// <summary>The last sound played.</summary>
        private SoundSystem.Sound _lastSound;

        /// <summary>Plays a sound.</summary>
        /// <param name="soundIndex">The index of the sound to play.</param>
        public void PlaySound(int soundIndex)
        {
            if(_disabled)
                return;

            StopSound();

            float volume;
            if(soundIndex == 0 && _sys.SimulateIntroSound)
                volume = _speakerVolume; // Always use speaker volume when simulated
            else
                volume = (SpeakerMode ? _speakerVolume : _sampledSoundVolume);

            List<SoundSystem.Sound> sounds = (SpeakerMode ? _speakerSounds : _sampledSounds);
            SoundSystem.Sound sound = sounds[soundIndex];
            sound.Play(volume);
            _lastSound = sound;
        }

        /// <summary>Stops sound playback.</summary>
        public void StopSound()
        {
            if(_lastSound != null)
            {
                _lastSound.Stop();
                _lastSound = null;
            }
        }

        /// <summary>Gets whether a sound is playing.</summary>
        public bool IsSoundPlaying
        {
            get
            {
                if(_lastSound == null)
                    return false;

                return _lastSound.IsPlaying;
            }
        }

        /// <summary>Whether the speaker mode is enabled.</summary>
		private bool _speakerMode;

        /// <summary>Gets or sets whether the speaker mode is enabled.</summary>
        public bool SpeakerMode
        {
            get { return _speakerMode; }
            set { _speakerMode = value; }
        }

        /// <summary>Whether sound is disabled.</summary>
		private bool _disabled;
		
        /// <summary>Gets or sets whether sound is disabled.</summary>
        public bool Disabled
        {
            get { return _disabled; }
            set
            {
                if(value != _disabled)
                {
                    _disabled = value;

                    if(_disabled)
                        StopSound();
                }
            }
        }
		
        /// <summary>The volume for sampled sounds.</summary>
		private float _sampledSoundVolume;

        /// <summary>Gets or sets the volume for sampled sounds.</summary>
        public float SampledSoundVolume
        {
            get { return _sampledSoundVolume; }
            set { _sampledSoundVolume = value; }
        }

        /// <summary>The volume for playback of speaker sounds.</summary>
        private float _speakerVolume;

        /// <summary>Gets or sets the volume for playback of speaker sounds.</summary>
        public float SpeakerVolume
        {
            get { return _speakerVolume; }
            set { _speakerVolume = value; }
        }

        /// <summary>The sample frequency for speaker sounds.</summary>
		private int _speakerSampleFrequency;

        /// <summary>Gets or sets the sample frequency for speaker sounds.</summary>
        public int SpeakerSampleFrequency
        {
            get { return _speakerSampleFrequency; }
            set { _speakerSampleFrequency = value; }
        }
		
        /// <summary>The peak amplitude for speaker sounds.</summary>
		private float _speakerSampleAmplitude;

        /// <summary>Gets or sets the peak amplitude for speaker sounds.</summary>
        public float SpeakerSampleAmplitude
        {
            get { return _speakerSampleAmplitude; }
            set { _speakerSampleAmplitude = value; }
        }

        /// <summary>Gets the index of the first sound.</summary>
        public int MinSoundIndex
        {
            get { return 1; }
        }

        /// <summary>Gets the index of the last sound.</summary>
        public int MaxSoundIndex
        {
            get { return _soundLinks.Length - 1; }
        }

        /// <summary>Converts a sample sound in U8 format to S16 format.</summary>
        /// <param name="sampledSound">The SampledSound.</param>
        /// <returns>A byte array.</returns>
        private byte[] ConvertSamplesU8toS16(SampledSound sampledSound)
        {
            // Convert Unsigned 8-bit to Signed 16-bit Mono
            byte[] conversionBuffer = new byte[sampledSound.length * 2];
            Array.Copy(sampledSound.Pointer.Buffer, (int) sampledSound.offset, conversionBuffer, 0, (int) sampledSound.length);

            for(int i = (int) sampledSound.length - 1; i >= 0; i--)
            {
                byte sample = (byte) (conversionBuffer[i] - 128);
                conversionBuffer[i * 2] = sample;
                conversionBuffer[i * 2 + 1] = sample;
            }

            return conversionBuffer;
        }

        /// <summary>Convers a buffer containing 16-bit samples to floating point samples.</summary>
        /// <param name="samples">The byte array containing the samples.</param>
        /// <returns>A buffer containing the converted samples.</returns>
        public static float[] ConvertSamplesS16toF32(byte[] samples)
        {
            // Convert signed 16-bit to floating point format
            float[] conversionBuffer = new float[samples.Length / 2];

            int index = 0;
            for(uint i = 0; i < conversionBuffer.Length; i++)
            {
                short sample = samples[index++];
                sample |= (short) (samples[index++] * 256);
                conversionBuffer[i] = sample / 32767.0f;
            }

            return conversionBuffer;
        }

        /// <summary>Returns a SampleSound for the specified sound.</summary>
        /// <param name="sampledSoundsPointer">The pointer to the sampled sounds.</param>
        /// <param name="soundIndex">The sound index.</param>
        /// <returns>A SampledSound.</returns>
        public static SampledSound GetSampledSound(memptr sampledSoundsPointer, int soundIndex)
        {
            return new SampledSound(new memptr(sampledSoundsPointer, (soundIndex - 1) * SampledSound.SizeOf));
        }

        /// <summary>Returns a spksndtype for the specified sound.</summary>
        /// <param name="soundsPointer">The pointer to the speaker sounds.</param>
        /// <param name="soundIndex">The sound index.</param>
        /// <returns>A spksndtype.</returns>
        public static spksndtype GetSpeakerSound(memptr soundsPointer, int soundIndex)
        {
            return new spksndtype(new memptr(soundsPointer, soundIndex * spksndtype.SizeOf));
        }

        /// <summary>Returns a byte array containing 16-bit signed samples for the specified speaker sound.</summary>
        /// <param name="speakerSound">The speaker sound.</param>
        /// <param name="soundsPointer">The speaker sounds pointer.</param>
        /// <param name="sampleFrequency">The sample frequency in Hz.</param>
        /// <param name="amplitude">The peak amplitude (0 to 1).</param>
        /// <returns>A byte array.</returns>
        public static byte[] GetSpeakerSamplesS16(spksndtype speakerSound, memptr soundsPointer, double sampleFrequency, double amplitude)
        {
            soundsPointer.Offset(speakerSound.start);
            
            PCSpeaker pcSpeaker = new PCSpeaker(sampleFrequency, amplitude);

            // as: Added checks to catch problems with SOUNDS.HOV files (Demon Hunter v1.1):
            // Also, the maximum sample time has been limited to 10s
            // and: sound generation stops if the end of the soundsPointer buffer is reached

            double time = 1.0 / spksndtype.Frequency(0x2147);
            ushort timerValue = soundsPointer.GetUInt16(0);
            double maxTime = 10.0;
            while(timerValue != spksndtype.SpeakerData_EndSound)
            {
                pcSpeaker.SetTimer((ushort) timerValue);
                pcSpeaker.WriteSamples(time);

                soundsPointer.Offset(2);

                if(soundsPointer.BaseIndex + 1 >= soundsPointer.Buffer.Length)
                    break;

                timerValue = soundsPointer.GetUInt16(0);

                if(pcSpeaker.ElapsedTime > maxTime)
                    break;
            }

            return pcSpeaker.ToArray();
        }

        /// <summary>Returns a byte array containing samples (Mono 16-bit Signed) for the intro sound.</summary>
        /// <param name="sampleFrequency">The sample frequency in Hz.</param>
        /// <param name="amplitude">The peak amplitude (0 to 1).</param>
        /// <returns>A byte array.</returns>
        public static byte[] CreateIntroSoundSamplesS16(double sampleFrequency, double amplitude)
        {
            PCSpeaker pcSpeaker = new PCSpeaker(sampleFrequency, amplitude);

            // from HOVMAIN.C:Intro
            float NUMFRAMES = 300.0f;
            float MAXANGLE = (3.141592657f * 0.6f);
            short f = 1;
            do
            {
                float step = f / NUMFRAMES;
                float angle = MAXANGLE * step;
                float sn = (float) Math.Sin(angle);
                int frequency = (int) (sn * 1500);

                pcSpeaker.SetFrequency(frequency);
                pcSpeaker.WriteSamples(1.0 / 140.0);

                f++;
            } while(f <= NUMFRAMES);
            
            return pcSpeaker.ToArray();
        }

    }

}
