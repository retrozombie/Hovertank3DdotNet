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

namespace Hovertank3DdotNet
{
    /// <summary>Hovertank strings, provides functionality to replace strings.</summary>
    class Strings
    {
        /// <summary>Initialises the string replacement system.</summary>
        /// <param name="hovertank">The hovertank game.</param>
        public void Initialise(Hovertank hovertank)
        {
            _replacements = new string[Strings.Count];

            Array.Copy(_originalStrings, _replacements, _originalStrings.Length);

            for(int i = 0; i < hovertank.levnames.Length; i++)
                _replacements[Strings.levnames(i)] = hovertank.levnames[i];

            for(int i = 0; i < hovertank.levtext.Length; i++)
                _replacements[Strings.levtext(i)] = hovertank.levtext[i];
        }

        /// <summary>Original strings.</summary>
        private static readonly string[] _originalStrings = new string[]
        {
            // Intro#
            "Hovertank v1.17\n\n",
            "Softdisk Publishing delivers a\n",
            "high quality EGA game to\n",
            "your door every month!\n",
            "Call 1-800-831-2694 for\n",
            "subscription rates and\n",
            "back issues.\n",

            // DoHelpText#
            "HoverTank Commands\n",

            "F2  : Sound on / off\n" +
            "F3  : Keyboard mode / custom\n" +
            "F4  : Joystick mode\n" +
            "F5  : Reset game\n" +
            "ESC : Quit\n",

            "UP / DOWN : forward/reverse\n" +
            "LEFT / RIGHT : turn\n",

            "<MORE>",

            "Button 1 / Ctrl\n",

            "---------------\n\n",

            "Charge cannon.  A fully charged cannon\n" +
            "can shoot through multiple targets.\n" +
            "While the cannon is charging, your\n" +
            "tanks turning speed is halved for fine\n" +
            "aiming adjustments.\n\n",

            "<MORE>",

            "Button 2 / Alt\n",

            "---------------\n\n",

            "Afterburner thrusting.  Doubles your\n" +
            "forward or backwards speed, but does\n" +
            "not effect your turning speed.\n",

            // DebugMemory#
            "Memory Usage\n",
            
            "------------",

            "\nTotal     :",

            "k\nFree      :",

            "k\nWith purge:",

            "k\n",

            "",

            // DebugKeys#
            "God mode off",

            "God mode on",

            "Warp to which level(1-20):",

            // CheckKeys#
            "Sound (Y/N)?",

            "RESET GAME (Y/N)?",

            "QUIT (Y/N)?",

            // main#
            "HOVERTANK 3-D\n",

            "Copyright 1991-93 Softdisk Publishing\n",

            "Version 1.17\n",

            // GameOver#
            "New High Score!\n",

            "Score\n",

            "\n\n",

            "Old\n",

            "\n",

            "Congratulations!\n",

            "",

            // Victory#
            "Crowds of cheering people surround\n",

            "your tank. Cargo lifts deliver your\n",

            "impressive bounty. The crowd quiets\n",

            "as a distinguished man steps forward.\n",

            "'Well done,' says the UFA President.\n",

            "'You have saved many deserving people.'\n",

            "\n",

            "'Mr. Sledge?  I said you've done well.'\n",

            "You ignore him and count the reward\n",

            "again. He says, 'Too bad about those\n",

            "ones you lost...'\n",

            "'What?  I dropped some bills?' you say.\n",

            "Game Over!",

            // BaseScreen#
            "UFA Headquarters\n",

            "Saved:",

            "\nLost:",

            "\nSavior reward...",

            "\nTime bonus...\n",

            "\nRepairing tank...",

            "Mission completed!",

            "GO TO NEXT SECTOR",

            // PlayGame#
            " Start at what level (1-",

            ")?",

            "Continue game ?",

            // CalibrateJoy#
            "Joystick Configuration\n",

            "Hold the joystick in the UPPER LEFT\n",

            "corner and press a button:",

            "\nHold the joystick in the LOWER RIGHT\n",

            "corner and press a button:",

            "\n(F)ire or (A)fterburn with B1 ?",

            // calibratekeys#
            "Keyboard Configuration",

            "\n1 north",

            "\n2 east",

            "\n3 south",

            "\n4 west",

            "\n5 button1",

            "\n6 button2",

            "\nModify which action:",

            ":",

            "\nPress the new key:",
        };

