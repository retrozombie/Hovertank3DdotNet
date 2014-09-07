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

// Normally you have to quit the game (without an error) for CTLPANEL.HOV
// (the user settings file) to be updated, with the following defined, 
// the user settings are updated whenever they change
#define SAVE_DATA_WHEN_CHANGED

using System;
using System.Collections.Generic;
using System.Text;

namespace Hovertank3DdotNet
{
    partial class Hovertank
    {
        /// <summary>The current state.</summary>
        private State _currentState;

        /// <summary>Initialises the state machine.</summary>
        public void StateInitialise()
        {
            _currentState = _stateMain1;
        }

        /// <summary>Updates the state machine.</summary>
        public void StateUpdate()
        {
            // Update keyboard input
            _sys.InputSystem.UpdateKeyboardInput(this);

            // Execute states until a looping state is active
            State newState;
            int stateLoopCount = 0;
            while(true)
            {
                newState = _currentState.Execute(this);

                if(newState == null)
                    break;

                _currentState = newState;

                stateLoopCount++;
                if(stateLoopCount == 1000)
                    throw new Exception("Infinite state loop!");
            }
        }

        // ==========================================================================================

        /// <summary>Represents a state.</summary>
        /// <remarks>
        /// This file contains the main program separated into discrete steps.
        /// Each step is captured in an Execute method and consists of a run of the program
        /// until the next point where it would start looping.
        /// The original main() is not used and has been left in for future / reference.
        /// 
        /// NOTE: This system is a really bad idea / gross hack however for the time being it does
        /// get the game up and running.
        /// 
        /// The naming convention is: [source method name][step number]
        ///     i.e. main1()
        ///         
        /// Some parts have been simplified (i.e. removed or re-ordered).
        /// </remarks>
        abstract class State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public abstract State Execute(Hovertank hovertank);
        }

        // ==========================================================================================

        /// <summary>main state - part 1.</summary>
        private Main1State _stateMain1 = new Main1State();

