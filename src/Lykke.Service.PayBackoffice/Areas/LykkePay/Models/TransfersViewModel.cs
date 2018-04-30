using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class TransfersPageViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public string SelectedAsset { get; set; }
        public IReadOnlyList<string> Assets { get; set; }
        public bool IsFullAccess { get; set; }
    }
    public class TransfersListViewModel
    {
        public string SelectedMerchant { get; set; }
        //public List<PaymentRequestModel> List { get; set; }
        public List<RequestTransferModel> List { get; set; }
        public string Assets { get; set; }
        public string Error { get; set; }
    }
    public class RequestTransferModel
    {
        public PaymentRequestModel PaymentRequest { get; set; }
        public IReadOnlyList<string> SourceWallet { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
    }
}
