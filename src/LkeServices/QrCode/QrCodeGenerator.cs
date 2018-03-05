using System;
using System.IO;
using Core.QrCode;

namespace LkeServices.QrCode
{
    //TODO: fix QrCodeGenerator
    public class QrCodeGenerator : IQrCodeGenerator
    {
        public MemoryStream GenerateGifQrCode(string value, int size)
        {
            throw new NotImplementedException();
        }

        //public MemoryStream GenerateGifQrCode(string value, int size)
        //{
        //    QrEncoder encoder = new QrEncoder(ErrorCorrectionLevel.M);
        //    Gma.QrCodeNet.Encoding.QrCode qrCode;
        //    encoder.TryEncode(value, out qrCode);

        //    GraphicsRenderer gRenderer = new GraphicsRenderer(
        //        new FixedCodeSize(size, QuietZoneModules.Two),
        //        Brushes.Black, Brushes.White);

        //    MemoryStream ms = new MemoryStream();
        //    gRenderer.WriteToStream(qrCode.Matrix, ImageFormat.Gif, ms);

        //    return ms;
        //}
    }
}
