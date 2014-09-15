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

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>System implementation for SharpDX.</summary>
    class SharpDXSys : Sys
    {
        /// <summary>Creates a new SharpDXSys.</summary>
        /// <param name="commandLineArguments">The command line arguments.</param>
        public SharpDXSys(string[] commandLineArguments)
            : base(commandLineArguments)
        {
        }

        /// <summary>Gets whether to simulate the intro sound using a sampled sound.</summary>
        public override bool SimulateIntroSound
        {
            get { return true; }
        }

        /// <summary>Logs a message for debugging.</summary>
        /// <param name="message">The message.</param>
        public override void Log(object message)
        {
            System.Diagnostics.Debug.WriteLine(message);
        }

        /// <summary>Gets whether the system can quit.</summary>
        public override bool CanQuit
        {
            get { return true; }
        }

        /// <summary>Exits the application.</summary>
        /// <param name="exitCode">The exit code.</param>
        public override void exit(short exitCode)
        {
            throw new ExitException();
        }

    }
}
