using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;

namespace RDPStreaming.Model
{
    public class BitmapBufferedData
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public byte[] Buffer { get; set; }

        public BitmapBufferedData(Rectangle rectangle)
        {
            Width = rectangle.Width;
            Height = rectangle.Height;
            X = rectangle.X;
            Y = rectangle.Y;
        }

        public BitmapBufferedData(byte[] buffer)
        {
            int index = 0;
            Width = BitConverter.ToInt32(buffer, index);
            index += 4;
            Height = BitConverter.ToInt32(buffer, index);
            index += 4;
            X = BitConverter.ToInt32(buffer, index);
            index += 4;
            Y = BitConverter.ToInt32(buffer, index);
            index += 4;
            int bufferSize = buffer.Length - sizeof(int) * 4;
            Buffer = new byte[bufferSize];
            Array.Copy(buffer, index, Buffer, 0, bufferSize);
        }

        public byte[] ToBytes()
        {
            if (Buffer == null)
                Buffer = new byte[0];

            int capacity = sizeof(int) * 4 + Buffer.Length;

            using (var memoryStream = new MemoryStream(capacity))
            {
                memoryStream.Write(BitConverter.GetBytes(Width), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(Height), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(X), 0, 4);
                memoryStream.Write(BitConverter.GetBytes(Y), 0, 4);
                memoryStream.Write(Buffer, 0, Buffer.Length);

                return memoryStream.ToArray();
            }
        }
    }
}
