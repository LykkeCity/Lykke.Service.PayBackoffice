using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Merchant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class MerchantsListViewModel : PagedListModel
    {
        public IEnumerable<MerchantModel> Merchants { get; set; }
        public string SearchValue { get; set; }
    }
}
