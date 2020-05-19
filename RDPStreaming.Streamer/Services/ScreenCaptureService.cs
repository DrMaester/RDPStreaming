using RDPStreaming.Streamer.Cropping;
using RDPStreaming.Streamer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RDPStreaming.Streamer.Services
{
    public class ScreenCaptureService : IDisposable
    {
        private DesktopDuplication.DesktopDuplicator _duplicator;
        private Action<RDPStreaming.Model.BitmapBufferedData[]> _newFrameCallback;

        public ScreenCaptureService(Action<RDPStreaming.Model.BitmapBufferedData[]> callback)
        {
            _duplicator = new DesktopDuplication.DesktopDuplicator(0, 0);
            _newFrameCallback = callback;
        }

        public Task StartCaptureAsync(CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var frame = _duplicator.GetLatestFrame();
                        if (frame != null && frame.UpdatedRegions.Length > 0)
                        {
                            var bufferedBitmaps = BitmapBufferedDataFactory.Create(frame);
                            _newFrameCallback?.Invoke(bufferedBitmaps.ToArray());
                        }
                        Task.Delay(20).Wait();
                    }
                }
                catch (OperationCanceledException)
                {

                }
            });
        }

        public void Dispose()
        {
            _duplicator = null;
        }
    }
}
