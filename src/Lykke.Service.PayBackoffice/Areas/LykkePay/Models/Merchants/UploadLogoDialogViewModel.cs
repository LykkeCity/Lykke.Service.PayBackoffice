using BackOffice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackOffice.Areas.LykkePay.Models.Merchants
{
    public class UploadLogoDialogViewModel : IPersonalAreaDialog
    {
        public string MerchantId { get; set; }
        public string LogoImage { get; set; }
        public string Caption { get; set; }
        public string Width { get; set; }
    }
}
