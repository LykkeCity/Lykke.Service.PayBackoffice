using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using Lykke.Service.PayInvoice.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
