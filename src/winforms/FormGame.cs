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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace Hovertank3DdotNet.WinForms
{
    /// <summary>WinForms game form.</summary>
    partial class FormGame : Form
    {
        /// <summary>Creates a new FormGame.</summary>
        public FormGame()
        {
            _sys = new WinFormsSys(Program.CommandLineArguments);

            if(_sys.GameConfig.VideoWindowed)
            {
                Screen screen = Screen.FromHandle(Handle);

                int width = _sys.GameConfig.VideoWindowWidth(screen.Bounds.Width);
                int height = _sys.GameConfig.VideoWindowHeight(screen.Bounds.Height);

                FormBorderStyle = FormBorderStyle.FixedSingle;
                StartPosition = FormStartPosition.Manual;
                WindowState = FormWindowState.Normal;

                ClientSize = new Size(width, height);

                int left, top;
                if(_sys.GameConfig.VideoWindowCenter)
                {
                    left = (screen.Bounds.Width - Size.Width) / 2;
                    top = (screen.Bounds.Height - Size.Height) / 2;
                }
                else
                {
                    left = _sys.GameConfig.VideoWindowLeft(screen.Bounds.Width);
                    top = _sys.GameConfig.VideoWindowTop(screen.Bounds.Height);
                }
                Location = new Point(left, top);
            }
            else
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.CenterScreen;
                WindowState = FormWindowState.Maximized;
                MaximizeBox = false;
            }

            InitializeComponent();

            _sys.InitialiseSound(new WinFormsSoundSystem());

            WinFormsInputSystem input = new WinFormsInputSystem(this);
            _sys.InitialiseInput(input);

            BackColor = Color.Black;

            _imageView = new ImageView();
            _imageView.Location = new Point();
            _imageView.Size = ClientSize;
            _imageView.LinearFilter = _sys.GameConfig.VideoFilter;
            Controls.Add(_imageView);

            _hovertank = new Hovertank(_sys);
            _hovertank.StateInitialise();

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += backgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;

            backgroundWorker.RunWorkerAsync();
        }

        /// <summary>The system.</summary>
        private WinFormsSys _sys;

        /// <summary>Occurs when the form size is changed.</summary>
        /// <param name="e">The EventArgs.</param>
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            if(_imageView != null)
            {   // Center the game view
                Size clientSize = ClientSize;

                int scale = clientSize.Height / 240;
                if(scale == 0)
                    scale = 1;

                Size imageViewSize = new Size(320 * scale, 240 * scale);

                _imageView.Size = imageViewSize;
                _imageView.Location = new Point((clientSize.Width - imageViewSize.Width) / 2,
                    (clientSize.Height - imageViewSize.Height) / 2);
            }
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

        /// <summary>Overrides OnFormClosing to quit the game.</summary>
        /// <param name="e">The FormClosingEventArgs.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(!_quitting)
            {
                _quitting = true;
                try { backgroundWorker.CancelAsync(); }
                catch { }
            }

            base.OnFormClosing(e);
        }

        /// <summary>Background worker DoWork handler.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The DoWorkEventArgs.</param>
        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            // ReportProgress every 1/70th of a second
            while(!backgroundWorker.CancellationPending)
            {
                TimeSpan timeSpan = stopwatch.Elapsed;

                if(timeSpan.TotalSeconds >= 1.0 / 70.0)
                {
                    stopwatch.Reset();
                    stopwatch.Start();

                    backgroundWorker.ReportProgress(0);
                }

                Thread.Sleep(0);
            }
        }

        /// <summary>Background worker ProgressChanged handler.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The ProgressChangedEventArgs.</param>
        private void backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdateGame();
        }

        /// <summary>Background worker RunWorkerCompleted handler.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The RunWorkerCompletedEventArgs.</param>
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if(_fatalException != null)
            {
                MessageBox.Show("Fatal exception:\n" + _fatalException.Message + "\n\n" + _fatalException);
                Close();
            }
        }

        /// <summary>The image view control.</summary>
        private ImageView _imageView;

        /// <summary>The hover tank game engine.</summary>
        private Hovertank _hovertank;

        /// <summary>Updates the game.</summary>
        private void UpdateGame()
        {
            if(_quitting)
                return;

            try { _hovertank.StateUpdate(); }
            catch(ExitException)
            {
                _quitting = true;
                backgroundWorker.CancelAsync();
                Close();
                return;
            }
            catch(Exception ex)
            {
                _fatalException = ex;
                _sys.Log(ex);
                backgroundWorker.CancelAsync();
                return;
            }

            _hovertank.UpdateSPKR();
            _hovertank.UpdateSPKR();
            _hovertank.SetVBL();

            _imageView.UpdateView(_hovertank);
        }

        /// <summary>The fatal exception.</summary>
        private Exception _fatalException;

        /// <summary>Whether the game is quitting.</summary>
        private bool _quitting;
    }
}
