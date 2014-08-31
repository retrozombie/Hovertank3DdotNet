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
    partial class Hovertank
    {
        // as: memory management is handled by the CLR

        private ushort totalmem
        {
            get { return 0; } // total paragraphs available with 64k EMS
        }

        /*
        ===================
        =
        = MMStartup
        =
        = Initializes the memory manager and returns the total
        = allocatable free space.
        =
        = Grabs all space from turbo with farmalloc
        =
        ===================
        */
        private void MMStartup()
        {
        }

        //==========================================================================

        /*
        ====================
        =
        = MMShutdown
        =
        = Frees all conventional, EMS, and XMS allocated
        =
        ====================
        */
        private void MMShutdown()
        {
        }

        //==========================================================================

        /*
        ====================
        =
        = MMGetPtr
        =
        = Allocates an unlocked, unpurgable block
        = Start looking at the top of memory
        =
        ====================
        */
        private void MMGetPtr(ref memptr baseptr, int size)
        {
            baseptr.Buffer = new byte[size];
            baseptr.BaseIndex = 0;
        }

        //==========================================================================

        /*
        =====================
        =
        = MMFreePtr
        =
        = Frees up a block and NULL's the pointer
        =
        =====================
        */
        private void MMFreePtr(ref memptr baseptr)
        {
            baseptr.Buffer = null;
            baseptr.BaseIndex = 0;
        }

        //==========================================================================

        /*
        =====================
        =
        = MMSetPurge
        =
        = Sets the purge level for a block
        =
        =====================
        */
        private void MMSetPurge(ref memptr baseptr, short purge)
        {
        }

        //
        // MMSortMem
        //
        private void MMSortMem()
        {
        }

        //==========================================================================

        /*
        ======================
        =
        = MMUnusedMemory
        =
        = Returns the total free space without purging
        =
        ======================
        */
        private ushort MMUnusedMemory()
        {
            return 0;
        }

        //==========================================================================
        
        /*
        ======================
        =
        = MMTotalFree
        =
        = Returns the total free space with purging
        =
        ======================
        */
        private ushort MMTotalFree()
        {
            return 0;
        }

        //==========================================================================
    }
}
