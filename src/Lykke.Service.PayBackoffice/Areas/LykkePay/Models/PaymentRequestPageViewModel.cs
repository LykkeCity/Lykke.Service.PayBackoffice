using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;

namespace BackOffice.Areas.LykkePay.Models
{
    public class PaymentRequestPageViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public string MerchantName { get; set; }
    }
    public class PaymentRequestListViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<PaymentRequestModel> Requests { get; set; }
    }
}
