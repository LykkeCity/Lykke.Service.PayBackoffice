using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AddOrEditMerchantDialogViewModel : IPersonalAreaDialog
    {
        public bool IsNewMerchant { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        
        public string PublicKey { get; set; }
        public string ApiKey { get; set; }
        public double DeltaSpread { get; set; }
        public int TimeCacheRates { get; set; }
        public double LpMarkupPercent { get; set; }
        public int LpMarkupPips { get; set; }
        public double MarkupFixedFee { get; set; }
        public string LwId { get; set; }
        public string Certificate { get; set; }
        public string SystemId { get; set; }
    }
    public class DeleteMerchantDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }

        public string Name { get; set; }
        public string Id { get; set; }
    }
}
