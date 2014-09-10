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
using System.Windows.Forms;
using SlimDX.Windows;

namespace Hovertank3DdotNet
{
    static class Program
    {
        /// <summary>The main entry point for the game.</summary>
        [STAThread]
        static void Main()
        {
            _commandLineArguments = Environment.GetCommandLineArgs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

			using(SlimDX.FormGame gameForm = new SlimDX.FormGame())
			{
				gameForm.InitialiseGame();
				MessagePump.Run(gameForm, gameForm.UpdateGame);
			}
        }

        /// <summary>The command line arguments.</summary>
        private static string[] _commandLineArguments;

        /// <summary>Gets the command line arguments.</summary>
        public static string[] CommandLineArguments
        {
            get { return Program._commandLineArguments; }
        }

        /// <summary>Returns the index of the specified command line argument.</summary>
        /// <param name="argument">The argument to find.</param>
        /// <returns>The index of the argument or -1 if it is not present.</returns>
        public static int IndexOfCommandLineArgument(string argument)
        {
            for(int i = 0; i < _commandLineArguments.Length; i++)
                if(string.Compare(_commandLineArguments[i], argument, StringComparison.OrdinalIgnoreCase) == 0)
                    return i;

            return -1;
        }
       
    }
}
