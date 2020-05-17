using Protos;
using RDPStreaming.Viewer.Rendering;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace RDPStreaming.Viewer.ViewModel
{
    public class ViewerViewModel : BaseViewModel, IDisposable
    {
        private bool isDisposing;
        private string _connectionName;
        private System.Windows.Forms.Control _hostControl;
        public StreamerClient StreamerClient { get; private set; }
        public IScreenRenderer ScreenRenderer { get; private set; }
        public WindowsFormsHost HostControlWrapper { get; private set; }
        public string ConnectionName
        {
            get { return _connectionName; }
            set { _connectionName = value; OnPropertyChanged(nameof(ConnectionName)); }
        }

        public ViewerViewModel(StreamerClient streamerClient)
        {
            StreamerClient = streamerClient;
            ConnectionName = $"{StreamerClient.ComputerName} - {StreamerClient.Id}";
            CreateRendererControl();
        }

        public void Dispose()
        {
            isDisposing = true;
            StreamerClient = default;

            ScreenRenderer.Dispose();
            ScreenRenderer = default;

            _hostControl.Dispose();
            _hostControl = default;

            HostControlWrapper.SizeChanged -= ChangeRenderSize;
            HostControlWrapper.Dispose();
            HostControlWrapper = default;
        }

        public void StartRendering()
        {
            Task.Run(() =>
            {
                while (!isDisposing)
                {
                    ScreenRenderer.Render();
                    System.Windows.Forms.Application.DoEvents();
                }
            }).ConfigureAwait(false);
        }

        private void CreateRendererControl()
        {
            _hostControl = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = Color.Transparent,
                ForeColor = Color.Transparent,
            };
            HostControlWrapper = new WindowsFormsHost()
            {
                Child = _hostControl,
            };

            void EnsureFocus(System.Windows.Forms.Control control)
            {
                if (!control.Focused)
                {
                    control.Focus();
                }
            }

            _hostControl.MouseEnter += (sender, args) => EnsureFocus(_hostControl);
            _hostControl.MouseClick += (sender, args) => EnsureFocus(_hostControl);

            HostControlWrapper.SizeChanged += ChangeRenderSize;

            var size = new System.Drawing.Size((int)HostControlWrapper.Width, (int)HostControlWrapper.Height);
            if (size.Width < 1 || size.Height < 1)
            {
                size = new System.Drawing.Size(1, 1);
            }
            ScreenRenderer = new ScreenRenderer(_hostControl.Handle, size);
        }

        private void ChangeRenderSize(object sender, SizeChangedEventArgs args)
        {
            var size = new System.Drawing.Size((int)args.NewSize.Width, (int)args.NewSize.Height);
            if (size.Width < 1 || size.Height < 1)
            {
                size = new System.Drawing.Size(1, 1);
            }
            ScreenRenderer.UpdateHostHandle(_hostControl.Handle, size);
        }
    }
}
