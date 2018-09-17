using BackOffice.Models;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class AddOrEditMerchantVolatilitySettingsDialog : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
        public string ZeroCoverageAssetPairs { get; set; }
        public bool IsNew { get; set; }
    }
}
