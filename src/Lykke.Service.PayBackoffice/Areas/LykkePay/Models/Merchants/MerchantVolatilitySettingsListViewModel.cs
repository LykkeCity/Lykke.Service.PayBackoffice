using System.Collections.Generic;
using BackOffice.Models;
using Lykke.Service.PayMerchant.Client.Models;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class MerchantVolatilitySettingsListViewModel : PagedListModel
    {
        public IEnumerable<MerchantModel> Merchants { get; set; }
        public bool IsEditAccess { get; set; }
        public bool IsFullAccess { get; set; }
        public string SelectedMerchant { get; set; }
        public IEnumerable<VolatilitySettingsResponse> Settings { get; set; }
    }
}
