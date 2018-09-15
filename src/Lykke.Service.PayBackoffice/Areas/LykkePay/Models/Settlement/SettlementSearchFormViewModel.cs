using Lykke.Service.PayMerchant.Client.Models;
using System.Collections.Generic;

namespace BackOffice.Areas.LykkePay.Models.Settlement
{
    public class SettlementSearchFormViewModel : ContinuationFormViewModel
    {      
        public string MerchantId { get; set; }

        public IReadOnlyList<MerchantModel> Merchants { get; set; }

        public string Query { get; set; }
    }
}
