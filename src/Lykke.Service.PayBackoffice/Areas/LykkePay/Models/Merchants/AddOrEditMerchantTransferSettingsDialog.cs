using System.Collections.Generic;
using BackOffice.Models;
using Lykke.Service.PayTransferValidation.Client.Models.Rule;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class AddOrEditMerchantTransferSettingsDialog : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
        public string RuleId { get; set; }
        public string RuleInput { get; set; }
        public bool Enabled { get; set; }
        public bool IsNew { get; set; }
        public IEnumerable<RegisteredRuleModel> RuleList { get; set; }
    }
}
