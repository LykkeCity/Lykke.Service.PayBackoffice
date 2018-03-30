using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AddAssetsByMerchantDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
        public string MerchantId { get; set; }
    }
}
