using Core.Settings;

namespace BackOffice.Settings
{
    public class BackOfficeSettings
    {
        public string BlockchainExplorerUrl { get; set; }
        public DbSettings Db { get; set; }

        public BackOfficeServiceSettings Service { get; set; }

        public DeploymentSettings DeploymentSettings { get; set; }

        public BitcoinCoreSettings BitcoinCoreSettings { get; set; }

        public GoogleAuthSettings GoogleAuthSettings { get; set; }

        public TwoFactorVerificationSettingsEx TwoFactorVerification { get; set; }
        public LykkePayWalletListSettings LykkePayWalletList { get; set; }
    }
}
