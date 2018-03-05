using System.Text;
using Common.EncryptionTools;
using Core.Security;

namespace LkeServices.Security
{
    public class SrvSecurityHelper : ISrvSecurityHelper
    {
        public string EncodePrivateKey(string privateKey, string password)
        {
            var key = PrepareKey(password);

            return AESHelper.Encrypt128ECB(privateKey, key);
        }

        private string PrepareKey(string password)
        {
            const int keyLenght = 16;
            StringBuilder sb = new StringBuilder(password);
            if (sb.Length > keyLenght)
                sb.Remove(keyLenght, sb.Length - keyLenght);
            else
                sb.Append(' ', keyLenght - sb.Length);

            return sb.ToString();
        }
    }
}
