﻿using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models.Markup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models
{
    public class AddOrEditMarkupDialogViewModel : IPersonalAreaDialog
    {
        public bool IsEditMode { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
        public string SelectedMerchant { get; set; }
        public string AssetPairId { get; set; }
        public decimal DeltaSpread { get; set; }
        public decimal FixedFee { get; set; }
        public decimal Percent { get; set; }
        public int Pips { get; set; }
        public string PriceAssetPairId { get; set; }
        public string SelectedPriceMethod { get; set; }
        public List<PriceMethod> PriceMethod { get; set; }
    }
}
