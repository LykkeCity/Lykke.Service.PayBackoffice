using Core.StaticContent;

namespace LkeServices.StaticContent
{
    public class StaticContentManager : IStaticContentManager
    {
        public string GetWalletIconUrl(WalletIconSize iconSize, string walletName, string defaultWalletName)
        {
            const string walletIconPathTemplate = "https://lkefiles.blob.core.windows.net:443/images/wallet_icons/{0}.png";

            string prefix = walletName == defaultWalletName ? "Lykke" : char.ToUpper(walletName[0]).ToString();

            string sizeSuffix = string.Empty;
            switch (iconSize)
            {
                case WalletIconSize.Medium:
                    sizeSuffix = "_md";
                    break;
                case WalletIconSize.Large:
                    sizeSuffix = "_lg";
                    break;
            }

            return string.Format(walletIconPathTemplate, $"{prefix}{sizeSuffix}");
        }
    }
}
