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
using System.Collections;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        /*
        ;============================================================================
        ;
        ;                 Gamer's Edge Library, ASM section
        ;
        ;============================================================================

        ;=======================================================================
        ;
        ;                     KEYBOARD ROUTINES
        ;
        ;=======================================================================
        */

        public BitArray keydown = new BitArray(128);

        public ushort NBKscan;

        public ushort NBKascii;

        public byte[] scanascii = new byte[]
        {
            0, 27, 49, 50, 51, 52, 53, 54, 55, 56, 57, 48, 45, 61, 8, 9, 113, // 'q'
	        119, 101, 114, 116, 121, 117, 105, 111, 112, 91, 93, 13, 0, 97, 115, // 's'
	        100, 102, 103, 104, 106, 107, 108, 59, 39, 96, 0, 92, 122, 120, 99, // 'c'
	        118, 98, 110, 109, 44, 46, 47, 0, 42, 0, 32, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, // f10
	        0, 0, 1, 1, 1, 45, 1, 1, 1, 43, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // shift-f10
	        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, // ctl-home
	        0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0
        };

        private void StartupKbd()
        {
            // as: Functional Description:
            // Reads previous INT 9 handler using INT 21h ax=3509h and saves to variable oldint9
            // Sets new INT 9 handler to asm function Int9Isr using INT 21h ax=2509h
        }

        /*
        ;========
        ;
        ; ShutdownKbd
        ;
        ;========
        */
        private void ShutdownKbd()
        {
            // as: Functional Description:
            // Restores previously saved INT 9 handler using INT 21h ax=2509h
            // Clears CTRL/ALT/SHIFT flags by ANDing [0040h:0017h] with 1111110011110000b
        }

        /*
        ;========
        ;
        ; NoBiosKey
        ;
        ;========
        */
        private ushort NoBiosKey(ushort parm)
        {
            // as: Functional Description:
            // if(parm == 0)
            // loop:
            //      if((NBKscan & 0x80) == 0)
            //          goto loop // wait for a keypress (set by interrupt handler)
            //      if(NBKascii == 0)
            //          goto loop // wait for a key that has an ascii value
            //      ah = NBKscan // Note: scancode has bit 7 set
            //      al = NBKascii
            //      NBKscan &= 0x7f
            //      return ax
            // else
            //      al = NBKascii
            //      ah = NBKscan & 0x7f
            //      if(al == 0) // Clear the scan code if the key has no ascii code
            //          ah = 0
            //      return ax

            if(parm == 0)
            {
                // as: originally this used to loop here until a new key was received via the
                // keyboard interrupt, modified to return 0 instead of blocking which should be
                // ok because in Hovertank NoBiosKey(1) is only ever called after NoBiosKey(0)
                // returns a non-zero value
                if((NBKscan & 0x80) == 0)
                    return 0;

                if(NBKascii == 0)
                    return 0;

                ushort ax = (ushort) ((NBKscan << 8) | NBKascii);
                NBKscan &= 0x7f;

                return ax;
            }

            if(NBKascii == 0)
                return 0;

            return (ushort) (((NBKscan & 0x7f) << 8) | NBKascii);
        }

        /*
        ;========
        ;
        ; Int9ISR
        ; only called by interrupt $9!
        ;
        ;=========
        */
        // as: Functional Description:
        // Reads the scan code and state
        //      (bit 7: 0 = pressed, 1 = released, bits 6-0 = scan code)
        // Acknowledges XT End of Interrupt
        // if(key_released)
        //      keydown[scancode & 0x7f] = false
        // else
        //      NBKscan = scancode
        //      NBKscan |= 0x80
        //      keydown[scancode] = true
        //      NBKascii = scanascii[scancode]
        // Acknowledges PIC End of Interrupt

        /*
        ;============================================================================
        ;
        ;                           SOUND ROUTINES
        ;
        ;============================================================================
         */

        //private ushort timerspeed; // clock speed for tic counting

        private ushort inttime // fast timer tics since startup
        {
            get { return (ushort) timecount; }
            set
            {
                // as: this property simulates the inttime union with timecount
                timecount &= 0xffff0000;
                timecount |= value;
            }
        }

        private uint timecount;

        // 0 = nosound, 1 = SPKR, 2 = adlib...
        private ushort soundmode
        {
            get { return (_sfxPlayer.Disabled ? (ushort) 0 : (ushort) 1); }
            set
            {
                if(value != 0)
                {
                    // as: modified to switch between sampled sound and 
                    // PC speakers sounds each time the sound is enabled
                    _sfxPlayer.Disabled = false;
                    _sfxPlayer.SpeakerMode = !_sfxPlayer.SpeakerMode;
                }
                else
                {
                    _sfxPlayer.Disabled = true;
                }
            }
        }

        private memptr soundseg;

        /*
        ;========
        ;
        ; StartupSound
        ;
        ; Sets up the new INT 8 ISR and various internal pointers.
        ; Assumes that the calling program has pointer soundseg to something
        ; meaningful...
        ;
        ;========
         */
        private void StartupSound()
        {
            // as: Functional Description:
            // Save the address of the previous timer interrupt handler (restored by ShutdownSound)
            // Install a new timer interrupt handler
            // Set the timer speed
            //   Mode(0x43) = 0x36
            //   Ch0Data(0x40) = 0x47
            //   Ch0Data(0x40) = 0x21
            // http://wiki.osdev.org/Programmable_Interval_Timer
            // 65536 = 18.2065Hz, 18.2065 * (65536 / 0x2147) = 140.06 Interrupts / Second
            // UpdateSPKR is called 140 times per second (once every 7.14ms)
        }

        /*
        ;========
        ;
        ; ShutdownSound
        ;
        ;========
        */
        private void ShutdownSound()
        {
            // as: Functional Description:
            // Resets timer
            // Restores previous Int 8h handler
            // Switches speaker output off
        }

        /// <summary>Plays a sound.</summary>
        /// <param name="playnum">The index of the sound to play.</param>
        private void PlaySound(ushort playnum)
        {
            OldPlaySound(_sfxPlayer.SoundLinks[playnum]);
        }

        /*
        ;===========
        ;
        ; PlaySoundSPK (soundnum)
        ;
        ; If the sound's priority is >= the current priority, SoundPtr, SndPriority,
        ; and the timer speed are changed
        ;
        ; Hacked for sound blaster support!
        ;
        ;===========
        */
        private void OldPlaySound(ushort playnum)
        {
            // as: Functional Description:
            // If sound was enabled this would begin playback of either
            // a PC speaker sequence or a sound sample
            // For PC speaker:
            //   the offset to the sound info (spksndtype) was calculated
            //   then the priority of the new sound to a playing sound was checked
            //   lower priority sounds aren't played
            // For sound samples:
            //   jmPlaySample was called (passing in playnum)

            // as: SndPriority isn't set to zero anymore when a sound finishes, 
            // the following mod should hopefully emulate the original system
            byte priority = soundseg.GetUInt8(playnum * spksndtype.SizeOf + spksndtype.snd_priority);
            if(priority < SndPriority)
            {
                if(_sfxPlayer.IsSoundPlaying)
                    return; // Higher priority sound was still playing
            }

            _sfxPlayer.PlaySound(playnum);
            SndPriority = priority;
        }

        /// <summary>The priority of the sound being played.</summary>
        private byte SndPriority;

        /*
        ;=========================================================================

        ;========
        ;
        ; UpdateSPKR
        ; only called by interrupt $8!
        ;
        ;=========
        */
        public void UpdateSPKR()
        {
            // as: Functional Description:
            // timecount is incremented
            // a user routine was called if set (appears to be unused by Hovertank)
            // If enabled the PC Speaker was updated:
            //   The next timer value was read and used to program channel 2 (speaker timer)
            //   A timer value of zero stops the speaker
            //   A timer value of -1 ends the sound sequence

            timecount++;
        }

        /// <summary>Records a vbl occurred.</summary>
        /// <remarks>as: Added to simulate VBL.</remarks>
        private bool verticalBlank;

        /// <summary>Simulates a VBL.</summary>
        /// <remarks>as: Added to simulate VBL.</remarks>
        public void SetVBL()
        {
            verticalBlank = true;
        }

        /*
        ;============================================================================
        ;
        ;                           RANDOM ROUTINES
        ;
        ;============================================================================
        */
        private ushort rndindex;

        // as: hmmm, deja vu
        private byte[] rndtable = new byte[]
        {
            0,   8, 109, 220, 222, 241, 149, 107,  75, 248, 254, 140,  16,  66,
            74,  21, 211,  47,  80, 242, 154,  27, 205, 128, 161,  89,  77,  36,
            95, 110,  85,  48, 212, 140, 211, 249,  22,  79, 200,  50,  28, 188,
            52, 140, 202, 120,  68, 145,  62,  70, 184, 190,  91, 197, 152, 224,
            149, 104,  25, 178, 252, 182, 202, 182, 141, 197,   4,  81, 181, 242,
            145,  42,  39, 227, 156, 198, 225, 193, 219,  93, 122, 175, 249,   0,
            175, 143,  70, 239,  46, 246, 163,  53, 163, 109, 168, 135,   2, 235,
            25,  92,  20, 145, 138,  77,  69, 166,  78, 176, 173, 212, 166, 113,
            94, 161,  41,  50, 239,  49, 111, 164,  70,  60,   2,  37, 171,  75,
            136, 156,  11,  56,  42, 146, 138, 229,  73, 146,  77,  61,  98, 196,
            135, 106,  63, 197, 195,  86,  96, 203, 113, 101, 170, 247, 181, 113,
            80, 250, 108,   7, 255, 237, 129, 226,  79, 107, 112, 166, 103, 241,
            24, 223, 239, 120, 198,  58,  60,  82, 128,   3, 184,  66, 143, 224,
            145, 224,  81, 206, 163,  45,  63,  90, 168, 114,  59,  33, 159,  95,
            28, 139, 123,  98, 125, 196,  15,  70, 194, 253,  54,  14, 109, 226,
            71,  17, 161,  93, 186,  87, 244, 138,  20,  52, 123, 251,  26,  36,
            17,  46,  52, 231, 232,  76,  31, 221,  84,  37, 216, 165, 212, 106,
            197, 242,  98,  43,  39, 175, 254, 145, 190,  84, 118, 222, 187, 136,
            120, 163, 236, 249
        };

        /*
        ;
        ; Random # Generator vars
        ;
        */
        private ushort indexi; // Rnd#Generator

        private ushort indexj;

        private ushort LastRnd;

        private ushort[] RndArray = new ushort[17];

        private ushort[] baseRndArray = new ushort[]
        {
            1, 1, 2, 3, 5, 8, 13, 21, 54, 75, 129, 204, 323, 527, 850, 1377, 2227
        };

        /*
        ;=================================================
        ;
        ; InitRnd (boolean randomize)
        ; if randomize is false, the counter is set to 0
        ;
        ;=================================================
        */
        private void InitRnd(bool randomize)
        {
            // as: converted from asm
            Array.Copy(baseRndArray, RndArray, RndArray.Length);

            LastRnd = 0;
            indexi = 17 * 2;
            indexj = 5 * 2;

            if(randomize)
            {
                DateTime time = DateTime.Now;

                // INT21, AH = 0x2C GetSystemTime returns CH = hour CL = minute DH = second DL = 1/100 seconds
                ushort cx = (ushort) ((time.Hour << 8) | time.Minute);
                ushort dx = (ushort) ((time.Second << 8) | (time.Millisecond / 10));

                RndArray[16] = dx;
                dx ^= cx;
                RndArray[4] = dx;
            }

            Rnd(0xffff);
        }

        /*
        ;=================================================
        ;
        ; unsigned Random (unsigned maxval)
        ; Return a random # between 0-?
        ;
        ;=================================================
        */
        private ushort Rnd(ushort maxval)
        {
            // as: converted from asm
            ushort ax = maxval;

            ushort dx = 0xffff; // full-mask
            
            while((ax & 0x8000) == 0)
            {
                ax <<= 1;
                dx >>= 1;
            }

            ushort bx = indexi; // this routine was converted from
            ushort si = indexj; // the Random macro on Merlin GS
            ax = RndArray[-1 + bx / 2];
            ax += RndArray[-1 + si / 2];
            ax++; // ADC CF = 1
            RndArray[-1 + bx / 2] = ax;
            
            ax += LastRnd;
            LastRnd = ax;
            
            bx -= 2;

            if(bx == 0)
                bx = 17 * 2;

            si -= 2;
            if(si == 0)
                si = 17 * 2;

            indexi = bx;
            indexj = si;

            ax &= dx; // AND our mask!

            if(ax > maxval) // SUBTRACT to be within range
                ax >>= 1;

            return ax;
        }
        
        /*
        ;=================================================
        ;
        ; InitRndT (boolean randomize)
        ; Init table based RND generator
        ; if randomize is false, the counter is set to 0
        ;
        ;=================================================
        */
        private void InitRndT(bool randomize)
        {
            // as: converted from asm
            ushort dx;
            if(!randomize)
            {
                dx = 0; // set to a definate value
            }
            else
            {   // if randomize is true, really random
                DateTime time = DateTime.Now;

                // INT21, AH = 0x2C GetSystemTime returns CH = hour CL = minute DH = second DL = 1/100 seconds
                dx = (ushort) ((time.Second << 8) | (time.Millisecond / 10));
                dx &= 0xff;
            }

            rndindex = dx;
        }

        /*
        ;=================================================
        ;
        ; int RandomT (void)
        ; Return a random # between 0-255
        ; Exit : AX = value
        ;
        ;=================================================
        */
        private ushort RndT()
        {
            // as: converted from asm
            rndindex = (ushort) ((rndindex + 1) & 0xff);
            return rndtable[rndindex];
        }

        /*
        ;============================================================================
        ;
        ;                           MISC VIDEO ROUTINES
        ;
        ;============================================================================

        ;========
        ;
        ; WaitVBL (int number)
        ;
        ;========
        */
        private void WaitVBL(ushort number)
        {
            // as: Functional Description:
            // This function used to loop the specified number of times
            // waiting for VBL to finish if active then waiting for
            // a VBL to start

#if NOT_USING_STATES
            for(ushort i = 0; i < number; i++)
            {
                // Wait for vsync
            }
#endif
        }

        /*
        ;ÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ
        ;
        ; Name:	VideoID
        ;
        ; Function:	Detects the presence of various video subsystems
        ;
        ; int VideoID;
        ;
        ; Subsystem ID values:
        ; 	 0  = (none)
        ; 	 1  = MDA
        ; 	 2  = CGA
        ; 	 3  = EGA
        ; 	 4  = MCGA
        ; 	 5  = VGA
        ; 	80h = HGC
        ; 	81h = HGC+
        ; 	82h = Hercules InColor
        ;
        ;ÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍÍ
        */
        private cardtype VideoID()
        {
            // as: Functional Description:
            // This used to determine the type of video card
            return cardtype.EGAcard;
        }

    }
}
