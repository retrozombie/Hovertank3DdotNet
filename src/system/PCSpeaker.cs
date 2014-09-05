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
using System.IO;

namespace Hovertank3DdotNet
{
    /// <summary>Helper for generating simulations of PC speaker sound effects.</summary>
    class PCSpeaker
    {
        /// <summary>Creates a new PCSpeaker.</summary>
        /// <param name="sampleFrequency">The sample frequency.</param>
        /// <param name="peakAmplitude">The peak amplitude.</param>
        public PCSpeaker(double sampleFrequency, double peakAmplitude)
        {
            _sampleFrequency = sampleFrequency;
            _peakAmplitude = (short) (peakAmplitude * 32767.0);
            _memoryStream = new MemoryStream();
            _timeIndex = 1;
        }

        /// <summary>The sample frequency.</summary>
        private double _sampleFrequency;

        /// <summary>The peak amplitude.</summary>
        private short _peakAmplitude;

        /// <summary>The sample index.</summary>
        private int _sampleIndex;

        /// <summary>The time index.</summary>
        private int _timeIndex;

        /// <summary>The memory stream.</summary>
        private MemoryStream _memoryStream;

        /// <summary>The timer value.</summary>
        private ushort _timerValue;

        /// <summary>The new timer value.</summary>
        private ushort _newTimerValue;

        /// <summary>The timer count.</summary>
        private double _timerCount;

        /// <summary>The speaker output state.</summary>
        private bool _output;

        /// <summary>Sets the sound frequency.</summary>
        /// <param name="frequency">The frequency in Hertz.</param>
        public void SetFrequency(double frequency)
        {
            _newTimerValue = spksndtype.TimerValue(frequency);
        }

        /// <summary>Sets the timer value.</summary>
        /// <param name="timerValue">The timer value.</param>
        public void SetTimer(ushort timerValue)
        {
            _newTimerValue = timerValue;
        }

        /// <summary>The elapsed time.</summary>
        private double _elapsedTime;

        /// <summary>Gets the elapsed time.</summary>
        public double ElapsedTime
        {
            get { return _elapsedTime; }
        }

        /// <summary>Writes samples for the specified duration.</summary>
        /// <param name="time">The length of time in seconds.</param>
        public void WriteSamples(double time)
        {
            int nextSampleIndex = _sampleIndex + (int) (_timeIndex * time * _sampleFrequency);
            while(_sampleIndex != nextSampleIndex)
            {
                if(_newTimerValue == spksndtype.SpeakerData_StopSound)
                {
                    _memoryStream.WriteByte(0);
                    _memoryStream.WriteByte(0);
                    _timerValue = spksndtype.SpeakerData_StopSound;
                }
                else
                {
                    if(_timerValue == spksndtype.SpeakerData_StopSound)
                        _timerValue = _newTimerValue;

                    short sample = _peakAmplitude;
                    if(!_output)
                        sample = (short) -sample;

                    _memoryStream.WriteByte((byte) sample);
                    _memoryStream.WriteByte((byte) (sample >> 8));

                    _timerCount += (spksndtype.TimerFrequency * 2.0) / _sampleFrequency;
                    if(_timerCount >= _timerValue)
                    {
                        _timerCount -= _timerValue;
                        _output = !_output;
                        _timerValue = _newTimerValue;
                    }
                }

                _sampleIndex++;
            }

            _elapsedTime += time;
        }

        /// <summary>Gets a buffer containing the sample data.</summary>
        /// <returns>A byte array.</returns>
        public byte[] ToArray()
        {
            return _memoryStream.ToArray();
        }
    }
}