        /// <summary>The string replacements.</summary>
        private string[] _replacements;

        /// <summary>Gets or sets a string.<summary>
        /// <param name="stringIndex">The string index (Strings.xxx).</param>
        /// <returns>The StringReplacement.</returns>
        public string this[int stringIndex]
        {
            get { return _replacements[stringIndex]; }
            set { _replacements[stringIndex] = value; }
        }

        #region String Indices

        public const int Intro1 = 0;

        public const int Intro2 = 1;

        public const int Intro3 = 2;

        public const int Intro4 = 3;

        public const int Intro5 = 4;

        public const int Intro6 = 5;

        public const int Intro7 = 6;

        public const int DoHelpText1 = 7;

        public const int DoHelpText2 = 8;

        public const int DoHelpText3 = 9;

        public const int DoHelpText4 = 10;

        public const int DoHelpText5 = 11;

        public const int DoHelpText6 = 12;

        public const int DoHelpText7 = 13;

        public const int DoHelpText8 = 14;

        public const int DoHelpText9 = 15;

        public const int DoHelpText10 = 16;

        public const int DoHelpText11 = 17;

        public const int DebugMemory1 = 18;

        public const int DebugMemory2 = 19;

        public const int DebugMemory3 = 20;

        public const int DebugMemory4 = 21;

        public const int DebugMemory5 = 22;

        public const int DebugMemory6 = 23;

        public const int DebugMemory7 = 24;

        public const int DebugKeys1 = 25;

        public const int DebugKeys2 = 26;

        public const int DebugKeys3 = 27;

        public const int CheckKeys1 = 28;

        public const int CheckKeys2 = 29;

        public const int CheckKeys3 = 30;

        public const int main1 = 31;

        public const int main2 = 32;

        public const int main3 = 33;

        public const int GameOver1 = 34;

        public const int GameOver2 = 35;

        public const int GameOver3 = 36;

        public const int GameOver4 = 37;

        public const int GameOver5 = 38;

        public const int GameOver6 = 39;

        public const int GameOver7 = 40;

        public const int Victory1 = 41;

        public const int Victory2 = 42;

        public const int Victory3 = 43;

        public const int Victory4 = 44;

        public const int Victory5 = 45;

        public const int Victory6 = 46;

        public const int Victory7 = 47;

        public const int Victory8 = 48;

        public const int Victory9 = 49;

        public const int Victory10 = 50;

        public const int Victory11 = 51;

        public const int Victory12 = 52;

        public const int Victory13 = 53;

        public const int BaseScreen1 = 54;

        public const int BaseScreen2 = 55;

        public const int BaseScreen3 = 56;

        public const int BaseScreen4 = 57;

        public const int BaseScreen5 = 58;

        public const int BaseScreen6 = 59;

        public const int BaseScreen7 = 60;

        public const int BaseScreen8 = 61;

        public const int PlayGame1 = 62;

        public const int PlayGame2 = 63;

        public const int PlayGame3 = 64;

        public const int CalibrateJoy1 = 65;

        public const int CalibrateJoy2 = 66;

        public const int CalibrateJoy3 = 67;

        public const int CalibrateJoy4 = 68;

        public const int CalibrateJoy5 = 69;

        public const int CalibrateJoy6 = 70;

        public const int calibratekeys1 = 71;

        public const int calibratekeys2 = 72;

        public const int calibratekeys3 = 73;

        public const int calibratekeys4 = 74;

        public const int calibratekeys5 = 75;

        public const int calibratekeys6 = 76;

        public const int calibratekeys7 = 77;

        public const int calibratekeys8 = 78;

