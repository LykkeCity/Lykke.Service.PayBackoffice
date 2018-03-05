namespace Core.Security
{
    public interface ISrvSecurityHelper
    {
        string EncodePrivateKey(string privateKey, string password);
    }
}
