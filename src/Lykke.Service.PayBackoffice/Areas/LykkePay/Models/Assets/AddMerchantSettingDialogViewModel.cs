using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models.Assets
{
    public class AddMerchantSettingDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string PaymentAssets { get; set; }
        public string SettlementAssets { get; set; }
        public string MerchantId { get; set; }
    }
}
