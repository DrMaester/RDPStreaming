using RDPStreaming.Viewer.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace RDPStreaming.Viewer.Rendering
{
    public interface IScreenRenderer : IDisposable
    {
        ConcurrentBag<Bitmap> RenderBitmaps { get; }
        IntPtr HostHandle { get; }
        IntPtr GraphicsHostDeviceContext { get; }
        Graphics GraphicsHost { get; }
        DirectBitmap BackBuffer { get; }
        BufferedGraphics BufferedGraphics { get; }
        FpsCounter FpsCounter { get; }
        void Render();
        void UpdateHostHandle(IntPtr hostHandle, Size bufferSize);
    }
}
