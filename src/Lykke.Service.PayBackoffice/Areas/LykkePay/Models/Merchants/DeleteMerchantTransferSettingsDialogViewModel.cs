using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class DeleteMerchantTransferSettingsDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
        public string RuleId { get; set; }
        public string RuleDisplayName { get; set; }
    }
}
