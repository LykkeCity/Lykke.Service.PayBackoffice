using BackOffice.Models;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayInvoice.Core.Domain;
using System.Collections.Generic;

namespace BackOffice.Areas.LykkePay.Models
{
    public class MerchantSettingsListViewModel : PagedListModel
    {
        public IEnumerable<MerchantModel> Merchants { get; set; }
        public bool IsEditAccess { get; set; }
        public bool IsFullAccess { get; set; }
        public string SelectedMerchant { get; set; }
        public IEnumerable<MerchantSetting> Settings { get; set; }
    }
}
