using System.Collections.Generic;
using BackOffice.Models;
using Lykke.Service.PayMerchant.Client.Models;
using Lykke.Service.PayTransferValidation.Client.Models.MerchantConfiguration;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class MerchantTransferSettingsListViewModel : PagedListModel
    {
        public IEnumerable<MerchantModel> Merchants { get; set; }
        public bool IsEditAccess { get; set; }
        public bool IsFullAccess { get; set; }
        public string SelectedMerchant { get; set; }
        public IEnumerable<LineModel> Settings { get; set; }
    }
}
