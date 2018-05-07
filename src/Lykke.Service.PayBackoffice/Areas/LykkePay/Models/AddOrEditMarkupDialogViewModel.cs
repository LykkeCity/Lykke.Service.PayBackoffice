using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AddOrEditMarkupDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string SelectedMerchant { get; set; }
        public string AssetPairId { get; set; }
        public decimal DeltaSpread { get; set; }
        public decimal FixedFee { get; set; }
        public decimal Percent { get; set; }
        public int Pips { get; set; }
    }
}
