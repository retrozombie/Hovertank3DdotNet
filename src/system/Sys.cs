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
    /// <summary>System abstraction.</summary>
    abstract class Sys : IDisposable
    {
        /// <summary>Initialises a new system.</summary>
        /// <param name="commandLineArguments">The command line arguments.</param>
        /// <param name="resourceProvider">The resource provider.</param>
        protected Sys(string[] commandLineArguments, ResourceProvider resourceProvider)
        {
            _commandLineArguments = commandLineArguments;

            _sfxPlayer = new SfxPlayer();

            if(resourceProvider != null)
            {
                _resourceProvider = resourceProvider;
            }
            else
            {   // Resources provided by the file system
                _resourceProvider = new FileResourceProvider();

                // Check for -gamedir
                int index = IndexOfCommandLineArgument("-gamedir");
                if(index != -1 && index + 1 < _commandLineArguments.Length)
                {
                    string path = _commandLineArguments[index + 1];

                    if(!Directory.Exists(path))
                        throw new DirectoryNotFoundException("The path specified by -gamedir does not exist!");

                    _gameDirectory = path;
                }
            }
            
            _introLogo = (IndexOfCommandLineArgument("-intrologo") != -1);
            _introInfo = (IndexOfCommandLineArgument("-introinfo") != -1);

            _strings = new Strings();

            _gameConfig = new GameConfig();
            LoadConfig();
        }

        /// <summary>Initialises a new system.</summary>
        /// <param name="commandLineArguments">The command line arguments.</param>
        protected Sys(string[] commandLineArguments)
            : this(commandLineArguments, null)
        {
        }

        /// <summary>Disposes resources.</summary>
        public virtual void Dispose()
        {
            if(_sfxPlayer != null)
            {
                try { _sfxPlayer.Dispose(); }
                catch { }
                _sfxPlayer = null;
            }
        }

        /// <summary>Initialises the sound system.</summary>
        /// <param name="soundSystem">The sound system.</param>
        public virtual void InitialiseSound(SoundSystem soundSystem)
        {
            _sfxPlayer.Disabled = _gameConfig.SoundDisabled;
            _sfxPlayer.SpeakerMode = _gameConfig.SoundSpeakerMode;
            _sfxPlayer.SampledSoundVolume = _gameConfig.SoundSampledSoundVolume;
            _sfxPlayer.SpeakerVolume = _gameConfig.SoundSpeakerVolume;

            _sfxPlayer.Initialise(this, soundSystem);
        }

        /// <summary>The sound effect player.</summary>
        private SfxPlayer _sfxPlayer;

        /// <summary>Gets the sound effect player.</summary>
        public SfxPlayer SfxPlayer
        {
            get { return _sfxPlayer; }
        }

        /// <summary>Initialises the input system.</summary>
        /// <param name="inputSystem">The input system.</param>
        public virtual void InitialiseInput(InputSystem inputSystem)
        {
            _inputSystem = inputSystem;
            _inputSystem.ControllerIndex = _gameConfig.InputControllerIndex;
        }

        /// <summary>The input system.</summary>
        private InputSystem _inputSystem;

        /// <summary>Gets the input system.</summary>
        public InputSystem InputSystem
        {
            get { return _inputSystem; }
        }

        /// <summary>The game directory or null.</summary>
        private string _gameDirectory;

        /// <summary>Returns a the filename relative to the game directory (if it had been specified by -gamedir).</summary>
        /// <param name="fileName">The filename.</param>
        /// <returns>A string.</returns>
        protected string GetGameFile(string fileName)
        {
            if(_gameDirectory == null)
                return fileName;

            return Path.Combine(_gameDirectory, fileName);
        }

        /// <summary>The game configuration.</summary>
        protected GameConfig _gameConfig;

        /// <summary>Gets the game configuration.</summary>
        public GameConfig GameConfig
        {
            get { return _gameConfig; }
        }

        /// <summary>Loads the game configuration.</summary>
        protected virtual void LoadConfig()
        {
            try
            {
                byte[] configData = null;
                string configFileName;
                int index = IndexOfCommandLineArgument("-config");
                if(index != -1 && index + 1 < _commandLineArguments.Length)
                {
                    configFileName = _commandLineArguments[index + 1];

                    if(!File.Exists(configFileName))
                        throw new Exception("The configuration file specified by '-config' does not exist!");

                    configData = FileReadAllBytes(configFileName);
                }
                else
                {
                    configFileName = "CONFIG." + Hovertank.EXTENSION;

                    if(_gameDirectory != null && FileExists(configFileName))
                        configData = FileReadAllBytes(configFileName);

                    if(configData == null && _resourceProvider.Exists(configFileName))
                        configData = _resourceProvider.GetBytes(configFileName);
                }

                if(configData != null)
                {
                    _gameConfig.Read(new MemoryStream(configData));
                }
                else
                {
                    _gameConfig.SetDefault();
                }
            }
            catch(Exception ex)
            {
                Log("LoadConfig failed: " + ex.Message);
                _gameConfig.SetDefault();
            }
        }

        /// <summary>The resource provider.</summary>
        private ResourceProvider _resourceProvider;

        /// <summary>Gets the resource provider.</summary>
        public ResourceProvider ResourceProvider
        {
            get { return _resourceProvider; }
        }

        /// <summary>The string replacements system.</summary>
        private Strings _strings;

        /// <summary>Gets the string replacements system.</summary>
        public Strings Strings
        {
            get { return _strings; }
        }

        /// <summary>Whether to display the scaled logo during the intro.</summary>
        private bool _introLogo;

        /// <summary>Gets whether to display the scaled logo during the intro.</summary>
        public bool IntroLogo
        {
            get { return _introLogo; }
        }

        /// <summary>Whether the info window can be displayed during the intro.</summary>
        private bool _introInfo;

        /// <summary>Gets whether the info window can be displayed during the intro.</summary>
        public bool IntroInfo
        {
            get { return _introInfo; }
        }

        /// <summary>The command line arguments.</summary>
        private string[] _commandLineArguments;

        /// <summary>Returns the index of the specified command line argument.</summary>
        /// <param name="argument">The argument to find.</param>
        /// <returns>The index of the argument or -1 if it is not present.</returns>
        public int IndexOfCommandLineArgument(string argument)
        {
            for(int i = 0; i < _commandLineArguments.Length; i++)
                if(string.Compare(_commandLineArguments[i], argument, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;

            return -1;
        }
       
        /// <summary>Gets the command line arguments.</summary>
        public string[] argv
        {
            get { return _commandLineArguments; }
        }

        /// <summary>Gets the number of command line arguments.</summary>
        public int argc
        {
            get { return _commandLineArguments.Length; }
        }

        /// <summary>Returns whether the character is classed as a letter.</summary>
        /// <param name="chr">The character.</param>
        /// <returns>A bool.</returns>
        public bool isalpha(sbyte chr)
        {
            return char.IsLetter((char) chr);
        }

        /// <summary>Returns whether the character is classed as an upper case letter.</summary>
        /// <param name="chr">The character.</param>
        /// <returns>A bool.</returns>
        public bool isupper(sbyte chr)
        {
            return isalpha(chr) && char.IsUpper((char) chr);
        }

        /// <summary>Converts the case of the character to lower case.</summary>
        /// <param name="chr">The character.</param>
        /// <returns>The converted character.</returns>
        public sbyte tolower(sbyte chr)
        {
            return (sbyte) char.ToLower((char) chr);
        }

        /// <summary>Converts the case of the character to upper case.</summary>
        /// <param name="chr">The character.</param>
        /// <returns>The converted character.</returns>
        public sbyte toupper(sbyte chr)
        {
            return (sbyte) char.ToUpper((char) chr);
        }

        /// <summary>Allocates a block of memory and returns a far pointer.</summary>
        /// <param name="length">The length of the memory block.</param>
        /// <returns>A byte array.</returns>
        public byte[] farmalloc(int length)
        {
            // as: The original memory management system is not simulated
            return new byte[length];
        }

        /// <summary>Returns the offset part of a far pointer.</summary>
        /// <param name="pointer">The memory pointer.</param>
        /// <returns>The offset.</returns>
        public ushort FP_OFF(memptr pointer)
        {
            return (ushort) pointer.BaseIndex;
        }

        /// <summary>Starts a speaker sound at the specified frequency.</summary>
        /// <param name="frequency">The frequency in hertz.</param>
        public virtual void sound(ushort frequency)
        {
        }

        /// <summary>Stops speaker sound.</summary>
        public virtual void nosound()
        {
        }

        /// <summary>Gets whether to simulate the intro sound using a sampled sound.</summary>
        public abstract bool SimulateIntroSound
        {
            get;
        }

        /// <summary>Logs a message for debugging.</summary>
        /// <param name="message">The message / object.</param>
        public abstract void Log(object message);

        /// <summary>Gets whether the system can quit.</summary>
        public abstract bool CanQuit
        {
            get;
        }

        /// <summary>Exits the application.</summary>
        /// <param name="exitCode">The exit code.</param>
        public abstract void exit(short exitCode);

        /// <summary>Returns info about the last bios key pressed.</summary>
        /// <param name="test">Whether the key press is tested for and not consumed.</param>
        /// <returns>The key code.</returns>
        public virtual int bioskey(bool test)
        {
            // as: This isn't needed at the moment, it is only called if video card detection fails

            // test == 1 - return whether a key was pressed but don't consume it
            // test == 0 - return key and consume it

            return 0;
        }

        /// <summary>Reads a file completely and returns the memory block.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A byte array.</returns>
        public byte[] FileReadAllBytes(string fileName)
        {
            fileName = GetGameFile(fileName);
            return _resourceProvider.GetBytes(fileName);
        }

        /// <summary>Returns whether the specified file exists.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>True if the file exists.</returns>
        public bool FileExists(string fileName)
        {
            fileName = GetGameFile(fileName);
            return _resourceProvider.Exists(fileName);
        }

        /// <summary>Returns the length of the file.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The length of the file in bytes.</returns>
        public int FileLength(string fileName)
        {
            fileName = GetGameFile(fileName);
            return _resourceProvider.GetLength(fileName);
        }

        /// <summary>Loads hovertank's configuration data.</summary>
        /// <returns>A pointer to the buffer that contains the data.</returns>
        public virtual memptr LoadControls()
        {
            string fileName = GetGameFile("CTLPANEL." + Hovertank.EXTENSION);
            
            if(!File.Exists(fileName))
                return new memptr();

            return new memptr(File.ReadAllBytes(fileName));
        }

        /// <summary>Saves hovertank's configuration data.</summary>
        /// <param name="pointer">A pointer to the buffer that contains the data.</param>
        public virtual void SaveControls(memptr pointer)
        {
            string fileName = GetGameFile("CTLPANEL." + Hovertank.EXTENSION);
            File.WriteAllBytes(fileName, pointer.Buffer);
        }

        /// <summary>Reads a file completely into the specified buffer.</summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>The number of bytes read.</returns>
        public uint FileReadAllBytes(string fileName, byte[] buffer)
        {
            byte[] bytes = FileReadAllBytes(fileName);
            Array.Copy(bytes, buffer, bytes.Length);
            return (uint) bytes.Length;
        }

        /// <summary>Opens a file for reading and returns a handle to it.</summary>
        /// <remarks>as: For simplicity changed from a handle to a memptr.</remarks>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A memory pointer.</returns>
        public memptr open(string fileName)
        {
            byte[] buffer = null;

            if(FileExists(fileName))
                buffer = FileReadAllBytes(fileName);

            return new memptr(buffer);
        }

        /// <summary>Sets the background color for text.</summary>
        /// <param name="color">The color.</param>
        public virtual void textbackground(short color)
        {
        }

        /// <summary>Sets the foreground color for text.</summary>
        /// <param name="color">The color.</param>
        public virtual void textcolor(short color)
        {
        }

        /// <summary>Prints formatted output to the screen.</summary>
        /// <param name="format">The format string.</param>
        /// <param name="values">The parameters.</param>
        public virtual void printf(string format, params object[] values)
        {
        }

        /// <summary>Clears the screen.</summary>
        public virtual void clrscr()
        {
        }

        /// <summary>Writes a string to the screen.</summary>
        /// <param name="s">The string.</param>
        public virtual void puts(string s)
        {
        }

        /// <summary>The axis count for a disconnected joystick.</summary>
        protected const short JoystickDisconnectedAxisValue = 500;

        /// <summary>The value for a joystick without any buttons pressed.</summary>
        protected const byte JoystickUnpressedButtonsValue = 0xff;

        /// <summary>The mask for setting joystick 1 button 1 as pressed.</summary>
        protected const byte Joystick1Button1Mask = 0xef;

        /// <summary>The mask for setting joystick 1 button 2 as pressed.</summary>
        protected const byte Joystick1Button2Mask = 0xdf;

        /// <summary>The mask for setting joystick 2 button 1 as pressed.</summary>
        protected const byte Joystick2Button1Mask = 0xbf;

        /// <summary>The mask for setting joystick 2 button 2 as pressed.</summary>
        protected const byte Joystick2Button2Mask = 0x7f;

        /// <summary>Reads the specified joystick's buttons.</summary>
        /// <param name="joynum">The joystick number.</param>
        /// <returns>
        /// The button states.
        /// 0x10 = Joystick 1 - button 1 (0 = pressed).
        /// 0x20 = Joystick 1 - button 2 (0 = pressed).
        /// 0x40 = Joystick 2 - button 1 (0 = pressed).
        /// 0x80 = Joystick 2 - button 2 (0 = pressed).
        /// </returns>
        public virtual byte ReadJoystickButtons(short joynum)
        {
            byte value = JoystickUnpressedButtonsValue;

            if(joynum == 1)
            {
                if((_inputSystem.JoystickButtons & 1) != 0)
                    value &= Joystick1Button1Mask;

                if((_inputSystem.JoystickButtons & 2) != 0)
                    value &= Joystick1Button2Mask;
            }

            // as: Joystick 2 wasn't used

            return value;
        }

        /// <summary>Reads the specified joystick's axes.</summary>
        /// <param name="joynum">The joystick number.</param>
        /// <param name="xcount">The variable that will receive the x-axis count.</param>
        /// <param name="ycount">The variable that will receive the y-axis count.</param>
        public virtual void ReadJoystick(short joynum, out short xcount, out short ycount)
        {
            if(_inputSystem.JoystickConnected)
            {
                xcount = (short) (200.0f + _inputSystem.JoystickXAxis * 200.0f);
                ycount = (short) (200.0f + _inputSystem.JoystickYAxis * 200.0f);
            }
            else
            {
                xcount = JoystickDisconnectedAxisValue;
                ycount = JoystickDisconnectedAxisValue;
            }
        }

    }
}
