using Lykke.Service.PayMerchant.Client.Models;
using System.Collections.Generic;

namespace BackOffice.Areas.LykkePay.Models.Assets
{
    public class MerchantsSettingsViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
    }
}
