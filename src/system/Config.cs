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
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hovertank3DdotNet
{
    /// <summary>A readonly configuration.</summary>
    class Config
    {
        /// <summary>Creates a new Config.</summary>
        /// <param name="encoding">The character encoding.</param>
        public Config(Encoding encoding)
        {
            _options = new Dictionary<string, string>(StringComparer.Ordinal);
            _encoding = encoding;
        }

        /// <summary>The id string.</summary>
        private string _id;

        /// <summary>Gets or sets the ID string.</summary>
        public string ID
        {
            get { return _id; }
        }

        /// <summary>The dictionary of options (maps an identifier to a value).</summary>
        private Dictionary<string, string> _options;

        /// <summary>The character encoding.</summary>
        private Encoding _encoding;

        /// <summary>Clears configuration settings.</summary>
        public void Clear()
        {
            _options.Clear();
        }

        /// <summary>Reads the configuration from a stream.</summary>
        /// <param name="stream">The stream.</param>
        public void Read(Stream stream)
        {
            Parser parser = new Parser();
            parser.Run(this, stream);
        }

        /// <summary>Gets an option string.</summary>
        /// <param name="identifier">The option's identifier.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <returns>A string.</returns>
        public string GetString(string identifier, string defaultValue)
        {
            string value;

            if(!_options.TryGetValue(identifier, out value))
                value = defaultValue;

            return value;
        }

        /// <summary>Sets an option string.</summary>
        /// <param name="identifier">The option's identifier.</param>
        /// <param name="value">The value.</param>
        public void SetString(string identifier, string value)
        {
            _options[identifier] = value;
        }

        /// <summary>Gets an integer option.</summary>
        /// <param name="identifier">The option identifier.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="defaultValue">The default value to use on failure.</param>
        /// <returns>An int.</returns>
        public int GetInt32(string identifier, int minimumValue, int maximumValue, int defaultValue)
        {
            string s;
            if(!_options.TryGetValue(identifier, out s))
                return defaultValue;

            int value;
            if(int.TryParse(s, out value))
            {
                if(value < minimumValue || value > maximumValue)
                    value = defaultValue;
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Gets a floating point option (single precision).</summary>
        /// <param name="identifier">The option identifier.</param>
        /// <param name="minimumValue">The minimum value.</param>
        /// <param name="maximumValue">The maximum value.</param>
        /// <param name="defaultValue">The default value to use on failure.</param>
        /// <returns>A float.</returns>
        public float GetSingle(string identifier, float minimumValue, float maximumValue, float defaultValue)
        {
            string s;
            if(!_options.TryGetValue(identifier, out s))
                return defaultValue;

            float value;
            if(float.TryParse(s, out value))
            {
                if(value < minimumValue || value > maximumValue)
                    value = defaultValue;
            }
            else
            {
                value = defaultValue;
            }

            return value;
        }

        /// <summary>Configuration parser.</summary>
        class Parser
        {
            /// <summary>The config.</summary>
            private Config _config;

            /// <summary>The stream reader.</summary>
            private StreamReader _streamReader;

            /// <summary>The current line.</summary>
            private string _line;

            /// <summary>The current line number.</summary>
            private int _lineNumber;

            /// <summary>The current character index.</summary>
            private int _chrIndex;

            /// <summary>Reads the next line.</summary>
            /// <returns>False if the end of the stream has been reached.</returns>
            private bool ReadLine()
            {
                _line = _streamReader.ReadLine();

                if(_line != null)
                    _lineNumber++;

                _chrIndex = 0;

                return (_line != null);
            }

            /// <summary>Runs the parser.</summary>
            /// <param name="config">The config.</param>
            /// <param name="stream">The stream containing the configuration data.</param>
            public void Run(Config config, Stream stream)
            {
                _config = config;
                _streamReader = new StreamReader(stream, config._encoding);
                _lineNumber = 0;

                if(!ReadLine())
                    throw new Exception("Unexpected end of stream, expecting configuration version identifier!");

                config._id = _line;
                config._options.Clear();

                _optionValue = new StringBuilder();

                try
                {
                    while(ReadLine())
                    {
                        if(_line.Length == 0)
                            continue; // Empty line

                        // Comment
                        if(_line.StartsWith("//", StringComparison.Ordinal))
                            continue;

                        _optionIdentifier = ParseIdentifier();

                        string value = ParseValue();

                        _config.SetString(_optionIdentifier, value);
                    }
                }
                catch(Exception ex)
                {
                    throw new Exception(ex.Message + ", at line " + _lineNumber, ex);
                }
            }

            /// <summary>Parses an identifier from the specified character position.</summary>
            /// <returns>A string.</returns>
            private string ParseIdentifier()
            {
                // A legal option identifier is like a C# namespace
                // identifier = <letter or underscore>[letter, underscore or digit]
                // option_identifier = identifier ['.' identifier]+
                // i.e. "_blah", "Example42" or "Window.Left"

                char chr = _line[_chrIndex];
                if(chr != '_' && !char.IsLetter(chr))
                    throw new Exception("Expecting an option identifier!");

                while(++_chrIndex < _line.Length)
                {
                    chr = _line[_chrIndex];

                    if(chr != '_' && !char.IsLetterOrDigit(chr))
                    {   // End of identifier
                        if(chr == '.')
                        {
                            if(_line[_chrIndex - 1] == '.')
                                throw new Exception("Illegal option identifier!");

                            continue;
                        }
                        else if(!char.IsWhiteSpace(chr))
                        {
                            throw new Exception("Illegal option identifier!");
                        }

                        break;
                    }
                }

                if(_chrIndex == _line.Length)
                    throw new Exception("Unexpected end of line, expecting the option's value!");

                if(_line[_chrIndex - 1] == '.')
                    throw new Exception("Illegal option identifier!");

                return _line.Substring(0, _chrIndex);
            }

            /// <summary>Skips whitespace from the current character position.</summary>
            /// <returns>False if the end of line was reached.</returns>
            private bool SkipWhiteSpace()
            {
                while(_chrIndex < _line.Length)
                {
                    if(!char.IsWhiteSpace(_line[_chrIndex]))
                        break;

                    _chrIndex++;
                }

                return (_chrIndex < _line.Length);
            }

            /// <summary>The identifier for the option being parsed.</summary>
            private string _optionIdentifier;

            /// <summary>The value for the option being parsed.</summary>
            private StringBuilder _optionValue;

            /// <summary>Parses a value.</summary>
            /// <returns>A string.</returns>
            private string ParseValue()
            {
                // A value is either a string or not a string
                // Non-string values are a sequence of characters without whitespace until the end of the line
                // Strings are enclosed in quotes (only one string can be specified per line).
                // Multi-line strings can be formed by ending the line with '+' and specifying the next
                // continuation on the next line.
                // Example "Line 1" +
                //         "Line 2"
                // Produces the string "Line 1Line 2".
                // The following escape sequences are supported for string values:
                // \n - new line, \\ - backslash, \" - quote

                _optionValue.Length = 0;

                if(!SkipWhiteSpace())
                    throw new Exception("Unexpected end of line, expecting a value!");

                if(_line[_chrIndex] == '"')
                {   // String
                    do
                    {
                        // Skip quote
                        _chrIndex++;

                        bool inEscape = false;
                        while(true)
                        {
                            if(_chrIndex == _line.Length)
                                throw new Exception("Unexpected end of line, expecting closing quote!");

                            char chr = _line[_chrIndex++];

                            if(inEscape)
                            {   // End escape sequence
                                switch(chr)
                                {
                                    case 'n': _optionValue.Append('\n'); break;
                                    case '\\': _optionValue.Append('\\'); break;
                                    case '"': _optionValue.Append('"'); break;
                                    default: throw new Exception("Unsupported escape sequence!");
                                }

                                inEscape = false;
                            }
                            else if(chr == '"')
                            {   // End of string
                                break;
                            }
                            else if(chr == '\\')
                            {   // Begin escape sequence
                                inEscape = true;
                            }
                            else
                            {   // Character value
                                _optionValue.Append(chr);
                            }
                        }

                        if(_chrIndex == _line.Length)
                            break; // End of value

                        if(!SkipWhiteSpace())
                            throw new Exception("Unexpected end of line, expecting a string continuation character!");

                        if(_line[_chrIndex++] != '+')
                            throw new Exception("Unexpected end of line, expecting a string continuation character!");

                        if(SkipWhiteSpace())
                            throw new Exception("Unexpected characters after string continuation character!");

                        if(!ReadLine())
                            throw new Exception("Unexpected end of file, expecting continuation of string value!");

                        // Empty lines are not allowed between strings
                        if(!SkipWhiteSpace())
                            throw new Exception("Expecting the continuation of a string value, not an empty line!");

                        if(_line[_chrIndex] != '"')
                            throw new Exception("Expecting the continuation of a string value!");

                    } while(true);
                }
                else
                {   // Character sequence (excluding whitespace) terminated by an end of line
                    int startIndex = _chrIndex++;

                    while(_chrIndex < _line.Length)
                    {
                        if(char.IsWhiteSpace(_line[_chrIndex]))
                            throw new Exception("Illegal option value!");

                        _chrIndex++;
                    }

                    _optionValue.Append(_line.Substring(startIndex));
                }

                return _optionValue.ToString();
            }
        }

    }
}
