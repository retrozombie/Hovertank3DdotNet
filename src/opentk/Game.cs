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
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Hovertank3DdotNet.OpenTK
{
    /// <summary>The game.</summary>
    class Game
    {
        /// <summary>Creates a new game.</summary>
        public Game()
        {
            _disposableObjects = new List<IDisposable>();
        }

        /// <summary>The system.</summary>
        private OpenTKSys _sys;

        /// <summary>The game window.</summary>
        private GameWindow _gameWindow;

        /// <summary>The hovertank engine.</summary>
        private Hovertank _hovertank;

        /// <summary>The input helper.</summary>
        private OpenTKInputSystem _input;

        /// <summary>The texture buffer.</summary>
        private TextureBuffer _textureBuffer;

        /// <summary>Runs the game.</summary>
        public void Run()
        {
            string[] commandLineArguments = Environment.GetCommandLineArgs();
            
            _sys = new OpenTKSys(commandLineArguments);
            
            DisplayDevice displayDevice = DisplayDevice.Default;

            bool windowed = _sys.GameConfig.VideoWindowed;
            int width, height;
            if(windowed)
            {
                width = _sys.GameConfig.VideoWindowWidth(displayDevice.Width);
                height = _sys.GameConfig.VideoWindowHeight(displayDevice.Height);
            }
            else
            {
                width = displayDevice.Width;
                height = displayDevice.Height;
            }

            GameWindowFlags gameWindowFlags = (windowed ? GameWindowFlags.FixedWindow : GameWindowFlags.Fullscreen);
            using(_gameWindow = new GameWindow(width, height, GraphicsMode.Default, "Hovertank3DdotNet (OpenTK)", gameWindowFlags))
            {
                _gameWindow.VSync = (_sys.GameConfig.VideoVSync ? VSyncMode.On : VSyncMode.Off);
                
                if(windowed)
                {
                    int left, top;
                    if(_sys.GameConfig.VideoWindowCenter)
                    {
                        left = (displayDevice.Width - _gameWindow.Width) / 2;
                        top = (displayDevice.Height - _gameWindow.Height) / 2;
                    }
                    else
                    {
                        left = _sys.GameConfig.VideoWindowLeft(displayDevice.Width);
                        top = _sys.GameConfig.VideoWindowTop(displayDevice.Height);
                    }

                    _gameWindow.Location = new Point(left, top);
                }

                _viewportSize = new Size(_gameWindow.Width, _gameWindow.Height);

                _gameWindow.Load += gameWindow_Load;
                _gameWindow.Resize += gameWindow_Resize;
                _gameWindow.UpdateFrame += gameWindow_UpdateFrame;
                _gameWindow.RenderFrame += gameWindow_RenderFrame;
                _gameWindow.Unload += gameWindow_Unload;

                // This can fail if OpenAL isn't installed
                SoundSystem soundSystem = new OpenTKSoundSystem();
                _sys.InitialiseSound(soundSystem);

                _input = new OpenTKInputSystem(_gameWindow);
                _disposableObjects.Add(_input);
                _sys.InitialiseInput(_input);

                _hovertank = new Hovertank(_sys);
                _hovertank.StateInitialise();

                // Dispose sound system after sounds
                _disposableObjects.Add(soundSystem);

                _gameWindow.Run(70.0);
            }
        }

        /// <summary>Handles the game window's Load event, initialises the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_Load(object sender, EventArgs e)
        {
            _textureBuffer = new TextureBuffer(Display.Width, Display.Height, _sys.GameConfig.VideoFilter);
            _disposableObjects.Add(_textureBuffer);
        }

        /// <summary>The list of disposable objects.</summary>
        private List<IDisposable> _disposableObjects;

        /// <summary>Handles the game window's Unload event.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_Unload(object sender, EventArgs e)
        {
            for(int i = 0; i < _disposableObjects.Count; i++)
            {
                try { _disposableObjects[i].Dispose(); }
                catch { }
            }

            _disposableObjects.Clear();
        
        }

        /// <summary>Handles the game window's Resize event, adjusts the viewport size.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_Resize(object sender, EventArgs e)
        {
            _viewportSize = new Size(_gameWindow.Width, _gameWindow.Height);
            GL.Viewport(0, 0, _gameWindow.Width, _gameWindow.Height);
        }

        /// <summary>The viewport size.</summary>
        private Size _viewportSize;
        
        /// <summary>Handles the game window's UpdateFrame event, updates the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The EventArgs.</param>
        private void gameWindow_UpdateFrame(object sender, FrameEventArgs e)
        {
            _input.Update();

            UpdateGame();
        }

        /// <summary>Updates the display texture.</summary>
        private void UpdateDisplayTexture()
        {
            // Update the image buffer
            Display display = _hovertank.Display;
            byte[] videoBuffer = display.VideoBuffer;
            int[] palette = display.Palette;
            int srcStride = display.Stride;
            int srcOffset = srcStride - Display.Width;
            int srcIndex = display.ScreenStartIndex + display.PixelOffset;

            int[] imageBuffer = _textureBuffer.ImageBuffer;
            int dstIndex = 0;
            int dstOffset = _textureBuffer.TextureWidth - _textureBuffer.ImageWidth;
            for(int y = 0; y < Display.Height; y++)
            {
                if(y == display.SplitScreenLines)
                    srcIndex = 0;

                for(int x = 0; x < Display.Width; x++)
                    imageBuffer[dstIndex++] = palette[videoBuffer[srcIndex++]];

                srcIndex += srcOffset;
                dstIndex += dstOffset;
            }

            // Update the texture
            GL.BindTexture(TextureTarget.Texture2D, _textureBuffer.TextureID);
            _textureBuffer.UpdateTexture();
        }

        /// <summary>Handles the game window's load event, initialises the game.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The FrameEventArgs.</param>
        private void gameWindow_RenderFrame(object sender, FrameEventArgs e)
        {
            GL.ClearColor(Color.Black);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            float aspect = ((float) _viewportSize.Width) / _viewportSize.Height;
            GL.Ortho(-aspect, aspect, -1.0, 1.0, 0.0, 1.0);

            GL.BindTexture(TextureTarget.Texture2D, _textureBuffer.TextureID);

            GL.Begin(PrimitiveType.Triangles);

            aspect = 4.0f / 3.0f;
            GL.Color3(Color.White);

            GL.TexCoord2(_textureBuffer.SMin, _textureBuffer.TMin);
            GL.Vertex2(-aspect, 1.0f);
            GL.TexCoord2(_textureBuffer.SMin, _textureBuffer.TMax);
            GL.Vertex2(-aspect, -1.0f);
            GL.TexCoord2(_textureBuffer.SMax, _textureBuffer.SMin);
            GL.Vertex2(aspect, 1.0f);

            GL.TexCoord2(_textureBuffer.SMax, _textureBuffer.TMin);
            GL.Vertex2(aspect, 1.0f);
            GL.TexCoord2(_textureBuffer.SMin, _textureBuffer.TMax);
            GL.Vertex2(-aspect, -1.0f);
            GL.TexCoord2(_textureBuffer.SMax, _textureBuffer.TMax);
            GL.Vertex2(aspect, -1.0f);

            GL.End();

            GL.BindTexture(TextureTarget.Texture2D, 0);

            _gameWindow.SwapBuffers();
        }

        /// <summary>Whether the game is quitting.</summary>
        private bool _quitting;

        /// <summary>Updates the game.</summary>
        private void UpdateGame()
        {
            if(_quitting)
                return;

            try { _hovertank.StateUpdate(); }
            catch(ExitException)
            {
                _quitting = true;
                _gameWindow.Exit();
                return;
            }
            catch(Exception ex)
            {
                _quitting = true;
                _sys.Log("StateUpdated failed: " + ex);
                // TODO display failure info
                _gameWindow.Exit();
                return;
            }

            UpdateDisplayTexture();

            _hovertank.SetVBL();
            _hovertank.UpdateSPKR();
            _hovertank.UpdateSPKR();
        }

    }
}
