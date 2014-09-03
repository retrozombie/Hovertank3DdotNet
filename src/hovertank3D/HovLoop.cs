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
using System.Collections.Generic;
using System.Text;

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        private void HovLoopInitialise()
        {
            for(short i = 0; i < MAXOBJECTS; i++)
                objlist[i] = new objtype();
        }

        //==========================================================================

        //
        // map arrays
        //
        private ushort[,] tilemap = new ushort[MAPSIZE, MAPSIZE];

        //
        // play stuff
        //

        private short godmode;

        public short singlestep;

        private short leveldone;

        private short startlevel;

        private timetype timestruct = new timetype();

        private objtype[] objlist = new objtype[MAXOBJECTS];

        private objtype obon = new objtype();

        private objtype _new;

        private objtype obj;

        private short lastobjIndex; // as: Changed from pointer to index

        private objtype check;

        private ControlStruct c;

        private short numrefugees;

        private short totalrefugees;

        private short savedcount;

        private short killedcount;

        private short bordertime;

        //
        // actor stuff
        //

        private fixed_t warpx; // where to spawn warp gate

        private fixed_t warpy;

        private fixed_t xmove;

        private fixed_t ymove;

        private const int TIMEPOINTS = 100;

        private const int REFUGEEPOINTS = 10000;

        //==========================================================================

        /*
        ==================
        =
        = DrawCockpit
        =
        ==================
        */
        private void DrawCockpit()
        {
            screenofs = 0;

            screencenterx = 19;
            screencentery = 7;

            EGAWRITEMODE(0);
            DrawPic(0, 0, DASHPIC);
            DrawScore();
        }

        //==========================================================================

        /*
        ===================
        =
        = DrawScore
        =
        ===================
        */
        private void DrawScore()
        {
            // as: converted
            StringBuilder str = new StringBuilder();
            str.Append(score.ToString());
            sx = (short) (22 - str.Length);
            sy = 7;
            screenofs = (ushort) (linewidth * 2);

            for(short i = 0; i < str.Length; i++)
                str[i] = (char) (str[i] - ('0' - 23)); // the digit pictures start at 23

            Print(str.ToString());
            screenofs = 0;
        }

        //==========================================================================

        private void BadThink()
        {
            Quit("BadThink called!");
        }

        /*
        ===============
        =
        = FindFreeobj
        =
        = Assigned global variable *new to a free spot in the object list
        =
        ===============
        */
        private void FindFreeObj()
        {
            short newIndex = 1;
            while(objlist[newIndex]._class != classtype.nothing && newIndex <= lastobjIndex)
                newIndex++;

            if(newIndex > lastobjIndex)
            {
                lastobjIndex++;

                if(lastobjIndex >= MAXOBJECTS)
                    Quit("Object list overflow!");
            }

            _new = objlist[newIndex];

            _new.Clear();

            _new.think = BadThink;
        }

        //==========================================================================

        /*
        ==================
        =
        = StartLevel
        =
        ==================
        */
        private void StartLevel(memptr plane1)
        {
            numrefugees = 0;

            for(ushort y = 0; y < levelheader.height; y++)
            {
                for(ushort x = 0; x < levelheader.width; x++)
                {
                    ushort tile = plane1.GetUInt16(0);
                    plane1.Offset(2);
                    if(tile > 0)
                    {
                        ushort dir = (ushort) (tile >> 8); // high byte gives starting dir
                        tile &= 0xff;
                        fixed_t gx = x * TILEGLOBAL + TILEGLOBAL / 2;
                        fixed_t gy = y * TILEGLOBAL + TILEGLOBAL / 2;

                        // as: Added LevelObjects
                        switch(tile)
                        {
                            case LevelObjects.MaleRefugee:
                                SpawnRefugee(gx, gy, true);
                                break;

                            case LevelObjects.Drone:
                                SpawnDrone(gx + TILEGLOBAL / 2, gy + TILEGLOBAL / 2);
                                break;

                            case LevelObjects.Tank:
                                SpawnTank(gx + TILEGLOBAL / 2, gy + TILEGLOBAL / 2);
                                break;

                            case LevelObjects.Mutant:
                                SpawnMutant(gx + TILEGLOBAL / 2, gy + TILEGLOBAL / 2);
                                break;

                            case LevelObjects.Shield:
                                SpawnShield(gx, gy);
                                break;

                            case LevelObjects.FemaleRefugee:
                                SpawnRefugee(gx, gy, false);
                                break;

                            case LevelObjects.WarpGate:
                                warpx = gx; // warp gate is spawned when all men are done
                                warpy = gy;
                                break;

                            case LevelObjects.Player:
                                SpawnPlayer(gx, gy);

                                short angle = (short) (ANGLES / 4 - dir * ANGLES / 4);
                                
                                if(angle < 0)
                                    angle += ANGLES;

                                objlist[0].angle = angle;
                                break;
                        }
                    }
                }
            }

            totalrefugees = numrefugees;
        }

        //==========================================================================

        /*
        =====================
        =
        = DropTime
        =
        =====================
        */

        private int secondtics;

        private void DropTime()
        {
            secondtics += tics;
            if(secondtics < 70)
                return;

            secondtics = 0; // give the slow systems a little edge

            if(--timestruct.sec < 0)
            {
                timestruct.sec = 59;

                if(--timestruct.min < 0)
                {   // Out of time
                    leveldone = -1;
                }
                else
                {
                    //
                    // draw new minutes
                    //
                    DrawPic(6, 48, (short) (DIGIT0PIC + timestruct.min));
                }
            }

            //
            // draw new seconds
            //
            DrawPic(9, 48, (short) (DIGIT0PIC + timestruct.sec / 10));
            DrawPic(11, 48, (short) (DIGIT0PIC + timestruct.sec % 10));

            if(timestruct.min == 0 && timestruct.sec <= 20)
                PlaySound(LOWTIMESND);
        }

        //==========================================================================

        /*
        ===================
        =
        = ClipMove
        =
        = Only checks corners, so the object better be less than one tile wide!
        =
        ===================
        */
        private void ClipMove()
        {
            //
            // move player and check to see if any corners are in solid tiles
            //
            if(xmove < 0)
                xmove = -(xmove ^ SIGNBIT);

            if(ymove < 0)
                ymove = -(ymove ^ SIGNBIT);

            obon.x += xmove;
            obon.y += ymove;

            CalcBounds();

            short xl = (short) (obon.xl >> TILESHIFT);
            short yl = (short) (obon.yl >> TILESHIFT);

            short xh = (short) (obon.xh >> TILESHIFT);
            short yh = (short) (obon.yh >> TILESHIFT);

            if(tilemap[xl, yl] == 0 && tilemap[xh, yl] == 0 && tilemap[xh, yh] == 0 && tilemap[xl, yh] == 0)
                return; // no corners in wall

            if(!SoundPlaying())
                PlaySound(BUMPWALLSND);

            //
            // intersect the path with the tile edges to determine point of impact
            //

            //
            // clip to east / west walls
            //
            short nt1, nt2;
            ushort inside, total;
            int intersect;
            if(xmove > 0)
            {
                inside = (ushort) (obon.xh & 0xffff);
                total = (ushort) xmove;
                if(inside <= total)
                {
                    if(total > 1)
                        intersect = ymove * inside / (total - 1);
                    else
                        intersect = ymove;

                    nt1 = (short) ((obon.yl - intersect) >> TILESHIFT);
                    nt2 = (short) ((obon.yh - intersect) >> TILESHIFT);

                    if((tilemap[xh, nt1] != 0 && tilemap[xh - 1, nt1] == 0) || 
                        (tilemap[xh, nt2] != 0 && tilemap[xh - 1, nt2] == 0))
                    {
                        obon.x = ((obon.xh & -65536) - (MINDIST + 1));
                    }
                }
            }
            else if(xmove < 0)
            {
                inside = (ushort) (TILEGLOBAL - (obon.xl & 0xffff));
                total = (ushort) (-xmove);
                if(inside <= total)
                {
                    if(total > 1)
                        intersect = ymove * inside / (total - 1);
                    else
                        intersect = ymove;

                    nt1 = (short) ((obon.yl - intersect) >> TILESHIFT);
                    nt2 = (short) ((obon.yh - intersect) >> TILESHIFT);

                    if((tilemap[xl, nt1] != 0 && tilemap[xl + 1, nt1] == 0) ||
                        (tilemap[xl, nt2] != 0 && tilemap[xl + 1, nt2] == 0))
                    {
                        obon.x = (obon.xl & -65536) + TILEGLOBAL + (MINDIST + 1);
                    }
                }
            }

            //
            // clip to north / south walls
            //
            if(ymove > 0)
            {
                inside = (ushort) (obon.yh & 0xffff);
                total = (ushort) (ymove);
                if(inside <= total)
                {
                    if(total > 1)
                        intersect = xmove * inside / (total - 1);
                    else
                        intersect = xmove;

                    nt1 = (short) ((obon.xl - intersect) >> TILESHIFT);
                    nt2 = (short) ((obon.xh - intersect) >> TILESHIFT);

                    if((tilemap[nt1, yh] != 0 && tilemap[nt1, yh - 1] == 0) ||
                        (tilemap[nt2, yh] != 0 && tilemap[nt2, yh - 1] == 0))
                    {
                        obon.y = (obon.yh & -65536) - (MINDIST + 1);
                    }
                }
            }
            else if(ymove < 0)
            {
                inside = (ushort) (TILEGLOBAL - (obon.yl & 0xffff));
                total = (ushort) (-ymove);
                if(inside <= total)
                {
                    if(total > 1)
                        intersect = xmove * inside / (total - 1);
                    else
                        intersect = xmove;
                    
                    nt1 = (short) ((obon.xl - intersect) >> TILESHIFT);
                    nt2 = (short) ((obon.xh - intersect) >> TILESHIFT);

                    if((tilemap[nt1, yl] != 0 && tilemap[nt1, yl + 1] == 0) ||
                        (tilemap[nt2, yl] != 0 && tilemap[nt2, yl + 1] == 0))
                    {
                        obon.y = (obon.yl & -65536) + TILEGLOBAL + (MINDIST + 1);
                    }
                }
            }
        }

        //==========================================================================

        /*
        ==================
        =
        = CalcBounds
        =
        ==================
        */
        private void CalcBounds()
        {
            //
            // calculate hit rect
            //
            obon.xl = obon.x - obon.size;
            obon.xh = obon.x + obon.size;
            obon.yl = obon.y - obon.size;
            obon.yh = obon.y + obon.size;
        }

        private void CalcBoundsNew()
        {
            //
            // calculate hit rect
            //
            _new.xl = _new.x - _new.size;
            _new.xh = _new.x + _new.size;
            _new.yl = _new.y - _new.size;
            _new.yh = _new.y + _new.size;
        }

        //==========================================================================

        /*
        ==================
        =
        = TransformObon
        =
        = Calculates transformed position and updates radar
        =
        ==================
        */
        private void TransformObon()
        {
            //
            // translate point to view centered coordinates
            //
            fixed_t gx = FixedAdd(obon.x, viewx | SIGNBIT);
            fixed_t gy = FixedAdd(obon.y, viewy | SIGNBIT);

            //
            // calculate newx
            //
            fixed_t gxt = FixedByFrac(gx, viewcos);
            fixed_t gyt = FixedByFrac(gy, viewsin);
            obon.viewx = FixedAdd(gxt, gyt ^ SIGNBIT);

            //
            // calculate newy
            //
            gxt = FixedByFrac(gx, viewsin);
            gyt = FixedByFrac(gy, viewcos);
            obon.viewy = FixedAdd(gyt, gxt);

            //
            // update radar
            //
            if(obon.radarx != 0)
                XPlot(obon.radarx, obon.radary, obon.radarcolor);

            int absdx = obon.viewx & (~SIGNBIT);
            int absdy = obon.viewy & (~SIGNBIT);

            if(obon.viewx < 0)
                obon.viewx = -absdx;

            if(obon.viewy < 0)
                obon.viewy = -absdy;

            if(absdx < RADARRANGE && absdy < RADARRANGE)
            {
                obon.radarx = (short) (RADARX + obon.viewy / RADARSCALE);
                obon.radary = (short) (RADARY - obon.viewx / RADARSCALE);
                XPlot(obon.radarx, obon.radary, obon.radarcolor);
            }
            else
            {
                obon.radarx = 0;
            }
        }
        
        //==========================================================================

        /*
        =====================
        =
        = WarpEffect
        =
        =====================
        */
        private void Block(short x, short y, short color)
        {
            // as: converted from asm
            // Adapted for simpler display emulation

            int dstIndex = screenofs * Display.AddressScale + (ylookup[y << 3] + x) * Display.ColumnScale;
            byte[] videoBuffer = _display.VideoBuffer;
            byte colorIndex = (byte) color;
            for(int by = 0; by < 8; by++)
            {
                for(int bx = 0; bx < 8; bx++)
                    videoBuffer[dstIndex + bx] = colorIndex;

                dstIndex += SCREENWIDTH * Display.ColumnScale;
            }
        }

        private void Frame(short xl, short yl, short xh, short yh, short color)
        {
            for(short x = xl; x <= xh; x++)
            {
                Block(x, yl, color);
                Block(x, yh, color);
            }

            for(short y = (short) (yl + 1); y < yh; y++)
            {
                Block(xl, y, color);
                Block(xh, y, color);
            }
        }

        private const short NUMCYCLES = 3;

        private const int WARPSTEPS = 200;

        private const int CYCLETIME = 6;

        private const int FOCUS = 10;

        private short[] cyclecolors = new short[]
        {
            3, 3, 11
        };

        private void WarpEffect()
        {
            screenofs = screenloc[screenpage];
            SetScreen(screenofs, 0);

            Array.Clear(zbuffer, 0, zbuffer.Length);

            EGAWRITEMODE(2);

            for(short size = 0; size < 8; size++)
            {
                screenofs = screenloc[screenpage];
                Frame(size, size, (short) (39 - size), (short) (15 - size), cyclecolors[size % NUMCYCLES]);

                screenofs = screenloc[(screenpage + 1) % 3];
                Frame(size, size, (short) (39 - size), (short) (15 - size), cyclecolors[(size + 1) % NUMCYCLES]);

                screenofs = screenloc[(screenpage + 2) % 3];
                Frame(size, size, (short) (39 - size), (short) (15 - size), cyclecolors[(size + 2) % NUMCYCLES]);
            }

            int oldtime = (int) timecount;

            PlaySound(WARPGATESND);

            int time;
            do
            {
                time = (int) (timecount - oldtime);

                if(time > WARPSTEPS)
                    time = WARPSTEPS;

                screenofs = screenloc[(screenpage + time / CYCLETIME) % 3];

                SC_ScaleShape(CENTERX, 64, (ushort) (255 * FOCUS / (WARPSTEPS + FOCUS - time)),
                  scalesegs[WARP1PIC + (time / CYCLETIME) % 4]);

                SetScreen(screenloc[(screenpage + time / CYCLETIME) % 3], 0);

            } while(time < WARPSTEPS && NBKascii != 27);

            ClearKeys();

            EGAWRITEMODE(0);
            EGABITMASK(255);
        }

        //==========================================================================

        /*
        =====================
        =
        = GameOver
        =
        =====================
        */
        private void GameOver()
        {
            if(level != 21)
            {
                FadeUp();
                CacheDrawPic(DEATHPIC);
                PlaySound(NUKESND);
                FadeDown();
                Ack();
            }

            //
            // high score?
            //
            if(score > highscore)
            {
                PlaySound(HIGHSCORESND);
                ExpWin(18, 11);
                py += 3;
                CPPrint(_strings[Strings.GameOver1]); // as: string replacements
                py += 5;
                CPPrint(_strings[Strings.GameOver2]); // as: string replacements
                CPPrint(score.ToString());
                PPrint(_strings[Strings.GameOver3]); // as: string replacements
                CPPrint(_strings[Strings.GameOver4]); // as: string replacements
                CPPrint(highscore.ToString());
                PPrint(_strings[Strings.GameOver5]); // as: string replacements
                py += 5;
                CPPrint(_strings[Strings.GameOver6]); // as: string replacements
                CPPrint(_strings[Strings.GameOver7]); // as: string replacements
                Ack();
                highscore = score;
            }

        }

        //==========================================================================

        /*
        =====================
        =
        = Victory
        =
        =====================
        */
        private void Victory()
        {
            FadeOut();
            CacheDrawPic(ENDPIC);
            FadeIn();
            DrawWindow(0, 0, 39, 6);
            CPPrint(_strings[Strings.Victory1]); // as: string replacements
            CPPrint(_strings[Strings.Victory2]); // as: string replacements
            CPPrint(_strings[Strings.Victory3]); // as: string replacements
            CPPrint(_strings[Strings.Victory4]); // as: string replacements

            Ack();
            EraseWindow();

            CPPrint(_strings[Strings.Victory5]); // as: string replacements
            CPPrint(_strings[Strings.Victory6]); // as: string replacements
            CPPrint(_strings[Strings.Victory7]); // as: string replacements
            CPPrint(_strings[Strings.Victory8]); // as: string replacements

            Ack();
            EraseWindow();

            CPPrint(_strings[Strings.Victory9]); // as: string replacements
            CPPrint(_strings[Strings.Victory10]); // as: string replacements
            CPPrint(_strings[Strings.Victory11]); // as: string replacements
            CPPrint(_strings[Strings.Victory12]); // as: string replacements

            Ack();

            DrawWindow(10, 21, 30, 24);
            py += 3;
            CPPrint(_strings[Strings.Victory13]); // as: string replacements

            Ack();
        }

        //==========================================================================

        /*
        =====================
        =
        = BaseScreen
        =
        = Drop off hostages, get score, start new level
        =
        =====================
        */
        private void BaseScreen()
        {
#if TESTCASE
            level++;
            LoadLevel();
            StopDrive();
            return;
#endif

            CachePic(STARTPICS + MISSIONPIC);

            //
            // cash screen
            //
            if(level != startlevel) // send them straight into the first level
            {
#if !PROFILE
                WarpEffect();
#endif

                CachePic(STARTPICS + UFAPIC);
                DrawPic(0, 0, UFAPIC);
                if(killedcount >= savedcount)
                {
                    CachePic(STARTPICS + MADUFAPIC);
                    DrawPic(0, 0, MADUFAPIC);
                    MMSetPurge(ref grsegs[STARTPICS + MADUFAPIC], 3);
                }
                MMSetPurge(ref grsegs[STARTPICS + UFAPIC], 3);

                pxl = 176;
                pxh = 311;

                py = 10;
                CPPrint(_strings[Strings.BaseScreen1]); // as: string replacements
                py += 5;
                PPrint(_strings[Strings.BaseScreen2]); // as: string replacements
                PPrintInt(savedcount);
                PPrint(_strings[Strings.BaseScreen3]); // as: string replacements
                PPrintInt(killedcount);
                ushort topofs = screenofs;

                py += 5;
                PPrint(_strings[Strings.BaseScreen4]); // as: string replacements
                screenofs = 0; // draw into the split screen

                //
                // points for saving refugees
                //
                for(short i = 1; i <= savedcount; i++)
                {
                    DrawPic((ushort) (1 + 2 * (savedcount - i)), 6, EMPTYGUYPIC);
                    score += REFUGEEPOINTS;
                    PlaySound(GUYSCORESND);
                    DrawScore();
#if !PROFILE
                    if(NBKascii != 27)
                    {
                        WaitVBL(30);
                    }
#endif
                }

                screenofs = topofs;
                py += 5;
                PPrint(_strings[Strings.BaseScreen5]); // as: string replacements
                screenofs = 0; // draw into the split screen

                //
                // points for time remaining
                //
                while(timestruct.sec !=0 || timestruct.min != 0)
                {
                    score += TIMEPOINTS;

                    if(--timestruct.sec < 0)
                    {
                        timestruct.sec = 59;
                        if(--timestruct.min < 0)
                        {
                            timestruct.sec = timestruct.min = 0;
                        }

                        DrawPic(6, 48, (short) (DIGIT0PIC + timestruct.min));
                    }
                    DrawPic(9, 48, (short) (DIGIT0PIC + timestruct.sec / 10));
                    DrawPic(11, 48, (short) (DIGIT0PIC + timestruct.sec % 10));

                    if((timestruct.sec % 5) == 0)
                    {
                        PlaySound(TIMESCORESND);
                    }

                    DrawScore();
#if !PROFILE
                    if(NBKascii != 27)
                    {
                        WaitVBL(2);
                    }
#endif
                }

                if(objlist[0].hitpoints < 3)
                {
                    screenofs = topofs;
                    PPrint(_strings[Strings.BaseScreen6]); // as: string replacements
                    screenofs = 0; // draw into the split screen

                    //
                    // heal tank
                    //
                    while(objlist[0].hitpoints < 3 && score > 10000)
                    {
                        score -= 10000;
                        DrawScore();
                        HealPlayer();
#if !PROFILE
                        if(NBKascii != 27)
                        {
                            WaitVBL(60);
                        }
#endif
                        ColorBorder(0);
                        bordertime = 0;
                    }
                }

                screenofs = topofs;
                py = 110;

                if(level == NUMLEVELS)
                    CPPrint(_strings[Strings.BaseScreen7]); // as: string replacements
                else
                    CPPrint(_strings[Strings.BaseScreen8]); // as: string replacements

                StopDrive();

#if !PROFILE
                Ack();
#endif

                if(level == NUMLEVELS)
                {
                    Victory();
                    level++;
                    return;
                }
            }

            MMSetPurge(ref grsegs[STARTPICS + MISSIONPIC], 3);
            MMSortMem(); // push all purgable stuff high for good cache

            FadeOut();

            //
            // briefing screen
            //
            level++;
            LoadLevel();
            StopDrive();
            
            EGAWRITEMODE(0);
            _display.Clear();

            EGASplitScreen(200 - STATUSLINES);
            SetLineWidth(SCREENWIDTH);
            DrawCockpit();

            //
            // draw custom dash stuff
            //
            DrawPic(1, 48, (short) (DIGIT0PIC + level / 10));
            DrawPic(3, 48, (short) (DIGIT0PIC + level % 10));
            for(short i = 0; i < numrefugees; i++)
                DrawPic((ushort) (1 + 2 * i), 6, EMPTYGUYPIC);

            //
            // do mission briefing
            //

            screenofs = screenloc[0];
            SetScreen(screenofs, 0);
            DrawPic(0, 0, MISSIONPIC);

            pxl = 10;
            pxh = 310;
            
            py = 10;
            CPPrint(_strings[Strings.levnames(level - 1)]); // as: string replacements

            py = 37;
            px = pxl;

            PPrint(_strings[Strings.levtext(level - 1)]); // as: string replacements

            FadeIn();
            ClearKeys();

#if !PROFILE
            Ack();

            WarpEffect();
#endif
        }

        //==========================================================================

        /*
        ===================
        =
        = PlayLoop
        =
        ===================
        */
        private void PlayLoop()
        {
            do
            {
                c = ControlPlayer(1);

                screenofs = 0; // draw in split screen (radar, time, etc)

                for(short objIndex = 0; objIndex <= lastobjIndex; objIndex++)
                {
                    obj = objlist[objIndex];

                    if(obj._class != 0)
                    {
                        obj.CopyTo(obon);
                        obon.think();
                        obon.CopyTo(obj);
                    }
                }

                DropTime();

                if(keydown[0x57]) // DEBUG!
                {
                    DamagePlayer();
                    ClearKeys();
                }

                if(bordertime != 0 && (bordertime -= tics) <= 0)
                {
                    bordertime = 0;
                    ColorBorder(0);
                }

                FinishView(); // base drawn by player think

                CheckKeys();

            } while(leveldone == 0);
        }

        /*
        ===================
        =
        = PlayGame
        =
        ===================
        */

        private short[] levmin = new short[]
        {
            3, 3, 4, 4, 6, 
            5, 5, 5, 5, 5, 
            7, 7, 7, 7, 7, 
            9, 9, 9, 9, 9
        };

        // as: modified games
        private short[] levsec = new short[]
        {
            0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 
            0, 0, 0, 0, 0, 
            0, 0, 0, 0, 59
        };

        private void PlayGame()
        {
            level = startlevel = 0;

            if(bestlevel > 1)
            {
                ExpWin(28, 3);
                py += 6;
                PPrint(_strings[Strings.PlayGame1]); // as: string replacements
                PPrint(bestlevel.ToString());
                PPrint(_strings[Strings.PlayGame2]); // as: string replacements

                short i = (short) InputInt();
                if(i >= 1 && i <= bestlevel)
                {
                    startlevel = (short) (i - 1);
                    level = startlevel;
                }
            }

        restart:
            score = 0;
            resetgame = false;

            do
            {
                lastobjIndex = 0;

#if TESTCASE
	            level = 12;
#endif
                BaseScreen();

                if(level == 21)
                    break;

#if TESTCASE
	            objlist[0].x = 3126021;
	            objlist[0].y = 522173;
	            objlist[0].angle = 170;
#endif

                savedcount = killedcount = 0;

                timestruct.min = levmin[level - 1];
                timestruct.sec = levsec[level - 1]; // as: modified games

                screenofs = 0;
                DrawPic(6, 48, (short) (DIGIT0PIC + timestruct.min));
                DrawPic(9, 48, (short) (DIGIT0PIC + timestruct.sec / 10));
                DrawPic(11, 48, (short) (DIGIT0PIC + timestruct.sec % 10));

                lasttimecount = (int) timecount;
                tics = 1;
                leveldone = 0;

                if(level > bestlevel)
                    bestlevel = level;

                PlayLoop();

                screenofs = 0;
                for(short objIndex = 1; objIndex < lastobjIndex; objIndex++)
                {
                    obj = objlist[objIndex];
                    if(obj._class != 0 && obj.radarx != 0)
                        XPlot(obj.radarx, obj.radary, obj.radarcolor);
                }

                if(bordertime != 0)
                {
                    bordertime = 0;
                    ColorBorder(0);
                }
            }
            while(leveldone > 0);

            if(resetgame)
                return;

            GameOver();

            //
            // continue
            //
            if(level > 2 && level < 21)
            {
                DrawWindow(10, 20, 30, 23);
                py += 3;
                CPPrint(_strings[Strings.PlayGame3]); // as: string replacements
                ClearKeys();

                ch = (sbyte) PGet();
                if(_sys.toupper(ch) == 'Y')
                {
                    level--;
                    startlevel = level; // don't show base screen
                    goto restart;
                }
            }
        }
    }

}
