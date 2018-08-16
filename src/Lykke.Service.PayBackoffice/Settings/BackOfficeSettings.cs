using Core.Settings;

namespace BackOffice.Settings
{
    public class BackOfficeSettings
    {
        public string BlockchainExplorerUrl { get; set; }
        public string EthereumBlockchainExplorerUrl { get; set; }
        public string PayInvoicePortalResetPasswordLink { get; set; }

        public DbSettings Db { get; set; }


        public GoogleAuthSettings GoogleAuth { get; set; }

        public BackOfficeTwoFactorVerificationSettings TwoFactorVerification { get; set; }

        public LykkePayWalletListSettings LykkePayWalletList { get; set; }

        public SupportedBrowsersSettings SupportedBrowsers { get; set; }
    }
}
