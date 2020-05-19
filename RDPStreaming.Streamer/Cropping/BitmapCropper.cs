using DesktopDuplication;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using RDPStreaming.Streamer.Model;

namespace RDPStreaming.Streamer.Cropping
{
    public static class BitmapCropper
    {
        public static CroppedBitmap[] GetCroppedBitmaps(Bitmap source, int xCount, int yCount)
        {
            float xSize = source.Width / xCount;
            float ySize = source.Height / yCount;
            var bmps = new List<CroppedBitmap>();
            for (int x = 0; x < xCount; x++)
            {
                for (int y = 0; y < yCount; y++)
                {
                    var rectangle = new RectangleF(x * xSize, y * ySize, xSize, ySize);
                    try
                    {
                        var bmp = new CroppedBitmap(source.Clone(rectangle, source.PixelFormat), x * xSize, y * ySize);
                        bmps.Add(bmp);
                    }
                    catch (Exception ex)
                    {
                        return bmps.ToArray();
                    }
                }
            }

            return bmps.ToArray();
        }

        public static CroppedBitmap[] GetCroppedBitmaps(DesktopFrame frame)
        {
            var bmps = new List<CroppedBitmap>();
            try
            {
                foreach (var rect in frame.UpdatedRegions)
                {
                    var splittedRects = SplitRectangle(rect);
                    foreach (var splittedRect in splittedRects)
                    {
                        bmps.Add(new CroppedBitmap(frame.DesktopImage.Clone(splittedRect, frame.DesktopImage.PixelFormat), splittedRect));
                    }
                }
            }
            catch (Exception ex)
            {
            }

            return bmps.ToArray();
        }

        private static Rectangle[] SplitRectangle(Rectangle rectangle)
        {
            List<Rectangle> rectangles = new List<Rectangle>();
            if (IsRectangleToBig(rectangle))
            {
                if (rectangle.Width > rectangle.Height)
                {
                    var firstRectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width / 2, rectangle.Height);
                    var secondRectangle = new Rectangle(rectangle.X + (rectangle.Width / 2), rectangle.Y, rectangle.Width / 2, rectangle.Height);
                    rectangles.AddRange(SplitRectangle(firstRectangle));
                    rectangles.AddRange(SplitRectangle(secondRectangle));
                }
                else
                {
                    var firstRectangle = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height / 2);
                    var secondRectangle = new Rectangle(rectangle.X, rectangle.Y + (rectangle.Height / 2), rectangle.Width, rectangle.Height / 2);
                    rectangles.AddRange(SplitRectangle(firstRectangle));
                    rectangles.AddRange(SplitRectangle(secondRectangle));
                }
            }
            else
            {
                rectangles.Add(rectangle);
            }
            return rectangles.ToArray();
        }

        private static bool IsRectangleToBig(Rectangle rectangle)
        {
            return (rectangle.Width * rectangle.Height) > (15000 * 15);
        }
    }
}
