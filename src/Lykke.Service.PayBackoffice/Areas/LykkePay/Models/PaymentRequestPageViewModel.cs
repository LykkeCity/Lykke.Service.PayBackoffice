using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models
{
    public class PaymentRequestPageViewModel : PagedListModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public string MerchantName { get; set; }
    }
    public class PaymentRequestListViewModel : PagedListModel
    {
        public string SelectedMerchant { get; set; }
        public IEnumerable<PaymentRequestModel> Requests { get; set; }
        public string BlockchainExplorerUrl { get; set; }
    }
}
