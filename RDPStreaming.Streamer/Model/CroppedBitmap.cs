using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;
using System.Drawing;

namespace RDPStreaming.Streamer.Model
{
    public class CroppedBitmap : IDisposable
    {
        private MemoryStream _memoryStream;
        public Bitmap Bitmap { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
        public System.Drawing.Rectangle Rectangle { get; set; }

        public CroppedBitmap(System.Drawing.Bitmap bitmap, float x, float y)
        {
            Bitmap = bitmap;
            X = x;
            Y = y;
        }

        public CroppedBitmap(System.Drawing.Bitmap bitmap, System.Drawing.Rectangle rect)
        {
            Bitmap = bitmap;
            X = rect.X;
            Y = rect.Y;
            Rectangle = rect;
        }

        public void Dispose()
        {
            Bitmap.Dispose();
            _memoryStream?.Dispose();
        }

        public byte[] GetBytes()
        {
            byte[] buffer;
            using (var ms = new MemoryStream())
            {
                var xBuffer = BitConverter.GetBytes(X);
                var yBuffer = BitConverter.GetBytes(Y);
                ms.Write(xBuffer);
                ms.Write(yBuffer);
                buffer = ms.ToArray();
            }
            using (var ms = new MemoryStream())
            {
                Bitmap.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                var bytes = ms.ToArray();
                buffer = buffer.Concat(bytes).ToArray();
            }

            return buffer;
        }

        public CroppedBitmap(byte[] croppedBitmapBuffer)
        {
            X = BitConverter.ToSingle(croppedBitmapBuffer.Take(sizeof(float)).ToArray(), 0);
            Y = BitConverter.ToSingle(croppedBitmapBuffer.Skip(sizeof(float)).Take(sizeof(float)).ToArray(), 0);
            _memoryStream = new MemoryStream(croppedBitmapBuffer.Skip(sizeof(float) * 2).ToArray());
            Bitmap = (System.Drawing.Bitmap)System.Drawing.Image.FromStream(_memoryStream);
        }
    }
}
