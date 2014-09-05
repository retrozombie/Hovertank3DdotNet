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
using System.Text;

namespace Hovertank3DdotNet
{
    /// <summary>The game configuration / options (read only).</summary>
    class GameConfig
    {
        /// <summary>Creates a new GameConfig.</summary>
        public GameConfig()
        {
            _config = new Config(Encoding.ASCII);
        }

        /// <summary>The config.</summary>
        private Config _config;

        /// <summary>Reads the game configuration from a stream.</summary>
        /// <param name="stream">The stream.</param>
        public void Read(Stream stream)
        {
            _config.Read(stream);
        }

        /// <summary>Sets a default configuration.</summary>
        public void SetDefault()
        {
            _config.Clear();
        }

        /// <summary>Gets the index of the game / joystick controller to use.</summary>
        public int InputControllerIndex
        {
            get { return _config.GetInt32("Input.ControllerIndex", 0, MaxControllerIndex, 0); }
        }

        /// <summary>The maximum controller index.</summary>
        public const int MaxControllerIndex = 7;

        /// <summary>Gets whether sound is disabled.</summary>
        public bool SoundDisabled
        {
            get { return (_config.GetInt32("Sound.Disabled", 0, 1, 0) == 1); }
        }

        /// <summary>Gets whether to use speaker mode.</summary>
        public bool SoundSpeakerMode
        {
            get { return (_config.GetInt32("Sound.SpeakerMode", 0, 1, 0) == 1); }
        }

        /// <summary>Gets the volume for playback of sampled sounds.</summary>
        public float SoundSampledSoundVolume
        {
            get { return _config.GetSingle("Sound.SampledSoundVolume", 0.0f, 1.0f, 0.75f); }
        }

        /// <summary>Gets the volume for playback of speaker sounds.</summary>
        public float SoundSpeakerVolume
        {
            get { return _config.GetSingle("Sound.SpeakerVolume", 0.0f, 1.0f, 0.25f); }
        }
        
        /// <summary>Gets whether to run the game in windowed mode otherwise fullscreen.</summary>
        public bool VideoWindowed
        {
            get { return (_config.GetInt32("Video.Windowed", 0, 1, 1) == 1); }
        }

        /// <summary>Gets whether to center the window when running in windowed mode.</summary>
        public bool VideoWindowCenter
        {
            get { return (_config.GetInt32("Video.Window.Center", 0, 1, 1) == 1); }
        }

        /// <summary>Gets the left location for the window when running in windowed mode and not centering.</summary>
        public int VideoWindowLeft(int maximumLeft)
        {
            return _config.GetInt32("Video.Window.Left", 0, maximumLeft, 0);
        }

        /// <summary>Gets the top location for the window when running in windowed mode and not centering.</summary>
        public int VideoWindowTop(int maximumTop)
        {
            return _config.GetInt32("Video.Window.Top", 0, maximumTop, 0);
        }

        /// <summary>Gets the window width to set when running in windowed mode.</summary>
        public int VideoWindowWidth(int maximumWidth)
        {
            return _config.GetInt32("Video.Window.Width", 160, maximumWidth, 640);
        }

        /// <summary>Gets the window height to set when running in windowed mode.</summary>
        public int VideoWindowHeight(int maximumHeight)
        {
            return _config.GetInt32("Video.Window.Height", 120, maximumHeight, 480);
        }

        /// <summary>Gets whether to use vsync.</summary>
        public bool VideoVSync
        {
            get { return (_config.GetInt32("Video.VSync", 0, 1, 1) == 1); }
        }

        /// <summary>Gets whether to use filtering.</summary>
        public bool VideoFilter
        {
            get { return (_config.GetInt32("Video.Filter", 0, 1, 0) == 1); }
        }

    }
}
