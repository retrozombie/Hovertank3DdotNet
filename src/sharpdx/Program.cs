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
using SharpDX.Windows;

namespace Hovertank3DdotNet.SharpDX
{
    static class Program
    {
        /// <summary>The main entry point for the application.</summary>
        [STAThread]
        static void Main()
        {
            _commandLineArguments = Environment.GetCommandLineArgs();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            using(FormGame gameForm = new FormGame())
            {
                gameForm.InitialiseGame();
                RenderLoop.Run(gameForm, gameForm.UpdateGame, false);
            }
        }

        /// <summary>The command line arguments.</summary>
        private static string[] _commandLineArguments;

        /// <summary>Gets the command line arguments.</summary>
        public static string[] CommandLineArguments
        {
            get { return Program._commandLineArguments; }
        }
    }
}
