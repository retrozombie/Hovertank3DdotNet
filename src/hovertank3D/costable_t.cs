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

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        public class costable_t
        {
            public costable_t(fixed_t[] sintable)
            {
                _sintable = sintable;
            }

            private fixed_t[] _sintable;

            public fixed_t this[int angle]
            {
                get { return _sintable[Hovertank.ANGLES / 4 + angle]; }
            }
        }
    }
}
