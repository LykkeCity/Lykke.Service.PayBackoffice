using Core.Settings;
using Core.Settings.LocalClients;
using Lykke.Service.ClientAccount.Client;
//using Lykke.Service.ClientAssetRule.Client;
using Lykke.Service.ExchangeOperations.Client;
using Lykke.Service.JumioIntegration.Client;
using Lykke.Service.Kyc.Client;
using Lykke.Service.PersonalData.Settings;
//using Lykke.Service.SwiftCredentials.Client;
using LkeServices.Strategy;
//using Lykke.Service.AssetDisclaimers.Client;
using Lykke.Service.FeeCalculator.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayAuth.Client;

namespace BackOffice.Settings
{
    public class BackOfficeBundle
    {
        public BackOfficeSettings BackOffice { get; set; }
        public PersonalDataServiceClientSettings PersonalDataServiceClient { get; set; }
        public JumioServiceClientSettings JumioServiceClient { get; set; }
        public ClientAccountServiceClientSettings ClientAccountServiceClient { get; set; }

        // TODO: rename class once package updated >= v.1.0.45
        public KycServiceSettings KycServiceClient { get; set; }
        public StrategiesSettings StrategiesSettings { get; set; }
        public RiskManagementServiceClientSettings RiskManagementServiceClient { get; set; }
        public ExchangeOperationsServiceClientSettings ExchangeOperationsServiceClient { get; set; }
        public NinjaServiceClientSettings NinjaServiceClient { get; set; }
        public RegulationServiceClientSettings RegulationServiceClient { get; set; }
        //public ClientAssetRuleServiceClientSettings ClientAssetRuleServiceClient { get; set; }
        //public SwiftCredentialsServiceClientSettings SwiftCredentialsServiceClient { get; set; }
        public SmsNotificationsSettings SmsNotifications { get; set; }
        public FeeCalculatorServiceClientSettings FeeCalculatorServiceClient { get; set; }
        public FeeSettings FeeSettings { get; set; }
        //public AssetDisclaimersServiceClientSettings AssetDisclaimersServiceClient { get; set; }

        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        public PayAuthServiceClientSettings PayAuthServiceClient { get; set; }
        public string BlockchainExplorerUrl { get; set; }
    }
}
