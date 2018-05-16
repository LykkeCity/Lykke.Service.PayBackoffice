using BackOffice.Models;
using Lykke.Service.PayInvoice.Core.Domain;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class AddOrEditMerchantSettingDialog : IPersonalAreaDialog
    {
        public string Caption { get; set; }
        public string Width { get; set; }
        public string MerchantId { get; set; }
        public string BaseAsset { get; set; }
    }
}
