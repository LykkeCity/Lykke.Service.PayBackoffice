namespace Core.Images
{
    public enum ImageFormat
    {
        Png
    }
    
    public interface IImageBuilder
    {
        byte[] Resize(byte[] inStream, ImageFormat format, int? height = null, int? width = null);
    }
}
