using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class BtctransfersPageViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
    }
    public class BtctransfersListViewModel
    {
        public string SelectedMerchant { get; set; }
        //public List<PaymentRequestModel> List { get; set; }
        public List<RequestTransferModel> List { get; set; }
    }
    public class RequestTransferModel
    {
        public PaymentRequestModel PaymentRequest { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
    }
}
