using BackOffice.Models;
using Lykke.Service.PayMerchant.Client.Models;
using System.Collections.Generic;

namespace BackOffice.Areas.LykkePay.Models
{
    public class MerchantsListViewModel : PagedListModel
    {
        public IEnumerable<MerchantModel> Merchants { get; set; }
        public bool IsEditAccess { get; set; }
        public bool IsFullAccess { get; set; }
        public string SearchValue { get; set; }
        public bool FilterByEmail { get; set; }
    }
}
