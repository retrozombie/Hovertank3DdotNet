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

using fixed_t = System.Int32;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        private const short GUNARM = 50;

        private const short GUNCHARGE = 70;

        private const short EXPLODEANM = 8;

        private const fixed_t SHOTSPEED	= 8192;

        private const fixed_t MSHOTSPEED = 6000;

        private const short REFUGEEANM = 30;

        private const short WARPANM = 10;

        private const fixed_t SPDMUTANT = 2500;

        private const short MUTANTANM = 20;

        private const short MUTANTATTACK = 120;

        private const ushort DRONEANM = 10;

        private const short TANKRELOAD = 200;

        private const short ANGLESTEP = 1;

        private const fixed_t PLAYERTHRUST = 4096;

        private const fixed_t PLAYERREVERSE = 2048;

        private const fixed_t PLAYERAFTERBURN = 8192;

        private const short SHIELDANM = 10;

        private const fixed_t MINCHASE = 4096;

        private static dirtype[] opposite = new dirtype[] 
        {
            dirtype.south, dirtype.southwest, dirtype.west, dirtype.northwest,
            dirtype.north, dirtype.northeast, dirtype.east, dirtype.southeast,
            dirtype.nodir
        };

        //==========================================================================

        private gunstates gunstate;

        private short guncount;

        /*
        =============================================================================

			           PLAYER

        =============================================================================
        */

        /*
        ===================
        =
        = SpawnPlayer
        =
        ===================
        */
        private void SpawnPlayer(fixed_t gx, fixed_t gy)
        {
            objlist[0].x = gx;
            objlist[0].y = gy;
            objlist[0].angle = 0;
            objlist[0].think = PlayerThink;
            objlist[0]._class = classtype.playerobj;
            objlist[0].size = MINDIST;
            objlist[0].radarcolor = 15;
            objlist[0].hitpoints = 3;

            objlist[0].xl = objlist[0].x - objlist[0].size;
            objlist[0].xh = objlist[0].x + objlist[0].size;
            objlist[0].yl = objlist[0].y - objlist[0].size;
            objlist[0].yh = objlist[0].y + objlist[0].size;

            gunstate = gunstates.ready;
        }

        /*
        ===================
        =
        = GetRefugee
        =
        ===================
        */
        private void GetRefugee(objtype hit)
        {
            // as: Support for extra sound effects
            if(hit.temp1 == MAN1PIC)
                PlaySound(SAVEHOSTAGESND);
            else
                PlaySound(SNDEX_SAVHOSTAGE2); // SAVEHOSTAGESND

            hit._class = classtype.nothing;
            
            if(hit.radarx != 0)
                XPlot(hit.radarx, hit.radary, hit.radarcolor);

            DrawPic((ushort) (2 * savedcount + 1), 6, SAVEDGUYPIC);

            savedcount++;
            if(--numrefugees == 0)
            {
                // as: Support for extra sound effects
                if(hit.temp1 == MAN1PIC)
                    PlaySound(LASTHOSTAGESND);
                else
                    PlaySound(SNDEX_LSTHOSTAGE2); // LASTHOSTAGESND

                SpawnWarp(warpx, warpy);
            }
        }

        /*
        ===================
        =
        = CheckFire
        =
        ===================
        */
        private void CheckFire()
        {
            if(gunstate == gunstates.rearming)
            {
                if((guncount += tics) > GUNARM)
                {
                    gunstate = gunstates.ready;
                    PlaySound(GUNREADYSND);
                    DrawPic(14, 40, READYPIC);
                }
                return;
            }

            if(c.button1)
            {
                // holding down button
                if(gunstate == gunstates.ready)
                {
                    gunstate = gunstates.charging;
                    DrawPic(14, 40, CHARGINGPIC);
                    guncount = 0;
                }
                if(gunstate == gunstates.charging && (guncount += tics) > GUNCHARGE)
                {
                    PlaySound(MAXPOWERSND);
                    gunstate = gunstates.maxpower;
                    DrawPic(14, 40, MAXPOWERPIC);
                }
            }
            else
                // button up
                if(gunstate > gunstates.ready) // fire small shot if charging, large if maxed
                {
                    DrawPic(14, 40, REARMINGPIC);
                    guncount = 0;

                    classtype shotType = (classtype) (classtype.pshotobj + (short) (gunstate - gunstates.charging));
                    SpawnShot(obon.x, obon.y, obon.angle, shotType);

                    gunstate = gunstates.rearming;

                    // as: Support for extra sound effects
                    if(shotType == classtype.pshotobj)
                        PlaySound(FIRESND);
                    else
                        PlaySound(SNDEX_FIRE2);
                }
        }

        /*
        ===================
        =
        = DamagePlayer
        =
        ===================
        */
        private void DamagePlayer(ushort sound)
        {
            // as: Support for extra sound effects
            PlaySound(sound); // TAKEDAMAGESND

            if(godmode == 0 && --objlist[0].hitpoints == 0)
            {
                PlaySound(PLAYERDEADSND);
#if NOT_USING_STATES // as: Prevent infinite loop upon death
                while(SoundPlaying())
                {
                }
#endif
                leveldone = -1; // all hit points gone
            }
            else
            {
                DrawPic(24, 36, (short) (SHIELDLOWPIC + 1 - objlist[0].hitpoints));
            }

            ColorBorder(12);
            bordertime = 60;
        }

        private void HealPlayer(ushort sound)
        {
            // as: Support for extra sound effects
            PlaySound(sound); // ARMORUPSND

            if(objlist[0].hitpoints < 3)
                objlist[0].hitpoints++;

            DrawPic(24, 36, (short) (SHIELDLOWPIC + 1 - objlist[0].hitpoints));

            ColorBorder(9);
            bordertime = 60;
        }

        /*
        ===================
        =
        = Thrust
        =
        ===================
        */
        private void Thrust()
        {
            xmove = FixedByFrac(PLAYERTHRUST * tics, costable[obon.angle]);
            ymove = FixedByFrac(PLAYERTHRUST * tics, sintable[obon.angle]) ^ SIGNBIT;
            ClipMove();
        }

        /*
        ===================
        =
        = Reverse
        =
        ===================
        */
        private void Reverse()
        {
            xmove = FixedByFrac(PLAYERREVERSE * tics, costable[obon.angle]) ^ SIGNBIT;
            ymove = FixedByFrac(PLAYERREVERSE * tics, sintable[obon.angle]);
            ClipMove();
        }

        /*
        ===================
        =
        = AfterBurn
        =
        ===================
        */
        private void AfterBurn()
        {
            xmove = FixedByFrac(PLAYERAFTERBURN * tics, costable[obon.angle]);
            ymove = FixedByFrac(PLAYERAFTERBURN * tics, sintable[obon.angle]) ^ SIGNBIT;
            ClipMove();

            if(!SoundPlaying())
                PlaySound(AFTERBURNSND);
        }

        private void BeforeBurn()
        {
            xmove = FixedByFrac(PLAYERAFTERBURN * tics, costable[obon.angle] ^ SIGNBIT);
            ymove = FixedByFrac(PLAYERAFTERBURN * tics, sintable[obon.angle]);
            ClipMove();

            if(!SoundPlaying())
                PlaySound(AFTERBURNSND);
        }

        /*
        ===================
        =
        = PlayerThink
        =
        ===================
        */
        private void PlayerThink()
        {
            short anglechange;
            if(c.button1) // hold down fire for slow adjust
            {
                if(tics <= 4)
                {
                    anglechange = (short) (ANGLESTEP * tics / 2);

                    if(anglechange == 0)
                        anglechange = 1;
                }
                else
                    anglechange = ANGLESTEP * 2;
            }
            else
            {
                anglechange = (short) (ANGLESTEP * tics);
            }

            if(c.dir == dirtype.west || c.dir == dirtype.northwest || 
                c.dir == dirtype.southwest)
            {
                obon.angle += anglechange;

                if(obon.angle >= ANGLES)
                    obon.angle -= ANGLES;
            }
            else if(c.dir == dirtype.east || c.dir == dirtype.northeast || 
                c.dir == dirtype.southeast)
            {
                obon.angle -= anglechange;

                if(obon.angle < 0)
                    obon.angle += ANGLES;
            }

            if(c.button2 && (c.dir == dirtype.south || c.dir == dirtype.southeast ||
                c.dir == dirtype.southwest))
            {
                BeforeBurn();
            }
            else if(c.button2)
            {
                AfterBurn();
            }
            else if(c.dir == dirtype.north || c.dir == dirtype.northeast ||
                c.dir == dirtype.northwest)
            {
                Thrust();
            }
            else if(c.dir == dirtype.south || c.dir == dirtype.southeast ||
                c.dir == dirtype.southwest)
            {
                Reverse();
            }

            CheckFire();

            for(short checkIndex = 1; checkIndex <= lastobjIndex; checkIndex++)
            {
                objtype check = objlist[checkIndex];
                
                if(check._class != 0 && check.xl <= obon.xh && check.xh >= obon.xl && 
                    check.yl <= obon.yh && check.yh >= obon.yl)
                {
                    switch(check._class)
                    {
                        case classtype.refugeeobj:
                            GetRefugee(check);
                            break;

                        case classtype.shieldobj:
                            obon.CopyTo(objlist[0]);

                            // as: Support for extra sound effects
                            HealPlayer(SNDEX_SHIELDUP); // ARMORUPSND

                            objlist[0].CopyTo(obon);

                            check._class = classtype.nothing;

                            if(check.radarx != 0)
                                XPlot(check.radarx, check.radary, check.radarcolor);

                            break;

                        case classtype.warpobj:
                            leveldone = 1;
                            break;
                    }
                }
            }

            try
            {
                StartView(); // calculate view position and trace walls
                // FinishView in PlayLoop draws everything
            }
            catch(Exception ex)
            {
                // as: Display failure info for debugging

                // Display view position
                _sys.Log("x = " + obon.x);
                _sys.Log("y = " + obon.y);
                _sys.Log("angle = " + obon.angle);
                _sys.Log("level = " + level);
                _sys.Log(ex);

                throw ex;
            }
        }

        /*
        =============================================================================

			           SHOTS

        =============================================================================
        */

        /*
        ====================
        =
        = SpawnShot
        =
        ====================
        */
        private void SpawnShot(fixed_t gx, fixed_t gy, short angle, classtype _class)
        {
            FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = angle;
            _new.think = ShotThink;
            _new._class = _class;
            _new.size = TILEGLOBAL / 8;

            switch(_class)
            {
                case classtype.pshotobj:
                    _new.shapenum = PSHOTPIC;
                    _new.speed = SHOTSPEED;
                    break;

                case classtype.pbigshotobj:
                    _new.shapenum = BIGSHOTPIC;
                    _new.speed = SHOTSPEED;
                    break;

                case classtype.mshotobj:
                    _new.shapenum = MSHOTPIC;
                    _new.speed = MSHOTSPEED;
                    break;
            }

            CalcBoundsNew();
        }

        /*
        ===================
        =
        = ExplodeShot
        =
        ===================
        */
        private void ExplodeShot()
        {
            // as: Support for extra sound effects
            ushort sound = SNDEX_TSHOTWALL;
            if(obon._class == classtype.pshotobj)
                sound = SNDEX_PSHOTWALL;
            else if(obon._class == classtype.pbigshotobj)
                sound = SNDEX_PSHOTWALL2;

            PlaySound(sound); // SHOOTWALLSND

            obon._class = classtype.inertobj;
            obon.shapenum = obon.temp1 = SHOTDIE1PIC;
            obon.think = ExplodeThink;
            obon.stage = 0;
            TransformObon();
        }

        private const fixed_t SHOTCLIP = 0x1000; // explode 1/16th tile from wall

        /*
        ====================
        =
        = ClipPointMove
        =
        ====================
        */
        private bool ClipPointMove()
        {
            xmove = FixedByFrac(obon.speed * tics, costable[obon.angle]);

            if(xmove < 0)
                xmove = -(xmove ^ SIGNBIT);

            ymove = FixedByFrac(obon.speed * tics, sintable[obon.angle]) ^ SIGNBIT;

            if(ymove < 0)
                ymove = -(ymove ^ SIGNBIT);

            obon.x += xmove;
            obon.y += ymove;

            short xt = (short) (obon.x >> TILESHIFT);
            short yt = (short) (obon.y >> TILESHIFT);

            if(tilemap[xt, yt] == 0)
                return false;

            //
            // intersect the path with the tile edges to determine point of impact
            //
            int basex = obon.x & -65536; // 0xffff0000
            int basey = obon.y & -65536; // 0xffff0000

            obon.x &= 0xffff; // move origin to ul corner of tile
            obon.y &= 0xffff;

            ushort inside, total;
            int intersect;
            if(xmove > 0)
            {
                inside = (ushort) obon.x;
                total = (ushort) xmove;
                intersect = obon.y - ymove * inside / total;
                if(intersect <= TILEGLOBAL)
                {
                    obon.x = basex - SHOTCLIP;
                    obon.y = basey + intersect;
                    return true;
                }
            }
            else if(xmove < 0)
            {
                inside = (ushort) (TILEGLOBAL - obon.x);
                total = (ushort) (-xmove);
                intersect = obon.y - ymove * inside / total;
                if(intersect <= TILEGLOBAL)
                {
                    obon.x = basex + TILEGLOBAL + SHOTCLIP;
                    obon.y = basey + intersect;
                    return true;
                }
            }

            if(ymove > 0)
            {
                inside = (ushort) obon.y;
                total = (ushort) ymove;
                intersect = obon.x - xmove * inside / total;
                if(intersect <= TILEGLOBAL)
                {
                    obon.x = basex + intersect;
                    obon.y = basey - SHOTCLIP;
                    return true;
                }
            }
            else if(ymove < 0)
            {
                inside = (ushort) (TILEGLOBAL - obon.y);
                total = (ushort) (-ymove);
                intersect = obon.x - xmove * inside / total;
                if(intersect <= TILEGLOBAL)
                {
                    obon.x = basex + intersect;
                    obon.y = basey + TILEGLOBAL + SHOTCLIP;
                    return true;
                }
            }

            return true;
        }

        /*
        ===================
        =
        = ShotThink
        =
        ===================
        */
        private void ShotThink()
        {
            for(short i = 0; i < 3; i++) // so it can move over one tile distance
            {
                if(ClipPointMove())
                {
                    ExplodeShot();
                    return;
                }

                CalcBounds();

                for(short checkIndex = 0; checkIndex <= lastobjIndex; checkIndex++)
                {
                    objtype check = objlist[checkIndex];

                    if(check._class != 0 && check.xl <= obon.xh && check.xh >= obon.xl && 
                        check.yl <= obon.yh && check.yh >= obon.yl)
                    {
                        switch(check._class)
                        {
                            case classtype.playerobj:
                                if(obon._class == classtype.mshotobj)
                                {
                                    // as: Support for extra sound effects
                                    DamagePlayer(SNDEX_TANKDAMAGE);
                                    obon._class = classtype.nothing;
                                    return;
                                }
                                break;

                            case classtype.refugeeobj:
                                // as: Support for extra sound effects
                                KillRefugee(check, obon._class != classtype.mshotobj);

                                if(obon._class == classtype.pbigshotobj)
                                    break;

                                obon._class = classtype.nothing;
                                return;

                            case classtype.mutantobj:
                                KillMutant(check);

                                if(obon._class == classtype.pbigshotobj)
                                    break;

                                obon._class = classtype.nothing;
                                return;

                            case classtype.tankobj:
                                if(obon._class != classtype.mshotobj)
                                {
                                    KillTank(check);

                                    if(obon._class == classtype.pbigshotobj)
                                        break;

                                    obon._class = classtype.nothing;
                                    return;
                                }
                                break;

                            case classtype.droneobj:
                                KillDrone(check);

                                if(obon._class == classtype.pbigshotobj)
                                    break;

                                obon._class = classtype.nothing;
                                return;
                        }
                    }
                }
            }

            TransformObon();
        }

        /*
        ==================
        =
        = InertThink
        =
        = Corpses, etc...
        =
        ==================
        */
        private void InertThink()
        {
            TransformObon();
        }
        
        /*
        ===================
        =
        = ExplodeThink
        =
        ===================
        */
        private void ExplodeThink()
        {
            obon.ticcount += tics;
            if(obon.ticcount > EXPLODEANM)
            {
                obon.ticcount -= EXPLODEANM;
                if(++obon.stage == 5)
                {
                    if(obon.temp1 == SHOTDIE1PIC) // shopt explosions go away
                        obon._class = classtype.nothing;
                    else
                        obon.think = InertThink;

                    return;
                }
                obon.shapenum = (short) (obon.temp1 + obon.stage);
            }

            TransformObon();
        }

        /*
        =============================================================================

			           WARP GATE

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnWarp
        =
        ==================
        */
        private void SpawnWarp(fixed_t gx, fixed_t gy)
        {
            FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = WarpThink;
            _new._class = classtype.warpobj;
            _new.size = MINDIST;
            _new.radarcolor = 14;
            _new.shapenum = WARP1PIC;
            CalcBoundsNew();
        }

        /*
        ===================
        =
        = WarpThink
        =
        ===================
        */
        private void WarpThink()
        {
            obon.ticcount += tics;
            if(obon.ticcount > WARPANM)
            {
                obon.ticcount -= WARPANM;

                if(++obon.stage == 4)
                    obon.stage = 0;

                obon.shapenum = (short) (WARP1PIC + obon.stage);
            }

            TransformObon();
        }

        /*
        =============================================================================

			           REFUGEE

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnRefugee
        =
        = obon.temp2 is true if a drone is seeking it
        =
        ==================
        */
        private void SpawnRefugee(fixed_t gx, fixed_t gy, bool sex)
        {
            numrefugees++;

            FindFreeObj();

            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = RefugeeThink;
            _new._class = classtype.refugeeobj;
            _new.size = TILEGLOBAL / 3;
            _new.radarcolor = 15;

            if(sex)
            {
                _new.shapenum = MAN1PIC;
                _new.temp1 = MAN1PIC;
            }
            else
            {
                _new.shapenum = WOMAN1PIC;
                _new.temp1 = WOMAN1PIC;
            }

            _new.temp2 = 0;
            _new.ticcount = (short) Rnd((ushort) (REFUGEEANM * 3));

            CalcBoundsNew();
        }

        /*
        ===================
        =
        = KillRefugee
        =
        ===================
        */
        private void KillRefugee(objtype hit, bool player)
        {
            // as: Support for extra sound effects
            ushort sound = HOSTAGEDEADSND;
            if(player)
            {
                if(hit.temp1 == MAN1PIC)
                    sound = SNDEX_HSTAGEDEAD3;
                else
                    sound = SNDEX_HSTAGEDEAD4;
            }
            else
            {
                if(hit.temp1 != MAN1PIC)
                    sound = SNDEX_HSTAGEDEAD2;
            }
            PlaySound(sound); // HOSTAGEDEADSND

            if(hit.radarx != 0)
                XPlot(hit.radarx, hit.radary, hit.radarcolor);

            killedcount++;

            DrawPic((ushort) (2 * (totalrefugees - killedcount) + 1), 6, DEADGUYPIC);
            if(--numrefugees == 0)
            {
                // as: Support for extra sound effects
                if(player)
                {
                    if(hit.temp1 == MAN1PIC)
                        sound = SNDEX_LASTDEAD3;
                    else
                        sound = SNDEX_LASTDEAD4;
                }
                else
                {
                    if(hit.temp1 == MAN1PIC)
                        sound = SNDEX_LASTDEAD1;
                    else
                        sound = SNDEX_LASTDEAD2;
                }
                PlaySound(sound); // WARPGATESND

                SpawnWarp(warpx, warpy);
            }

            hit.radarcolor = 0;
            hit._class = classtype.inertobj;

            if(hit.temp1 == MAN1PIC)
            {
                hit.shapenum = hit.temp1 = MANDIE1PIC;
            }
            else
            {
                hit.shapenum = hit.temp1 = WOMANDIE1PIC;
            }

            hit.think = ExplodeThink;
            hit.stage = hit.ticcount = 0;
        }

        /*
        ===================
        =
        = RefugeeThink
        =
        ===================
        */
        private void RefugeeThink()
        {
            obon.ticcount += tics;
            if(obon.ticcount > REFUGEEANM)
            {
                obon.ticcount -= REFUGEEANM;

                if(++obon.stage == 2)
                    obon.stage = 0;

                obon.shapenum = (short) (obon.temp1 + obon.stage);
            }

            TransformObon();
        }

        /*
        =============================================================================

			             DRONE

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnDrone
        =
        = obon.temp1 is a pointer to the refugee the drone is currently seeking
        =
        ==================
        */
        private void SpawnDrone(fixed_t gx, fixed_t gy)
        {
            // as: Enemy stats
            totalEnemies++;

            short newIndex = FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = DroneThink;
            _new._class = classtype.droneobj;
            _new.size = MINDIST;
            _new.radarcolor = 10;
            _new.hitpoints = 2;
            _new.shapenum = DRONE1PIC;
            _new.ticcount = (short) Rnd(DRONEANM * 3);
            _new.temp1 = newIndex; // will hunt first think
            CalcBoundsNew();
        }

        /*
        ===================
        =
        = KillDrone
        =
        ===================
        */
        private void KillDrone(objtype hit)
        {
            // as: Enemy stats
            enemiesKilled++;

            // as: Support for extra sound effects
            PlaySound(SNDEX_DRONEDIE); // SHOOTTHINGSND

            if(hit.radarx != 0)
                XPlot(hit.radarx, hit.radary, hit.radarcolor);

            hit.radarcolor = 0;
            hit._class = classtype.inertobj;
            hit.shapenum = hit.temp1 = DRONEDIE1PIC;
            hit.think = ExplodeThink;
            hit.stage = hit.ticcount = 0;
        }

        /*
        ==================
        =
        = DroneLockOn
        =
        ==================
        */
        private void DroneLockOn()
        {
            for(short checkIndex = 2; checkIndex < lastobjIndex; checkIndex++)
            {
                objtype check = objlist[checkIndex];
                if(check._class == classtype.refugeeobj && check.temp2 == 0)
                {
                    check.temp2++;
                    obon.temp1 = checkIndex;
                    return;
                }
            }

            obon.temp1 = 0; // go after player last
        }

        /*
        ===================
        =
        = DroneThink
        =
        ===================
        */
        private void DroneThink()
        {
            // as: temp1 was objtype pointer for drone's target, now it is the objtype's index
            objtype obj = objlist[obon.temp1];
            if(obj._class != classtype.refugeeobj && obj._class != classtype.playerobj)
                DroneLockOn(); // target died

            obon.ticcount += tics;
            if(obon.ticcount > DRONEANM)
            {
                obon.ticcount -= (short) DRONEANM;

                if(++obon.stage == 4)
                    obon.stage = 0;

                obon.shapenum = (short) (DRONE1PIC + obon.stage);
            }

            ChaseThing(objlist[obon.temp1]);

            CalcBounds();

            TransformObon();

            for(short checkIndex = 0; checkIndex <= lastobjIndex; checkIndex++)
            {
                check = objlist[checkIndex];

                if(check._class != 0 && 
                    check.xl <= obon.xh && check.xh >= obon.xl && 
                    check.yl <= obon.yh && check.yh >= obon.yl)
                {
                    switch(check._class)
                    {
                        case classtype.playerobj: // kill player and blow up
                            // as: Support for extra sound effects
                            DamagePlayer(SNDEX_DRONEDAMAGE);

                            // as: Support for extra sound effects
                            PlaySound(SNDEX_DRONEDIE); // SHOOTTHINGSND

                            if(obon.radarx != 0)
                                XPlot(obon.radarx, obon.radary, obon.radarcolor);

                            // as: Enemy stats
                            enemiesKilled++;

                            obon.radarcolor = 0;
                            obon._class = classtype.inertobj;
                            obon.shapenum = obon.temp1 = DRONEDIE1PIC;
                            obon.think = ExplodeThink;
                            obon.stage = obon.ticcount = 0;
                            return;

                        case classtype.refugeeobj:
                            // as: Support for extra sound effects
                            KillRefugee(check, false);
                            break;
                    }
                }
            }
        }

        /*
        =============================================================================

			              TANK

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnTank
        =
        ==================
        */
        private void SpawnTank(fixed_t gx, fixed_t gy)
        {
            // as: Enemy stats
            totalEnemies++;

            FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = TankThink;
            _new._class = classtype.tankobj;
            _new.size = MINDIST;
            _new.shapenum = TANK1PIC;
            _new.radarcolor = 13;
            _new.hitpoints = 3;
            CalcBoundsNew();
        }

        /*
        ===================
        =
        = KillTank
        =
        ===================
        */
        private void KillTank(objtype hit)
        {
            // as: Enemy stats
            enemiesKilled++;

            // as: Support for extra sound effects
            PlaySound(SNDEX_TANKDIE); // SHOOTTHINGSND
            
            if(hit.radarx != 0)
                XPlot(hit.radarx, hit.radary, hit.radarcolor);

            hit.radarcolor = 0;
            hit._class = classtype.inertobj;
            hit.shapenum = hit.temp1 = TANKDIE1PIC;
            hit.think = ExplodeThink;
            hit.stage = hit.ticcount = 0;
        }

        /*
        ======================
        =
        = AimAtPlayer
        =
        = Hunt for player
        =
        ======================
        */
        private void AimAtPlayer()
        {
            dirtype olddir = obon.dir;
            dirtype turnaround = opposite[(short) olddir];

            int deltax = objlist[0].x - obon.x;
            int deltay = objlist[0].y - obon.y;

            // as: d[3] changed to d1, d2
            dirtype d1 = dirtype.nodir;
            dirtype d2 = dirtype.nodir;

            if(deltax > MINCHASE)
                d1 = dirtype.east;
            else if(deltax < -MINCHASE)
                d1 = dirtype.west;

            if(deltay > MINCHASE)
                d2 = dirtype.south;
            else if(deltay < -MINCHASE)
                d2 = dirtype.north;

            dirtype tdir;
            if(LABS(deltay) < LABS(deltax))
            {
                tdir = d1;
                d1 = d2;
                d2 = tdir;
            }

            if(d1 == turnaround)
                d1 = dirtype.nodir;

            if(d2 == turnaround)
                d2 = dirtype.nodir;

            //
            // shoot at player if even aim and not reloading
            //
            if(d1 == dirtype.nodir && obon.stage == 0)
            {
                short xstep, ystep;
                
                xstep = ystep = 0;

                short steps = 0; // as: Added assignment
                bool stepsAssigned = false; // as: Check for uninitialised variable
                if(deltax > MINCHASE)
                {
                    xstep = 1;
                    steps = (short) (((objlist[0].x - obon.x) >> TILESHIFT) - 1);
                    obon.angle = 0;
                    stepsAssigned = true;
                }
                else if(deltax < -MINCHASE)
                {
                    xstep = -1;
                    steps = (short) (((obon.x - objlist[0].x) >> TILESHIFT) - 1);
                    obon.angle = 180;
                    stepsAssigned = true;
                }

                if(deltay > MINCHASE)
                {
                    ystep = 1;
                    steps = (short) (((objlist[0].y - obon.y) >> TILESHIFT) - 1);
                    obon.angle = 270;
                    stepsAssigned = true;
                }
                else if(deltay < -MINCHASE)
                {
                    ystep = -1;
                    steps = (short) (((obon.y - objlist[0].y) >> TILESHIFT) - 1);
                    obon.angle = 90;
                    stepsAssigned = true;
                }

                if(!stepsAssigned)
                {   // as: this can happen, assume the shot is blocked
                    _sys.Log("AimAtPlayer: steps not assigned!");
                    goto cantshoot;
                }

                short tx = (short) (obon.x >> TILESHIFT);
                short ty = (short) (obon.y >> TILESHIFT);

                for(short i = 0; i < steps; i++)
                {
                    tx += xstep;
                    ty += ystep;

                    if(tilemap[tx, ty] != 0)
                        goto cantshoot; // shot is blocked
                }

                // as: Support for extra sound effects
                PlaySound(SNDEX_TANKFIRE); // FIRESND
                SpawnShot(obon.x, obon.y, obon.angle, classtype.mshotobj);
                obon.ticcount = 0;
                obon.stage = 1;
            }

            if(d1 != dirtype.nodir)
            {
                obon.dir = d1;
                if(Walk())
                    return;
            }

        cantshoot:
            if(d2 != dirtype.nodir)
            {
                obon.dir = d2;
                if(Walk())
                    return;
            }

            // there is no direct path to the player, so pick another direction
            obon.dir = olddir;

            if(Walk())
                return;

            if(RndT() > 128) // randomly determine direction of search
            {
                for(tdir = dirtype.north; tdir <= dirtype.west; tdir += 2)
                {
                    if(tdir != turnaround)
                    {
                        obon.dir = tdir;
                 
                        if(Walk())
                            return;
                    }
                }
            }
            else
            {
                for(tdir = dirtype.west; tdir >= dirtype.north; tdir -= 2)
                {
                    if(tdir != turnaround)
                    {
                        obon.dir = tdir;
                        if(Walk())
                            return;
                    }
                }
            }

            obon.dir = turnaround;

            Walk(); // last chance, don't worry about returned value
        }

        /*
        ===================
        =
        = TankThink
        =
        ===================
        */
        private void TankThink()
        {
            if(obon.stage == 1) // just fired?
            {
                if((obon.ticcount += tics) >= TANKRELOAD)
                    obon.stage = 0;
            }

            AimAtPlayer();
            TransformObon();
        }

        /*
        =============================================================================

			                MUTANT

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnMutant
        =
        ==================
        */
        private void SpawnMutant(fixed_t gx, fixed_t gy)
        {
            // as: Enemy stats
            totalEnemies++;

            FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = MutantThink;
            _new._class = classtype.mutantobj;
            _new.size = MINDIST;
            _new.shapenum = MUTANT1PIC;
            _new.radarcolor = 12;
            _new.hitpoints = 1;
            _new.ticcount = (short) Rnd(MUTANTANM * 3);
            CalcBoundsNew();
        }

        /*
        ===================
        =
        = KillMutant
        =
        ===================
        */
        private void KillMutant(objtype hit)
        {
            // as: Enemy stats
            enemiesKilled++;

            // as: Support for extra sound effects
            PlaySound(SNDEX_MUTEDIE); // SHOOTTHINGSND

            if(hit.radarx != 0)
                XPlot(hit.radarx, hit.radary, hit.radarcolor);

            hit.radarcolor = 0;
            hit._class = classtype.inertobj;
            hit.shapenum = hit.temp1 = MUTANTDIE1PIC;
            hit.think = ExplodeThink;
            hit.stage = hit.ticcount = 0;
        }

        private const fixed_t WALLZONE = (2 * TILEGLOBAL / 3);

        /*
        ======================
        =
        = Walk
        =
        = Returns true if a movement of obon.dir/obon.speed is ok or causes
        = an attack at player
        =
        ======================
        */
        private bool Walk()
        {
            short xmove, ymove;
            switch(obon.dir)
            {
                case dirtype.north:
                    xmove = 0;
                    ymove = -SPDMUTANT;
                    break;

                case dirtype.east:
                    xmove = SPDMUTANT;
                    ymove = 0;
                    break;

                case dirtype.south:
                    xmove = 0;
                    ymove = SPDMUTANT;
                    break;

                case dirtype.west:
                    xmove = -SPDMUTANT;
                    ymove = 0;
                    break;

                default:
                    Quit("Walk: Bad dir!");
                    // as: Quit throws an exception so the following is unreachable
                    throw new Exception();
            }

            obon.x += xmove;
            obon.y += ymove;

            //
            // calculate a hit rect to stay away from walls in
            //
            obon.xl = obon.x - WALLZONE;
            obon.xh = obon.x + WALLZONE;
            obon.yl = obon.y - WALLZONE;
            obon.yh = obon.y + WALLZONE;

            //
            // tile coordinate edges
            //
            short xt = (short) (obon.x >> TILESHIFT);
            short yt = (short) (obon.y >> TILESHIFT);

            short xl = (short) (obon.xl >> TILESHIFT);
            short yl = (short) (obon.yl >> TILESHIFT);

            short xh = (short) (obon.xh >> TILESHIFT);
            short yh = (short) (obon.yh >> TILESHIFT);

            //
            // check corners
            //
            if(tilemap[xl, yl] != 0 || tilemap[xh, yl] != 0 ||
                tilemap[xl, yh] != 0 || tilemap[xh, yh] != 0 ||
                tilemap[xt, yh] != 0 || tilemap[xt, yl] != 0 ||
                tilemap[xl, yt] != 0 || tilemap[xh, yt] != 0)
            {
                obon.x -= xmove;
                obon.y -= ymove;
                return false;
            }

            //
            // check contact with player
            //
            return true;
        }

        /*
        ======================
        =
        = ChaseThing
        =
        = Hunt for player
        =
        ======================
        */
        private void ChaseThing(objtype chase)
        {
            dirtype olddir = obon.dir;
            dirtype turnaround = opposite[(short) olddir];

            int deltax = chase.x - obon.x;
            int deltay = chase.y - obon.y;

            // as: d[3] changed to d1, d2
            dirtype d1 = dirtype.nodir;
            dirtype d2 = dirtype.nodir;

            if(deltax > MINCHASE)
                d1 = dirtype.east;
            else if(deltax < -MINCHASE)
                d1 = dirtype.west;

            if(deltay > MINCHASE)
                d2 = dirtype.south;
            else if(deltay < -MINCHASE)
                d2 = dirtype.north;

            dirtype tdir;
            if(LABS(deltay) > LABS(deltax))
            {
                tdir = d1;
                d1 = d2;
                d2 = tdir;
            }

            if(d1 == turnaround)
                d1 = dirtype.nodir;

            if(d2 == turnaround)
                d2 = dirtype.nodir;

            if(d1 != dirtype.nodir)
            {
                obon.dir = d1;
                if(Walk())
                {
                    if(d2 != dirtype.nodir)
                    {
                        obon.dir = d2;
                        Walk(); // try to go diagonal if possible
                    }
                    return;
                }
            }

            if(d2 != dirtype.nodir)
            {
                obon.dir = d2;

                if(Walk())
                    return;
            }

            // there is no direct path to the player, so pick another direction

            obon.dir = olddir;

            if(Walk())
                return;

            if(RndT() > 128) // randomly determine direction of search
            {
                for(tdir = dirtype.north; tdir <= dirtype.west; tdir += 2)
                {
                    if(tdir != turnaround)
                    {
                        obon.dir = tdir;
                 
                        if(Walk())
                            return;
                    }
                }
            }
            else
            {
                for(tdir = dirtype.west; tdir >= dirtype.north; tdir -= 2)
                {
                    if(tdir != turnaround)
                    {
                        obon.dir = tdir;
                        
                        if(Walk())
                            return;
                    }
                }
            }

            obon.dir = turnaround;

            Walk(); // last chance, don't worry about returned value
        }

        private const fixed_t ATTACKZONE = (TILEGLOBAL);

        /*
        ===================
        =
        = MutantThink
        =
        ===================
        */
        private void MutantThink()
        {
            obon.ticcount += tics;

            if(obon.stage == 4) // attack stage
            {
                if(obon.ticcount < MUTANTATTACK)
                {
                    TransformObon();
                    return;
                }

                obon.ticcount = (short) (MUTANTANM + 1 - tics);
                obon.stage = 0;
            }

            if(obon.ticcount > MUTANTANM)
            {
                obon.ticcount -= MUTANTANM;

                if(++obon.stage == 4)
                    obon.stage = 0;

                obon.shapenum = (short) (MUTANT1PIC + obon.stage);
            }

            if(objlist[0].xl <= obon.x + ATTACKZONE &&
                objlist[0].xh >= obon.x - ATTACKZONE &&
                objlist[0].yl <= obon.y + ATTACKZONE &&
                objlist[0].yh >= obon.y - ATTACKZONE)
            {
                obon.stage = 4;
                obon.ticcount = 0;
                obon.shapenum = MUTANTHITPIC;
                // as: Support for extra sound effects
                DamagePlayer(SNDEX_MUTEDAMAGE);
            }
            else
            {
                ChaseThing(objlist[0]);
            }

            CalcBounds();

            TransformObon();
        }

        /*
        =============================================================================

			              SHIELD

        =============================================================================
        */

        /*
        ==================
        =
        = SpawnShield
        =
        ==================
        */
        private void SpawnShield(fixed_t gx, fixed_t gy)
        {
            FindFreeObj();
            _new.x = gx;
            _new.y = gy;
            _new.angle = 0;
            _new.think = ShieldThink;
            _new._class = classtype.shieldobj;
            _new.size = MINDIST;
            _new.shapenum = SHIELD1PIC;
            _new.radarcolor = 9;
            _new.hitpoints = 3;
            CalcBoundsNew();
        }

        /*
        ===================
        =
        = ShieldThink
        =
        ===================
        */
        private void ShieldThink()
        {
            obon.ticcount += tics;

            if(obon.ticcount > SHIELDANM)
            {
                obon.ticcount -= SHIELDANM;

                if(++obon.stage == 2)
                    obon.stage = 0;

                obon.shapenum = (short) (SHIELD1PIC + obon.stage);
            }

            TransformObon();
        }

    }
}
