using BackOffice.Models;
using Lykke.Service.PayInternal.Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models.Assets
{
    public class AddGeneralSettingsDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string AssetDisplayId { get; set; }
        public List<string> NetworkList { get; set; }
        public string SelectedNetwork { get; set; }
        public bool PaymentAvailable { get; set; }
        public bool SettlementAvailable { get; set; }
    }
}
