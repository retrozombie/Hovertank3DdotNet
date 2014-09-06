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

#if !PROFILE
#define ADAPTIVE
#endif

using System;
using System.IO;
using System.Text;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        /// <summary>Creates a new Hovertank game.</summary>
        /// <param name="sys">The system.</param>
        public Hovertank(Sys sys)
        {
            _sfxPlayer = sys.SfxPlayer;
            _display = new Display();
            _picCache = new PicCache(NUMCHUNKS);

            _sys = sys;

            // as: string replacements
            _strings = sys.Strings;
            _strings.Initialise(this);

            // If the config file GAME.HOV exists use it for game modifications
            string modFileName = "GAME." + EXTENSION;
            if(sys.FileExists(modFileName))
            {
                Config config = new Config(Encoding.ASCII);
                config.Read(new MemoryStream(sys.FileReadAllBytes(modFileName)));

                if(string.CompareOrdinal(config.ID, "Hovertank3D.Game") == 0)
                {
                    // as: string replacements
                    for(int i = 0; i < Strings.Identifiers.Length; i++)
                    {
                        string identifier = Strings.Identifiers[i];
                        string replacementText = config.GetString(identifier, null);
                        if(replacementText != null)
                            _strings[i] = replacementText;
                   }

                    // Level time limits
                    for(int i = 0; i < 20; i++)
                    {
                        levmin[i] = (short) config.GetInt32("levmin" + i, 0, 9, levmin[i]);
                        levsec[i] = (short) config.GetInt32("levsec" + i, 0, 59, levsec[i]);
                    }

                    // Warp colors
                    for(int i = 0; i < cyclecolors.Length; i++)
                        cyclecolors[i] = (short) config.GetInt32("cyclecolors" + i, 0, 15, cyclecolors[i]);
                }
                else
                {
                    _sys.Log("Unexpected configuration id '" + config.ID + "'!");
                }
            }

            HovDrawInitialise();
            HovLoopInitialise();
        }

        /// <summary>The system.</summary>
        private Sys _sys;

        /// <summary>The sound player.</summary>
        private SfxPlayer _sfxPlayer;

        /// <summary>The display.</summary>
        private Display _display;

        /// <summary>Gets the display.</summary>
        public Display Display
        {
            get { return _display; }
        }

        /// <summary>The pic cache.</summary>
        private PicCache _picCache;

        /// <summary>The string replacements system.</summary>
        private Strings _strings;

        /*

        NOTICE TO ANYONE READING THIS:

        This is the last gasp of our old routines!  Everything is being rewritten
        from scratch to work with new graphic modes and utilities.  This code
        stinks!

        */


        /*
        =============================================================================

                           GLOBALS

        =============================================================================
        */

        // whether int handlers were started
        private bool SNDstarted;

        private bool KBDstarted;

        // as: changed int (file handle) to Pointer
        private memptr grhandle; // handle to egagraph, kept open allways

        private grheadtype grhead; // sets grhuffman and grstarts from here

        private huffnode grhuffman; // huffman dictionary for egagraph

        private memptr grstarts; // array of offsets in egagraph, -1 for sparse

        private int chunkcomplen; // compressed length of a chunk

        private bool soundblaster; // present?

        private const int BUFFERSIZE = 1024;

        private memptr bufferseg; // small general purpose memory block

        private memptr levelseg;

        private bool resetgame;

        private memptr[] scalesegs = new memptr[NUMPICS];

        /*
        =============================================================================
        */

        /*
        =============
        =
        = DoHelpText
        =
        =============
        */
        private void DoHelpText()
        {
            CenterWindow(38, 14);

            fontcolor = 13;
            CPPrint(_strings[Strings.DoHelpText1]); // as: string replacements

            fontcolor = 15;
            py += 6;

            PPrint(_strings[Strings.DoHelpText2]); // as: string replacements

            py += 6;
            PPrint(_strings[Strings.DoHelpText3]); // as: string replacements

            py += 6;
            CPPrint(_strings[Strings.DoHelpText4]); // as: string replacements
            Ack();
            EraseWindow();

            CPPrint(_strings[Strings.DoHelpText5]); // as: string replacements
            CPPrint(_strings[Strings.DoHelpText6]); // as: string replacements

            PPrint(_strings[Strings.DoHelpText7]); // as: string replacements

            CPPrint(_strings[Strings.DoHelpText8]); // as: string replacements
            Ack();
            EraseWindow();

            CPPrint(_strings[Strings.DoHelpText9]); // as: string replacements
            CPPrint(_strings[Strings.DoHelpText10]); // as: string replacements
            PPrint(_strings[Strings.DoHelpText11]); // as: string replacements

            Ack();
        }

        /*
        ==================
        =
        = DebugMemory
        =
        ==================
        */
        private void DebugMemory()
        {
            CenterWindow(16, 7);

            CPPrint(_strings[Strings.DebugMemory1]); // as: string replacements
            CPPrint(_strings[Strings.DebugMemory2]); // as: string replacements
            PPrint(_strings[Strings.DebugMemory3]); // as: string replacements
            PPrintUnsigned((ushort) (totalmem / 64));
            PPrint(_strings[Strings.DebugMemory4]); // as: string replacements
            PPrintUnsigned((ushort) (MMUnusedMemory() / 64));
            PPrint(_strings[Strings.DebugMemory5]); // as: string replacements
            PPrintUnsigned((ushort) (MMTotalFree() / 64));
            PPrint(_strings[Strings.DebugMemory6]); // as: string replacements
            CPPrint(_strings[Strings.DebugMemory7]); // as: string replacements
            PGet();
        }

        /*
        ================
        =
        = DebugKeys
        =
        ================
        */
        private void DebugKeys()
        {
            if(keydown[0x22]) // G = god mode
            {
                ExpWin(12, 1);

                if(godmode != 0)
                    CPPrint(_strings[Strings.DebugKeys1]); // as: string replacements
                else
                    CPPrint(_strings[Strings.DebugKeys2]); // as: string replacements

                Ack();
                godmode ^= 1;
            }
            else if(keydown[0x32]) // M = memory info
            {
                DebugMemory();
            }

            if(keydown[0x19]) // P = pause with no screen disruptioon
            {
                singlestep = 1;
            }

            if(keydown[0x1f]) // S = shield point
            {
                screenofs = 0;
                HealPlayer();
            }
            else if(keydown[0x14]) // T = free time
            {
                if(timestruct.min < 9)
                    timestruct.min++;

                screenofs = 0;
                DrawPic(6, 48, (short) (DIGIT0PIC + timestruct.min));
            }
            else if(keydown[0x11]) // W = warp to level
            {
                ExpWin(26, 1);
                PPrint(_strings[Strings.DebugKeys3]); // as: string replacements

                short i = (short) InputInt();
                if(i >= 1 && i <= 21)
                {
                    level = (short) (i - 1);
                    leveldone = 1;
                }
            }
        }

        /*=========================================================================*/

        /*
        =============
        =
        = CheckKeys
        =
        = Checks to see if an F-key is being pressed and handles it
        =
        =============
        */
        private bool CheckKeys()
        {
            if(NBKscan == 0)
                return false;
            
            switch(NBKscan & 0x7f)
            {
                case 0x3b: // F1 = help
                    ClearKeys();
                    DoHelpText();
                    break;

                case 0x3c: // F2 = sound on/off
                    ClearKeys();

                    ExpWin(13, 1);
                    PPrint(_strings[Strings.CheckKeys1]); // as: string replacements

                    ch = _sys.toupper((sbyte) PGet());
                    if(ch == 'N')
                        soundmode = 0;
                    else if(ch == 'Y')
                        soundmode = 1;

                    break;

                case 0x3d: // F3 = keyboard mode
                    ClearKeys();
                    calibratekeys();
                    break;

                case 0x3e: // F4 = joystick mode
                    ClearKeys();
                    CalibrateJoy(1);
                    break;

                case 0x3f: // F5 = reset game
                    ClearKeys();

                    ExpWin(18, 1);
                    PPrint(_strings[Strings.CheckKeys2]); // as: string replacements

                    ch = _sys.toupper((sbyte) PGet());
                    if(ch == 'Y')
                    {
                        resetgame = true;
                        leveldone = -99;
                    }
                    break;

                case 0x58: // F12 + ? = debug keys
                    DebugKeys();
                    break;

                case 1: // ESC = quit
                    ClearKeys();

                    ExpWin(12, 1);
                    PPrint(_strings[Strings.CheckKeys3]); // as: string replacements

                    ch = _sys.toupper((sbyte) PGet());
                    if(ch == 'Y')
                        Quit("");

                    break;

                default:
                    return false;
            }

            ClearKeys();
            return true;
        }

        //==========================================================================

        /*
        ============================
        =
        = GetChunkLength
        =
        = Seeks into the igrab data file at the start of the given chunk and
        = reads the uncompressed length (first four bytes).  The file pointer is
        = positioned so the compressed data can be read in next.
        = ChunkCompLen is set to the calculated compressed length
        =
        ============================
        */
        private int GetChunkLength(int chunk)
        {
            // as: grhandle changed to memptr
            grhandle.BaseIndex = grstarts.GetInt32(chunk * 4);
            
            int len = grhandle.GetInt32(0);
            grhandle.Offset(4);

            chunkcomplen = grstarts.GetInt32((chunk + 1) * 4) - grstarts.GetInt32(chunk * 4) - 4;

            return len;
        }

        //==========================================================================

        /*
        ============================
        =
        = LoadNearData
        =
        = Load stuff into data segment before memory manager is
        = started (which takes all available memory, near and far)
        =
        ============================
        */
        private void LoadNearData()
        {
            //
            // load egahead.ext (offsets and dictionary for graphics file)
            //
            string fileName = "EGAHEAD." + EXTENSION;

            if(!_sys.FileExists(fileName))
                Quit("Can't open EGAHEAD." + EXTENSION + "!");

            byte[] buffer = _sys.FileReadAllBytes(fileName);
            
            grhead = new grheadtype(buffer);

            if(grhead.dataoffsets + NUMCHUNKS * 4 + 3 >= grhead.Buffer.Length)
            {
                // as: CacheGrFile reads past the end of grstarts (on the last entry)
                // workaround - create an extra entry and add it to the end of grstarts
                Array.Resize<byte>(ref buffer, buffer.Length + 4);

                int fileLength = _sys.FileLength("EGAGRAPH." + EXTENSION);
                if(fileLength == -1)
                    Quit("Cannot open EGAGRAPH." + EXTENSION + "!");

                buffer[buffer.Length - 4] = (byte) fileLength;
                fileLength >>= 8;
                buffer[buffer.Length - 3] = (byte) fileLength;
                fileLength >>= 8;
                buffer[buffer.Length - 2] = (byte) fileLength;
                fileLength >>= 8;
                buffer[buffer.Length - 1] = (byte) fileLength;

                grhead = new grheadtype(buffer);
            }
        }

        //==========================================================================

        /*
        ==========================
        =
        = SegRead
        =
        = Read from a file to a segment pointer
        =
        ==========================
        */
        private void SegRead(ref memptr handle, memptr dest, int length)
        {
            if(length > 0xffff)
                Quit("SegRead doesn't support 64K reads yet!");

            Array.Copy(handle.Buffer, handle.BaseIndex, dest.Buffer, dest.BaseIndex, length);
            handle.Offset(length);
        }

        //==========================================================================

        /////////////////////////////////////////////////////////
        //
        // InitGrFile
        //
        /////////////////////////////////////////////////////////
        private void InitGrFile()
        {
            //
            // calculate some offsets in the header
            //
            grhuffman = new huffnode(grhead);
            grstarts = new memptr(grhead.Buffer, grhead.dataoffsets);

            OptimizeNodes(grhuffman);

            //
            // Open the graphics file, leaving it open until the game is finished
            //
            grhandle = _sys.open("EGAGRAPH." + EXTENSION);
            if(grhandle.IsNull)
                Quit("Cannot open EGAGRAPH." + EXTENSION + "!");

            memptr buffer = new memptr();

            //
            // load the pic and sprite headers into the data segment
            //
            needgr[STRUCTPIC] = 1; // make sure this chunk never reloads
            grsegs[STRUCTPIC] = new memptr(null, 0xffff);
            GetChunkLength(STRUCTPIC); // position file pointer
            MMGetPtr(ref buffer, chunkcomplen);
            SegRead(ref grhandle, buffer, chunkcomplen);

            // as: Added temp pointer
            memptr temp = new memptr(new byte[pictable.Length * pictype.SizeOf], 0);

            HuffExpand(buffer.Buffer, buffer.BaseIndex, temp.Buffer, 0, temp.Buffer.Length, grhuffman);

            // as: Initialise pictypes
            for(int i = 0; i < pictable.Length; i++)
            {
                pictable[i] = new pictype(temp);
                temp.Offset(pictype.SizeOf);
            }

            MMFreePtr(ref buffer);
        }

        //==========================================================================

        /*
        ==========================
        =
        = CacheGrFile
        =
        = Goes through grneeded and grsegs, and makes sure
        = everything needed is in memory
        =
        ==========================
        */

        // base tile sizes for EGA mode
        private const int BLOCK = 32;

        private const int MASKBLOCK = 40;

        private void CacheGrFile()
        {
            //
            // make unneeded chunks purgable
            //
            for(short i = 0; i < NUMCHUNKS; i++)
                if(grsegs[i].Buffer != null && needgr[i] == 0)
                    MMSetPurge(ref grsegs[i], 3);

            MMSortMem();

            //
            // load new stuff
            //
            grhandle.BaseIndex = 0;
            int filepos = 0;

            for(short i = 0; i < NUMCHUNKS; i++)
            {
                if(grsegs[i].Buffer == null && needgr[i] != 0)
                {
                    int newpos = grstarts.GetInt32(i * 4);
                    if(newpos != filepos)
                        grhandle.Offset(newpos - filepos);

                    // chunk lengths
                    int compressed = grstarts.GetInt32((i + 1) * 4) - grstarts.GetInt32(i * 4) - 4;

                    int expanded;
                    if(i >= STARTTILE8)
                    {
                        //
                        // tiles are of a known size
                        //
                        if(i < STARTTILE8M) // tile 8s are all in one chunk!
                            expanded = BLOCK * NUMTILE8;
                        else if(i < STARTTILE16)
                            expanded = MASKBLOCK * NUMTILE8M;
                        else if(i < STARTTILE16M) // all other tiles are one/chunk
                            expanded = BLOCK * 4;
                        else if(i < STARTTILE32)
                            expanded = MASKBLOCK * 4;
                        else if(i < STARTTILE32M)
                            expanded = BLOCK * 16;
                        else
                            expanded = MASKBLOCK * 16;

                        compressed = grstarts.GetInt32((i + 1) * 4) - grstarts.GetInt32(i * 4);
                    }
                    else
                    {
                        //
                        // other things have a length header at start of chunk
                        //
                        expanded = grhandle.GetInt32(0);
                        grhandle.Offset(4);

                        compressed = grstarts.GetInt32((i + 1) * 4) - grstarts.GetInt32(i * 4) - 4;
                    }

                    //
                    // allocate space for expanded chunk
                    //
                    MMGetPtr(ref grsegs[i], expanded);

                    //
                    // if the entire compressed length can't fit in the general purpose
                    // buffer, allocate a temporary memory block for it
                    //
                    if(compressed <= BUFFERSIZE)
                    {
                        SegRead(ref grhandle, bufferseg, compressed);
                        HuffExpand(bufferseg, grsegs[i], expanded, grhuffman);
                    }
                    else
                    {
                        memptr bigbufferseg = new memptr(); // for compressed
                        MMGetPtr(ref bigbufferseg, compressed);
                        SegRead(ref grhandle, bigbufferseg, compressed);
                        HuffExpand(bigbufferseg, grsegs[i], expanded, grhuffman);
                        MMFreePtr(ref bigbufferseg);
                    }

                    filepos = grstarts.GetInt32((i + 1) * 4); // file pointer is now at start of next one
                }
            }

            // as: pic cache
            _picCache.CachePic(STARTTILE8, grsegs[STARTTILE8], 8, 8, 72);
        }

        //==========================================================================

        /*
        =====================
        =
        = CachePic
        =
        = Make sure a graphic chunk is in memory
        =
        =====================
        */
        private void CachePic(short picnum)
        {
            if(grsegs[picnum].Buffer != null)
                return;

            grhandle.BaseIndex = grstarts.GetInt32(picnum * 4);

            // chunk lengths
            int compressed = grstarts.GetInt32((picnum + 1) * 4) - grstarts.GetInt32(picnum * 4) - 4; // as: redundant
            int expanded;
            if(picnum >= STARTTILE8)
            {
                //
                // tiles are of a known size
                //
                if(picnum < STARTTILE8M) // tile 8s are all in one chunk!
                    expanded = BLOCK * NUMTILE8;
                else if(picnum < STARTTILE16)
                    expanded = MASKBLOCK * NUMTILE8M;
                else if(picnum < STARTTILE16M) // all other tiles are one/chunk
                    expanded = BLOCK * 4;
                else if(picnum < STARTTILE32)
                    expanded = MASKBLOCK * 4;
                else if(picnum < STARTTILE32M)
                    expanded = BLOCK * 16;
                else
                    expanded = MASKBLOCK * 16;

                compressed = grstarts.GetInt32((picnum + 1) * 4) - grstarts.GetInt32(picnum * 4);
            }
            else
            {
                //
                // other things have a length header at start of chunk
                //
                expanded = grhandle.GetInt32(0);
                grhandle.Offset(4);
                compressed = grstarts.GetInt32((picnum + 1) * 4) - grstarts.GetInt32(picnum * 4) - 4;
            }

            //
            // allocate space for expanded chunk
            //
            MMGetPtr(ref grsegs[picnum], expanded);

            memptr bigbufferseg = new memptr(); // for compressed
            MMGetPtr(ref bigbufferseg, compressed);
            SegRead(ref grhandle, bigbufferseg, compressed);
            HuffExpand(bigbufferseg, grsegs[picnum], expanded, grhuffman);
            MMFreePtr(ref bigbufferseg);

            // as: Add to pic cache if necessary
            if(_picCache[picnum] == null)
                _picCache.CachePic(picnum, grsegs[picnum], pictable[picnum - STARTPICS]);
        }

        //==========================================================================

        /*
        =====================
        ==
        == Quit
        ==
        =====================
        */
        public void Quit(string error)
        {
            if(!_sys.CanQuit)
                return;

            if(error == "")
            {
                SaveCtrls();
            }
            else
            {   // as: Record error
                _sys.Log("Quit(" + error + ")");
            }

            MMShutdown();

            if(KBDstarted)
                ShutdownKbd(); // shut down the interrupt driven stuff if needed

            if(SNDstarted)
                ShutdownSound();

            if(soundblaster)
                jmShutSB();

            grhandle.SetNull();

            SetScreenMode(grtype.text);

            if(error == "")
            {
#if DISABLED && !CATALOG
                _sys.argc = 2;
                _sys.argv[1] = "LAST.SHL";
                _sys.argv[2] = "ENDSCN.SCN";
                _sys.argv[3] = null;
                if(_sys.execv("LOADSCN.EXE", _sys.argv) == -1)
                {
                    _sys.clrscr();
                    _sys.puts("Couldn't find executable LOADSCN.EXE.\n");
                    _sys.exit(1);
                }
#endif
            }
            else
            {
                _sys.puts(error);
            }

            _sys.exit(0); // quit to DOS
        }


        //==========================================================================

        /*
        ======================
        =
        = LoadLevel
        =
        = Loads LEVEL00.EXT (00 = global variable level)
        =
        ======================
        */
        private void LoadLevel()
        {
            //
            // load the new level in and decompress
            //
            string filename, num;
            if(level < 10)
            {
                num = level.ToString();
                filename = "LEVEL0";
            }
            else
            {
                num = level.ToString();
                filename = "LEVEL";
            }

            filename = string.Concat(filename, num);
            filename = string.Concat(filename, "." + EXTENSION);

            memptr bufferseg = new memptr();
            BloadinMM(filename, ref bufferseg);

            ushort length = bufferseg.GetUInt16(0);

            if(levelseg.Buffer != null)
                MMFreePtr(ref levelseg);

            MMGetPtr(ref levelseg, length);

            // as: Made RLEWExpand static
            try { RLEWExpand(bufferseg, levelseg); }
            catch(Exception ex)
            {   // as: Quit on failure
                Quit(ex.Message);
            }

            MMFreePtr(ref bufferseg);

            levelheader = new LevelDef(levelseg);

            //
            // copy plane 0 to tilemap
            //
            memptr planeptr = new memptr(levelseg, 32);
            int index = 0;
            for(short y = 0; y < levelheader.height; y++)
                for(short x = 0; x < levelheader.width; x++, index += 2)
                    tilemap[x, y] = planeptr.GetUInt16(index);

            //
            // spawn tanks
            //
            planeptr = new memptr(levelseg, 32 + levelheader.planesize);
            StartLevel(planeptr);

            MMFreePtr(ref levelseg);
        }

        //==========================================================================

        /*
        =================
        =
        = CacheDrawPic
        =
        =================
        */
        private void CacheDrawPic(short picnum)
        {
            CachePic((short) (STARTPICS + picnum));

            EGASplitScreen(200);
            SetScreen(0, 0);
            SetLineWidth(80);
            screenofs = 0;

            EGAWRITEMODE(0);
            DrawPic(0, 0, picnum);

            EGAWRITEMODE(1);
            EGAMAPMASK(15);
            CopyEGA(80, 200, 0, 0x4000);
            EGAWRITEMODE(0);

            MMSetPurge(ref grsegs[STARTPICS + picnum], 3);
        }

        //==========================================================================

        private bool SoundPlaying()
        {
            return _sfxPlayer.IsSoundPlaying;
        }

        //==========================================================================

        private const short PICHEIGHT = 64; // full size height of scaled pic

        private const float NUMFRAMES = 300.0f; // go from 0 to this in numframes

        private const float MAXANGLE = (3.141592657f * 0.6f);

        private const float RADIUS = 1000.0f; // world coordinates

        private const float DISTANCE = 1000.0f; // center point z distance

        /*
        =====================
        =
        = Intro
        =
        =====================
        */
        private void Intro()
        {
            memptr shapeseg;
            short i, f, sx, sy, page;
            ushort[] pageptr = new ushort[2];
            ushort[] pagewidth = new ushort[2];
            ushort[] pageheight = new ushort[2];
            float x, y, z, angle, step, sn, cs, sizescale, scale;
            float ytop, xmid, minz, worldycenter, worldxcenter;

            FadeOut();

            SetLineWidth(SCREENWIDTH);

            screenofs = 0;

            CacheDrawPic(STARSPIC);

            pxl = 0;
            pxh = 320;
            py = 180;

#if DISABLED && !CATALOG
            CPPrint("Copyright (c) 1991-93 Softdisk Publishing\n");
            //CPPrint("'I' for information");
#endif

            EGAWRITEMODE(1);
            EGAMAPMASK(15);
            CopyEGA(40, 200, 0, 0x4000);
            CopyEGA(40, 200, 0, 0x8000);
            CopyEGA(40, 200, 0, 0xc000);
            StopDrive();

            CachePic(STARTPICS + LOGOPIC);

            // as: The following is disabled because it throws an exception
            // due to trying to make a 0x0 shape
#if DISABLED_BROKEN
            shapeseg = new memptr();
            SC_MakeShape(
              grsegs[STARTPICS + LOGOPIC],
              0,
              0,
              ref shapeseg);
#endif

            // as: the following was commented out, it displays the logo zooming in an arc
            // this can be enabled with the -intrologo command
            shapeseg = new memptr();
            if(_sys.IntroLogo)
            {
                SC_MakeShape(
                    grsegs[STARTPICS + LOGOPIC],
                    pictable[LOGOPIC].width,
                    pictable[LOGOPIC].height,
                    ref shapeseg);
            }

            MMFreePtr(ref grsegs[STARTPICS + LOGOPIC]);

            FadeIn();

            sx = 160;
            sy = 180;

            /*
            =============================================================================

                      SCALED PICTURE DIRECTOR

            =============================================================================
            */

            minz = (float) (Math.Cos(MAXANGLE) * RADIUS); // closest point
            minz += DISTANCE;
            sizescale = 256 * minz; // closest point will be full size
            ytop = 80 - (PICHEIGHT / 2) * (sizescale / DISTANCE) / 256;
            z = sizescale / (DISTANCE * 256);
            ytop = ytop / z; // world coordinates
            worldycenter = ytop - RADIUS;
            xmid = (float) (Math.Sin(MAXANGLE) * RADIUS / 2);
            worldxcenter = -xmid;

            if(_sys.SimulateIntroSound)
                _sfxPlayer.PlaySound(INTROSND);

            f = 1;
            page = 0;
            inttime = 0;
            screenofs = 0;
            pagewidth[0] = 0;
            pagewidth[1] = 0;
            do
            {
                step = f / NUMFRAMES;
                angle = MAXANGLE * step;
                sn = (float) Math.Sin(angle);
                cs = (float) Math.Cos(angle);
                x = worldxcenter + sn * RADIUS / 2;
                y = worldycenter + sn * RADIUS;
                z = DISTANCE + cs * RADIUS;
                scale = sizescale / z;
                sx = (short) (160 + (short) (x * scale / 256));
                sy = (short) (100 - (short) (y * scale / 256));

                inttime = 0;
                _sys.sound((ushort) ((short) (sn * 1500)));

                if(_sys.IntroLogo)
                {
                    //
                    // erase old position
                    //
                    if(pagewidth[page] != 0)
                    {
                        EGAWRITEMODE(1);
                        EGAMAPMASK(15);
                        CopyEGA(pagewidth[page], pageheight[page], (ushort) (pageptr[page] + 0x8000), pageptr[page]);
                    }

                    //
                    // draw new position
                    //
                    EGAWRITEMODE(2);
                    if(SC_ScaleShape(sx, sy, (ushort) ((short) scale < 40 ? 10 : scale / 4), shapeseg))
                    {
                        pagewidth[page] = scaleblockwidth;
                        pageheight[page] = scaleblockheight;
                        pageptr[page] = scaleblockdest;
                    }
                    else
                    {
                        pagewidth[page] = 0;
                    }
                }

                EGAWRITEMODE(0);
                EGABITMASK(255);

                //
                // display it
                //
                SetScreen(screenofs, 0);

                page ^= 1;
                screenofs = (ushort) (0x4000 * page);

                f++;
                if(f < NUMFRAMES)
                {
                    f += (short) inttime;
                    if(f > NUMFRAMES)
                        f = (short) NUMFRAMES;
                }
                else
                {
                    f++; // last frame is shown
                }

                if(NBKscan > 0x7f)
                    break;

            } while(f <= NUMFRAMES);

            if(_sys.SimulateIntroSound) // as: Intro sound stops when a key is pressed
                _sfxPlayer.StopSound();

            _sys.nosound();

            for(i = 0; i < 200; i++)
            {
                WaitVBL(1);

                if(NBKscan > 0x7f)
                {
                    // as: With the following enabled, pressing I during the intro displays a
                    // window with a message, part of the text overflows the window though
                    // this can be enabled with the -introinfo command
//#if false
                    if(_sys.IntroInfo)
                    {
                        if(NBKscan == 0x97) //'I' for info
                        {
                            screenofs ^= 0x4000;
                            CenterWindow(24, 10);
                            py += 2;
                            CPPrint(_strings[Strings.Intro1]); // as: string replacements
                            CPPrint(_strings[Strings.Intro2]); // as: string replacements
                            CPPrint(_strings[Strings.Intro3]); // as: string replacements
                            CPPrint(_strings[Strings.Intro4]); // as: string replacements
                            CPPrint(_strings[Strings.Intro5]); // as: string replacements
                            CPPrint(_strings[Strings.Intro6]); // as: string replacements
                            CPPrint(_strings[Strings.Intro7]); // as: string replacements
                            ClearKeys();
                            Ack();
                        }
                        ClearKeys();
                        break;
                    }
//#endif
                }
            }

            MMFreePtr(ref shapeseg);
        }

        //==========================================================================

        private const int PAUSE = 300;

        /*
        =====================
        ==
        == DemoLoop
        ==
        =====================
        */
        private void DemoLoop()
        {
            FadeOut();

            CacheDrawPic(TITLEPIC);
            StopDrive(); // make floppy motors turn off

            FadeIn();

            short originx = 0;
            short i = 100;
            while(true)
            {
                if(i > PAUSE && i <= PAUSE + 80)
                    originx += 4;

                if(i > PAUSE * 2 && i <= PAUSE * 2 + 80)
                    originx -= 4;

                if(i > PAUSE * 2 + 80)
                    i = 0;

                SetScreen((ushort) (originx / 8), (ushort) (originx % 8));

                i++;

                screenofs = (ushort) (originx / 8);
                if(CheckKeys())
                {
                    EGAWRITEMODE(1);
                    EGAMAPMASK(15);
                    CopyEGA(80, 200, 0x4000, 0);
                }

                ControlStruct c = ControlPlayer(1);

                if(c.button1 || c.button2)
                    break;

                if(keydown[0x39])
                    break;
            }

            ClearKeys();
        }
        
        //==========================================================================

        /*
        ====================
        =
        = SetupGraphics
        =
        ====================
        */
        private void SetupGraphics()
        {
            InitGrFile(); // load the graphic file header

            //
            // go through the pics and make scalable shapes, the discard the pic
            //
            for(int i = MAN1PIC; i < DASHPIC; i++)
            {
                CachePic((short) (STARTPICS + i));

                SC_MakeShape(
                  grsegs[STARTPICS + i],
                  pictable[i].width,
                  pictable[i].height,
                  ref scalesegs[i]);

                MMFreePtr(ref grsegs[STARTPICS + i]);
            }

            //
            // load the basic graphics
            //
            needgr[STARTFONT] = 1;
            needgr[STARTTILE8] = 1;

            for(int i = DASHPIC; i < ENDPIC; i++)
                needgr[STARTPICS + i] = 1;

            CacheGrFile(); // load all graphics now (no caching)

            fontseg = new fontstruct(grsegs[STARTFONT]);
        }

        //==========================================================================

        ////////////////////////////////////////////////////////////
        //
        // Allocate memory and load file in
        //
        ////////////////////////////////////////////////////////////
        private void LoadIn(string filename, /*char huge **baseptr*/ out memptr baseptr)
        {
            if(!_sys.FileExists(filename))
            {
                _sys.printf("Error loading file '%s'!\n", filename);
                _sys.exit(1);
            }

            int len = _sys.FileLength(filename);
            baseptr = new memptr(_sys.farmalloc(len));
            
            LoadFile(filename, baseptr);
        }

        ///////////////////////////////////////////////////////////////////////////
        //
        //      US_CheckParm() - checks to see if a string matches one of a set of
        //              strings. The check is case insensitive. The routine returns the
        //              index of the string that matched, or -1 if no matches were found
        //
        ///////////////////////////////////////////////////////////////////////////
        private int US_CheckParm(string parm, string[] strings)
        {
            short parmIndex = 0;

            while(!_sys.isalpha((sbyte) parm[parmIndex])) // Skip non-alphas
                parmIndex++;

            for(short i = 0; i < strings.Length; i++)
            {
                string s = strings[i];
                sbyte cs = 0, cp = 0;
                for(int sIndex = 0, pIndex = parmIndex; cs == cp; )
                {
                    if(sIndex == s.Length) // as: Changed to length check and re-ordered
                        return i;

                    cs = (sbyte) s[sIndex++];

                    cp = (sbyte) parm[pIndex++];

                    if(_sys.isupper((sbyte) cs))
                        cs = _sys.tolower((sbyte) cs);

                    if(_sys.isupper((sbyte) cp))
                        cp = _sys.tolower((sbyte) cp);
                }
            }

            return -1;
        }

        ///////////////////////////////////////////////////////////////////////////
        /*
        =================
        =
        = main
        =
        =================
        */

