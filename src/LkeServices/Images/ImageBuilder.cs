using System;
using System.IO;
using Core.Images;
using ImageSharp;
using ImageSharp.Formats;

namespace LkeServices.Images
{
   
    public class ImageBuilder : IImageBuilder
    {
        public byte[] Resize(byte[] inStream, ImageFormat format, int? height = null, int? width = null)
        {
            using (var stream = new MemoryStream(inStream))
            using (var outStream = new MemoryStream())
            {
                var img = new Image(stream);

                var h = height ?? img.Height;
                var w = width ?? img.Width;

                var result = img.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Max,
                    Size = new Size(h, w)
                });

                switch (format)
                {
                    case ImageFormat.Png:
                        result.SaveAsPng(outStream);
                        break;
                    default:
                        throw new NotImplementedException();
                }

                return outStream.ToArray();
            }
        }
    }
}
