using BackOffice.Settings.SlackNotifications;
using Lykke.Service.BackofficeMembership.Client;
using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayAuth.Client;
using Lykke.Service.PayMerchant.Client;
using Lykke.Service.PaySettlement.Client;

namespace BackOffice.Settings
{
    public class BackOfficeBundle
    {
        public MonitoringServiceClientSettings MonitoringServiceClient { get; set; }
        public BackOfficeSettings PayBackOffice { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        public PayAuthServiceClientSettings PayAuthServiceClient { get; set; }
        public NinjaServiceClientSettings NinjaServiceClient { get; set; }
        public BackofficeMembershipServiceClientSettings BackofficeMembershipServiceClient { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public PayMerchantServiceClientSettings PayMerchantServiceClient { get; set; }
        public PaySettlementServiceClientSettings PaySettlementServiceClient { get; set; }
    }
}