#if DISABLED && !CATALOG // as: Added to remove compiler warning
        private static readonly string[] EntryParmStrings = new string[] { "detour" };
#endif

        private static readonly string[] SBlasterStrings = new string[] { "NOBLASTER" };

        public void main()
        {
#if DISABLED && !CATALOG // as: Added to remove compiler warning
            bool LaunchedFromShell = false;
#endif

            _sys.textbackground(0);
            _sys.textcolor(7);

            if(_sys.argv.Length > 1 && string.Compare(_sys.argv[1], "/VER", StringComparison.OrdinalIgnoreCase) == 0)
            {   // as: Added length check
                _sys.printf(_strings[Strings.main1]); // as: string replacements
                _sys.printf(_strings[Strings.main2]); // as: string replacements
                _sys.printf(_strings[Strings.main3]); // as: string replacements
                _sys.exit(0);
            }

            short i;
#if DISABLED && !CATALOG // as: Extended to remove compiler warning
            for(i = 1; i < _sys.argc; i++)
            {
                switch(US_CheckParm(_sys.argv[i], EntryParmStrings))
                {
                    case 0:
                        LaunchedFromShell = true;
                        break;
                }
            }

            if(!LaunchedFromShell)
            {
                _sys.clrscr();
                _sys.puts("You must type START at the DOS prompt to run HOVERTANK 3-D.");
                _sys.exit(0);
            }
#endif

            // _sys.puts("HoverTank 3-D is executing...");

            //
            // detect video
            //
            videocard = VideoID();

            if(videocard == cardtype.EGAcard) { }
            //    _sys.puts("EGA card detected");
            else if(videocard == cardtype.VGAcard) { }
            //    _sys.puts("VGA card detected");
            else
            {
                _sys.clrscr();
                _sys.puts("Hey, I don't see an EGA or VGA card here!  Do you want to run the program ");
                _sys.puts("anyway (Y = go ahead, N = quit to dos) ?");
                ClearKeys();
                sbyte c = _sys.toupper((sbyte) _sys.bioskey(false));
                if(c != 'Y')
                    _sys.exit(1);
            }

            grmode = grtype.EGAgr;

            //
            // setup for sound blaster
            //
            soundblaster = true;
            for(i = 1; i < _sys.argc; i++)
            {
                switch(US_CheckParm(_sys.argv[i], SBlasterStrings))
                {
                    case 0:
                        soundblaster = false;
                        break;
                }
            }

            if(soundblaster)
                soundblaster = jmDetectSoundBlaster(-1);

#if false
            if(string.Compare(_sys.argv[1], "NOBLASTER", StringComparison.OrdinalIgnoreCase) == 0)
                soundblaster = false;
            else
                soundblaster = jmDetectSoundBlaster(-1);
#endif
            // as: Enable speaker mode when NOBLASTER command is specified
            if(_sys.IndexOfCommandLineArgument("NOBLASTER") != -1)
                _sfxPlayer.SpeakerMode = true;

            if(soundblaster)
            {
                //puts("Sound Blaster detected! (HOVER NOBLASTER to void detection)");
                memptr baseptr;
                LoadIn("DSOUND.HOV", out baseptr);

                SampledSound samples = new SampledSound(baseptr);
                jmStartSB();
                jmSetSamplePtr(samples);
            }
            //else
            //  _sys.puts("Sound Blaster not detected");

            LoadNearData(); // load some stuff before starting the memory manager

            MMStartup();
            MMGetPtr(ref bufferseg, BUFFERSIZE); // small general purpose buffer

            BloadinMM("SOUNDS." + EXTENSION, ref soundseg);

            // as: Added initialisation of speaker sounds
            _sfxPlayer.InitSpeakerSound(soundseg);

#if ADAPTIVE
            // timerspeed = 0x2147; // 140 ints / second (2/VBL)
            StartupSound(); // interrupt handlers that must be removed at quit
            SNDstarted = true;
#endif

            StartupKbd();
            KBDstarted = true;

            SetupGraphics();

            InitRndT(true); // setup random routines
            InitRnd(true);

            LoadCtrls();

            //_sys.puts ("Calculating...");
            BuildTables();

            SC_Setup();

            SetScreenMode(grmode);

            SetLineWidth(SCREENWIDTH);

            screencenterx = 19;
            screencentery = 12;

#if !(PROFILE || TESTCASE)
            if(!keydown[1]) // hold ESC to bypass intro
                Intro();
#endif

#if PROFILE
            JoyXlow[1] = 16;
            JoyYlow[1] = 16;
            JoyXhigh[1] = 70;
            JoyYhigh[1] = 70;
            playermode[1] = inputtype.joystick1;
#endif

            while(true)
            {
#if !(PROFILE || TESTCASE)
                DemoLoop(); // do title, demo, etc
#endif
                PlaySound(STARTGAMESND);
                PlayGame();
            }
        }
   
    }
}
