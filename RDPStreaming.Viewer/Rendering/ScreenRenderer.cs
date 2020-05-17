using RDPStreaming.Viewer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;

namespace RDPStreaming.Viewer.Rendering
{
    public class ScreenRenderer : IScreenRenderer
    {
        private Font _fontConsolas12;
        private Size _bufferSize;
        private readonly object _updateLock = new object();
        public IntPtr HostHandle { get; private set; }
        public ConcurrentBag<Bitmap> RenderBitmaps { get; private set; }
        public Graphics GraphicsHost { get; private set; }
        public FpsCounter FpsCounter { get; private set; }
        public DirectBitmap BackBuffer { get; private set; }
        public BufferedGraphics BufferedGraphics { get; private set; }
        public IntPtr GraphicsHostDeviceContext { get; private set; }

        public ScreenRenderer(IntPtr hostHandle, Size bufferSize)
        {
            _fontConsolas12 = new Font("Consolas", 12);
            FpsCounter = new FpsCounter(new TimeSpan(0, 0, 0, 0, 1000));
            BackBuffer = new DirectBitmap(bufferSize);
            RenderBitmaps = new ConcurrentBag<Bitmap>();
            GraphicsHost = Graphics.FromHwnd(hostHandle);
            GraphicsHostDeviceContext = GraphicsHost.GetHdc();
            BufferedGraphics = BufferedGraphicsManager.Current.Allocate(GraphicsHostDeviceContext, new Rectangle(new Point(0, 0), bufferSize));
            HostHandle = hostHandle;
            _bufferSize = bufferSize;
        }
        public void Dispose()
        {
            lock (_updateLock)
            {
                HostHandle = default;

                GraphicsHost?.Dispose();
                GraphicsHost = default;

                FpsCounter?.Dispose();
                FpsCounter = default;

                RenderBitmaps.Clear();
                RenderBitmaps = default;

                _fontConsolas12?.Dispose();
                _fontConsolas12 = default;
            }
        }

        public void Render()
        {
            lock (_updateLock)
            {
                FpsCounter.StartFrame();
                Task.Delay(10).Wait();
                var t = DateTime.UtcNow.Millisecond / 1000.0;
                Color GetColor(int x, int y) => Color.FromArgb
                (
                    byte.MaxValue,
                    (byte)((double)x / _bufferSize.Width * byte.MaxValue),
                    (byte)((double)y / _bufferSize.Height * byte.MaxValue),
                    (byte)(Math.Sin(t * Math.PI) * byte.MaxValue)
                );

                Parallel.For(0, BackBuffer.Buffer.Length, index =>
                {
                    BackBuffer.GetXY(index, out var x, out var y);
                    BackBuffer.Buffer[index] = GetColor(x, y).ToArgb();
                });

                BackBuffer.Graphics.DrawString(FpsCounter.FpsString, _fontConsolas12, Brushes.Red, 0, 0);

                //BackBuffer.Graphics.Clear(Color.Black);
                BufferedGraphics.Graphics.DrawImage(BackBuffer.Bitmap, new RectangleF(PointF.Empty, _bufferSize),
                    new RectangleF(new PointF(-0.5f, -0.5f), _bufferSize), GraphicsUnit.Pixel);
                BufferedGraphics.Render(GraphicsHostDeviceContext);
                FpsCounter.StopFrame();
            }
        }

        public void UpdateHostHandle(IntPtr hostHandle, Size bufferSize)
        {
            lock (_updateLock)
            {
                var oldGraphics = GraphicsHost;
                var oldBackBuffer = BackBuffer;
                var oldBufferedGraphics = BufferedGraphics;

                var newHostHandle = hostHandle;
                var newBufferSize = bufferSize;
                var newGraphicsHost = Graphics.FromHwnd(hostHandle);
                var newGraphicsHostDeviceContext = newGraphicsHost.GetHdc();
                var newBufferedGraphics = BufferedGraphicsManager.Current.Allocate(newGraphicsHostDeviceContext, new Rectangle(new Point(0, 0), bufferSize));
                var newBackBuffer = new DirectBitmap(bufferSize);

                HostHandle = newHostHandle;
                _bufferSize = newBufferSize;
                GraphicsHost = newGraphicsHost;
                GraphicsHostDeviceContext = newGraphicsHostDeviceContext;
                BufferedGraphics = newBufferedGraphics;
                BackBuffer = newBackBuffer;

                oldGraphics?.Dispose();
                oldBackBuffer?.Dispose();
                oldBufferedGraphics?.Dispose();
            }
        }
    }
}