        public const int calibratekeys9 = 79;

        public const int calibratekeys10 = 80;

        /// <summary>The base index for levnames.</summary>
        private const int levnames_base = 81;

        /// <summary>Returns the string index for the specified level.</summary>
        /// <param name="arrayIndex">The levname array index.</param>
        /// <returns>An int.</returns>
        public static int levnames(int arrayIndex)
        {
            return levnames_base + arrayIndex;
        }

        /// <summary>The base index for levtext.</summary>
        private const int levtext_base = levnames_base + Hovertank.NUMLEVELS;

        /// <summary>Returns the string index for the specified level text.</summary>
        /// <param name="arrayIndex">The levtext array index.</param>
        /// <returns>An int.</returns>
        public static int levtext(int arrayIndex)
        {
            return levtext_base + arrayIndex;
        }

        #endregion

        /// <summary>The total number of string replacements.</summary>
        public const int Count = 81 + Hovertank.NUMLEVELS + Hovertank.NUMLEVELS;

        /// <summary>Identifiers for externalised game data.</summary>
        public static readonly string[] Identifiers =
        {
            "Intro1",
            "Intro2",
            "Intro3",
            "Intro4",
            "Intro5",
            "Intro6",
            "Intro7",

            "DoHelpText1",
            "DoHelpText2",
            "DoHelpText3",
            "DoHelpText4",
            "DoHelpText5",
            "DoHelpText6",
            "DoHelpText7",
            "DoHelpText8",
            "DoHelpText9",
            "DoHelpText10",
            "DoHelpText11",

            "DebugMemory1",
            "DebugMemory2",
            "DebugMemory3",
            "DebugMemory4",
            "DebugMemory5",
            "DebugMemory6",
            "DebugMemory7",

            "DebugKeys1",
            "DebugKeys2",
            "DebugKeys3",

            "CheckKeys1",
            "CheckKeys2",
            "CheckKeys3",

            "main1",
            "main2",
            "main3",

            "GameOver1",
            "GameOver2",
            "GameOver3",
            "GameOver4",
            "GameOver5",
            "GameOver6",
            "GameOver7",

            "Victory1",
            "Victory2",
            "Victory3",
            "Victory4",
            "Victory5",
            "Victory6",
            "Victory7",
            "Victory8",
            "Victory9",
            "Victory10",
            "Victory11",
            "Victory12",
            "Victory13",

            "BaseScreen1",
            "BaseScreen2",
            "BaseScreen3",
            "BaseScreen4",
            "BaseScreen5",
            "BaseScreen6",
            "BaseScreen7",
            "BaseScreen8",

            "PlayGame1",
            "PlayGame2",
            "PlayGame3",

            "CalibrateJoy1",
            "CalibrateJoy2",
            "CalibrateJoy3",
            "CalibrateJoy4",
            "CalibrateJoy5",
            "CalibrateJoy6",

            "calibratekeys1",
            "calibratekeys2",
            "calibratekeys3",
            "calibratekeys4",
            "calibratekeys5",
            "calibratekeys6",
            "calibratekeys7",
            "calibratekeys8",
            "calibratekeys9",
            "calibratekeys10",

            "levnames0",
            "levnames1",
            "levnames2",
            "levnames3",
            "levnames4",
            "levnames5",
            "levnames6",
            "levnames7",
            "levnames8",
            "levnames9",
            "levnames10",
            "levnames11",
            "levnames12",
            "levnames13",
            "levnames14",
            "levnames15",
            "levnames16",
            "levnames17",
            "levnames18",
            "levnames19",

            "levtext0",
            "levtext1",
            "levtext2",
            "levtext3",
            "levtext4",
            "levtext5",
            "levtext6",
            "levtext7",
            "levtext8",
            "levtext9",
            "levtext10",
            "levtext11",
            "levtext12",
            "levtext13",
            "levtext14",
            "levtext15",
            "levtext16",
            "levtext17",
            "levtext18",
            "levtext19",
        };

    }
}
