using Lykke.Service.PaySettlement.Models;

namespace BackOffice.Areas.LykkePay.Models.Settlement
{
    public class SettlementPaymentRequestsViewModel
    {
        public PaymentRequestModel[] PaymentRequests { get; set; }

        public string ErrorMessage { get; set; }

        public string ContinuationToken { get; set; }
    }
}
