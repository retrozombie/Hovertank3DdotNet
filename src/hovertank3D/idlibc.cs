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
using System.Text;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        /*
        =============================================================================

		          Library, C section

        =============================================================================
        */

        private const short BLANKCHAR = 9;

        private sbyte ch; // scratch space

        private inputtype[] playermode = new inputtype[3];

        private short[] JoyXlow = new short[3];

        private short[] JoyXhigh = new short[3];

        private short[] JoyYlow = new short[3];

        private short[] JoyYhigh = new short[3];

        private bool buttonflip;

        private sbyte[] key = new sbyte[8];

        private sbyte keyB1;

        private sbyte keyB2;

        //=========================================================================

        ////////////////
        //
        // CalibrateJoy
        // Brings up a dialog and has the user calibrate
        // either joystick1 or joystick2
        //
        ////////////////
        private void CalibrateJoy(short joynum)
        {
            ExpWin(34, 11);

            fontcolor = 13;
            CPPrint(_strings[Strings.CalibrateJoy1]); // as: string replacements

            py += 6;
            fontcolor = 15;
            PPrint(_strings[Strings.CalibrateJoy2]); // as: string replacements
            PPrint(_strings[Strings.CalibrateJoy3]); // as: string replacements

            short stage = 15;
            sx = (short) ((px + 7) / 8);
            ControlStruct ctr;
            short xl, yl, xh, yh;
            do // wait for a button press
            {
                DrawChar((ushort) sx, py, stage);

                WaitVBL(3);

                if(++stage == 23)
                    stage = 15;

                ReadJoystick(joynum, out xl, out yl);

                ctr = ControlJoystick(joynum);

                if(keydown[1])
                    return;

            } while(!ctr.button1 && !ctr.button2);

            DrawChar((ushort) sx, py, BLANKCHAR);
            do // wait for the button release
            {
                ctr = ControlJoystick(joynum);
            } while(ctr.button1);

            WaitVBL(4); // so the button can't bounce

            py += 6;
            PPrint(_strings[Strings.CalibrateJoy4]); // as: string replacements
            PPrint(_strings[Strings.CalibrateJoy5]); // as: string replacements

            do // wait for a button press
            {
                DrawChar((ushort) sx, py, stage);
                WaitVBL(3);

                if(++stage == 23)
                    stage = 15;

                ReadJoystick(joynum, out xh, out yh);

                ctr = ControlJoystick(joynum);

                if(keydown[1])
                    return;

            } while(!ctr.button1 && !ctr.button2);

            DrawChar((ushort) sx, py, BLANKCHAR);
            do // wait for the button release
            {
                ctr = ControlJoystick(joynum);

            } while(ctr.button1);

            //
            // figure out good boundaries
            //
            short dx = (short) ((xh - xl) / 6);
            short dy = (short) ((yh - yl) / 6);
            JoyXlow[joynum] = (short) (xl + dx);
            JoyXhigh[joynum] = (short) (xh - dx);
            JoyYlow[joynum] = (short) (yl + dy);
            JoyYhigh[joynum] = (short) (yh - dy);

            if(joynum == 1)
                playermode[1] = inputtype.joystick1;
            else
                playermode[1] = inputtype.joystick2;

            py += 6;
            PPrint(_strings[Strings.CalibrateJoy6]); // as: string replacements

            ch = (sbyte) PGet();
            if(ch == 'A' || ch == 'a')
                buttonflip = true;
            else
                buttonflip = false;
        }

        // as: left as char for convenience
        private static char[] chartable = new char[]
        {
            '?','?','1','2','3','4','5','6','7','8','9','0','-','+','?','?',
            'Q','W','E','R','T','Y','U','I','O','P','[',']','|','?','A','S',
            'D','F','G','H','J','K','L',';','"','?','?','?','Z','X','C','V',
            'B','N','M',',','.','/','?','?','?','?','?','?','?','?','?','?',
            '?','?','?','?','?','?','?','?', (char) 15,'?','-', (char) 21,'5', (char) 17,'+','?',
            (char) 19,'?','?','?','?','?','?','?','?','?','?','?','?','?','?','?',
            '?','?','?','?','?','?','?','?','?','?','?','?','?','?','?','?',
            '?','?','?','?','?','?','?','?','?','?','?','?','?','?','?','?'
        };

        /////////////////////////////
        //
        // print a representation of the scan code key
        //
        ////////////////////////////
        private void printscan(short sc)
        {
            sc &= 0x7f;

            if(sc == 1)
            {
                PPrint("ESC");
            }
            else if(sc == 0xe)
            {
                PPrint("BKSP");
            }
            else if(sc == 0xf)
            {
                PPrint("TAB");
            }
            else if(sc == 0x1d)
            {
                PPrint("CTRL");
            }
            else if(sc == 0x2A)
            {
                PPrint("LSHIFT");
            }
            else if(sc == 0x39)
            {
                PPrint("SPACE");
            }
            else if(sc == 0x3A)
            {
                PPrint("CAPSLK");
            }
            else if(sc >= 0x3b && sc <= 0x44)
            {
                PPrint("F");
                PPrint((sc - 0x3a).ToString());
            }
            else if(sc == 0x57)
            {
                PPrint("F11");
            }
            else if(sc == 0x59)
            {
                PPrint("F12");
            }
            else if(sc == 0x46)
            {
                PPrint("SCRLLK");
            }
            else if(sc == 0x1c)
            {
                PPrint("ENTER");
            }
            else if(sc == 0x36)
            {
                PPrint("RSHIFT");
            }
            else if(sc == 0x37)
            {
                PPrint("PRTSC");
            }
            else if(sc == 0x38)
            {
                PPrint("ALT");
            }
            else if(sc == 0x47)
            {
                PPrint("HOME");
            }
            else if(sc == 0x49)
            {
                PPrint("PGUP");
            }
            else if(sc == 0x4f)
            {
                PPrint("END");
            }
            else if(sc == 0x51)
            {
                PPrint("PGDN");
            }
            else if(sc == 0x52)
            {
                PPrint("INS");
            }
            else if(sc == 0x53)
            {
                PPrint("DEL");
            }
            else if(sc == 0x45)
            {
                PPrint("NUMLK");
            }
            else if(sc == 0x48)
            {
                PPrint("UP");
            }
            else if(sc == 0x50)
            {
                PPrint("DOWN");
            }
            else if(sc == 0x4b)
            {
                PPrint("LEFT");
            }
            else if(sc == 0x4d)
            {
                PPrint("RIGHT");
            }
            else
            {
                PPrint(chartable[sc].ToString());
            }
        }

        /////////////////////////////
        //
        // calibratekeys
        //
        ////////////////////////////
        private void calibratekeys()
        {
            ExpWin(22, 12);

            fontcolor = 13;
            CPPrint(_strings[Strings.calibratekeys1]); // as: string replacements

            fontcolor = 15;
            PPrint(_strings[Strings.calibratekeys2]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys3]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys4]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys5]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys6]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys7]); // as: string replacements
            PPrint(_strings[Strings.calibratekeys8]); // as: string replacements

            string separator = _strings[Strings.calibratekeys9]; // as: string replacements

            short hx = (short) ((px + 7) / 8);
            short hy = (short) py;
            for(short i = 0; i < 4; i++)
            {
                px = (ushort) (pxl + 8 * 12);
                py = (ushort) (pyl + 10 * (1 + i));
                PPrint(separator); // as: string replacements);
                printscan(key[i * 2]);
            }

            px = (ushort) (pxl + 8 * 12);
            py = (ushort) (pyl + 10 * 5);
            PPrint(separator); // as: string replacements);
            printscan(keyB1);

            px = (ushort) (pxl + 8 * 12);
            py = (ushort) (pyl + 10 * 6);
            PPrint(separator); // as: string replacements);
            printscan(keyB2);

            do
            {
                px = (ushort) (hx * 8);
                py = (ushort) hy;
                DrawChar((ushort) hx, (ushort) hy, BLANKCHAR);

                ch = (sbyte) (PGet() % 256);
                if(ch < '1' || ch > '6')
                    continue;

                short select = (short) (ch - '1');
                DrawPchar((ushort) ch);

                PPrint(_strings[Strings.calibratekeys10]); // as: string replacements

                ClearKeys();

                short _new = -1;

                while(!keydown[++_new])
                {
                    if(_new == 0x79)
                        _new = -1;
                    else if(_new == 0x29)
                        _new++; // skip STUPID left shifts!
                }

                Bar((ushort) leftedge, py, 22, 10, 0xff);
                
                if(select < 4)
                    key[select * 2] = (sbyte) _new;
                
                if(select == 4)
                    keyB1 = (sbyte) _new;
                
                if(select == 5)
                    keyB2 = (sbyte) _new;

                px = (ushort) (pxl + 8 * 12);
                py = (ushort) (pyl + (select + 1) * 10);
                Bar((ushort) (px / 8), py, 9, 10, 0xff);

                PPrint(separator); // as: string replacements);
                printscan(_new);

                ClearKeys();

                ch = (sbyte) '0'; // so the loop continues

            } while(ch >= '0' && ch <= '9');

            playermode[1] = inputtype.keyboard;
        }

        //=========================================================================

        /*
        ===========================
        =
        = ControlKBD
        =
        ===========================
        */
        private ControlStruct ControlKBD()
        {
            short xmove = 0;
            short ymove = 0;

            if(keydown[key[(short) dirtype.north]])
                ymove = -1;

            if(keydown[key[(short) dirtype.east]])
                xmove = 1;

            if(keydown[key[(short) dirtype.south]])
                ymove = 1;

            if(keydown[key[(short) dirtype.west]])
                xmove = -1;

            if(keydown[key[(short) dirtype.northeast]])
            {
                ymove = -1;
                xmove = 1;
            }

            if(keydown[key[(short) dirtype.northwest]])
            {
                ymove = -1;
                xmove = -1;
            }

            if(keydown[key[(short) dirtype.southeast]])
            {
                ymove = 1;
                xmove = 1;
            }

            if(keydown[key[(short) dirtype.southwest]])
            {
                ymove = 1;
                xmove = -1;
            }

            ControlStruct action = new ControlStruct();

            switch(ymove * 3 + xmove)
            {
                case -4: action.dir = dirtype.northwest; break;
                case -3: action.dir = dirtype.north; break;
                case -2: action.dir = dirtype.northeast; break;
                case -1: action.dir = dirtype.west; break;
                case 0: action.dir = dirtype.nodir; break;
                case 1: action.dir = dirtype.east; break;
                case 2: action.dir = dirtype.southwest; break;
                case 3: action.dir = dirtype.south; break;
                case 4: action.dir = dirtype.southeast; break;
            }

            action.button1 = keydown[keyB1];
            action.button2 = keydown[keyB2];

            return action;
        }

        /*
        ===============================
        =
        = ReadJoystick
        = Just return the resistance count of the joystick
        =
        ===============================
        */
        private void ReadJoystick(short joynum, out short xcount, out short ycount)
        {
            _sys.ReadJoystick(joynum, out xcount, out ycount);
        }

        /*
        =============================
        =
        = ControlJoystick (joy# = 1 / 2)
        =
        =============================
        */
        private ControlStruct ControlJoystick(short joynum)
        {
            short joyx = 0; /* resistance in joystick */
            short joyy = 0;

            ReadJoystick(joynum, out joyx, out joyy);

            if((joyx > 500) | (joyy > 500))
            {
                joyx = (short) (JoyXlow[joynum] + 1); /* no joystick connected, do nothing */
                joyy = (short) (JoyYlow[joynum] + 1);
            }

            short xmove = 0;
            if(joyx > JoyXhigh[joynum])
                xmove = 1;
            else if(joyx < JoyXlow[joynum])
                xmove = -1;

            short ymove = 0;
            if(joyy > JoyYhigh[joynum])
                ymove = 1;
            else if(joyy < JoyYlow[joynum])
                ymove = -1;

            ControlStruct action = new ControlStruct();
            switch(ymove * 3 + xmove)
            {
                case -4: action.dir = dirtype.northwest; break;
                case -3: action.dir = dirtype.north; break;
                case -2: action.dir = dirtype.northeast; break;
                case -1: action.dir = dirtype.west; break;
                case 0: action.dir = dirtype.nodir; break;
                case 1: action.dir = dirtype.east; break;
                case 2: action.dir = dirtype.southwest; break;
                case 3: action.dir = dirtype.south; break;
                case 4: action.dir = dirtype.southeast; break;
            }

            short buttons = _sys.ReadJoystickButtons(joynum); /* Get all four button status */

            if(joynum == 1)
            {
                action.button1 = ((buttons & 0x10) == 0);
                action.button2 = ((buttons & 0x20) == 0);
            }
            else
            {
                action.button1 = ((buttons & 0x40) == 0);
                action.button2 = ((buttons & 0x80) == 0);
            }

            if(buttonflip)
            {
                bool buttonState = action.button1;
                action.button1 = action.button2;
                action.button2 = buttonState;
            }

            return action;
        }

        /*
        =============================
        =
        = ControlPlayer
        =
        = Expects a 1 or a 2
        =
        =============================
        */
        private ControlStruct ControlPlayer(short player)
        {
            switch(playermode[player])
            {
                case inputtype.keyboard:
                    return ControlKBD();

                case inputtype.joystick1:
                    return ControlJoystick(1);

                case inputtype.joystick2:
                    return ControlJoystick(2);
            }

            return ControlKBD();
        }

        /*
        =============================================================================
        **
        ** Miscellaneous library routines
        **
        =============================================================================
        */

        ///////////////////////////////
        //
        // ClearKeys
        // Clears out the bios buffer and zeros out the keydown array
        //
        ///////////////////////////////
        private void ClearKeys()
        {
            NBKascii = 0;
            NBKscan = 0;
            keydown.SetAll(false);
        }

        /*
        ===============
        =
        = Ack
        =
        = Waits for a keypress or putton press
        =
        ===============
        */
        private void Ack()
        {
            ClearKeys();
            while(true)
            {
                if(NBKscan > 127)
                {
                    NBKscan &= 0x7f;
                    return;
                }

                ControlStruct c = ControlPlayer(1);

                if(c.button1 || c.button2)
                    return;
            }
        }

        //==========================================================================

        /////////////////////////////////////////////////////////
        //
        // Load a LARGE file into a FAR buffer!
        //
        /////////////////////////////////////////////////////////
        private uint LoadFile(string filename, memptr buffer)
        {
            return _sys.FileReadAllBytes(filename, buffer.Buffer);
        }

        //===========================================================================

        /*
        ====================================
        =
        = BloadinMM
        =
        ====================================
        */

        private void BloadinMM(string filename, ref memptr spot)
        {
            if(_sys.FileExists(filename))
            {
                int length = _sys.FileLength(filename);
                MMGetPtr(ref spot, length);
                LoadFile(filename, spot);
            }
            else
            {
                Quit("BloadinMM: Can't find file " + filename);
            }
        }

        /*
        ====================
        =
        = StopDrive
        =
        = Stop a floppy drive after sounds have been started
        =
        ====================
        */
        private void StopDrive()
        {
        }

        /*
        ============================================================================

	            COMPRESSION routines, see JHUFF.C for more

        ============================================================================
        */

        /*
        ===============
        =
        = OptimizeNodes
        =
        = Goes through a huffman table and changes the 256-511 node numbers to the
        = actular address of the node.  Must be called before HuffExpand
        =
        ===============
        */
        private void OptimizeNodes(huffnode table)
        {
            // as: Leave nodes as they are, HuffExpand has been modified to handle this
        }

        /*
        ======================
        =
        = HuffExpand
        =
        ======================
        */
        public static void HuffExpand(byte[] sourceBuffer, int sourceIndex, byte[] destBuffer, int destIndex, int length, huffnode hufftable)
        {
            memptr source = new memptr(sourceBuffer, sourceIndex);
            memptr dest = new memptr(destBuffer, destIndex);

            ushort bit, _byte, code;
            huffnode nodeon, headptr;

            headptr = new huffnode(hufftable, 254); // head node is allways node 254

            // as: The disabled C code that was in this function appears to be the C version of the asm code
            // this came in handy during the conversion

            nodeon = new huffnode(headptr);

            // as: bugfix - refactored to prevent the out of bounds read that can occur occasionally with the final byte
            bit = 256;
            _byte = 0;
            while(length != 0)
            {
                if(bit == 256)
                {
                    bit = 1;
                    _byte = source.GetUInt8(0);
                    source.Offset(1);
                }

                if((_byte & bit) != 0)
                    code = nodeon.bit1;
                else
                    code = nodeon.bit0;

                bit <<= 1;

                if(code < 256)
                {
                    dest.SetUInt8(0, (byte) code);
                    dest.Offset(1);
                    nodeon = headptr;
                    length--;
                }
                else
                {
                    nodeon = new huffnode(hufftable, code - 256);
                }
            }
        }

        public static void HuffExpand(memptr source, memptr dest, int length, huffnode hufftable)
        {
            HuffExpand(source.Buffer, source.BaseIndex, dest.Buffer, dest.BaseIndex, length, hufftable);
        }

        /*========================================================================*/

        /*
        ======================
        =
        = RLEWexpand
        =
        ======================
        */
        public const ushort RLETAG = 0xFEFE;

        public static void RLEWExpand(memptr source, memptr dest)
        {
            int length = source.GetInt32(0);

            memptr end = new memptr(dest, length);

            source.Offset(4); // skip length words

            //
            // expand it
            //
            do
            {
                ushort value = source.GetUInt16(0);
                source.Offset(2);

                if(value != RLETAG)
                {
                    //
                    // uncompressed
                    //
                    dest.SetUInt16(0, value);
                    dest.Offset(2);
                }
                else
                {
                    //
                    // compressed string
                    //
                    ushort count = source.GetUInt16(0);
                    source.Offset(2);

                    value = source.GetUInt16(0);
                    source.Offset(2);

                    if(dest.BaseIndex + count * 2 > end.BaseIndex)
                        throw new Exception("RLEWExpand error!");

                    for(ushort i = 1; i <= count; i++)
                    {
                        dest.SetUInt16(0, value);
                        dest.Offset(2);
                    }
                }
            } while(dest.BaseIndex < end.BaseIndex);
        }
        
        /*
        ============================================================================

			          GRAPHIC ROUTINES

        ============================================================================
        */

        /*
        ** Graphic routines
        */

        private cardtype videocard;

        private grtype grmode;

        // as: Set to zero to remove compiler warning, used when setting the palette
        // but never written, ColorBorder also sets the border color directly
        private short bordercolor = 0;

        /*
        ========================
        =
        = GenYlookup
        =
        = Builds ylookup based on linewidth
        =
        ========================
        */
        private void GenYlookup()
        {
            for(short i = 0; i < 256; i++)
                ylookup[i] = (ushort) (i * linewidth);
        }

        /// <summary>The current screen mode.</summary>
        public grtype screenMode;

        /*
        ========================
        =
        = SetScreenMode
        = Call BIOS to set TEXT / CGAgr / EGAgr / VGAgr
        =
        ========================
        */
        private void SetScreenMode(grtype mode)
        {
            // text = 80 x 25 x 16 color (640x200x16 CGA, 640x350x16/64 EGA) @B000?
            // cga = 320 x 200 x 4 color graphics @B800
            // ega = 320 x 200 x 16 color graphics @A000 <-- Hovertank forces this mode

            screenMode = grmode;
        }

        /*
        ========================
        =
        = egasplitscreen
        =
        ========================
        */
        private void EGASplitScreen(short linenum)
        {
            WaitVBL(1);

            _display.SplitScreenLines = linenum;
        }

        /*
        ========================
        =
        = EGAVirtualScreen
        =
        ========================
        */
        private void EGAVirtualScreen(short width) // sets screen width
        {
            // as: linewidth sets stride now
            _display.Stride = width * Display.ColumnScale;

            WaitVBL(1);
        }

        /*
        ========================
        =
        = ColorBorder
        =
        ========================
        */
        private void ColorBorder(short color)
        {
            // Int 10h, AX=0x1001, BH = color
            _display.SetColorEGA(Display.BorderColorIndex, (byte) color);
        }

        ////////////////////////////////////////////////////////////////////
        //
        // Fade EGA screen in
        //
        ////////////////////////////////////////////////////////////////////

        private byte[][] colors = new byte[][]
        {
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, 
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 7, 0 }, 
            new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0 }, 
            new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x18, 0x19, 0x1a, 0x1b, 0x1c, 0x1d, 0x1e, 0x1f, 0 }, 
            new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0 }, 
            new byte[] { 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f, 0x1f }
        };

        private void SetPalette(short i)
        {
            // Int 10h, AX=1002h - Set All Palette Registers
            // ES:DX = Palette register list (16 colors + border color)
            byte[] palette = colors[i];

            Display display = Display;
            for(int n = 0; n < 17; n++)
                display.SetColorEGA(n, palette[n]);
        }

        private void SetDefaultColors()
        {
            colors[3][16] = (byte) bordercolor;

            SetPalette(3);
        }

        private void FadeIn()
        {
            for(short i = 0; i < 4; i++)
            {
                colors[i][16] = (byte) bordercolor;

                SetPalette(i);

                WaitVBL(6);
            }
        }

        private void FadeUp()
        {
            for(short i = 3; i < 6; i++)
            {
                colors[i][16] = (byte) bordercolor;

                SetPalette(i);

                WaitVBL(6);
            }
        }

        private void FadeDown()
        {
            for(short i = 5; i > 2; i--)
            {
                colors[i][16] = (byte) bordercolor;

                SetPalette(i);

                WaitVBL(6);
            }
        }

        private void SetNormalPalette()
        {
            colors[3][16] = (byte) bordercolor;

            SetPalette(3);
        }

        ////////////////////////////////////////////////////////////////////
        //
        // Fade EGA screen out
        //
        ////////////////////////////////////////////////////////////////////
        private void FadeOut()
        {
            for(short i = 3; i >= 0; i--)
            {
                colors[i][16] = (byte) bordercolor;

                SetPalette(i);

                WaitVBL(6);
            }
        }

        /*
        ====================
        =
        = SetLineWidth
        =
        ====================
        */
        private void SetLineWidth(short width)
        {
            EGAVirtualScreen(width);
            linewidth = (ushort) width;
            GenYlookup();
        }
        
        /*
        ============================================================================

			              IGRAB STUFF

        ============================================================================
        */

        private memptr[] grsegs = new memptr[NUMCHUNKS];

        private sbyte[] needgr = new sbyte[NUMCHUNKS]; // for caching

        private pictype[] pictable = new pictype[NUMPICS];

        /*
        ============================================================================

			        MID LEVEL GRAPHIC ROUTINES

        ============================================================================
        */
        private short win_xl;

        private short win_yl;

        private short win_xh;

        private short win_yh;

        private short sx;

        private short sy;

        private short leftedge;

        private short screencenterx = 20;

        private short screencentery = 11;

        private void DRAWCHAR(short x, short y, short n)
        {
            DrawChar((ushort) x, (ushort) (y * 8), n);
        }

        //////////////////////////
        //
        // DrawWindow
        // draws a bordered window and homes the cursor
        //
        //////////////////////////
        private void DrawWindow(short xl, short yl, short xh, short yh)
        {
            win_xl = xl;
            pxl = (ushort) (xl * 8 + 8);
            win_yl = yl;
            win_xh = xh;
            pxh = (ushort) (xh * 8);
            win_yh = yh; // so the window can be erased

            DRAWCHAR(xl, yl, 1);

            for(short x = (short) (xl + 1); x < xh; x++)
                DRAWCHAR(x, yl, 2);

            DRAWCHAR(xh, yl, 3);

            for(short y = (short) (yl + 1); y < yh; y++)
            {
                DRAWCHAR(xl, y, 4);

                for(short x = (short) (xl + 1); x < xh; x++)
                    DRAWCHAR(x, y, 9);

                DRAWCHAR(xh, y, 5);
            }

            DRAWCHAR(xl, yh, 6);

            for(short x = (short) (xl + 1); x < xh; x++)
                DRAWCHAR(x, yh, 7);

            DRAWCHAR(xh, yh, 8);

            leftedge = (short) (xl + 1);
            sx = leftedge;
            sy = (short) (yl + 1);
            px = (ushort) (sx * 8);
            pyl = (ushort) (sy * 8);
            py = pyl;
        }

        private void EraseWindow()
        {
            for(short y = (short) (win_yl + 1); y < win_yh; y++)
                for(short x = (short) (win_xl + 1); x < win_xh; x++)
                    DRAWCHAR(x, y, 9);

            leftedge = (short) (win_xl + 1);
            sx = leftedge;
            sy = (short) (win_yl + 1);
            px = (ushort) (sx * 8);
            pyl = (ushort) (sy * 8);
            py = pyl;
        }

        /////////////////////////////
        //
        // CenterWindow
        // Centers a DrawWindow of the given size
        //
        /////////////////////////////
        private void CenterWindow(short width, short height)
        {
            short xl = (short) (screencenterx - width / 2);
            short yl = (short) (screencentery - height / 2);
            DrawWindow(xl, yl, (short) (xl + width + 1), (short) (yl + height + 1));
        }

        ///////////////////////////////
        //
        // ExpWin {h / v}
        // Grows the window outward
        //
        ///////////////////////////////
        private void ExpWin(short width, short height)
        {
            if(width > 2)
            {
                if(height > 2)
                    ExpWin((short) (width - 2), (short) (height - 2));
                else
                    ExpWinH((short) (width - 2), height);
            }
            else
            {
                if(height > 2)
                    ExpWinV(width, (short) (height - 2));
            }

            WaitVBL(1);

            CenterWindow(width, height);
        }

        private void ExpWinH(short width, short height)
        {
            if(width > 2)
                ExpWinH((short) (width - 2), height);

            WaitVBL(1);

            CenterWindow(width, height);
        }

        private void ExpWinV(short width, short height)
        {
            if(height > 2)
                ExpWinV(width, (short) (height - 2));

            WaitVBL(1);

            CenterWindow(width, height);
        }

        /*
        ===========================================================================

		         CHARACTER BASED PRINTING ROUTINES

        ===========================================================================
        */
        
        /////////////////////////
        //
        // Print
        // Prints a string at sx,sy.  No clipping!!!
        //
        /////////////////////////
        private void Print(string str)
        {
            for(int i = 0; i < str.Length; i++)
            {
                char ch = str[i];

                if(ch == '\n')
                {
                    sy++;
                    sx = leftedge;
                }
                else if(ch == '\r')
                {
                    sx = leftedge;
                }
                else
                {
                    DRAWCHAR(sx++, sy, (short) ch);
                }
            }
        }

        ////////////////////////////////////////////////////////////////////
        //
        // Input unsigned
        //
        ////////////////////////////////////////////////////////////////////
        private ushort InputInt()
        {
            StringBuilder _string = new StringBuilder();

            Input(_string, 2);  // as: the return value isn't checked
                                // i.e. an empty string could be entered

            // as: Added check for empty string otherwise exception
            if(_string.Length == 0)
                return 0;

            // as: The following don't work because the shift key
            // doesn't transform scancodes
            ushort value, loop, loop1;
            if(_string[0] == '$')
            {   // Hex conversion
                short digits = (short) (_string.Length - 2);
                if(digits < 0)
                    return 0;

                string hexstr = "0123456789ABCDEF";
                for(value = 0, loop1 = 0; loop1 <= digits; loop1++)
                {
                    sbyte digit = _sys.toupper((sbyte) _string[loop1 + 1]);
                    for(loop = 0; loop < 16; loop++)
                    {
                        if(digit == hexstr[loop])
                        {
                            value |= (ushort) (loop << (digits - loop1) * 4);
                            break;
                        }
                    }
                }
            }
            else if(_string[0] == '%')
            {   // Binary conversion - enjoy warping to map 0 or 1 :)
                short digits = (short) (_string.Length - 2);
                if(digits < 0)
                    return 0;

                for(value = 0, loop1 = 0; loop1 <= digits; loop1++)
                {
                    if(_string[loop1 + 1] < '0' || _string[loop1 + 1] > '1')
                        return 0;

                    value |= (ushort) ((_string[loop1 + 1] - '0') << (digits - loop1));
                }
            }
            else
            {
                if(!ushort.TryParse(_string.ToString(), out value))
                    value = 0;
            }
            return value;
        }

        ////////////////////////////////////////////////////////////////////
        //
        // line Input routine (PROPORTIONAL)
        //
        ////////////////////////////////////////////////////////////////////
        private bool Input(StringBuilder _string, int max)
        {
            short[] pxt = new short[90];
            pxt[0] = (short) px;

            sbyte key;
            short count = 0;
            do
            {
                key = _sys.toupper((sbyte) (PGet() & 0xff));

                if((key == 127 || key == 8) && count > 0)
                {
                    count--;
                    px = (ushort) pxt[count];
                    DrawPchar(_string[count]);
                    px = (ushort) pxt[count];
                }

                if(key >= ' ' && key <= 'z' && count < max)
                {
                    if(count < _string.Length)
                        _string[count] = (char) key;
                    else
                        _string.Append((char) key);

                    count++;

                    DrawPchar((ushort) key);
                    pxt[count] = (short) px;
                }

            } while(key != 27 && key != 13);

            _string.Length = count;

            if(key == 13)
                return true;

            return false;
        }

        /*
        ===========================================================================

		         PROPORTIONAL PRINTING ROUTINES

        ===========================================================================
        */

        private ushort pxl;

        private ushort pxh;

        private ushort pyl;

        /////////////////////////
        //
        // PPrint
        // Prints a string at px,py.  No clipping!!!
        //
        /////////////////////////
        private void PPrint(string str)
        {
            short index = 0;
            while(index < str.Length)
            {
                char ch = str[index++];

                if(ch == '\n')
                {
                    py += 10;
                    px = pxl;
                }
                else if(ch == '\x7f')
                {
                    fontcolor = (ushort) (str[index++] - 'A'); // set color A-P
                }
                else
                {
                    DrawPchar(ch);
                }
            }
        }

        /////////////////////////
        //
        // PGet
        // Flash a cursor at px,py and waits for a user NoBiosKey
        //
        /////////////////////////
        private short PGet()
        {
            short oldx = (short) px;

            ClearKeys();
            while((NoBiosKey(1) & 0xff) == 0)
            {
                DrawPchar('_');
                WaitVBL(5);

                px = (ushort) oldx;
                DrawPchar('_');
                px = (ushort) oldx;

                if((NoBiosKey(1) & 0xff) != 0) // slight response improver
                    break;

                WaitVBL(5);
            }

            px = (ushort) oldx;

            return (short) NoBiosKey(0); // take it out of the buffer
        }

        /////////////////////////
        //
        // PSize
        // Return the pixels required to proportionaly print a string
        //
        /////////////////////////
        private short PSize(string str)
        {
            short length = 0;
            short index = 0;
            char ch;
            while(index < str.Length)
            {
                ch = str[index++];

                if(ch == '\x7f') // skip color changes
                {
                    index++;
                    continue;
                }

                length += fontseg.Get_width((byte) ch);
            }

            return length;
        }

        /////////////////////////
        //
        // CPPrint
        // Centers the string between pxl/pxh
        //
        /////////////////////////
        private void CPPrint(string str)
        {
            short width = PSize(str);
            px = (ushort) (pxl + (short) (pxh - pxl - width) / 2);
            PPrint(str);
        }

        private void PPrintInt(short val)
        {
            PPrint(val.ToString());
        }

        private void PPrintUnsigned(ushort val)
        {
            PPrint(val.ToString());
        }

        /*
        ===========================================================================

			             GAME ROUTINES

        ===========================================================================
        */

        private int score;

        private int highscore;

        private short level;

        private short bestlevel;

        private LevelDef levelheader;

        ////////////////////////
        //
        // loadctrls
        // Tries to load the control panel settings
        // creates a default if not present
        //
        ////////////////////////
        private void LoadCtrls()
        {
            memptr handle = _sys.LoadControls(); // as: Handled separately now
            if(handle.IsNull)
            {
                //
                // set up default control panel settings
                //
                key[0] = 0x48;
                key[1] = 0x49;
                key[2] = 0x4d;
                key[3] = 0x51;
                key[4] = 0x50;
                key[5] = 0x4f;
                key[6] = 0x4b;
                key[7] = 0x47;
                keyB1 = 0x1d;
                keyB2 = 0x38;

                // as: highscore and bestlevel weren't initialised
            }
            else
            {
                for(int i = 0; i < key.Length; i++)
                    key[i] = handle.GetInt8(i);

                handle.Offset(key.Length);

                keyB1 = handle.GetInt8(0);
                handle.Offset(1);

                keyB2 = handle.GetInt8(0);
                handle.Offset(1);

                highscore = handle.GetInt32(0);
                handle.Offset(4);

                bestlevel = handle.GetInt16(0);
                handle.Offset(2);
            }
        }

        private void SaveCtrls()
        {
            memptr handle = new memptr(new byte[key.Length + 2 + 4 + 2]);
            for(int i = 0; i < key.Length; i++)
                handle.SetInt8(i, key[i]);

            handle.Offset(key.Length);

            handle.SetInt8(0, keyB1);
            handle.Offset(1);

            handle.SetInt8(0, keyB2);
            handle.Offset(1);

            handle.SetInt32(0, highscore);
            handle.Offset(4);

            handle.SetInt16(0, bestlevel);
            handle.Offset(2);

            _sys.SaveControls(handle); // as: Handled separately now
        }

        /// <summary>The size of the CTLPANEL.HOV data.</summary>
        public const int SizeOfCtrls = 8 + 1 + 1 + 4 + 2;
    }
}
