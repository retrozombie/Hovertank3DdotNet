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
using System.Text;

namespace Hovertank3DdotNet
{
    /// <summary>Speaker sound structure.</summary>
    /// <remarks>
    /// The file SOUNDS.HOV contains the speaker sounds.
    /// The first part of the file contains an array of structures (spksndtype) which have information about each sound.
    /// The rest of the file contains the sound sequences.
    /// Each sequence contains timer values, a zero indicates the speaker should be switched off and 0xffff (-1) 
    /// indicates the end of the sequence.
    /// When a sound is played the sequence is stepped through every 1/140 second.
    /// </remarks>
	class spksndtype
	{
        /// <summary>Creates a new spksndtype.</summary>
        /// <param name="pointer">The pointer to the sound data.</param>
		public spksndtype(memptr pointer)
		{
			_pointer = pointer;
		}

        /// <summary>The pointer to the sound data.</summary>
        private memptr _pointer;

        /// <summary>Gets the pointer to the sound data.</summary>
        public memptr Pointer
		{
			get { return _pointer; }
		}

        /// <summary>Gets or sets the start of the sound sequence.</summary>
        /// <returns>A ushort.</returns>
		public ushort start
		{
            get { return _pointer.GetUInt16(snd_start); }
            set { _pointer.SetUInt16(snd_start, value); }
		}

        /// <summary>Gets or sets the sound's priority.</summary>
        /// <returns>A byte.</returns>
        public byte priority
        {
            get { return _pointer.GetUInt8(snd_priority); }
            set { _pointer.SetUInt8(snd_priority, value); }
        }

        /// <summary>Gets or sets the sound's samples value.</summary>
        /// <returns>A byte.</returns>
        public byte samples
        {
            get { return _pointer.GetUInt8(snd_samples); }
            set { _pointer.SetUInt8(snd_samples, value); }
        }

        /// <summary>Gets or sets the sound's name.</summary>
        /// <returns>A string.</returns>
		public string name
		{
            get
            {
                int length;
                for(length = 0; length < 12; length++)
                    if(_pointer.GetUInt8(snd_name + length) == 0)
                        break;

                return Encoding.ASCII.GetString(_pointer.Buffer, _pointer.BaseIndex + snd_name, length);
            }
            set
            {
                if(string.IsNullOrEmpty(value))
                    throw new Exception("value cannot be null or empty!");

                if(value.Length > 11)
                    throw new Exception("value maximum length is 11 characters!");

                Encoding.ASCII.GetBytes(value, 0, value.Length, _pointer.Buffer, _pointer.BaseIndex + snd_name);
            }
		}

        /// <summary>The size of the spksndtype structure in bytes.</summary>
		public const int SizeOf = 16;

        /// <summary>The offset for the start field.</summary>
        public const int snd_start = 0;

        /// <summary>The offset for the priority field.</summary>
        public const int snd_priority = 2;

        /// <summary>The offset for the samples field.</summary>
        public const int snd_samples = 3;

        /// <summary>The offset for the name field.</summary>
        public const int snd_name = 4;
        
        /// <summary>The clock frequency.</summary>
        public const double TimerFrequency = 1193181.666666666;

        /// <summary>Returns the frequency in Hz for the specified timer value.</summary>
        /// <remarks>ref: http://wiki.osdev.org/PC_Speaker for frequency.</remarks>
        /// <param name="timerValue">The timer value.</param>
        /// <returns>A double.</returns>
        public static double Frequency(ushort timerValue)
        {
            return TimerFrequency / timerValue;
        }

        /// <summary>Returns the frequency in Hz for the specified timer value.</summary>
        /// <param name="frequency">The frequency.</param>
        /// <returns>The timer value.</returns>
        public static ushort TimerValue(double frequency)
        {
            return (ushort) (TimerFrequency / frequency);
        }

        /// <summary>The special sound sequence value that indicates the speaker should be switched off.</summary>
        public const ushort SpeakerData_StopSound = 0;

        /// <summary>The special sound sequence value that indicates the end of the sound.</summary>
        public const ushort SpeakerData_EndSound = 0xffff;
	}
}
