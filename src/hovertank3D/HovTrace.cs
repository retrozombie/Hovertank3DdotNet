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
        //==========================================================================

        private fixed_t edgex;
        
        private fixed_t edgey;

        private short wallon;

        private short basecolor;

        private walltype oldwall;
        
        //
        // offsets from upper left corner of a tile to the left and right edges of
        // a given wall (NORTH-WEST)
        //
        private fixed_t[] point1x = new fixed_t[] { GLOBAL1, GLOBAL1, 0, 0 };

        private fixed_t[] point1y = new fixed_t[] { 0, GLOBAL1, GLOBAL1, 0 };

        private fixed_t[] point2x = new fixed_t[] { 0, GLOBAL1, GLOBAL1, 0 };

        private fixed_t[] point2y = new fixed_t[] { 0, 0, GLOBAL1, GLOBAL1 };

        //
        // offset from tile.x,tile.y of the tile that shares wallon side
        // (side is not visable if it is shared)
        //
        private short[] sharex = new short[] { 0, 1, 0, -1 };

        private short[] sharey = new short[] { -1, 0, 1, 0 };

        //
        // amount to move tile.x,tile.y to follow wallon to another tile
        //
        private short[] followx = new short[] { -1, 0, 1, 0 };

        private short[] followy = new short[] { 0, -1, 0, 1 };

        //
        // cornerwall gives the wall on the same tile to start following when the
        // wall ends at an empty tile (go around an edge on same tile)
        // turnwall gives the wall on tile.x+sharex,tile.y+sharey to start following
        // when the wall hits another tile (right angle corner)
        //
        private short[] cornerwall = new short[] { WEST, NORTH, EAST, SOUTH };

        private short[] turnwall = new short[] { EAST, SOUTH, WEST, NORTH };

        //
        // wall visabilities in reletive locations
        // -,- 0,- +,-
        // -,0 0,0 +,0
        // -,+ 0,+ +,+
        //
        private bool[,] visable = new bool[9, 4]
        {
            { false, true, true, false },  { false, false, true, false },  { false, false, true, true }, 
            { false, true, false, false }, { false, false, false, false }, { false, false, false, true }, 
            { true, true, false, false },  { true, false, false, false },  { true, false, false, true }
        };

        private short[] startwall = new short[]
        {
            2, 2, 3, 
            1, 0, 3, 
            1, 0, 0
        };

        private short[] backupwall = new short[]
        {
            3, 3, 0, 
            2, 0, 0, 
            2, 1, 1 
        };

        /*
        ========================
        =
        = FollowTrace
        =
        ========================
        */
        private bool FollowTrace(fixed_t tracex, fixed_t tracey, int deltax, int deltay, short max)
        {
            short tx = (short) (tracex >> TILESHIFT);
            short ty = (short) (tracey >> TILESHIFT);

            int absdx = LABS(deltax);
            int absdy = LABS(deltay);

            if(absdx > absdy)
            {
                int ystep = (deltay << 8) / (absdx >> 8);

                if(ystep == 0)
                    ystep = (deltay > 0 ? 1 : -1);

                short oty = (short) ((tracey + ystep) >> TILESHIFT);
                if(deltax > 0)
                {
                    //###############
                    //
                    // step x by +1
                    //
                    //###############
                    do
                    {
                        tx++;
                        tracey += ystep;
                        ty = (short) (tracey >> TILESHIFT);

                        if(ty != oty)
                        {
                            if(tilemap[tx - 1, ty] != 0)
                            {
                                tile.x = (short) (tx - 1);
                                tile.y = ty;
                                return true;
                            }
                            oty = ty;
                        }

                        if(tilemap[tx, ty] != 0)
                        {
                            tile.x = tx;
                            tile.y = ty;
                            return true;
                        }

                    } while(--max != 0);

                    return false;
                }
                else
                {
                    //###############
                    //
                    // step x by -1
                    //
                    //###############
                    do
                    {
                        tx--;
                        tracey += ystep;
                        ty = (short) (tracey >> TILESHIFT);

                        if(ty != oty)
                        {
                            if(tilemap[tx, oty] != 0)
                            {
                                tile.x = tx;
                                tile.y = oty;
                                return true;
                            }
                            oty = ty;
                        }

                        if(tilemap[tx, ty] != 0)
                        {
                            tile.x = tx;
                            tile.y = ty;
                            return true;
                        }

                    } while(--max != 0);

                    return false;
                }
            }
            else
            {
                int xstep = (deltax << 8) / (absdy >> 8);
                if(xstep == 0)
                    xstep = (deltax > 0 ? 1 : -1);

                short otx = (short) ((tracex + xstep) >> TILESHIFT);
                if(deltay > 0)
                {
                    //###############
                    //
                    // step y by +1
                    //
                    //###############
                    do
                    {
                        ty++;
                        tracex += xstep;
                        tx = (short) (tracex >> TILESHIFT);

                        if(tx != otx)
                        {
                            if(tilemap[tx, ty - 1] != 0)
                            {
                                tile.x = tx;
                                tile.y = (short) (ty - 1);
                                return true;
                            }
                            otx = tx;
                        }

                        if(tilemap[tx, ty] != 0)
                        {
                            tile.x = tx;
                            tile.y = ty;
                            return true;
                        }

                    } while(--max != 0);

                    return false;
                }
                else
                {
                    //###############
                    //
                    // step y by -1
                    //
                    //###############
                    do
                    {
                        ty--;
                        tracex += xstep;
                        tx = (short) (tracex >> TILESHIFT);

                        if(tx != otx)
                        {
                            if(tilemap[otx, ty] != 0)
                            {
                                tile.x = otx;
                                tile.y = ty;
                                return true;
                            }
                            otx = tx;
                        }

                        if(tilemap[tx, ty] != 0)
                        {
                            tile.x = tx;
                            tile.y = ty;
                            return true;
                        }

                    } while(--max != 0);

                    return false;
                }
            }
        }

        //===========================================================================

        /*
        =================
        =
        = BackTrace
        =
        = Traces backwards from edgex,edgey to viewx,viewy to see if a closer
        = tile obscures the given point.  If it does, it finishes the wall and
        = starts a new one.
        = Returns true if a tile is hit.
        = Call with a 1 to have it automatically finish the current wall
        =
        =================
        */
        private bool BackTrace(bool finish)
        {
            int deltax = viewx - edgex;
            int deltay = viewy - edgey;

            int absdx = LABS(deltax);
            int absdy = LABS(deltay);

            short steps;
            if(absdx > absdy)
                steps = (short) (ABS((short) (focal.x - (edgex >> TILESHIFT))) - 1);
            else
                steps = (short) (ABS((short) (focal.y - (edgey >> TILESHIFT))) - 1);

            if(steps <= 0)
                return false;

            short otx = tile.x;
            short oty = tile.y;
            if(!FollowTrace(edgex, edgey, deltax, deltay, steps))
                return false;

            //
            // if the start wall is behind the focal point, the trace went too far back
            //
            if(ABS((short) (tile.x - focal.x)) < 2 && ABS((short) (tile.y - focal.y)) < 2) // too close
            {
                if(tile.x == focal.x && tile.y == focal.y)
                {
                    tile.x = otx;
                    tile.y = oty;
                    return false;
                }

                short wall;
                if(tile.x < focal.x)
                {
                    if(tile.y < focal.y)
                        wall = SOUTH;
                    else
                        wall = EAST;
                }
                else if(tile.x == focal.x)
                {
                    if(tile.y < focal.y)
                        wall = SOUTH;
                    else
                        wall = NORTH;
                }
                else
                {
                    if(tile.y <= focal.y)
                        wall = WEST;
                    else
                        wall = NORTH;
                }

                //
                // rotate the X value to see if it is behind the view plane
                //
                if(TransformX(((int) tile.x << 16) + point1x[wall], ((int) tile.y << 16) + point1y[wall]) < FOCALLENGTH)
                {
                    tile.x = otx;
                    tile.y = oty;
                    return false;
                }
            }

            //
            // if the old wall is still behind a closer wall, ignore the back trace
            // and continue on (dealing with limited precision...)
            //
            if(finish && !FinishWall()) // the wall is still behind a forward wall
            {
                tile.x = otx;
                tile.y = oty;
                rightwall.x1 = oldwall.x2; // common edge with last wall
                rightwall.height1 = oldwall.height2;
                return false;
            }

            //
            // back up along the intersecting face to find the rightmost wall
            //
            short offset;
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            wallon = backupwall[offset];

            while(tilemap[tile.x, tile.y] != 0)
            {
                tile.x += followx[wallon];
                tile.y += followy[wallon];
            };

            tile.x -= followx[wallon];
            tile.y -= followy[wallon];

            wallon = cornerwall[wallon]; // turn to first visable face

            edgex = ((int) tile.x << 16);
            edgey = ((int) tile.y << 16);

            TransformPoint(edgex + point1x[wallon], edgey + point1y[wallon], ref rightwall.x1, ref rightwall.height1);

            basecolor = (short) tilemap[tile.x, tile.y];

            return true;
        }

        //===========================================================================

        /*
        =================
        =
        = ForwardTrace
        =
        = Traces forwards from edgex,edgey along the line from viewx,viewy until
        = a solid tile is hit.  Sets tile.x,tile.y
        =
        =================
        */
        private void ForwardTrace()
        {
            int deltax = edgex - viewx;
            int deltay = edgey - viewy;

            FollowTrace(edgex, edgey, deltax, deltay, 0);

            short offset;
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            wallon = startwall[offset];

            //
            // start the new wall
            //
            edgex = ((int) tile.x << 16);
            edgey = ((int) tile.y << 16);

            //
            // if entire first wall is invisable, corner
            //
            TransformPoint(edgex + point2x[wallon], edgey + point2y[wallon], ref rightwall.x2, ref rightwall.height2);

            walltype rightwall_1 = walls[rightwall.index - 1];
            if(tilemap[tile.x + sharex[wallon], tile.y + sharey[wallon]] != 0 || rightwall.x2 < rightwall_1.x2)
                wallon = cornerwall[wallon];

            //
            // transform first point
            //
            TransformPoint(edgex + point1x[wallon], edgey + point1y[wallon], ref rightwall.x1, ref rightwall.height1);

            basecolor = (short) tilemap[tile.x, tile.y];
        }

        //===========================================================================

        /*
        =================
        =
        = FinishWall
        =
        = Transforms edgex,edgey as the next point of the current wall
        = and sticks it in the wall list
        =
        =================
        */
        private bool FinishWall()
        {
            oldwall = rightwall;

            if((wallon & 1) != 0)
                rightwall.color = (ushort) (basecolor + 8); // high intensity walls
            else
                rightwall.color = (ushort) basecolor;

            TransformPoint(edgex, edgey, ref rightwall.x2, ref rightwall.height2);

            walltype rightwall_1 = walls[rightwall.index - 1];
            if(rightwall.x2 <= rightwall_1.x2 + 2 && rightwall.height2 < rightwall_1.height2)
                return false;

            rightwall = walls[rightwall.index + 1];

            return true;
        }

        //===========================================================================

        /*
        =================
        =
        = InsideCorner
        =
        =================
        */
        private void InsideCorner()
        {
            //
            // the wall turned -90 degrees, so draw what we have, move to the new tile,
            // change wallon, change color, and continue following.
            //
            FinishWall();

            tile.x += sharex[wallon];
            tile.y += sharey[wallon];

            wallon = turnwall[wallon];

            //
            // if the new wall is visable, continue following it.  Otherwise
            // follow it backwards until it turns
            //
            short offset;
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            if(visable[offset, wallon])
            {
                //
                // just turn to the next wall and continue
                //
                rightwall.x1 = oldwall.x2; // common edge with last wall
                rightwall.height1 = oldwall.height2;
                basecolor = (short) tilemap[tile.x, tile.y];
                return; // continue from here
            }

            //
            // back follow the invisable wall until it turns, then follow that
            //
            do
            {
                tile.x += followx[wallon];
                tile.y += followy[wallon];
            } while(tilemap[tile.x, tile.y] != 0);

            tile.x -= followx[wallon];
            tile.y -= followy[wallon];

            wallon = cornerwall[wallon]; // turn to first visable face

            edgex = ((int) tile.x << 16) + point1x[wallon];
            edgey = ((int) tile.y << 16) + point1y[wallon];

            if(!BackTrace(false)) // backtrace without finishing a wall
            {
                TransformPoint(edgex, edgey, ref rightwall.x1, ref rightwall.height1);
                basecolor = (short) tilemap[tile.x, tile.y];
            }
        }

        //===========================================================================

        /*
        =================
        =
        = OutsideCorner
        =
        =================
        */
        private void OutsideCorner()
        {
            //
            // edge is the outside edge of a corner, so draw the current wall and
            // turn the corner (+90 degrees)
            //
            FinishWall();

            tile.x -= followx[wallon]; // backup to the real tile
            tile.y -= followy[wallon];
            wallon = cornerwall[wallon];

            //
            // if the new wall is visable, continue following it.  Otherwise
            // trace a ray from the corner to find a wall in the distance to
            // follow
            //
            short offset;
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            if(visable[offset, wallon])
            {
                //
                // the new wall is visable, so just continue on
                //
                rightwall.x1 = oldwall.x2; // common edge with last wall
                rightwall.height1 = oldwall.height2;
                return; // still on same tile, so color is ok
            }

            //
            // start from a new tile further away
            //
            ForwardTrace(); // find the next wall further back
        }

        //===========================================================================

        /*
        =================
        =
        = FollowWalls
        =
        = Starts a wall edge at the leftmost edge of tile.x,tile.y and follows it
        = until something else is seen or the entire view area is covered
        =
        =================
        */
        private void FollowWalls()
        {
            //####################
            //
            // figure leftmost wall of new tile
            //
            //####################

        restart:
            short offset;
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            wallon = startwall[offset];

            //
            // if the start wall is inside a block, skip it by cornering to the second wall
            //
            if(tilemap[tile.x + sharex[wallon], tile.y + sharey[wallon]] != 0)
                wallon = cornerwall[wallon];

            //
            // transform first edge to screen coordinates
            //
            edgex = ((int) tile.x << 16);
            edgey = ((int) tile.y << 16);

            TransformPoint(edgex + point1x[wallon], edgey + point1y[wallon],
                ref rightwall.x1, ref rightwall.height1);

            basecolor = (short) tilemap[tile.x, tile.y];

            //##################
            //
            // follow the wall as long as possible
            //
            //##################

            int debugLoop = 0;
            do // while ( tile.x != right.x || tile.y != right.y)
            {
                // FIXME as: prevent an infinite loop
                debugLoop++;
                if(debugLoop == 1000)
                    throw new Exception("FollowWalls: Infinite loop!");

                //
                // check for conditions that shouldn't happed...
                //
                if(rightwall.x1 > VIEWXH) // somehow missed right tile...
                    return;

                if(rightwall == walls[DANGERHIGH])
                {
                    //
                    // somethiing got messed up!  Correct by thrusting ahead...
                    //
                    ColorBorder(15);
                    bordertime = 60;
                    Thrust();

                    if(++obon.angle == ANGLES)
                        obon.angle = 0;

                    obon.CopyTo(objlist[0]);
                    StartView();
                    
                    //_sys.Log("FollowWalls failed: Wall List Overflow!");
                    
                    goto restart;

#if false
                    StringBuilder s = new StringBuilder();
                    s.Append("Wall list overflow at LE:");
                    s.Append(level);
                    s.Append(" X:");
                    s.Append(objlist[0].x);
                    s.Append(" Y:");
                    s.Append(objlist[0].x);
                    s.Append(" AN:");
                    s.Append(objlist[0].angle);
                    _hovMain.Quit(s.ToString());
#endif
                }

                //
                // proceed along wall
                //
                edgex = ((int) tile.x << 16) + point2x[wallon];
                edgey = ((int) tile.y << 16) + point2y[wallon];

                if(BackTrace(true)) // went behind a closer wall
                    continue;

                //
                // advance to next tile along wall
                //
                tile.x += followx[wallon];
                tile.y += followy[wallon];

                if(tilemap[tile.x + sharex[wallon], tile.y + sharey[wallon]] != 0)
                {
                    InsideCorner(); // turn at a corner
                    continue;
                }

                short newcolor = (short) tilemap[tile.x, tile.y];

                if(newcolor == 0) // turn around an edge
                {
                    OutsideCorner();
                    continue;
                }

                if(newcolor != basecolor)
                {
                    //
                    // wall changed color, so draw what we have and continue following
                    //
                    FinishWall();

                    rightwall.x1 = oldwall.x2; // new wall shares this edge
                    rightwall.height1 = oldwall.height2;
                    basecolor = newcolor;

                    continue;
                }

            } while(tile.x != right.x || tile.y != right.y);

            //######################
            //
            // draw the last tile
            //
            //######################

            edgex = ((int) tile.x << 16) + point2x[wallon];
            edgey = ((int) tile.y << 16) + point2y[wallon];

            FinishWall();

            wallon = cornerwall[wallon];

            //
            // if the corner wall is visable, draw it
            //
            if(tile.y < focal.y)
                offset = 0;
            else if(tile.y == focal.y)
                offset = 3;
            else
                offset = 6;

            if(tile.x == focal.x)
                offset++;
            else if(tile.x > focal.x)
                offset += 2;

            if(visable[offset, wallon])
            {
                rightwall.x1 = oldwall.x2; // common edge with last wall
                rightwall.height1 = oldwall.height2;

                edgex = ((int) tile.x << 16) + point2x[wallon];
                edgey = ((int) tile.y << 16) + point2y[wallon];

                FinishWall();
            }
        }

        //===========================================================================
    }
}
