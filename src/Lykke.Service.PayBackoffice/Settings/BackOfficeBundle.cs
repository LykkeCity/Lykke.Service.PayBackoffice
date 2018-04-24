using Lykke.Service.PayInternal.Client;
using Lykke.Service.PayInvoice.Client;
using Lykke.Service.PayAuth.Client;

namespace BackOffice.Settings
{
    public class BackOfficeBundle
    {
        public BackOfficeSettings PayBackOffice { get; set; }
        public PayInternalServiceClientSettings PayInternalServiceClient { get; set; }
        public PayInvoiceServiceClientSettings PayInvoiceServiceClient { get; set; }
        public PayAuthServiceClientSettings PayAuthServiceClient { get; set; }
        public NinjaServiceClientSettings NinjaServiceClient { get; set; }
    }
}
