using Core.Settings;

namespace BackOffice.Settings
{
    public class BackOfficeSettings
    {
        public DbSettings Db { get; set; }

        public BackOfficeServiceSettings Service { get; set; }

        public MatchingOrdersSettings MatchingEngine { get; set; }

        public PaymentSystemsSettings PaymentSystems { get; set; }
        public LykkePayWalletListSettings LykkePayWalletList { get; set; }

        public LykkeServiceApiSettings LykkeServiceApi { get; set; }

        public DeploymentSettings DeploymentSettings { get; set; }

        public JobsSettings Jobs { get; set; }

        public CashoutSettings CashoutSettings { get; set; }

        public BitcoinCoreSettings BitcoinCoreSettings { get; set; }

        public MarginSettings MarginSettings { get; set; }

        public CacheSettings CacheSettings { get; set; }

        public GoogleAuthSettings GoogleAuthSettings { get; set; }

        public BcnReportsSettings BcnReports { get; set; }

        public double DefaultWithdrawalLimit { get; set; }

        public SwiftSettings SwiftSettings { get; set; }

        public PdfGeneratorSettings PdfGenerator { get; set; }

        public SupportToolsSettings SupportTools { get; set; }

        public TwoFactorVerificationSettingsEx TwoFactorVerification { get; set; }

        public GoogleDriveSettings GoogleDrive { get; set; }

        public InternalTransfersSettings InternalTransfersSettings { get; set; }
    }
}
