namespace Core.StaticContent
{
    public enum WalletIconSize
    {
        Small,
        Medium,
        Large
    }

    public interface IStaticContentManager
    {
        #region Wallet Icons

        string GetWalletIconUrl(WalletIconSize iconSize, string walletName, string defaultWalletName);

        #endregion
    }
}
