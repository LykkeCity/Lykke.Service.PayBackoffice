using System.Collections.Generic;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using BackOffice.Models;
using BackOffice.Helpers;
using Lykke.Service.PayInternal.Client.Models.Asset;

namespace BackOffice.Areas.LykkePay.Models
{
    public class PaymentRequestPageViewModel : PagedListModel
    {
        public string SelectedMerchant { get; set; }
        public PaymentRequestStatusHelper SelectedStatus { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
        public IReadOnlyList<PaymentRequestStatusHelper> Statuses { get; set; }
        public string MerchantName { get; set; }
        public string SearchValue { get; set; }
    }

    /// <summary>Represent a payment request.</summary>
    public class PaymentRequestViewModel: PaymentRequestModel
    {
        public AssetGeneralSettingsResponse PaymentAssetGeneralSettings { get; set; }
        public AssetGeneralSettingsResponse SettlementAssetGeneralSettings { get; set; }
    }

    public class PaymentRequestListViewModel : PagedListModel
    {
        public string SelectedMerchant { get; set; }
        public IEnumerable<PaymentRequestViewModel> Requests { get; set; }
        public string BlockchainExplorerUrl { get; set; }
        public string EthereumBlockchainExplorerUrl { get; set; }
    }
}
