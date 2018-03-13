using Lykke.Service.PayInternal.Client.Models.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AssetByMerchantViewModel
    {
        public string SelectedMerchant { get; set; }
        public IReadOnlyList<MerchantModel> Merchants { get; set; }
    }
}
