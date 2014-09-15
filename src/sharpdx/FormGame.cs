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
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using SharpDX.Windows;
using SharpDX.Direct3D9;
using SharpDX;

namespace Hovertank3DdotNet.SharpDX
{
    /// <summary>The form that hosts the game view.</summary>
    partial class FormGame : RenderForm
    {
        /// <summary>Creates a new instance of the game form.</summary>
        public FormGame()
        {
            _dx9Resources = new List<IDX9Resource>();
            _disposableResources = new List<IDisposable>();

            InitializeComponent();
        }

        /// <summary>Window style - The window has a window menu on its title bar.</summary>
        private const int WS_SYSMENU = 0x00080000;

        /// <summary>Override CreateParams to remove the window menu.</summary>
        /// <remarks>This is to allow the ALT key to be used by the game.</remarks>
        /// <returns>The CreateParams for the form.</returns>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams createParams = base.CreateParams;
                createParams.Style &= ~WS_SYSMENU;
                return createParams;
            }
        }

        /// <summary>The D3D9 resources.</summary>
        private List<IDX9Resource> _dx9Resources;

        /// <summary>The list of disposable resources.</summary>
        private List<IDisposable> _disposableResources;

        /// <summary>Adds a resource to the list of resources.</summary>
        /// <param name="dx9Resource">A resource.</param>
        public void AddResource(IDX9Resource dx9Resource)
        {
            if(_dx9Resources.Contains(dx9Resource))
                throw new Exception("Resource previously added!");

            _dx9Resources.Add(dx9Resource);
        }

        /// <summary>Removes a resource from the list of resources.</summary>
        /// <param name="dx9Resource">A resource.</param>
        public void RemoveResource(IDX9Resource dx9Resource)
        {
            if(!_dx9Resources.Contains(dx9Resource))
                throw new Exception("Resource was't added!");

            _dx9Resources.Remove(dx9Resource);
        }

        /// <summary>Occurs when the form is disposing.</summary>
        /// <param name="disposing">Whether Dispose was invoked by managed code.</param>
        private void FormDisposing(bool disposing)
        {
            for(int i = 0; i < _dx9Resources.Count; i++)
            {
                try { _dx9Resources[i].Dispose(); }
                catch { }
            }
            _dx9Resources.Clear();

            for(int i = 0; i < _disposableResources.Count; i++)
            {
                try { _disposableResources[i].Dispose(); }
                catch { }
            }
            _disposableResources.Clear();

            _input = null;
            _direct3D = null;
            _device = null;
        }

        /// <summary>The PresentParameters.</summary>
        private PresentParameters _presentParameters;

        /// <summary>Initialises Direct3D.</summary>
        /// <param name="windowed">Whether to display in a window.</param>
        /// <param name="vsync">Whether to use vsync.</param>
        private void InitialiseDirect3D(bool windowed, bool vsync)
        {
            _presentParameters = new PresentParameters(ClientSize.Width, ClientSize.Height);
            _presentParameters.Windowed = windowed;

            if(vsync)
                _presentParameters.PresentationInterval = PresentInterval.One;
            else
                _presentParameters.PresentationInterval = PresentInterval.Immediate;

            _direct3D = new Direct3D();
            _disposableResources.Add(_direct3D);

            _device = new Device(_direct3D, 0, DeviceType.Hardware, Handle, CreateFlags.HardwareVertexProcessing, _presentParameters);
            _disposableResources.Add(_device);

            for(int i = 0; i < _dx9Resources.Count; i++)
                _dx9Resources[i].DeviceCreated(_device);

            for(int i = 0; i < _dx9Resources.Count; i++)
                _dx9Resources[i].DeviceReset(_device);
        }

        /// <summary>The Direct3D9 device.</summary>
        private Device _device;

        /// <summary>The Direct3D9 Direct3D.</summary>
        private Direct3D _direct3D;

        /// <summary>Whether the Direct 3D device has been lost.</summary>
        private bool _deviceLost;

        /// <summary>Resets the Direct 3D device.</summary>
        private void ResetDirect3D()
        {
            for(int i = 0; i < _dx9Resources.Count; i++)
                _dx9Resources[i].DeviceLost();

            _device.Reset(_presentParameters);

            for(int i = 0; i < _dx9Resources.Count; i++)
                _dx9Resources[i].DeviceReset(_device);
        }

        /// <summary>Renders the game.</summary>
        private void RenderView()
        {
            try
            {
                _renderer.Update(_hovertank.Display);

                if(_deviceLost)
                {
                    try
                    {
                        Thread.Sleep(100);
                        ResetDirect3D();
                        _deviceLost = false;
                    }
                    catch(Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine("Failed to reset Direct3D: " + ex.Message);
                    }
                    return;
                }

                _device.Clear(ClearFlags.Target, new ColorBGRA(0.0f, 0.0f, 0.0f, 1.0f), 1.0f, 0);
                _device.BeginScene();

                _renderer.Render(_device);

                _device.EndScene();
                _device.Present();
            }
            catch(SharpDXException dex)
            {
                if(dex.ResultCode == ResultCode.DeviceLost)
                    _deviceLost = true;
                else
                    System.Diagnostics.Debug.WriteLine("Caught Direct3D9 exception during Render: " + dex);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Caught Exception during Render: " + ex.Message);
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        /// <summary>The system.</summary>
        private SharpDXSys _sys;

        /// <summary>The renderer.</summary>
        private DX9SoftwareRenderer _renderer;

        /// <summary>The input systen.</summary>
        private SharpDXInputSystem _input;

        /// <summary>Initialises the game.</summary>
        public void InitialiseGame()
        {
            _sys = new SharpDXSys(Program.CommandLineArguments);

            bool windowed = _sys.GameConfig.VideoWindowed;
            
            Screen screen = Screen.FromHandle(Handle);
            int width, height;

            StartPosition = FormStartPosition.Manual;
            WindowState = FormWindowState.Normal;
            if(windowed)
            {
                width = _sys.GameConfig.VideoWindowWidth(screen.Bounds.Width);
                height = _sys.GameConfig.VideoWindowHeight(screen.Bounds.Height);
                FormBorderStyle = FormBorderStyle.FixedSingle;
            }
            else
            {
                width = screen.Bounds.Width;
                height = screen.Bounds.Height;
                FormBorderStyle = FormBorderStyle.None;
            }

            ClientSize = new Size(width, height);

            if(windowed)
            {
                int left, top;
                if(_sys.GameConfig.VideoWindowCenter)
                {
                    left = (screen.Bounds.Width - Size.Width) / 2;
                    top = (screen.Bounds.Height - Size.Height) / 2;
                }
                else
                {
                    left = _sys.GameConfig.VideoWindowLeft(screen.Bounds.Right - 32);
                    top = _sys.GameConfig.VideoWindowTop(screen.Bounds.Bottom - 32);
                }
                Location = new System.Drawing.Point(left, top);
            }

            bool vsync = _sys.GameConfig.VideoVSync;

            InitialiseDirect3D(windowed, vsync);

            _renderer = new DX9SoftwareRenderer();
            _renderer.Initialise(_device, 320, 200);
            _renderer.UseLinearFiltering = _sys.GameConfig.VideoFilter;
            AddResource(_renderer);

            _stopwatch = new Stopwatch();

            _hovertank = new Hovertank(_sys);
            _hovertank.StateInitialise();
            _disposableResources.Add(_sys);

            SoundSystem soundSystem = new SharpDXSoundSystem(this);
            _sys.InitialiseSound(soundSystem);
            _disposableResources.Add(soundSystem); // Note: The sound system needs to be disposed after sys

            _input = new SharpDXInputSystem(this);
            _sys.InitialiseInput(_input);
            _disposableResources.Add(_input);

            _stopwatch.Start();
        }

        /// <summary>The hover tank engine.</summary>
        private Hovertank _hovertank;

        /// <summary>The game timer.</summary>
        private Stopwatch _stopwatch;

        /// <summary>Whether the game is quitting.</summary>
        private bool _quitting;

        /// <summary>Updates the game.</summary>
        public void UpdateGame()
        {
            if(_quitting)
                return;

            _input.Update();

            TimeSpan elapsedTime = _stopwatch.Elapsed;
            if(elapsedTime.TotalSeconds > 1.0 / 70.0)
            {
                _stopwatch.Reset();
                _stopwatch.Start();

                try { _hovertank.StateUpdate(); }
                catch(ExitException)
                {
                    _quitting = true;
                    Close();
                    return;
                }
                catch(Exception ex)
                {
                    _quitting = true;

                    System.Diagnostics.Debug.WriteLine(ex);

                    MessageBox.Show(this, "A fatal error occurred:\n" + ex.Message,
                        "Hovertank3DdotNet (SharpDX)", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);

                    Close();
                    return;
                }

                _hovertank.UpdateSPKR();
                _hovertank.UpdateSPKR();
                _hovertank.SetVBL();

                RenderView();
            }
        }

    }
}
