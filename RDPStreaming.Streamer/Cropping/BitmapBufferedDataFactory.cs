using DesktopDuplication;
using RDPStreaming.Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace RDPStreaming.Streamer.Cropping
{
    public static class BitmapBufferedDataFactory
    {
        public static IEnumerable<BitmapBufferedData> Create(DesktopFrame frame)
        {
            foreach (var croppedImage in BitmapCropper.GetCroppedBitmaps(frame))
            {
                var bitmapBufferData = new BitmapBufferedData(croppedImage.Rectangle);
                bitmapBufferData.Buffer = EncodePicture(croppedImage.Bitmap);
                croppedImage.Dispose();
                yield return bitmapBufferData;
            }
        }

        private static byte[] EncodePicture(Bitmap bitmap)
        {
            using (var stream = new MemoryStream())
            {
                ImageCodecInfo jpgEncoder = GetEncoder(ImageFormat.Jpeg);

                System.Drawing.Imaging.Encoder myEncoder = System.Drawing.Imaging.Encoder.Quality;
                EncoderParameters myEncoderParameters = new EncoderParameters(1);
                // Save the bitmap as a JPG file with zero quality level compression.  
                EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, 150L);
                myEncoderParameters.Param[0] = myEncoderParameter;

                bitmap.Save(stream, jpgEncoder, myEncoderParameters);
                //bitmap.Save(stream, ImageFormat.Jpeg);
                return stream.ToArray();
            }
        }

        private static ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}
