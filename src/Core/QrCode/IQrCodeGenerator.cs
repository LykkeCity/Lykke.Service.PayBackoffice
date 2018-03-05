using System.IO;

namespace Core.QrCode
{
    public interface IQrCodeGenerator
    {
        MemoryStream GenerateGifQrCode(string value, int size);
    }
}
