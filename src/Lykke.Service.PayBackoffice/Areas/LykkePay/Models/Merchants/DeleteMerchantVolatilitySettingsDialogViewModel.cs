using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class DeleteMerchantVolatilitySettingsDialogViewModel : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
    }
}