        /// <summary>Represents main() part 1 as a state.</summary>
        class Main1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.main1();
            }
        }

        /// <summary>
        /// The main() method - part 1.
        /// Initialises sound and graphic resources.
        /// </summary>
        /// <returns>The next state to set or null.</returns>
        private State main1()
        {
            soundblaster = jmDetectSoundBlaster(-1);

            // as: Enable PC speaker mode when NOBLASTER command is specified
            if(_sys.IndexOfCommandLineArgument("NOBLASTER") != -1)
                _sfxPlayer.SpeakerMode = true;

            if(soundblaster)
            {
                memptr baseptr;
                LoadIn("DSOUND.HOV", out baseptr);

                SampledSound samples = new SampledSound(baseptr);
                jmStartSB();
                jmSetSamplePtr(samples);
            }

            LoadNearData(); // load some stuff before starting the memory manager

            MMStartup();
            MMGetPtr(ref bufferseg, BUFFERSIZE); // small general purpose buffer

            BloadinMM("SOUNDS." + EXTENSION, ref soundseg);

            // as: Added initialisation of speaker sounds
            _sfxPlayer.InitSpeakerSound(soundseg);

            // timerspeed = 0x2147; // 140 ints / second (2/VBL)
            StartupSound(); // interrupt handlers that must be removed at quit
            SNDstarted = true;

            StartupKbd();
            KBDstarted = true;

            SetupGraphics();

            InitRndT(true); // setup random routines
            InitRnd(true);

            LoadCtrls();

            BuildTables();

            SC_Setup();

            SetScreenMode(grmode);

            SetLineWidth(SCREENWIDTH);

            screencenterx = 19;
            screencentery = 12;

            // as: added delay to allow detection of ESC in the next step to bypass the intro
            _stateWaitVBL.Initialise(this, 70, _stateMain2);
            return _stateWaitVBL;
        }

        // ==========================================================================================

        /// <summary>main state - part 2.</summary>
        private Main2State _stateMain2 = new Main2State();

        /// <summary>Represents main() part 2 as a state.</summary>
        class Main2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.main2();
            }
        }

        /// <summary>
        /// The main() method - part 2.
        /// Decide whether to skip the intro.
        /// </summary>
        /// <returns>The next state to set or null.</returns>
        private State main2()
        {
            if(!keydown[1]) // hold ESC to bypass intro
            {
                // Intro() / FadeOut()
                _stateFadeOut.Initialise(_stateIntro1);
                return _stateFadeOut;
            }

            return BeginDemoLoop1();
        }

        // ==========================================================================================

        /// <summary>Intro state - part 1.</summary>
        private Intro1State _stateIntro1 = new Intro1State();

        /// <summary>Represents Intro() part 1 as a state.</summary>
        class Intro1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro1();
            }
        }

        /// <summary>
        /// The Intro() method - part 1.
        /// Draws the title pic.
        /// </summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro1()
        {
            CacheDrawPic(STARSPIC);

            pxl = 0;
            pxh = 320;
            py = 180;

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
            memptr shapeseg = new memptr();
            if(_sys.IntroLogo)
            {
                SC_MakeShape(
                    grsegs[STARTPICS + LOGOPIC],
                    pictable[LOGOPIC].width,
                    pictable[LOGOPIC].height,
                    ref shapeseg);
            }

            MMFreePtr(ref grsegs[STARTPICS + LOGOPIC]);

            _stateIntro2.shapeseg = shapeseg;
            _stateIntro2.Initialise(this);

            _stateFadeIn.Initialise(_stateIntro2);
            return _stateFadeIn;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 2.</summary>
        private Intro2State _stateIntro2 = new Intro2State();

        /// <summary>Represents Intro() part 2 as a state.</summary>
        class Intro2State : State
        {
            /// <summary>The shape.</summary>
            public memptr shapeseg;

            /// <summary>The frame counter.</summary>
            public short f;

            /// <summary>The shape's x location.</summary>
            public short sx;

            /// <summary>The shape's y location.</summary>
            public short sy;

            /// <summary>The display page.</summary>
            public short page;

            /// <summary>The display page screen address.</summary>
            public ushort[] pageptr = new ushort[2];

            /// <summary>The shape width drawn to the associated display page.</summary>
            public ushort[] pagewidth = new ushort[2];

            /// <summary>The shape height drawn to the associated display page.</summary>
            public ushort[] pageheight = new ushort[2];

            public float sizescale;

            public float worldycenter;

            public float worldxcenter;

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            public void Initialise(Hovertank hovertank)
            {
                sx = 160;
                sy = 180;

                /*
                =============================================================================

                          SCALED PICTURE DIRECTOR

                =============================================================================
                */

                float minz = (float) (Math.Cos(MAXANGLE) * RADIUS); // closest point
                minz += DISTANCE;
                sizescale = 256 * minz; // closest point will be full size
                float ytop = 80 - (PICHEIGHT / 2) * (sizescale / DISTANCE) / 256;
                float z = sizescale / (DISTANCE * 256);
                ytop = ytop / z; // world coordinates
                worldycenter = ytop - RADIUS;
                float xmid = (float) (Math.Sin(MAXANGLE) * RADIUS / 2);
                worldxcenter = -xmid;

                f = 1;
                page = 0;
                hovertank.inttime = 0;
                hovertank.screenofs = 0;
                pagewidth[0] = 0;
                pagewidth[1] = 0;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro2();
            }
        }

        /// <summary>The Intro() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro2()
        {
            if(_sys.SimulateIntroSound && _stateIntro2.f == 1)
                _sfxPlayer.PlaySound(INTROSND);

            float step = _stateIntro2.f / NUMFRAMES;
            float angle = MAXANGLE * step;
            float sn = (float) Math.Sin(angle);
            float cs = (float) Math.Cos(angle);
            float x = _stateIntro2.worldxcenter + sn * RADIUS / 2;
            float y = _stateIntro2.worldycenter + sn * RADIUS;
            float z = DISTANCE + cs * RADIUS;

            float scale = _stateIntro2.sizescale / z;
            _stateIntro2.sx = (short) (160 + (short) (x * scale / 256));
            _stateIntro2.sy = (short) (100 - (short) (y * scale / 256));

            inttime = 0;
            _sys.sound((ushort) ((short) (sn * 1500)));

            if(_sys.IntroLogo)
            {
                //
                // erase old position
                //
                if(_stateIntro2.pagewidth[_stateIntro2.page] != 0)
                {
                    EGAWRITEMODE(1);
                    EGAMAPMASK(15);
                    CopyEGA(_stateIntro2.pagewidth[_stateIntro2.page], _stateIntro2.pageheight[_stateIntro2.page],
                        (ushort) (_stateIntro2.pageptr[_stateIntro2.page] + 0x8000), _stateIntro2.pageptr[_stateIntro2.page]);
                }

                //
                // draw new position
                //
                EGAWRITEMODE(2);
                if(SC_ScaleShape(_stateIntro2.sx, _stateIntro2.sy, (ushort) ((short) scale < 40 ? 10 : scale / 4), _stateIntro2.shapeseg))
                {
                    _stateIntro2.pagewidth[_stateIntro2.page] = scaleblockwidth;
                    _stateIntro2.pageheight[_stateIntro2.page] = scaleblockheight;
                    _stateIntro2.pageptr[_stateIntro2.page] = scaleblockdest;
                }
                else
                {
                    _stateIntro2.pagewidth[_stateIntro2.page] = 0;
                }
            }

            EGAWRITEMODE(0);
            EGABITMASK(255);

            //
            // display it
            //
            // SetScreen(screenofs, 0);
            _stateSetScreen.Initialise(this, screenofs, 0, _stateIntro3);
            return _stateSetScreen;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 3.</summary>
        private Intro3State _stateIntro3 = new Intro3State();

        /// <summary>Represents Intro() part 3 as a state.</summary>
        class Intro3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro3(hovertank);
            }
        }
        
        /// <summary>The Intro() method - part 3.</summary>
        /// <param name="hovertank">The hover tank game.</param>
        /// <returns>The next state to set or null.</returns>
        private State Intro3(Hovertank hovertank)
        {
            _stateIntro2.page ^= 1;
            screenofs = (ushort) (0x4000 * _stateIntro2.page);

            if(_stateIntro2.f < NUMFRAMES)
            {
                _stateIntro2.f += (short) inttime;
                if(_stateIntro2.f > NUMFRAMES)
                    _stateIntro2.f = (short) NUMFRAMES;
            }
            else
            {
                _stateIntro2.f++; // last frame is shown
            }

            if(NBKscan > 0x7f)
            {
                // break;
                goto while_break;
            }

            if(_stateIntro2.f <= NUMFRAMES)
                return _stateIntro2;

        while_break:

            if(hovertank._sys.SimulateIntroSound) // as: Intro sound stops when a key is pressed
                hovertank._sfxPlayer.StopSound();

            _sys.nosound();

            _stateIntro4.Initialise();
            return _stateIntro4;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 4.</summary>
        private Intro4State _stateIntro4 = new Intro4State();

        /// <summary>Represents Intro() - part 4 as a state.</summary>
        class Intro4State : State
        {
            /// <summary>The loop count.</summary>
            public short i;

            /// <summary>Initialises the state.</summary>
            public void Initialise()
            {
                i = 0;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro4();
            }
        }

        /// <summary>The Intro() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro4()
        {
            State nextState;
            if(_stateIntro4.i < 200)
                nextState = _stateIntro5;
            else
                nextState = _stateIntro7;

            // WaitVBL(1);
            _stateWaitVBL.Initialise(this, 1, nextState);
            return _stateWaitVBL;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 5.</summary>
        private Intro5State _stateIntro5 = new Intro5State();

        /// <summary>Represents Intro() - part 5 as a state.</summary>
        class Intro5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro5();
            }
        }

        /// <summary>The Intro() method - part 5.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro5()
        {
            State nextState = _stateIntro4;

            if(_sys.IntroInfo)
            {
                if(NBKscan > 0x7f)
                {
                    // as: With the following enabled, pressing I during the intro displays a
                    // window with a message, part of the text overflows the window though
                    // this can be enabled with the -introinfo command
//#if false
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

                        // Ack();
                        _stateAck.Initialise(this, _stateIntro6);
                        nextState = _stateAck;
                    }
//#endif
                }
            }

            _stateIntro4.i++;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 6.</summary>
        private Intro6State _stateIntro6 = new Intro6State();

        /// <summary>Represents Intro() - part 6 as a state.</summary>
        class Intro6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro6();
            }
        }

        /// <summary>The Intro() method - part 6.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro6()
        {
            ClearKeys();
            
            // break;
            return _stateIntro7;
        }

        // ==========================================================================================

        /// <summary>Intro state - part 7.</summary>
        private Intro7State _stateIntro7 = new Intro7State();

        /// <summary>Represents Intro() - part 7 as a state.</summary>
        class Intro7State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Intro7();
            }
        }

        /// <summary>The Intro() method - part 7.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Intro7()
        {
            // MMFreePtr(ref shapeseg); // as: Leave in memory
            
            return BeginDemoLoop1();
        }

        // ==========================================================================================

        /// <summary>Begins the DemoLoop state.</summary>
        /// <returns>The next state.</returns>
        private State BeginDemoLoop1()
        {
            // FadeOut()
            _stateFadeOut.Initialise(_stateDemoLoop1);
            return _stateFadeOut;
        }

        /// <summary>DemoLoop state - part 1.</summary>
        private DemoLoop1State _stateDemoLoop1 = new DemoLoop1State();

        /// <summary>Represents DemoLoop() - part 1 as a state.</summary>
        class DemoLoop1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.DemoLoop1();
            }
        }

        /// <summary>The DemoLoop() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State DemoLoop1()
        {
            CacheDrawPic(TITLEPIC);
            StopDrive(); // make floppy motors turn off

            // FadeIn();
            _stateDemoLoop2.Initialise();
            _stateFadeIn.Initialise(_stateDemoLoop2);
            return _stateFadeIn;
        }

        // ==========================================================================================

        /// <summary>DemoLoop state - part 2.</summary>
        private DemoLoop2State _stateDemoLoop2 = new DemoLoop2State();

        /// <summary>Represents DemoLoop() - part 2 as a state.</summary>
        class DemoLoop2State : State
        {
            /// <summary>The screen x origin.</summary>
            public short originx;

            /// <summary>The loop count.</summary>
            public short i;

            /// <summary>Initialises the state.</summary>
            public void Initialise()
            {
                originx = 0;
                i = 100;
            }
            
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.DemoLoop2();
            }
        }

        /// <summary>The DemoLoop() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State DemoLoop2()
        {
            if(_stateDemoLoop2.i > PAUSE && _stateDemoLoop2.i <= PAUSE + 80)
                _stateDemoLoop2.originx += 4;

            if(_stateDemoLoop2.i > PAUSE * 2 && _stateDemoLoop2.i <= PAUSE * 2 + 80)
                _stateDemoLoop2.originx -= 4;

            if(_stateDemoLoop2.i > PAUSE * 2 + 80)
                _stateDemoLoop2.i = 0;

            // SetScreen((ushort) (originx / 8), (ushort) (originx % 8));
            _stateSetScreen.Initialise(this, (ushort) (_stateDemoLoop2.originx / 8), 
                (ushort) (_stateDemoLoop2.originx % 8), _stateDemoLoop3);

            return _stateSetScreen;
        }

        // ==========================================================================================

        /// <summary>DemoLoop state - part 3.</summary>
        private DemoLoop3State _stateDemoLoop3 = new DemoLoop3State();

        /// <summary>Represents DemoLoop() - part 3 as a state.</summary>
        class DemoLoop3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.DemoLoop3();
            }
        }

        /// <summary>The DemoLoop() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State DemoLoop3()
        {
            _stateDemoLoop2.i++;

            screenofs = (ushort) (_stateDemoLoop2.originx / 8);

            // if(CheckKeys())
            _stateCheckKeys1.Initialise(_stateDemoLoop4);
            return _stateCheckKeys1;
        }

        // ==========================================================================================

        /// <summary>DemoLoop state - part 4.</summary>
        private DemoLoop4State _stateDemoLoop4 = new DemoLoop4State();

        /// <summary>Represents DemoLoop() - part 4 as a state.</summary>
        class DemoLoop4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.DemoLoop4();
            }
        }

        /// <summary>The DemoLoop() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State DemoLoop4()
        {
            // if(CheckKeys())
            if(_stateCheckKeys1.ReturnValue)
            {
                EGAWRITEMODE(1);
                EGAMAPMASK(15);
                CopyEGA(80, 200, 0x4000, 0);
            }

            ControlStruct c = ControlPlayer(1);

            if(c.button1 || c.button2)
            {
                // break;
                goto while_break;
            }

            if(keydown[0x39])
            {
                // break;
                goto while_break;
            }

            return _stateDemoLoop2;

        while_break:
            
            ClearKeys();

            PlaySound(STARTGAMESND);
            return _statePlayGame1;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 1.</summary>
        private PlayGame1State _statePlayGame1 = new PlayGame1State();

        /// <summary>Represents PlayGame() - part 1 as a state.</summary>
        class PlayGame1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame1();
            }
        }

        /// <summary>The PlayGame() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame1()
        {
            startlevel = 0;
            level = 0;

            State nextState;
            if(bestlevel > 1)
            {
                // ExpWin(28, 3);
                _stateExpWin.Initialise(28, 3, _statePlayGameLevelSelect1);
                nextState = _stateExpWin;
            }
            else
            {
                nextState = _statePlayGame2;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 2.</summary>
        private PlayGame2State _statePlayGame2 = new PlayGame2State();

        /// <summary>Represents PlayGame() - part 2 as a state.</summary>
        class PlayGame2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame2();
            }
        }

        /// <summary>The PlayGame() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame2()
        {
        // restart:

            score = 0;
            resetgame = false;

            return _statePlayGame3;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 3.</summary>
        private PlayGame3State _statePlayGame3 = new PlayGame3State();

        /// <summary>Represents PlayGame() - part 3 as a state.</summary>
        class PlayGame3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame3();
            }
        }

        /// <summary>The PlayGame() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame3()
        {
            lastobjIndex = 0;

            return _stateBaseScreen1;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 4.</summary>
        private PlayGame4State _statePlayGame4 = new PlayGame4State();

        /// <summary>Represents PlayGame() - part 4 as a state.</summary>
        class PlayGame4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame4();
            }
        }

        /// <summary>The PlayGame() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame4()
        {
            MMSetPurge(ref grsegs[STARTPICS + MISSIONPIC], 3);
            MMSortMem(); // push all purgable stuff high for good cache

            // FadeOut();
            _stateFadeOut.Initialise(_statePlayGame5);
            return _stateFadeOut;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 5.</summary>
        private PlayGame5State _statePlayGame5 = new PlayGame5State();

        /// <summary>Represents PlayGame() - part 5 as a state.</summary>
        class PlayGame5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame5();
            }
        }

        /// <summary>The PlayGame() method - part 5.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame5()
        {
            //
            // briefing screen
            //
            level++;
            LoadLevel();
            StopDrive();

            EGAWRITEMODE(0);
            _display.Clear(); // as: equivalent

            // as: Simplified and removed WaitVBLs

            // EGASplitScreen(200 - STATUSLINES);
            _display.SplitScreenLines = 200 - STATUSLINES;

            // SetLineWidth(SCREENWIDTH);
            {
                short width = SCREENWIDTH;

                // EGAVirtualScreen(width);
                // as: linewidth sets stride now
                _display.Stride = width * Display.ColumnScale;

                linewidth = (ushort) width;
                GenYlookup();
            }

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
            // SetScreen(screenofs, 0);
            _stateSetScreen.Initialise(this, screenofs, 0, _statePlayGame6);
            return _stateSetScreen;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 6.</summary>
        private PlayGame6State _statePlayGame6 = new PlayGame6State();

        /// <summary>Represents PlayGame() - part 6 as a state.</summary>
        class PlayGame6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame6();
            }
        }

        /// <summary>The PlayGame() method - part 7.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame6()
        {
            DrawPic(0, 0, MISSIONPIC);

            pxl = 10;
            pxh = 310;

            py = 10;
            CPPrint(_strings[Strings.levnames(level - 1)]); // as: string replacements

            py = 37;
            px = pxl;

            PPrint(_strings[Strings.levtext(level - 1)]); // as: string replacements

            // FadeIn();
            _stateFadeIn.Initialise(_statePlayGame7);
            return _stateFadeIn;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 7.</summary>
        private PlayGame7State _statePlayGame7 = new PlayGame7State();

        /// <summary>Represents PlayGame() - part 7 as a state.</summary>
        class PlayGame7State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame7();
            }
        }

        /// <summary>The PlayGame() method - part 7.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame7()
        {
            ClearKeys();

            // Ack();
            _stateWarpEffect1.Initialise(_statePlayGame8);
            _stateAck.Initialise(this, _stateWarpEffect1);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 8.</summary>
        private PlayGame8State _statePlayGame8 = new PlayGame8State();

        /// <summary>Represents PlayGame() - part 8 as a state.</summary>
        class PlayGame8State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame8();
            }
        }

        /// <summary>The PlayGame() method - part 8.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame8()
        {
            State nextState;

            if(level == 21)
                // break;
                nextState = _statePlayGame11;
            else
                nextState = _statePlayGame9;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 9.</summary>
        private PlayGame9State _statePlayGame9 = new PlayGame9State();

        /// <summary>Represents PlayGame() - part 9 as a state.</summary>
        class PlayGame9State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame9();
            }
        }

        /// <summary>The PlayGame() method - part 9.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame9()
        {
            killedcount = 0;
            savedcount = 0;

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
            {
                bestlevel = level;

#if SAVE_DATA_WHEN_CHANGED
                SaveCtrls();
#endif
            }

            return _statePlayLoop1;
        }

        // ==========================================================================================

        /// <summary>PlayLoop state - part 1.</summary>
        private PlayLoop1State _statePlayLoop1 = new PlayLoop1State();

        /// <summary>Represents PlayLoop() - part 1 as a state.</summary>
        class PlayLoop1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayLoop1();
            }
        }

        /// <summary>The PlayLoop() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayLoop1()
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

            return _stateFinishView1;
        }

        // ==========================================================================================

        /// <summary>FinishView state - part 1.</summary>
        private FinishView1State _stateFinishView1 = new FinishView1State();

        /// <summary>Represents FinishView() - part 1 as a state.</summary>
        class FinishView1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.FinishView1();
            }
        }

        /// <summary>The FinishView() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State FinishView1()
        {
            if(++screenpage == 3)
                screenpage = 0;

            screenofs = screenloc[screenpage];

            EGAWRITEMODE(2);

            //
            // draw the wall list
            //
            DrawWallList();

            //
            // draw all the scaled images
            //
            DrawScaleds();

            //
            // show screen and time last cycle
            //
            screenofs += (ushort) yshift[(ushort) inttime & 0xff]; // hover effect

            DrawCrossHairs();

            EGAWRITEMODE(0);

            // as: converted from asm
            _display.ScreenStartIndex = screenofs * Display.AddressScale;

            return _stateFinishView2;
        }

        // ==========================================================================================

        /// <summary>FinishView state - part 2.</summary>
        private FinishView2State _stateFinishView2 = new FinishView2State();

        /// <summary>Represents FinishView() - part 2 as a state.</summary>
        class FinishView2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.FinishView2();
            }
        }

        /// <summary>The FinishView() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State FinishView2()
        {
            if((tics = (short) ((timecount - lasttimecount) / 2)) < 2)
                return null;

            lasttimecount = (int) (timecount & 0xfffffffe); // (~1);

            if(tics > MAXTICS)
                tics = MAXTICS;

            _stateCheckKeys1.Initialise(_statePlayLoop2);
            return _stateCheckKeys1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys state - part 1.</summary>
        private CheckKeys1State _stateCheckKeys1 = new CheckKeys1State();

        /// <summary>Represents CheckKeys() - part 1 as a state.</summary>
        class CheckKeys1State : State
        {
            /// <summary>Whether a special key was handled.</summary>
            public bool ReturnValue;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Gets the next state.</summary>
            public State NextState
            {
                get { return _nextState; }
            }

            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The next state.</param>
            public void Initialise(State nextState)
            {
                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeys1();
            }
        }

        /// <summary>The CheckKeys() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeys1()
        {
            if(NBKscan == 0)
            {
                _stateCheckKeys1.ReturnValue = false;
                return _stateCheckKeys1.NextState;
            }

            State nextState;
            switch(NBKscan & 0x7f)
            {
                case 0x3b: // F1 = help
                    nextState = _stateCheckKeysDoHelpText1;
                    break;

                case 0x3c: // F2 = sound on/off
                    ClearKeys();

                    // ExpWin(13, 1);
                    _stateExpWin.Initialise(13, 1, _stateCheckKeysSound1);
                    nextState = _stateExpWin;
                    break;

                case 0x3d: // F3 = keyboard mode
                    ClearKeys();

                    // calibratekeys()
                    //  ExpWin(22, 12);
                    _stateExpWin.Initialise(22, 12, _stateCheckKeysCalibrateKeys1);
                    nextState = _stateExpWin;
                    break;

                case 0x3e: // F4 = joystick mode
                    ClearKeys();
                    // CalibrateJoy(1);
                    //  ExpWin(34, 11);
                    _stateCheckKeysCalibrateJoy1.joynum = 1;
                    _stateExpWin.Initialise(34, 11, _stateCheckKeysCalibrateJoy1);
                    nextState = _stateExpWin;
                    break;

                case 0x3f: // F5 = reset game
                    ClearKeys();

                    // ExpWin(18, 1);
                    _stateExpWin.Initialise(18, 1, _stateCheckKeysReset1);
                    nextState = _stateExpWin;
                    break;

                case 0x58: // F12 + ? = debug keys
                    // DebugKeys();
                    nextState = _stateCheckKeysDebugKeys1;
                    break;

                case 1: // ESC = quit
                    ClearKeys();

                    // ExpWin(12, 1);
                    _stateExpWin.Initialise(12, 1, _stateCheckKeysQuit1);
                    nextState = _stateExpWin;
                    break;

                default:
                    _stateCheckKeys1.ReturnValue = false;
                    return _stateCheckKeys1.NextState;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>CheckKeys state - part 2.</summary>
        private CheckKeys2State _stateCheckKeys2 = new CheckKeys2State();

        /// <summary>Represents CheckKeys() - part 2 as a state.</summary>
        class CheckKeys2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeys2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeys2()
        {
            ClearKeys();

            _stateCheckKeys1.ReturnValue = true;
            return _stateCheckKeys1.NextState;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DoHelpText state - part 1.</summary>
        private CheckKeysDoHelpText1State _stateCheckKeysDoHelpText1 = new CheckKeysDoHelpText1State();

        /// <summary>Represents CheckKeys() - DoHelpText - part 1 as a state.</summary>
        class CheckKeysDoHelpText1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDoHelpText1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDoHelpText1()
        {
            ClearKeys();

            // DoHelpText();

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
            // Ack();
            _stateAck.Initialise(this, _stateCheckKeysDoHelpText2);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DoHelpText state - part 2.</summary>
        private CheckKeysDoHelpText2State _stateCheckKeysDoHelpText2 = new CheckKeysDoHelpText2State();

        /// <summary>Represents CheckKeys() - DoHelpText - part 2 as a state.</summary>
        class CheckKeysDoHelpText2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDoHelpText2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDoHelpText2()
        {
            EraseWindow();

            CPPrint(_strings[Strings.DoHelpText5]); // as: string replacements
            CPPrint(_strings[Strings.DoHelpText6]); // as: string replacements

            PPrint(_strings[Strings.DoHelpText7]); // as: string replacements

            CPPrint(_strings[Strings.DoHelpText8]); // as: string replacements
            // Ack();
            _stateAck.Initialise(this, _stateCheckKeysDoHelpText3);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DoHelpText state - part 3.</summary>
        private CheckKeysDoHelpText3State _stateCheckKeysDoHelpText3 = new CheckKeysDoHelpText3State();

        /// <summary>Represents CheckKeys() - DoHelpText - part 3 as a state.</summary>
        class CheckKeysDoHelpText3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDoHelpText3();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDoHelpText3()
        {
            EraseWindow();

            CPPrint(_strings[Strings.DoHelpText9]); // as: string replacements
            CPPrint(_strings[Strings.DoHelpText10]); // as: string replacements
            PPrint(_strings[Strings.DoHelpText11]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateCheckKeys2);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Sound state - part 1.</summary>
        private CheckKeysSound1State _stateCheckKeysSound1 = new CheckKeysSound1State();

        /// <summary>Represents CheckKeys() - Sound - part 1 as a state.</summary>
        class CheckKeysSound1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysSound1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysSound1()
        {
            PPrint(_strings[Strings.CheckKeys1]); // as: string replacements

            // ch = _sys.toupper((sbyte) PGet());
            _statePGet1.Initialise(this, _stateCheckKeysSound2);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Sound state - part 2.</summary>
        private CheckKeysSound2State _stateCheckKeysSound2 = new CheckKeysSound2State();

        /// <summary>Represents CheckKeys() - Sound - part 2 as a state.</summary>
        class CheckKeysSound2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysSound2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysSound2()
        {
            // ch = _sys.toupper((sbyte) PGet());
            ch = _sys.toupper((sbyte) _statePGet1.ReturnValue);
            if(ch == 'N')
                soundmode = 0;
            else if(ch == 'Y')
                soundmode = 1;

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 1.</summary>
        private CheckKeysCalibrateKeys1State _stateCheckKeysCalibrateKeys1 = new CheckKeysCalibrateKeys1State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 1 as a state.</summary>
        class CheckKeysCalibrateKeys1State : State
        {
            public short hx;
            
            public short hy;
            
            public short select;

            public short _new;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys1()
        {
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

            _stateCheckKeysCalibrateKeys1.hx = (short) ((px + 7) / 8);
            _stateCheckKeysCalibrateKeys1.hy = (short) py;
            for(short i = 0; i < 4; i++)
            {
                px = (ushort) (pxl + 8 * 12);
                py = (ushort) (pyl + 10 * (1 + i));
                PPrint(separator); // as: string replacements
                printscan(key[i * 2]);
            }

            px = (ushort) (pxl + 8 * 12);
            py = (ushort) (pyl + 10 * 5);
            PPrint(separator); // as: string replacements
            printscan(keyB1);

            px = (ushort) (pxl + 8 * 12);
            py = (ushort) (pyl + 10 * 6);
            PPrint(separator); // as: string replacements
            printscan(keyB2);

            return _stateCheckKeysCalibrateKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 2.</summary>
        private CheckKeysCalibrateKeys2State _stateCheckKeysCalibrateKeys2 = new CheckKeysCalibrateKeys2State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 2 as a state.</summary>
        class CheckKeysCalibrateKeys2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys2()
        {
            //do
            //{
            px = (ushort) (_stateCheckKeysCalibrateKeys1.hx * 8);
            py = (ushort) _stateCheckKeysCalibrateKeys1.hy;
            DrawChar((ushort) _stateCheckKeysCalibrateKeys1.hx, (ushort) _stateCheckKeysCalibrateKeys1.hy, BLANKCHAR);

            // ch = (sbyte) (PGet() % 256);
            _statePGet1.Initialise(this, _stateCheckKeysCalibrateKeys3);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 3.</summary>
        private CheckKeysCalibrateKeys3State _stateCheckKeysCalibrateKeys3 = new CheckKeysCalibrateKeys3State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 3 as a state.</summary>
        class CheckKeysCalibrateKeys3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys3();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys3()
        {
            // ch = (sbyte) (PGet() % 256);
            ch = (sbyte) (_statePGet1.ReturnValue % 256);
            if(ch < '1' || ch > '6')
                // continue;
                return _stateCheckKeysCalibrateKeys6;

            _stateCheckKeysCalibrateKeys1.select = (short) (ch - '1');
            DrawPchar((ushort) ch);

            PPrint(_strings[Strings.calibratekeys10]); // as: string replacements

            ClearKeys();

            return _stateCheckKeysCalibrateKeys4;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 4.</summary>
        private CheckKeysCalibrateKeys4State _stateCheckKeysCalibrateKeys4 = new CheckKeysCalibrateKeys4State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 4 as a state.</summary>
        class CheckKeysCalibrateKeys4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys4();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys4()
        {
            _stateCheckKeysCalibrateKeys1._new = -1;
            while(!keydown[++_stateCheckKeysCalibrateKeys1._new])
            {
                if(_stateCheckKeysCalibrateKeys1._new == 0x79)
                {
                    _stateCheckKeysCalibrateKeys1._new = -1;
                    break; // as: Modified for single check
                }
                else if(_stateCheckKeysCalibrateKeys1._new == 0x29)
                {
                    _stateCheckKeysCalibrateKeys1._new++; // skip STUPID left shifts!
                }
            }

            if(_stateCheckKeysCalibrateKeys1._new == -1)
                return null;

            return _stateCheckKeysCalibrateKeys5;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 5.</summary>
        private CheckKeysCalibrateKeys5State _stateCheckKeysCalibrateKeys5 = new CheckKeysCalibrateKeys5State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 5 as a state.</summary>
        class CheckKeysCalibrateKeys5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys5();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys5()
        {
            Bar((ushort) leftedge, py, 22, 10, 0xff);

            if(_stateCheckKeysCalibrateKeys1.select < 4)
                key[_stateCheckKeysCalibrateKeys1.select * 2] = (sbyte) _stateCheckKeysCalibrateKeys1._new;

            if(_stateCheckKeysCalibrateKeys1.select == 4)
                keyB1 = (sbyte) _stateCheckKeysCalibrateKeys1._new;

            if(_stateCheckKeysCalibrateKeys1.select == 5)
                keyB2 = (sbyte) _stateCheckKeysCalibrateKeys1._new;

            px = (ushort) (pxl + 8 * 12);
            py = (ushort) (pyl + (_stateCheckKeysCalibrateKeys1.select + 1) * 10);
            Bar((ushort) (px / 8), py, 9, 10, 0xff);

            PPrint(_strings[Strings.calibratekeys9]); // as: string replacements
            printscan(_stateCheckKeysCalibrateKeys1._new);

            ClearKeys();

            ch = (sbyte) '0'; // so the loop continues
            return _stateCheckKeysCalibrateKeys6;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateKeys state - part 6.</summary>
        private CheckKeysCalibrateKeys6State _stateCheckKeysCalibrateKeys6 = new CheckKeysCalibrateKeys6State();

        /// <summary>Represents CheckKeys() - CalibrateKeys - part 6 as a state.</summary>
        class CheckKeysCalibrateKeys6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateKeys6();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateKeys6()
        {
            //} while(ch >= '0' && ch <= '9');
            if(ch >= '0' && ch <= '9')
                return _stateCheckKeysCalibrateKeys2;

            playermode[1] = inputtype.keyboard;

#if SAVE_DATA_WHEN_CHANGED
            SaveCtrls();
#endif
            
            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 1.</summary>
        private CheckKeysCalibrateJoy1State _stateCheckKeysCalibrateJoy1 = new CheckKeysCalibrateJoy1State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 1 as a state.</summary>
        class CheckKeysCalibrateJoy1State : State
        {
            /// <summary>The joystick number.</summary>
            public short joynum;

            /// <summary>The calibration stage.</summary>
            public short stage;

            /// <summary>The controller state.</summary>
            public ControlStruct ctr;

            /// <summary>The lowest x reading.</summary>
            public short xl;

            /// <summary>The lowest y reading.</summary>
            public short yl;

            /// <summary>The highest x reading.</summary>
            public short xh;

            /// <summary>The highest y reading.</summary>
            public short yh;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy1()
        {
            fontcolor = 13;
            CPPrint(_strings[Strings.CalibrateJoy1]); // as: string replacements

            py += 6;
            fontcolor = 15;
            PPrint(_strings[Strings.CalibrateJoy2]); // as: string replacements
            PPrint(_strings[Strings.CalibrateJoy3]); // as: string replacements

            _stateCheckKeysCalibrateJoy1.stage = 15;
            sx = (short) ((px + 7) / 8);

            return _stateCheckKeysCalibrateJoy2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 2.</summary>
        private CheckKeysCalibrateJoy2State _stateCheckKeysCalibrateJoy2 = new CheckKeysCalibrateJoy2State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 2 as a state.</summary>
        class CheckKeysCalibrateJoy2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy2()
        {
            // do // wait for a button press
            // {
            DrawChar((ushort) sx, py, _stateCheckKeysCalibrateJoy1.stage);

            // WaitVBL(3);
            _stateWaitVBL.Initialise(this, 3, _stateCheckKeysCalibrateJoy3);
            return _stateWaitVBL;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 3.</summary>
        private CheckKeysCalibrateJoy3State _stateCheckKeysCalibrateJoy3 = new CheckKeysCalibrateJoy3State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 3 as a state.</summary>
        class CheckKeysCalibrateJoy3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy3();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy3()
        {
            if(++_stateCheckKeysCalibrateJoy1.stage == 23)
                _stateCheckKeysCalibrateJoy1.stage = 15;

            ReadJoystick(_stateCheckKeysCalibrateJoy1.joynum, out _stateCheckKeysCalibrateJoy1.xl, out _stateCheckKeysCalibrateJoy1.yl);

            _stateCheckKeysCalibrateJoy1.ctr = ControlJoystick(_stateCheckKeysCalibrateJoy1.joynum);

            if(keydown[1])
                return _stateCheckKeys2;

            // } while(!ctr.button1 && !ctr.button2);
            if(!_stateCheckKeysCalibrateJoy1.ctr.button1 && !_stateCheckKeysCalibrateJoy1.ctr.button2)
                return _stateCheckKeysCalibrateJoy2;

            DrawChar((ushort) sx, py, BLANKCHAR);
            return _stateCheckKeysCalibrateJoy4;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 4.</summary>
        private CheckKeysCalibrateJoy4State _stateCheckKeysCalibrateJoy4 = new CheckKeysCalibrateJoy4State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 4 as a state.</summary>
        class CheckKeysCalibrateJoy4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy4();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy4()
        {
            // do // wait for the button release
            // {
            _stateCheckKeysCalibrateJoy1.ctr = ControlJoystick(_stateCheckKeysCalibrateJoy1.joynum);
            // } while(ctr.button1);
            if(_stateCheckKeysCalibrateJoy1.ctr.button1)
                return null;

            // WaitVBL(4); // so the button can't bounce
            _stateWaitVBL.Initialise(this, 4, _stateCheckKeysCalibrateJoy5);
            return _stateWaitVBL;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 5.</summary>
        private CheckKeysCalibrateJoy5State _stateCheckKeysCalibrateJoy5 = new CheckKeysCalibrateJoy5State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 5 as a state.</summary>
        class CheckKeysCalibrateJoy5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy5();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy5()
        {
            py += 6;
            PPrint(_strings[Strings.CalibrateJoy4]); // as: string replacements
            PPrint(_strings[Strings.CalibrateJoy5]); // as: string replacements

            return _stateCheckKeysCalibrateJoy6;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 6.</summary>
        private CheckKeysCalibrateJoy6State _stateCheckKeysCalibrateJoy6 = new CheckKeysCalibrateJoy6State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 6 as a state.</summary>
        class CheckKeysCalibrateJoy6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy6();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy6()
        {
            // do // wait for a button press
            // {
            DrawChar((ushort) sx, py, _stateCheckKeysCalibrateJoy1.stage);
            // WaitVBL(3);
            _stateWaitVBL.Initialise(this, 3, _stateCheckKeysCalibrateJoy7);
            return _stateWaitVBL;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 7.</summary>
        private CheckKeysCalibrateJoy7State _stateCheckKeysCalibrateJoy7 = new CheckKeysCalibrateJoy7State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 7 as a state.</summary>
        class CheckKeysCalibrateJoy7State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy7();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy7()
        {
            if(++_stateCheckKeysCalibrateJoy1.stage == 23)
                _stateCheckKeysCalibrateJoy1.stage = 15;

            ReadJoystick(_stateCheckKeysCalibrateJoy1.joynum, out _stateCheckKeysCalibrateJoy1.xh, out _stateCheckKeysCalibrateJoy1.yh);

            _stateCheckKeysCalibrateJoy1.ctr = ControlJoystick(_stateCheckKeysCalibrateJoy1.joynum);

            if(keydown[1])
                return _stateCheckKeys2;

            // } while(!ctr.button1 && !ctr.button2);
            if(!_stateCheckKeysCalibrateJoy1.ctr.button1 && !_stateCheckKeysCalibrateJoy1.ctr.button2)
                return _stateCheckKeysCalibrateJoy6;

            DrawChar((ushort) sx, py, BLANKCHAR);

            return _stateCheckKeysCalibrateJoy8;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 8.</summary>
        private CheckKeysCalibrateJoy8State _stateCheckKeysCalibrateJoy8 = new CheckKeysCalibrateJoy8State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 8 as a state.</summary>
        class CheckKeysCalibrateJoy8State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy8();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy8()
        {
            // do // wait for the button release
            // {
            _stateCheckKeysCalibrateJoy1.ctr = ControlJoystick(_stateCheckKeysCalibrateJoy1.joynum);
            // } while(ctr.button1);
            if(_stateCheckKeysCalibrateJoy1.ctr.button1)
                return null;
            
            //
            // figure out good boundaries
            //
            short dx = (short) ((_stateCheckKeysCalibrateJoy1.xh - _stateCheckKeysCalibrateJoy1.xl) / 6);
            short dy = (short) ((_stateCheckKeysCalibrateJoy1.yh - _stateCheckKeysCalibrateJoy1.yl) / 6);
            JoyXlow[_stateCheckKeysCalibrateJoy1.joynum] = (short) (_stateCheckKeysCalibrateJoy1.xl + dx);
            JoyXhigh[_stateCheckKeysCalibrateJoy1.joynum] = (short) (_stateCheckKeysCalibrateJoy1.xh - dx);
            JoyYlow[_stateCheckKeysCalibrateJoy1.joynum] = (short) (_stateCheckKeysCalibrateJoy1.yl + dy);
            JoyYhigh[_stateCheckKeysCalibrateJoy1.joynum] = (short) (_stateCheckKeysCalibrateJoy1.yh - dy);

            if(_stateCheckKeysCalibrateJoy1.joynum == 1)
                playermode[1] = inputtype.joystick1;
            else
                playermode[1] = inputtype.joystick2;

            py += 6;
            PPrint(_strings[Strings.CalibrateJoy6]); // as: string replacements

            // ch = (sbyte) PGet();
            _statePGet1.Initialise(this, _stateCheckKeysCalibrateJoy9);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - CalibrateJoy state - part 9.</summary>
        private CheckKeysCalibrateJoy9State _stateCheckKeysCalibrateJoy9 = new CheckKeysCalibrateJoy9State();

        /// <summary>Represents CheckKeys() - CalibrateJoy - part 9 as a state.</summary>
        class CheckKeysCalibrateJoy9State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysCalibrateJoy9();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysCalibrateJoy9()
        {
            // ch = (sbyte) PGet();
            ch = (sbyte) _statePGet1.ReturnValue;
            if(ch == 'A' || ch == 'a')
                buttonflip = true;
            else
                buttonflip = false;

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Reset state - part 1.</summary>
        private CheckKeysReset1State _stateCheckKeysReset1 = new CheckKeysReset1State();

        /// <summary>Represents CheckKeys() - Reset - part 1 as a state.</summary>
        class CheckKeysReset1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysReset1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysReset1()
        {
            PPrint(_strings[Strings.CheckKeys2]); // as: string replacements

            // ch = _sys.toupper((sbyte) PGet());
            _statePGet1.Initialise(this, _stateCheckKeysReset2);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Reset state - part 2.</summary>
        private CheckKeysReset2State _stateCheckKeysReset2 = new CheckKeysReset2State();

        /// <summary>Represents CheckKeys() - Reset - part 2 as a state.</summary>
        class CheckKeysReset2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysReset2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysReset2()
        {
            // ch = _sys.toupper((sbyte) PGet());
            ch = _sys.toupper((sbyte) _statePGet1.ReturnValue);
            if(ch == 'Y')
            {
                resetgame = true;
                leveldone = -99;
            }

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys state - part 1.</summary>
        private CheckKeysDebugKeys1State _stateCheckKeysDebugKeys1 = new CheckKeysDebugKeys1State();

        /// <summary>Represents CheckKeys() - DebugKeys() - part 1 as a state.</summary>
        class CheckKeysDebugKeys1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeys1();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeys1()
        {
            State nextState = _stateCheckKeys2;

            if(keydown[0x22]) // G = god mode
            {
                // ExpWin(12, 1);
                _stateExpWin.Initialise(12, 1, _stateCheckKeysDebugKeysGodMode1);
                nextState = _stateExpWin;
            }
            else if(keydown[0x32]) // M = memory info
            {
                // DebugMemory();
                nextState = _stateCheckKeysDebugKeysDebugMemory1;
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
                // ExpWin(26, 1);
                _stateExpWin.Initialise(26, 1, _stateCheckKeysDebugKeysLevelWarp1);
                nextState = _stateExpWin;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys - God Mode state - part 1.</summary>
        private CheckKeysDebugKeysGodMode1State _stateCheckKeysDebugKeysGodMode1 = new CheckKeysDebugKeysGodMode1State();

        /// <summary>Represents CheckKeys() - DebugKeys() method - God Mode part 1 as a state.</summary>
        class CheckKeysDebugKeysGodMode1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeysGodMode1();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() method - God Mode part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeysGodMode1()
        {
            if(godmode != 0)
                CPPrint(_strings[Strings.DebugKeys1]); // as: string replacements
            else
                CPPrint(_strings[Strings.DebugKeys2]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateCheckKeysDebugKeysGodMode2);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys - God Mode state - part 2.</summary>
        private CheckKeysDebugKeysGodMode2State _stateCheckKeysDebugKeysGodMode2 = new CheckKeysDebugKeysGodMode2State();

        /// <summary>Represents CheckKeys() - DebugKeys() method - God Mode part 2 as a state.</summary>
        class CheckKeysDebugKeysGodMode2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeysGodMode2();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() method - God Mode part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeysGodMode2()
        {
            godmode ^= 1;

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys - DebugMemory state - part 1.</summary>
        private CheckKeysDebugKeysDebugMemory1State _stateCheckKeysDebugKeysDebugMemory1 = new CheckKeysDebugKeysDebugMemory1State();

        /// <summary>Represents CheckKeys() - DebugKeys() - DebugMemory() method - part 1 as a state.</summary>
        class CheckKeysDebugKeysDebugMemory1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeysDebugMemory1();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() - DebugMemory() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeysDebugMemory1()
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

            // PGet();
            _statePGet1.Initialise(this, _stateCheckKeys2);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys - Level Warp state - part 1.</summary>
        private CheckKeysDebugKeysLevelWarp1State _stateCheckKeysDebugKeysLevelWarp1 = new CheckKeysDebugKeysLevelWarp1State();

        /// <summary>Represents CheckKeys() - DebugKeys() method - Level Warp part 1 as a state.</summary>
        class CheckKeysDebugKeysLevelWarp1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeysLevelWarp1();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() method - Level Warp part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeysLevelWarp1()
        {
            PPrint(_strings[Strings.DebugKeys3]); // as: string replacements

            // short i = (short) InputInt();
            _stateInputInt1.Initialise(this, 2, _stateCheckKeysDebugKeysLevelWarp2);
            return _stateInputInt1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - DebugKeys - Level Warp state - part 2.</summary>
        private CheckKeysDebugKeysLevelWarp2State _stateCheckKeysDebugKeysLevelWarp2 = new CheckKeysDebugKeysLevelWarp2State();

        /// <summary>Represents CheckKeys() - DebugKeys() method - Level Warp part 2 as a state.</summary>
        class CheckKeysDebugKeysLevelWarp2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysDebugKeysLevelWarp2();
            }
        }

        /// <summary>The CheckKeys() - DebugKeys() method - Level Warp part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysDebugKeysLevelWarp2()
        {
            // short i = (short) InputInt();
            short i = (short) _stateInputInt1.ReturnValue;
            if(i >= 1 && i <= 21)
            {
                level = (short) (i - 1);
                leveldone = 1;
            }

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Quit state - part 1.</summary>
        private CheckKeysQuit1State _stateCheckKeysQuit1 = new CheckKeysQuit1State();

        /// <summary>Represents CheckKeys() - Quit - part 1 as a state.</summary>
        class CheckKeysQuit1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysQuit1();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysQuit1()
        {
            PPrint(_strings[Strings.CheckKeys3]); // as: string replacements

            // ch = _sys.toupper((sbyte) PGet());
            _statePGet1.Initialise(this, _stateCheckKeysQuit2);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>CheckKeys - Quit state - part 2.</summary>
        private CheckKeysQuit2State _stateCheckKeysQuit2 = new CheckKeysQuit2State();

        /// <summary>Represents CheckKeys() - Quit - part 2 as a state.</summary>
        class CheckKeysQuit2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.CheckKeysQuit2();
            }
        }

        /// <summary>The CheckKeys() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State CheckKeysQuit2()
        {
            // ch = _sys.toupper((sbyte) PGet());
            ch = _sys.toupper((sbyte) _statePGet1.ReturnValue);

            if(ch == 'Y')
                Quit("");

            return _stateCheckKeys2;
        }

        // ==========================================================================================

        /// <summary>PlayLoop state - part 2.</summary>
        private PlayLoop2State _statePlayLoop2 = new PlayLoop2State();

        /// <summary>Represents PlayLoop() - part 2 as a state.</summary>
        class PlayLoop2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayLoop2();
            }
        }

        /// <summary>The PlayLoop() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayLoop2()
        {
            State nextState;

            if(leveldone == 0)
                nextState = _statePlayLoop1;
            else
                nextState = _statePlayGame10;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 10.</summary>
        private PlayGame10State _statePlayGame10 = new PlayGame10State();

        /// <summary>Represents PlayGame() - part 10 as a state.</summary>
        class PlayGame10State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame10();
            }
        }

        /// <summary>The PlayGame() method - part 10.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame10()
        {
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

            State nextState;

            if(leveldone > 0)
                nextState = _statePlayGame3;
            else
                nextState = _statePlayGame11;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 11.</summary>
        private PlayGame11State _statePlayGame11 = new PlayGame11State();

        /// <summary>Represents PlayGame() - part 11 as a state.</summary>
        class PlayGame11State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame11();
            }
        }

        /// <summary>The PlayGame() method - part 11.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame11()
        {
            State nextState;

            if(resetgame)
                nextState = BeginDemoLoop1();
            else
                nextState = _stateGameOver1;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 12.</summary>
        private PlayGame12State _statePlayGame12 = new PlayGame12State();

        /// <summary>Represents PlayGame() - part 12 as a state.</summary>
        class PlayGame12State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame12();
            }
        }

        /// <summary>The PlayGame() method - part 12.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame12()
        {
            State nextState;

            //
            // continue
            //
            if(level > 2 && level < 21)
            {
                DrawWindow(10, 20, 30, 23);
                py += 3;
                CPPrint(_strings[Strings.PlayGame3]); // as: string replacements
                ClearKeys();

                // ch = (sbyte) PGet();
                _statePGet1.Initialise(this, _statePlayGame13);
                nextState = _statePGet1;
            }
            else
            {
                nextState = BeginDemoLoop1();
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame state - part 13.</summary>
        private PlayGame13State _statePlayGame13 = new PlayGame13State();

        /// <summary>Represents PlayGame() - part 13 as a state.</summary>
        class PlayGame13State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGame13();
            }
        }

        /// <summary>The PlayGame() method - part 13.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGame13()
        {
            State nextState;

            // ch = (sbyte) PGet();
            ch = (sbyte) _statePGet1.ReturnValue;
            if(_sys.toupper(ch) == 'Y')
            {
                level--;
                startlevel = level; // don't show base screen
                // goto restart;
                nextState = _statePlayGame2;
            }
            else
            {
                nextState = BeginDemoLoop1();
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PlayGame - Level Select state - part 1.</summary>
        private PlayGameLevelSelect1State _statePlayGameLevelSelect1 = new PlayGameLevelSelect1State();

        /// <summary>Represents PlayGame() - Level Select - part 1 as a state.</summary>
        class PlayGameLevelSelect1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGameLevelSelect1();
            }
        }

        /// <summary>The PlayGame() method - part LevelSelect1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGameLevelSelect1()
        {
            py += 6;
            PPrint(_strings[Strings.PlayGame1]); // as: string replacements
            PPrint(bestlevel.ToString());
            PPrint(_strings[Strings.PlayGame2]); // as: string replacements

            // short i = (short) InputInt();
            _stateInputInt1.Initialise(this, 2, _statePlayGameLevelSelect2);
            return _stateInputInt1;
        }

        // ==========================================================================================

        /// <summary>PlayGame - Level Select state - part 2.</summary>
        private PlayGameLevelSelect2State _statePlayGameLevelSelect2 = new PlayGameLevelSelect2State();

        /// <summary>Represents PlayGame() - Level Select - part 2 as a state.</summary>
        class PlayGameLevelSelect2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PlayGameLevelSelect2();
            }
        }

        /// <summary>The PlayGame() method - part LevelSelect2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PlayGameLevelSelect2()
        {
            // short i = (short) InputInt();
            short i = (short) _stateInputInt1.ReturnValue;
            if(i >= 1 && i <= bestlevel)
            {
                startlevel = (short) (i - 1);
                level = startlevel;
            }

            return _statePlayGame2;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 1.</summary>
        private BaseScreen1State _stateBaseScreen1 = new BaseScreen1State();

        /// <summary>Represents BaseScreen() - part 1 as a state.</summary>
        class BaseScreen1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen1();
            }
        }

        /// <summary>The BaseScreen() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen1()
        {
            // BaseScreen starts here
            CachePic(STARTPICS + MISSIONPIC);

            //
            // cash screen
            //
            State nextState;
            if(level != startlevel) // send them straight into the first level
            {
                _stateWarpEffect1.Initialise(_stateBaseScreen2);
                nextState = _stateWarpEffect1;
            }
            else
            {
                nextState = _statePlayGame4;
            }
            return nextState;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 2.</summary>
        private BaseScreen2State _stateBaseScreen2 = new BaseScreen2State();

        /// <summary>Represents BaseScreen() - part 2 as a state.</summary>
        class BaseScreen2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen2();
            }
        }

        /// <summary>The BaseScreen() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen2()
        {
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
            _stateBaseScreen3.topofs = screenofs;

            py += 5;
            PPrint(_strings[Strings.BaseScreen4]); // as: string replacements
            screenofs = 0; // draw into the split screen

            _stateBaseScreen3.Initialise();
            return _stateBaseScreen3;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 3.</summary>
        private BaseScreen3State _stateBaseScreen3 = new BaseScreen3State();

        /// <summary>Represents BaseScreen() - part 3 as a state.</summary>
        class BaseScreen3State : State
        {
            /// <summary>The loop count.</summary>
            public short i;

            /// <summary>The screen offset.</summary>
            public ushort topofs;

            /// <summary>Initialises the state.</summary>
            public void Initialise()
            {
                i = 1;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen3();
            }
        }

        /// <summary>The BaseScreen() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen3()
        {
            State nextState = _stateBaseScreen4;

            if(_stateBaseScreen3.i <= savedcount)
            {
                DrawPic((ushort) (1 + 2 * (savedcount - _stateBaseScreen3.i)), 6, EMPTYGUYPIC);
                score += REFUGEEPOINTS;
                PlaySound(GUYSCORESND);
                DrawScore();

                if(NBKascii != 27)
                {
                    // WaitVBL(30);
                    _stateWaitVBL.Initialise(this, 30, _stateBaseScreen3);
                    nextState = _stateWaitVBL;
                }

                _stateBaseScreen3.i++;
            }
            else
            {
                screenofs = _stateBaseScreen3.topofs;
                py += 5;
                PPrint(_strings[Strings.BaseScreen5]); // as: string replacements
                screenofs = 0; // draw into the split screen
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 4.</summary>
        private BaseScreen4State _stateBaseScreen4 = new BaseScreen4State();

        /// <summary>Represents BaseScreen() - part 4 as a state.</summary>
        class BaseScreen4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen4();
            }
        }

        /// <summary>The BaseScreen() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen4()
        {
            State nextState = _stateBaseScreen4;

            //
            // points for time remaining
            //
            if(timestruct.sec != 0 || timestruct.min != 0)
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

                if(NBKascii != 27)
                {
                    // WaitVBL(2);
                    _stateWaitVBL.Initialise(this, 2, _stateBaseScreen4);
                    nextState = _stateWaitVBL;
                }
            }
            else
            {
                if(objlist[0].hitpoints < 3)
                {
                    screenofs = _stateBaseScreen3.topofs;
                    PPrint(_strings[Strings.BaseScreen6]); // as: string replacements
                    screenofs = 0; // draw into the split screen
                    nextState = _stateBaseScreen5;
                }
                else
                {
                    nextState = _stateBaseScreen7;
                }
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 5.</summary>
        private BaseScreen5State _stateBaseScreen5 = new BaseScreen5State();

        /// <summary>Represents BaseScreen() - part 5 as a state.</summary>
        class BaseScreen5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen5();
            }
        }

        /// <summary>The BaseScreen() method - part 5.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen5()
        {
            State nextState = _stateBaseScreen6;

            //
            // heal tank
            //
            if(objlist[0].hitpoints < 3 && score > 10000)
            {
                score -= 10000;
                DrawScore();
                HealPlayer();

                if(NBKascii != 27)
                {
                    // WaitVBL(60);
                    _stateWaitVBL.Initialise(this, 60, _stateBaseScreen6);
                    nextState = _stateWaitVBL;
                }
            }
            else
            {
                nextState = _stateBaseScreen7;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 6.</summary>
        private BaseScreen6State _stateBaseScreen6 = new BaseScreen6State();

        /// <summary>Represents BaseScreen() - part 6 as a state.</summary>
        class BaseScreen6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen6();
            }
        }

        /// <summary>The BaseScreen() method - part 6.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen6()
        {
            ColorBorder(0);
            bordertime = 0;

            return _stateBaseScreen5;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 7.</summary>
        private BaseScreen7State _stateBaseScreen7 = new BaseScreen7State();

        /// <summary>Represents BaseScreen() - part 7 as a state.</summary>
        class BaseScreen7State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen7();
            }
        }

        /// <summary>The BaseScreen() method - part 7.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen7()
        {
            screenofs = _stateBaseScreen3.topofs;
            py = 110;

            if(level == NUMLEVELS)
                CPPrint(_strings[Strings.BaseScreen7]); // as: string replacements
            else
                CPPrint(_strings[Strings.BaseScreen8]); // as: string replacements

            StopDrive();

            // Ack();
            _stateAck.Initialise(this, _stateBaseScreen8);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>BaseScreen state - part 8.</summary>
        private BaseScreen8State _stateBaseScreen8 = new BaseScreen8State();

        /// <summary>Represents BaseScreen() - part 8 as a state.</summary>
        class BaseScreen8State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.BaseScreen8();
            }
        }

        /// <summary>The BaseScreen() method - part 8.</summary>
        /// <returns>The next state to set or null.</returns>
        private State BaseScreen8()
        {
            State nextState;

            if(level == NUMLEVELS)
                nextState = _stateVictory1;
            else
                nextState = _statePlayGame4;

            return nextState;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 1.</summary>
        private Victory1State _stateVictory1 = new Victory1State();

        /// <summary>Represents Victory() - part 1 as a state.</summary>
        class Victory1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory1();
            }
        }

        /// <summary>The Victory() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory1()
        {
            // FadeOut();
            _stateFadeOut.Initialise(_stateVictory2);
            return _stateFadeOut;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 2.</summary>
        private Victory2State _stateVictory2 = new Victory2State();

        /// <summary>Represents Victory() - part 2 as a state.</summary>
        class Victory2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory2();
            }
        }

        /// <summary>The Victory() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory2()
        {
            CacheDrawPic(ENDPIC);

            // FadeIn();
            _stateFadeIn.Initialise(_stateVictory3);
            return _stateFadeIn;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 3.</summary>
        private Victory3State _stateVictory3 = new Victory3State();

        /// <summary>Represents Victory() - part 3 as a state.</summary>
        class Victory3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory3();
            }
        }

        /// <summary>The Victory() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory3()
        {
            DrawWindow(0, 0, 39, 6);
            CPPrint(_strings[Strings.Victory1]); // as: string replacements
            CPPrint(_strings[Strings.Victory2]); // as: string replacements
            CPPrint(_strings[Strings.Victory3]); // as: string replacements
            CPPrint(_strings[Strings.Victory4]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateVictory4);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 4.</summary>
        private Victory4State _stateVictory4 = new Victory4State();

        /// <summary>Represents Victory() - part 4 as a state.</summary>
        class Victory4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory4();
            }
        }

        /// <summary>The Victory() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory4()
        {
            EraseWindow();

            CPPrint(_strings[Strings.Victory5]); // as: string replacements
            CPPrint(_strings[Strings.Victory6]); // as: string replacements
            CPPrint(_strings[Strings.Victory7]); // as: string replacements
            CPPrint(_strings[Strings.Victory8]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateVictory5);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 5.</summary>
        private Victory5State _stateVictory5 = new Victory5State();

        /// <summary>Represents Victory() - part 5 as a state.</summary>
        class Victory5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory5();
            }
        }

        /// <summary>The Victory() method - part 5.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory5()
        {
            EraseWindow();

            CPPrint(_strings[Strings.Victory9]); // as: string replacements
            CPPrint(_strings[Strings.Victory10]); // as: string replacements
            CPPrint(_strings[Strings.Victory11]); // as: string replacements
            CPPrint(_strings[Strings.Victory12]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateVictory6);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 6.</summary>
        private Victory6State _stateVictory6 = new Victory6State();

        /// <summary>Represents Victory() - part 6 as a state.</summary>
        class Victory6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory6();
            }
        }

        /// <summary>The Victory() method - part 6.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory6()
        {
            DrawWindow(10, 21, 30, 24);
            py += 3;
            CPPrint(_strings[Strings.Victory13]); // as: string replacements

            // Ack();
            _stateAck.Initialise(this, _stateVictory7);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>Victory state - part 7.</summary>
        private Victory7State _stateVictory7 = new Victory7State();

        /// <summary>Represents Victory() - part 7 as a state.</summary>
        class Victory7State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.Victory7();
            }
        }

        /// <summary>The Victory() method - part 7.</summary>
        /// <returns>The next state to set or null.</returns>
        private State Victory7()
        {
            level++;
            // return;
            return _statePlayGame11;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 1.</summary>
        private GameOver1State _stateGameOver1 = new GameOver1State();

        /// <summary>Represents GameOver() - part 1 as a state.</summary>
        class GameOver1State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver1();
            }
        }

        /// <summary>The GameOver() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver1()
        {
            State nextState;

            if(level != 21)
            {
#if !BLOCKING
                if(_sfxPlayer.IsSoundPlaying)
                    return null;
#endif

                _stateFadeUp.Initialise(_stateGameOver2);
                nextState = _stateFadeUp;
            }
            else
            {
                nextState = _stateGameOver4;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 2.</summary>
        private GameOver2State _stateGameOver2 = new GameOver2State();

        /// <summary>Represents GameOver() - part 2 as a state.</summary>
        class GameOver2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver2();
            }
        }

        /// <summary>The GameOver() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver2()
        {
            CacheDrawPic(DEATHPIC);
            PlaySound(NUKESND);
            // FadeDown();
            _stateFadeDown.Initialise(_stateGameOver3);
            return _stateFadeDown;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 3.</summary>
        private GameOver3State _stateGameOver3 = new GameOver3State();

        /// <summary>Represents GameOver() - part 3 as a state.</summary>
        class GameOver3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver3();
            }
        }

        /// <summary>The GameOver() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver3()
        {
            // Ack();
            _stateAck.Initialise(this, _stateGameOver4);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 4.</summary>
        private GameOver4State _stateGameOver4 = new GameOver4State();

        /// <summary>Represents GameOver() - part 4 as a state.</summary>
        class GameOver4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver4();
            }
        }

        /// <summary>The GameOver() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver4()
        {
            State nextState = _statePlayGame12;
            
            //
            // high score?
            //
            if(score > highscore)
            {
                PlaySound(HIGHSCORESND);
                // ExpWin(18, 11);
                _stateExpWin.Initialise(18, 11, _stateGameOver5);
                nextState = _stateExpWin;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 5.</summary>
        private GameOver5State _stateGameOver5 = new GameOver5State();

        /// <summary>Represents GameOver() - part 5 as a state.</summary>
        class GameOver5State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver5();
            }
        }

        /// <summary>The GameOver() method - part 5.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver5()
        {
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
            // Ack();
            _stateAck.Initialise(this, _stateGameOver6);
            return _stateAck;
        }

        // ==========================================================================================

        /// <summary>GameOver state - part 6.</summary>
        private GameOver6State _stateGameOver6 = new GameOver6State();

        /// <summary>Represents GameOver() - part 6 as a state.</summary>
        class GameOver6State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.GameOver6();
            }
        }

        /// <summary>The GameOver() method - part 6.</summary>
        /// <returns>The next state to set or null.</returns>
        private State GameOver6()
        {
            highscore = score;

#if SAVE_DATA_WHEN_CHANGED
            SaveCtrls();
#endif

            return _statePlayGame12;
        }

        // ==========================================================================================

        /// <summary>Common handler for FadeX methods.</summary>
        /// <param name="i">The color palette index.</param>
        private void FadeUpdate(short i)
        {
            colors[i][16] = (byte) bordercolor;

            SetPalette(i);
        }

        // ==========================================================================================

        /// <summary>FadeOut state.</summary>
        private FadeOutState _stateFadeOut = new FadeOutState();

        /// <summary>Represents FadeOut() as a state.</summary>
        class FadeOutState : State
        {
            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The state that will be set when FadeOut completes.</param>
            public void Initialise(State nextState)
            {
                i = 3;
                _nextState = nextState;
            }

            /// <summary>The loop variable.</summary>
            private short i;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                State nextState;

                if(i >= 0)
                {
                    hovertank.FadeUpdate(i);

                    hovertank._stateWaitVBL.Initialise(hovertank, 6, this);
                    nextState = hovertank._stateWaitVBL;

                    i--;
                }
                else
                {
                    nextState = _nextState;
                }

                return nextState;
            }
        }

        // ==========================================================================================

        /// <summary>FadeIn state.</summary>
        private FadeInState _stateFadeIn = new FadeInState();

        /// <summary>Represents FadeIn() as a state.</summary>
        class FadeInState : State
        {
            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The state that will be set when FadeIn completes.</param>
            public void Initialise(State nextState)
            {
                i = 0;
                _nextState = nextState;
            }

            /// <summary>The loop variable.</summary>
            private short i;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                State nextState;

                if(i < 4)
                {
                    hovertank.FadeUpdate(i);

                    hovertank._stateWaitVBL.Initialise(hovertank, 6, this);
                    nextState = hovertank._stateWaitVBL;

                    i++;
                }
                else
                {
                    nextState = _nextState;
                }

                return nextState;
            }
        }

        // ==========================================================================================

        /// <summary>FadeUp state.</summary>
        private FadeUpState _stateFadeUp = new FadeUpState();

        /// <summary>Represents FadeUp() as a state.</summary>
        class FadeUpState : State
        {
            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The state that will be set when FadeUp completes.</param>
            public void Initialise(State nextState)
            {
                i = 3;
                _nextState = nextState;
            }

            /// <summary>The loop variable.</summary>
            private short i;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                State nextState;

                if(i < 6)
                {
                    hovertank.FadeUpdate(i);

                    hovertank._stateWaitVBL.Initialise(hovertank, 6, this);
                    nextState = hovertank._stateWaitVBL;

                    i++;
                }
                else
                {
                    nextState = _nextState;
                }

                return nextState;
            }
        }

        // ==========================================================================================

        /// <summary>FadeDown state.</summary>
        private FadeDownState _stateFadeDown = new FadeDownState();

        /// <summary>Represents FadeDown() as a state.</summary>
        class FadeDownState : State
        {
            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The state that will be set when FadeDown completes.</param>
            public void Initialise(State nextState)
            {
                i = 5;
                _nextState = nextState;
            }

            /// <summary>The loop variable.</summary>
            private short i;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                State nextState;
                if(i > 2)
                {
                    hovertank.FadeUpdate(i);

                    hovertank._stateWaitVBL.Initialise(hovertank, 6, this);
                    nextState = hovertank._stateWaitVBL;

                    i--;
                }
                else
                {
                    nextState = _nextState;
                }
                return nextState;
            }
        }
        
        // ==========================================================================================

        /// <summary>WaitVBL state.</summary>
        private WaitVBLState _stateWaitVBL = new WaitVBLState();

        /// <summary>Represents WaitVBL() as a state.</summary>
        class WaitVBLState : State
        {
            /// <summary>The loop index.</summary>
            private ushort _i;

            /// <summary>The number of VBLs.</summary>
            private ushort _number;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            /// <param name="number">The number of VBLs to wait for.</param>
            /// <param name="nextState">The state that will be set when WaitVBL completes.</param>
            public void Initialise(Hovertank hovertank, ushort number, State nextState)
            {
                hovertank.verticalBlank = false;
                _i = 0;
                _number = number;
                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                if(hovertank.verticalBlank)
                    _i++;

                return (_i < _number ? null : _nextState);
            }
        }

        // ==========================================================================================

        /// <summary>SetScreen state.</summary>
        private SetScreenState _stateSetScreen = new SetScreenState();

        /// <summary>Represents SetScreen() as a state.</summary>
        class SetScreenState : State
        {
            /// <summary>The crtc parameter.</summary>
            private ushort _crtc;

            /// <summary>The pel parameter.</summary>
            private ushort _pel;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            /// <param name="crtc">The crtc parameter value.</param>
            /// <param name="pel">The pel parameter value.</param>
            /// <param name="nextState">The state that will be set when SetScreen completes.</param>
            public void Initialise(Hovertank hovertank, ushort crtc, ushort pel, State nextState)
            {
                hovertank.verticalBlank = false; // as: Wait for next VBL

                _crtc = crtc;
                _pel = pel;
                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                if(!hovertank.verticalBlank)
                    return null;

                hovertank.SetScreen(_crtc, _pel);
                return _nextState;
            }
        }

        // ==========================================================================================

        /// <summary>Ack state.</summary>
        private AckState _stateAck = new AckState();

        /// <summary>Represents Ack() as a state.</summary>
        class AckState : State
        {
            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            /// <param name="nextState">The state that will be set when Ack completes.</param>
            public void Initialise(Hovertank hovertank, State nextState)
            {
                hovertank.ClearKeys();

                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                if(hovertank.NBKscan > 127)
                {
                    hovertank.NBKscan &= 0x7f;
                    return _nextState;
                }

                ControlStruct c = hovertank.ControlPlayer(1);

                if(c.button1 || c.button2)
                    return _nextState;

                return null;
            }
        }

        // ==========================================================================================

        /// <summary>WarpEffect state - part 1.</summary>
        private WarpEffect1State _stateWarpEffect1 = new WarpEffect1State();

        /// <summary>Represents WarpEffect() - part 1 as a state.</summary>
        class WarpEffect1State : State
        {
            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Gets that follows the WarpEffect state.</summary>
            public State NextState
            {
                get { return _nextState; }
            }

            /// <summary>Initialises the state.</summary>
            /// <param name="nextState">The state that will be set when WarpEffect completes.</param>
            public void Initialise(State nextState)
            {
                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.WarpEffect1();
            }
        }

        /// <summary>The WarpEffect() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State WarpEffect1()
        {
            screenofs = screenloc[screenpage];
            // SetScreen(screenofs, 0);
            _stateSetScreen.Initialise(this, screenofs, 0, _stateWarpEffect2);
            return _stateSetScreen;
        }

        // ==========================================================================================

        /// <summary>WarpEffect state - part 2.</summary>
        private WarpEffect2State _stateWarpEffect2 = new WarpEffect2State();

        /// <summary>Represents WarpEffect() - part 2 as a state.</summary>
        class WarpEffect2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.WarpEffect2();
            }
        }

        /// <summary>The WarpEffect() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State WarpEffect2()
        {
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

            _stateWarpEffect3.oldtime = (int) timecount;

            PlaySound(WARPGATESND);

            return _stateWarpEffect3;
        }

        // ==========================================================================================

        /// <summary>WarpEffect state - part 3.</summary>
        private WarpEffect3State _stateWarpEffect3 = new WarpEffect3State();

        /// <summary>Represents WarpEffect() - part 3 as a state.</summary>
        class WarpEffect3State : State
        {
            /// <summary>The last time.</summary>
            public int oldtime;

            /// <summary>The total time.</summary>
            public int time;

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.WarpEffect3();
            }
        }

        /// <summary>The WarpEffect() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State WarpEffect3()
        {
            _stateWarpEffect3.time = (int) (timecount - _stateWarpEffect3.oldtime);

            if(_stateWarpEffect3.time > WARPSTEPS)
                _stateWarpEffect3.time = WARPSTEPS;

            screenofs = screenloc[(screenpage + _stateWarpEffect3.time / CYCLETIME) % 3];

            SC_ScaleShape(CENTERX, 64, (ushort) (255 * FOCUS / (WARPSTEPS + FOCUS - _stateWarpEffect3.time)),
                scalesegs[WARP1PIC + (_stateWarpEffect3.time / CYCLETIME) % 4]);

            // SetScreen(screenloc[(screenpage + time / CYCLETIME) % 3], 0);
            _stateSetScreen.Initialise(this, screenloc[(screenpage + _stateWarpEffect3.time / CYCLETIME) % 3], 0, _stateWarpEffect4);
            return _stateSetScreen;
        }

        // ==========================================================================================

        /// <summary>WarpEffect state - part 4.</summary>
        private WarpEffect4State _stateWarpEffect4 = new WarpEffect4State();

        /// <summary>Represents WarpEffect() - part 4 as a state.</summary>
        class WarpEffect4State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.WarpEffect4();
            }
        }

        /// <summary>The WarpEffect() method - part 4.</summary>
        /// <returns>The next state to set or null.</returns>
        private State WarpEffect4()
        {
            if(_stateWarpEffect3.time < WARPSTEPS && NBKascii != 27)
                return _stateWarpEffect3;

            ClearKeys();

            EGAWRITEMODE(0);
            EGABITMASK(255);

            return _stateWarpEffect1.NextState;
        }

        // ==========================================================================================

        /// <summary>PGet state - part 1.</summary>
        private PGet1State _statePGet1 = new PGet1State();

        /// <summary>Represents PGet() - part 1 as a state.</summary>
        class PGet1State : State
        {
            /// <summary>The old x.</summary>
            public short oldx;

            /// <summary>The return value.</summary>
            public short ReturnValue;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Gets the next state.</summary>
            public State NextState
            {
                get { return _nextState; }
            }

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            /// <param name="nextState">The state that will be set when PGet completes.</param>
            public void Initialise(Hovertank hovertank, State nextState)
            {
                oldx = (short) hovertank.px;

                hovertank.ClearKeys();

                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PGet1();
            }
        }

        /// <summary>The PGet() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PGet1()
        {
            State nextState;

            if((NoBiosKey(1) & 0xff) == 0)
            {
                DrawPchar('_');
                // WaitVBL(5);
                _stateWaitVBL.Initialise(this, 5, _statePGet2);
                nextState = _stateWaitVBL;
            }
            else
            {
                nextState = _statePGet3;
            }

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PGet state - part 2.</summary>
        private PGet2State _statePGet2 = new PGet2State();

        /// <summary>Represents PGet() - part 2 as a state.</summary>
        class PGet2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PGet2();
            }
        }

        /// <summary>The PGet() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PGet2()
        {
            State nextState;

            px = (ushort) _statePGet1.oldx;
            DrawPchar('_');
            px = (ushort) _statePGet1.oldx;

            if((NoBiosKey(1) & 0xff) != 0) // slight response improver
            {
                // break;
                nextState = _statePGet3;
                goto while_break;
            }

            // WaitVBL(5);
            nextState = _stateWaitVBL;
            _stateWaitVBL.Initialise(this, 5, _statePGet1);

        while_break:

            return nextState;
        }

        // ==========================================================================================

        /// <summary>PGet state - part 3.</summary>
        private PGet3State _statePGet3 = new PGet3State();

        /// <summary>Represents PGet() - part 3 as a state.</summary>
        class PGet3State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.PGet3();
            }
        }

        /// <summary>The PGet() method - part 3.</summary>
        /// <returns>The next state to set or null.</returns>
        private State PGet3()
        {
            // PGet3
            px = (ushort) _statePGet1.oldx;

            _statePGet1.ReturnValue = (short) NoBiosKey(0); // take it out of the buffer

            return _statePGet1.NextState;
        }

        // ==========================================================================================

        /// <summary>ExpWin state.</summary>
        private ExpWinState _stateExpWin = new ExpWinState();

        /// <summary>Represents ExpWin() as a state.</summary>
        class ExpWinState : State
        {
            /// <summary>The window width.</summary>
            private short _width;

            /// <summary>The window height.</summary>
            private short _height;

            /// <summary>Defines a window expansion decision.</summary>
            enum Decision
            {
                /// <summary>None (complete).</summary>
                None,

                /// <summary>The window expanded both horizontally and vertically.</summary>
                ExpWin,

                /// <summary>The window expanded horizontally.</summary>
                ExpWinH,

                /// <summary>The window expanded vertically.</summary>
                ExpWinV
            }

            /// <summary>The decisions taken during window expansion.</summary>
            private List<Decision> _decisions = new List<Decision>();

            /// <summary>The index of the next decision.</summary>
            private int _decisionIndex;

            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Initialises the state.</summary>
            /// <param name="width">The window width.</param>
            /// <param name="height">The window height.</param>
            /// <param name="nextState">The state that will be set when ExpWin completes.</param>
            public void Initialise(short width, short height, State nextState)
            {
                _width = width;
                _height = height;
                
                _decisions.Clear();
                _decisions.Add(Decision.None);

                Decision decision = ModifySize();
                while(decision != Decision.None)
                {
                    _decisions.Add(decision);
                    decision = ModifySize();
                }

                _decisionIndex = _decisions.Count - 1;
                _nextState = nextState;
            }

            /// <summary>Modifies the window size if necessary and returns info about the modification.</summary>
            /// <returns>The modification decision.</returns>
            private Decision ModifySize()
            {
                Decision decision = Decision.None;

                if(_width > 2)
                {
                    if(_height > 2)
                    {
                        // ExpWin((short) (width - 2), (short) (height - 2));
                        _width -= 2;
                        _height -= 2;
                        decision = Decision.ExpWin;
                    }
                    else
                    {
                        // ExpWinH((short) (width - 2), height);
                        _width -= 2;
                        decision = Decision.ExpWinH;
                    }
                }
                else
                {
                    if(_height > 2)
                    {
                        // ExpWinV(width, (short) (height - 2));
                        _height -= 2;
                        decision = Decision.ExpWinV;
                    }
                }

                return decision;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                State nextState = null;

                if(_decisionIndex != _decisions.Count - 1)
                {
                    hovertank.CenterWindow(_width, _height);

                    Decision decision = _decisions[_decisionIndex + 1];
                    switch(decision)
                    {
                        case Decision.None:
                            nextState = _nextState;
                            break;

                        case Decision.ExpWin:
                            _width += 2;
                            _height += 2;
                            break;

                        case Decision.ExpWinH:
                            _width += 2;
                            break;

                        case Decision.ExpWinV:
                            _height += 2;
                            break;
                    }
                }

                _decisionIndex--;

                if(nextState == null)
                {
                    // WaitVBL(1);
                    hovertank._stateWaitVBL.Initialise(hovertank, 1, hovertank._stateExpWin);
                    nextState = hovertank._stateWaitVBL;
                }

                return nextState;
            }
        }

        // ==========================================================================================

        /// <summary>InputInt state - part 1.</summary>
        private InputInt1State _stateInputInt1 = new InputInt1State();

        /// <summary>Represents InputInt() - part 1 as a state.</summary>
        class InputInt1State : State
        {
            /// <summary>The next state.</summary>
            private State _nextState;

            /// <summary>Gets the next state.</summary>
            public State NextState
            {
                get { return _nextState; }
            }

            /// <summary>The string.</summary>
            public StringBuilder _string = new StringBuilder();

            /// <summary>Records the character x locations.</summary>
            public ushort[] pxt = new ushort[90];
            
            /// <summary>The number of characters entered.</summary>
            public short count;

            /// <summary>The maximum number of characters that can be entered.</summary>
            public short max;

            /// <summary>The return value.</summary>
            public ushort ReturnValue;

            /// <summary>Initialises the state.</summary>
            /// <param name="hovertank">The hover tank engine.</param>
            /// <param name="max">The maximum number of characters that can be entered.</param>
            /// <param name="nextState">The state that will be set when InputInt completes.</param>
            public void Initialise(Hovertank hovertank, short max, State nextState)
            {
                _string.Length = 0;
                this.max = max;
                pxt[0] = hovertank.px;
                count = 0;
                _nextState = nextState;
            }

            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.InputInt1();
            }
        }

        /// <summary>The InputInt() method - part 1.</summary>
        /// <returns>The next state to set or null.</returns>
        private State InputInt1()
        {
            // key = _sys.toupper((sbyte) (PGet() & 0xff));
            _statePGet1.Initialise(this, _stateInputInt2);
            return _statePGet1;
        }

        // ==========================================================================================

        /// <summary>InputInt state - part 2.</summary>
        private InputInt2State _stateInputInt2 = new InputInt2State();

        /// <summary>Represents InputInt() - part 2 as a state.</summary>
        class InputInt2State : State
        {
            /// <summary>Executes the state.</summary>
            /// <param name="hovertank">The hover tank game.</param>
            /// <returns>The next state to set or null.</returns>
            public override State Execute(Hovertank hovertank)
            {
                return hovertank.InputInt2();
            }
        }

        /// <summary>The InputInt() method - part 2.</summary>
        /// <returns>The next state to set or null.</returns>
        private State InputInt2()
        {
            State nextState;

            // as: This is Input merged with InputInt, the return value of Input wasn't used

            // key = _sys.toupper((sbyte) (PGet() & 0xff));
            sbyte key = _sys.toupper((sbyte) (_statePGet1.ReturnValue & 0xff));

            if((key == 127 || key == 8) && _stateInputInt1.count > 0)
            {
                _stateInputInt1.count--;
                px = _stateInputInt1.pxt[_stateInputInt1.count];
                DrawPchar(_stateInputInt1._string[_stateInputInt1.count]);
                px = _stateInputInt1.pxt[_stateInputInt1.count];
            }

            if(key >= ' ' && key <= 'z' && _stateInputInt1.count < _stateInputInt1.max)
            {
                if(_stateInputInt1.count < _stateInputInt1._string.Length)
                    _stateInputInt1._string[_stateInputInt1.count] = (char) key;
                else
                    _stateInputInt1._string.Append((char) key);

                _stateInputInt1.count++;

                DrawPchar((ushort) key);
                _stateInputInt1.pxt[_stateInputInt1.count] = px;
            }

            if(key != 27 && key != 13)
            {
                _statePGet1.Initialise(this, _stateInputInt2);
                nextState = _statePGet1;
            }
            else
            {
                _stateInputInt1._string.Length = _stateInputInt1.count;

                // as: Added check for empty string
                if(_stateInputInt1._string.Length == 0)
                {
                    // return 0;
                    _stateInputInt1.ReturnValue = 0;
                    return _stateInputInt1.NextState;
                }

                ushort value, loop, loop1;
                if(_stateInputInt1._string[0] == '$')
                {
                    short digits = (short) (_stateInputInt1._string.Length - 2);
                    if(digits < 0)
                    {
                        // return 0;
                        _stateInputInt1.ReturnValue = 0;
                        return _stateInputInt1.NextState;
                    }

                    string hexstr = "0123456789ABCDEF";
                    for(value = 0, loop1 = 0; loop1 <= digits; loop1++)
                    {
                        sbyte digit = _sys.toupper((sbyte) _stateInputInt1._string[loop1 + 1]);
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
                else if(_stateInputInt1._string[0] == '%')
                {
                    short digits = (short) (_stateInputInt1._string.Length - 2);
                    if(digits < 0)
                    {
                        // return 0;
                        _stateInputInt1.ReturnValue = 0;
                        return _stateInputInt1.NextState;
                    }

                    for(value = 0, loop1 = 0; loop1 <= digits; loop1++)
                    {
                        if(_stateInputInt1._string[loop1 + 1] < '0' || _stateInputInt1._string[loop1 + 1] > '1')
                        {
                            // return 0;
                            _stateInputInt1.ReturnValue = 0;
                            return _stateInputInt1.NextState;
                        }

                        value |= (ushort) ((_stateInputInt1._string[loop1 + 1] - '0') << (digits - loop1));
                    }
                }
                else
                {
                    if(!ushort.TryParse(_stateInputInt1._string.ToString(), out value))
                        value = 0;
                }

                _stateInputInt1.ReturnValue = value;
                nextState = _stateInputInt1.NextState;
            }

            return nextState;
        }

        // ==========================================================================================

    }
}
